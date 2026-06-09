using BloodDonationSystem.Models;
using DomainLayer.Enums;
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
    public class BloodPredictionService : IBloodPredictionService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;

        private const string PredictionEndpoint = "predict";
        private static readonly int[] AllowedHorizons = { 7, 14, 30 };

        public BloodPredictionService(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager,
            HttpClient httpClient)
        {
            _uow = uow;
            _userManager = userManager;
            _httpClient = httpClient;
        }

        // ── GET /api/hospital/predictions ────────────────────────────────────
        public async Task<FrontendPredictionResponseDto> GetPredictionsAsync(
            string userId,
            int horizonDays)
        {
            // ── 1. Validate horizonDays ───────────────────────────────────────
            if (!AllowedHorizons.Contains(horizonDays))
                throw new ArgumentException(
                    $"Invalid horizonDays value '{horizonDays}'. Allowed values: 7, 14, 30.");

            // ── 2. Resolve Hospital Admin ─────────────────────────────────────
            var admin = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (!admin.HospitalId.HasValue)
                throw new InvalidOperationException(
                    "Your account is not linked to any hospital. " +
                    "Please contact the system administrator.");

            // ── 3. Validate hospital exists ───────────────────────────────────
            var hospitalSpec = new HospitalByIdSpecification(admin.HospitalId.Value);
            var hospital = await _uow.Hospitals.GetEntityWithSpecAsync(hospitalSpec)
                ?? throw new KeyNotFoundException(
                    $"Hospital with id {admin.HospitalId.Value} was not found.");

            // ── 4. Load ALL blood bags (Python needs full history for ML) ──────
            var bagSpec = new AllBloodBagsByHospitalSpecification(admin.HospitalId.Value);
            var bags = (await _uow.BloodBags.GetAllWithSpecAsync(bagSpec)).ToList();

            // ── 5. Build payload for Python service ───────────────────────────
            var payload = new PredictionRequestDto
            {
                HospitalId = admin.HospitalId.Value,
                HorizonDays = horizonDays,
                BloodBags = bags.Select(b => new BloodBagPayloadDto
                   {
                        BloodType   = (int)b.BloodType,            // enum int value (1–8)
                        Status      = MapBagStatusToInt(b.Status), // Available=0, Withdrawn=1
                        CreatedAt   = b.CreatedAt,
                        ExpiryDate  = b.ExpiryDate,
                        WithdrawnAt = b.WithdrawnAt,              // null for Available bags
                    }).ToList(),
            };

            // ── 6. Call Python prediction service ─────────────────────────────
            PredictionResponseDto pythonResponse;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                Console.WriteLine($"BaseAddress = {_httpClient.BaseAddress}");
                Console.WriteLine($"Endpoint = {PredictionEndpoint}");

                var httpResponse = await _httpClient.PostAsJsonAsync(
                  "https://blood-prediction-service-production.up.railway.app/predict",
                   payload,
                   cts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"Prediction service returned {(int)httpResponse.StatusCode}: " +
                        await httpResponse.Content.ReadAsStringAsync());

                /////////////////////////////////////////////

                // Read as string first then deserialize — avoids Content-Type\n'
                // mismatch issues with ReadFromJsonAsync\n'
                    var responseBody = await httpResponse.Content.ReadAsStringAsync(cts.Token);
    
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
  
                   var parsed = JsonSerializer.Deserialize<PredictionResponseDto>(
                        responseBody, jsonOptions);
    
                   pythonResponse = parsed
                        ?? throw new InvalidOperationException(
                            "Prediction service returned an empty or unreadable response.");


                /////////////////////////////////
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException(
                    "The prediction service did not respond in time. Please try again.");
            }
            catch (HttpRequestException ex) when
                (ex.Message.Contains("Prediction service returned"))
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw new HttpRequestException(
                    "The prediction service is currently unavailable. Please try again later.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Path: {ex.Path}");
                Console.WriteLine($"Line: {ex.LineNumber}");
                Console.WriteLine($"BytePosition: {ex.BytePositionInLine}");
                Console.WriteLine(ex.ToString());

                throw new InvalidOperationException(
                    "Received an invalid response from the prediction service.", ex);
            }

            // ── 7. Map Python response → frontend DTO ────────────────────────
            return new FrontendPredictionResponseDto
            {
                HorizonDays = pythonResponse.HorizonDays,
                DemandLevel = pythonResponse.DemandLevel,
                TotalExpectedUnits = pythonResponse.TotalExpectedUnits,
                TotalUnitsRequired = pythonResponse.TotalUnitsRequired,
                OverallAccuracy = pythonResponse.OverallAccuracyPercent,
                Predictions = pythonResponse.Predictions.Select(p =>
                    new FrontendBloodTypePredictionDto
                    {
                        BloodType = p.BloodType,
                        CurrentStock = (int)Math.Round(p.CurrentStock ?? 0),
                        RequiredUnits = p.UnitsRequired,
                        DaysOfCoverage = p.DaysOfCoverage,
                        ShortageExpected = p.ShortageExpected,
                        PredictionMethod = FormatMethodName(p.Method),
                    }).ToList(),
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Maps BloodBagStatus enum to the int the Python service expects.
        /// Available = 0, Withdrawn = 1
        /// </summary>
        private static int MapBagStatusToInt(BloodBagStatus status) => status switch
        {
            BloodBagStatus.Available => 0,
            BloodBagStatus.Withdrawn => 1,
            _ => 0,
        };

        /// <summary>
        /// Converts Python method string to a human-readable label.
        /// "ml_random_forest" → "Random Forest"
        /// "statistical"      → "Statistical"
        /// </summary>
        private static string FormatMethodName(string method) => method.ToLower() switch
        {
            "ml_random_forest" => "Random Forest",
            "statistical" => "Statistical",
            "ml_linear" => "Linear Regression",
            "ml_gradient" => "Gradient Boosting",
            _ => method, // fallback — return as-is
        };
    }
}
