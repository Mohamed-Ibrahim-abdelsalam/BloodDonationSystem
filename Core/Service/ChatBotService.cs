using BloodDonationSystem.Enums;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using DomainLayer.Specifications;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction.Dtos;
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
    public class ChatBotService : IChatBotService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;

        private const string ChatBotEndpoint = "api/chatbot/message";

        // Chatbot Blood type string mapping (chatbot expects "O+" not "O_Positive")
        private static readonly Dictionary<BloodType, string> BloodTypeMap = new()
        {
            { BloodType.A_Positive,  "A+"  },
            { BloodType.A_Negative,  "A-"  },
            { BloodType.B_Positive,  "B+"  },
            { BloodType.B_Negative,  "B-"  },
            { BloodType.AB_Positive, "AB+" },
            { BloodType.AB_Negative, "AB-" },
            { BloodType.O_Positive,  "O+"  },
            { BloodType.O_Negative,  "O-"  },
        };

        public ChatBotService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            HttpClient httpClient)
        {
            _uow = uow;
            _userManager = userManager;
            _httpClient = httpClient;
        }

        // ── POST /api/chatbot/message ─────────────────────────────────────────
        public async Task<ChatMessageResponseDto> SendMessageAsync(
            string userId, ChatMessageRequestDto dto)
        {
            // ── STEP 1: Load user from DB ─────────────────────────────────────
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            // ── STEP 2: Load last confirmed donation (for LastDonationDate + donor data)
            // We only care about the most recent confirmed donation
            var donationsSpec = new DonationsByUserSpecification(userId);
            var allDonations = await _uow.Donations.GetAllWithSpecAsync(donationsSpec);
            var lastDonation = allDonations
                .Where(d => d.Status == DonationStatus.Confirmed)
                .OrderByDescending(d => d.ConfirmedAt)
                .FirstOrDefault();

            // ── STEP 3: Build user profile for chatbot ────────────────────────
            // Only send fields that actually exist in the DB.
            // Fields not in schema → null (never fake values).
            var userProfile = BuildUserProfile(user, lastDonation);

            // ── STEP 4: Build chatbot request payload ─────────────────────────
            var chatBotRequest = new ChatBotRequestDto
            {
                UserId = userId,
                Message = dto.Message.Trim(),
                Language = string.IsNullOrWhiteSpace(dto.Language) ? "ar" : dto.Language.Trim(),
                UserProfile = userProfile,
            };

            // ── STEP 5: Call external chatbot service ─────────────────────────
            ChatBotResponseDto chatBotResponse;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var httpResponse = await _httpClient.PostAsJsonAsync(
                    ChatBotEndpoint, chatBotRequest, cts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorBody = await httpResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"ChatBot service returned {(int)httpResponse.StatusCode}: {errorBody}");
                }

                var raw = await httpResponse.Content.ReadAsStringAsync();

                chatBotResponse = JsonSerializer.Deserialize<ChatBotResponseDto>(raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException(
                           "ChatBot service returned an invalid response.");
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException(
                    "ChatBot service timed out. Please try again.");
            }
            catch (HttpRequestException ex) when
                (ex.Message.Contains("503") || ex.Message.Contains("unavailable"))
            {
                throw new HttpRequestException(
                    "ChatBot service is currently unavailable. Please try again later.");
            }

            // ── STEP 6: Map to frontend DTO (never expose internal fields) ────
            return new ChatMessageResponseDto
            {
                Reply = chatBotResponse.Reply,
                Intent = chatBotResponse.Intent,
                IsEligible = chatBotResponse.IsEligible,
                WaitDays = chatBotResponse.WaitDays,
                Recommendations = chatBotResponse.Recommendations,
                Timestamp = chatBotResponse.Timestamp,
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Maps DB fields that exist in ApplicationUser + Donation to the chatbot profile.
        /// Fields NOT in the DB schema → null (never invent values).
        /// </summary>
        private ChatBotUserProfileDto BuildUserProfile(
            ApplicationUser user,
            Donation? lastDonation)
        {
            return new ChatBotUserProfileDto
            {
                // ── From ApplicationUser ──────────────────────────────────────
                Age = user.Age > 0 ? user.Age : null,
                Gender = MapGender(user.Gender),
                BloodType = BloodTypeMap.TryGetValue(user.BloodType, out var bt) ? bt : null,

                // ── From last Donation (if exists) ────────────────────────────
                WeightKg = lastDonation?.Weight,
                HasTattoo = lastDonation?.HasTattoo,
                LastDonationDate = lastDonation?.LastDonationDate
                                   ?? lastDonation?.ConfirmedAt,

                // ── Medical condition from last donation (free-text) ───────────
                // The DB stores MedicalCondition as a single string, not a list.
                // We wrap it in a list only if it has a value.
                MedicalConditions = !string.IsNullOrWhiteSpace(lastDonation?.MedicalCondition)
                    ? new List<string> { lastDonation!.MedicalCondition }
                    : new List<string>(),

                // ── Fields NOT in current DB schema → null ────────────────────
                TattooDate = null,
                IsPregnant = null,
                IsBreastfeeding = null,
                CurrentMedications = new List<string>(),
                Hemoglobin = null,
                RecentSurgery = null,
                RecentSurgeryMonths = null,
            };
        }

        private static string? MapGender(Gender gender) => gender switch
        {
            Gender.Male => "male",
            Gender.Female => "female",
            _ => null,
        };
    }
}
