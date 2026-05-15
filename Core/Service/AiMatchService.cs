using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Dtos;
using ServiceAbstraction.Dtos.BloodRequests;
using ServiceAbstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service
{
    public class AiMatchService : IAiMatchService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;

        // Priority upgrade threshold — must stay in sync with BloodRequestService
        private const int EmergencyThresholdDays = 3;

        // AI service endpoint (base URL set in DI via named HttpClient)
        private const string AiMatchEndpoint = "match/donor";

        public AiMatchService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            HttpClient httpClient)
        {
            _uow = uow;
            _userManager = userManager;
            _httpClient = httpClient;
        }

        // ── GET /api/ai/match-requests ────────────────────────────────────────
        public async Task<FrontendMatchResponseDto> GetMatchedRequestsAsync(
            string userId,
            BloodRequestQueryParams queryParams)
        {
            // ── STEP 1: Load donor profile ────────────────────────────────────
            var donor = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!donor.Latitude.HasValue || !donor.Longitude.HasValue)
                throw new InvalidOperationException(
                    "Your location is not set. Please update your profile with a location " +
                    "before using the matching feature.");

            // ── STEP 2: Fetch OPEN requests with optional pre-AI filters ──────
            var spec = new OpenBloodRequestsForAiSpecification(
                queryParams.BloodType,
                queryParams.Priority,
                queryParams.Search);

            var requests = (await _uow.BloodRequests.GetAllWithSpecAsync(spec)).ToList();

            if (!requests.Any())
                return new FrontendMatchResponseDto { Results = new List<FrontendMatchResultDto>() };

            // ── STEP 3: Recalculate priority (Normal → Emergency if overdue) ──
            var today = DateTime.UtcNow.Date;
            var changed = false;
            foreach (var r in requests)
            {
                if (r.NeededBy.HasValue &&
                    r.Priority != RequestPriority.Emergency &&
                    r.Status == BloodRequestStatus.Open)
                {
                    var days = (r.NeededBy.Value.Date - today).Days;
                    if (days <= EmergencyThresholdDays)
                    {
                        r.Priority = RequestPriority.Emergency;
                        _uow.BloodRequests.Update(r);
                        changed = true;
                    }
                }
            }
            if (changed)
                await _uow.SaveChangesAsync();

            // ── STEP 4: Build the AI request payload ──────────────────────────
            var donorBloodType = FormatBloodType(donor.BloodType);

            var aiPayload = new AiMatchRequestDto
            {
                UserId = userId,
                DonorBloodType = donorBloodType,
                DonorLatitude = donor.Latitude.Value,
                DonorLongitude = donor.Longitude.Value,
                TopN = 20,
                Requests = requests.Select(r => new AiRequestItemDto
                {
                    Id = r.Id,
                    RequestedByUserId = r.RequestedByUserId,
                    HospitalName = r.Hospital?.Name ?? r.HospitalName,
                    HospitalAddress = r.HospitalLocation,
                    HospitalLatitude = r.Latitude ?? 0,
                    HospitalLongitude = r.Longitude ?? 0,
                    BloodType = FormatBloodType(r.BloodType),
                    Quantity = r.Quantity,
                    Priority = MapPriorityToInt(r.Priority), // Normal=0, Emergency=1
                    NeededBy = r.NeededBy.HasValue
                                            ? r.NeededBy.Value.ToString("yyyy-MM-dd")
                                            : string.Empty,
                    Status = r.Status.ToString(),
                }).ToList(),
            };

            // ── STEP 5: Call external AI service ──────────────────────────────
            AiMatchResponseDto aiResponse;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var httpResponse = await _httpClient.PostAsJsonAsync(
                  "https://blood-matching-ai-production.up.railway.app/match/donor",
                   aiPayload,
                    cts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"AI service returned {(int)httpResponse.StatusCode}: " +
                        await httpResponse.Content.ReadAsStringAsync());

                var parsed = await httpResponse.Content
                    .ReadFromJsonAsync<AiMatchResponseDto>(
                        cancellationToken: cts.Token);

                aiResponse = parsed
                    ?? throw new InvalidOperationException(
                        "AI service returned an empty or unreadable response.");
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException(
                    "The AI matching service did not respond in time. Please try again.");
            }
            catch (HttpRequestException ex) when
                (ex.Message.Contains("AI service returned"))
            {
                throw; // re-throw our own descriptive message
            }
            catch (HttpRequestException)
            {
                throw new HttpRequestException(
                    "The AI matching service is currently unavailable. Please try again later.");
            }
            catch (JsonException)
            {
                throw new InvalidOperationException(
                    "Received an invalid response from the AI matching service.");
            }

            // ── STEP 6: Load requester full names from DB ─────────────────────
            // Collect unique requester IDs from AI results
            var requesterIds = aiResponse.Results
                .Select(r => r.RequestedByUserId)
                .Distinct()
                .ToHashSet();

            // Fetch users from DB using UserManager (Identity)
            var requesterNames = new Dictionary<string, string>();
            foreach (var id in requesterIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user is not null)
                    requesterNames[id] = user.FullName;
            }

            // ── STEP 7: Build user-friendly response ──────────────────────────
            var results = aiResponse.Results.Select(r => new FrontendMatchResultDto
            {
                RequestId = r.Id,
                RequestedByUserId = r.RequestedByUserId,
                RequesterName = requesterNames.GetValueOrDefault(r.RequestedByUserId, "Unknown"),
                HospitalName = r.HospitalName,
                HospitalAddress = r.HospitalAddress,
                BloodType = r.BloodType,
                Quantity = r.Quantity,
                Priority = MapPriorityToString(r.Priority),
                NeededBy = r.NeededBy,
                Status = r.Status,
                Distance = MapDistanceLabel(r.DistanceKm), // km → friendly label
                CompatibilityNote = r.CompatibilityNote,
            }).ToList();

            return new FrontendMatchResponseDto { Results = results };
        }

        // ══════════════════════════════════════════════════════════════════════
        // Private helpers
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Converts BloodType enum to the AI-expected string format.
        /// A_Positive → "A+" , O_Negative → "O-" , AB_Positive → "AB+" etc.
        /// </summary>
        private static string FormatBloodType(BloodType bt)
        {
            return bt.ToString()
                .Replace("_Positive", "+")
                .Replace("_Negative", "-");
        }

        /// <summary>
        /// Maps RequestPriority enum to the integer the AI expects.
        /// Normal = 0, Emergency = 1.
        /// </summary>
        private static int MapPriorityToInt(RequestPriority priority)
            => priority == RequestPriority.Emergency ? 1 : 0;

        /// <summary>
        /// Maps the AI integer priority back to a human-readable string.
        /// 1 → "Emergency", anything else → "Normal".
        /// </summary>
        private static string MapPriorityToString(int priority)
            => priority == 1 ? "Emergency" : "Normal";

        /// <summary>
        /// Converts a raw distance_km value into a user-friendly proximity label.
        /// &lt; 5 → "Near you" | 5–20 → "Moderate distance" | &gt; 20 → "Far"
        /// </summary>
        private static string MapDistanceLabel(double distanceKm) => distanceKm switch
        {
            < 5 => "Near you",
            <= 20 => "Moderate distance",
            _ => "Far",
        };
    }
}
