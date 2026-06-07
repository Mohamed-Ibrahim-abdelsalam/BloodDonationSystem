using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ══════════════════════════════════════════════════════════════════════════
    // 1. Sent TO the Python prediction service
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Root payload posted to POST /predict on the Python service.</summary>
    public class PredictionRequestDto
    {
        [JsonPropertyName("hospital_id")]
        public int HospitalId { get; set; }

        [JsonPropertyName("horizon_days")]
        public int HorizonDays { get; set; }

        [JsonPropertyName("blood_bags")]
        public List<BloodBagPayloadDto> BloodBags { get; set; } = new();
    }

    /// <summary>
    /// Single blood bag item sent to the prediction service.
    /// blood_type and status are sent as integers per the Python API contract.
    /// </summary>
    public class BloodBagPayloadDto
    {
        /// <summary>
        /// BloodType as integer:
        /// 1=A+, 2=A-, 3=B+, 4=B-, 5=AB+, 6=AB-, 7=O+, 8=O-
        /// </summary>
        [JsonPropertyName("blood_type")]
        public int BloodType { get; set; }

        /// <summary>
        /// BloodBagStatus as integer:
        /// 0=Available, 1=Withdrawn
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 2. Received FROM the Python prediction service
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Root response from the Python service.</summary>
    public class PredictionResponseDto
    {
        [JsonPropertyName("hospital_id")]
        public int HospitalId { get; set; }

        [JsonPropertyName("horizon_days")]
        public int HorizonDays { get; set; }

        [JsonPropertyName("demand_level")]
        public string DemandLevel { get; set; } = string.Empty;

        [JsonPropertyName("total_expected_units")]
        public double TotalExpectedUnits { get; set; }

        [JsonPropertyName("total_units_required")]
        public double TotalUnitsRequired { get; set; }

        [JsonPropertyName("overall_accuracy_percent")]
        public double OverallAccuracyPercent { get; set; }

        [JsonPropertyName("predictions")]
        public List<BloodTypePredictionDto> Predictions { get; set; } = new();
    }

    /// <summary>Per-blood-type prediction result from Python.</summary>
    public class BloodTypePredictionDto
    {
        [JsonPropertyName("blood_type")]
        public string BloodType { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("predicted_total")]
        public double PredictedTotal { get; set; }

        [JsonPropertyName("predicted_per_day")]
        public List<double> PredictedPerDay { get; set; } = new();

        [JsonPropertyName("accuracy_percent")]
        public double AccuracyPercent { get; set; }

        [JsonPropertyName("current_stock")]
        public int CurrentStock { get; set; }

        [JsonPropertyName("units_required")]
        public double UnitsRequired { get; set; }

        [JsonPropertyName("days_of_coverage")]
        public double DaysOfCoverage { get; set; }

        [JsonPropertyName("shortage_expected")]
        public bool ShortageExpected { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 3. Returned TO the frontend (clean, dashboard-ready)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Frontend prediction dashboard response.</summary>
    public class FrontendPredictionResponseDto
    {
        public int HorizonDays { get; set; }
        public string DemandLevel { get; set; } = string.Empty;
        public double TotalExpectedUnits { get; set; }
        public double TotalUnitsRequired { get; set; }
        public double OverallAccuracy { get; set; }
        public List<FrontendBloodTypePredictionDto> Predictions { get; set; } = new();
    }

    /// <summary>Single blood-type prediction row shown in the dashboard.</summary>
    public class FrontendBloodTypePredictionDto
    {
        public string BloodType { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public double RequiredUnits { get; set; }
        public double DaysOfCoverage { get; set; }
        public bool ShortageExpected { get; set; }

        /// <summary>
        /// Human-readable method name.
        /// "ml_random_forest" → "Random Forest" | "statistical" → "Statistical"
        /// </summary>
        public string PredictionMethod { get; set; } = string.Empty;
    }
}
