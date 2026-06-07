using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ══════════════════════════════════════════════════════════════════════════
    // Frontend → .NET
    // ══════════════════════════════════════════════════════════════════════════

    public class ChatMessageRequestDto
    {
        /// <summary>The message the user wants to send to the chatbot.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Language code: "ar" (default) or "en".</summary>
        public string Language { get; set; } = "ar";
    }

    // ══════════════════════════════════════════════════════════════════════════
    // .NET → External ChatBot Service
    // ══════════════════════════════════════════════════════════════════════════

    public class ChatBotUserProfileDto
    {
        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("weight_kg")]
        public double? WeightKg { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("blood_type")]
        public string? BloodType { get; set; }

        [JsonPropertyName("has_tattoo")]
        public bool? HasTattoo { get; set; }

        [JsonPropertyName("tattoo_date")]
        public DateTime? TattooDate { get; set; }

        [JsonPropertyName("last_donation_date")]
        public DateTime? LastDonationDate { get; set; }

        [JsonPropertyName("is_pregnant")]
        public bool? IsPregnant { get; set; }

        [JsonPropertyName("is_breastfeeding")]
        public bool? IsBreastfeeding { get; set; }

        [JsonPropertyName("medical_conditions")]
        public List<string> MedicalConditions { get; set; } = new();

        [JsonPropertyName("current_medications")]
        public List<string> CurrentMedications { get; set; } = new();

        [JsonPropertyName("hemoglobin")]
        public double? Hemoglobin { get; set; }

        [JsonPropertyName("recent_surgery")]
        public bool? RecentSurgery { get; set; }

        [JsonPropertyName("recent_surgery_months")]
        public int? RecentSurgeryMonths { get; set; }
    }

    public class ChatBotRequestDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = "ar";

        [JsonPropertyName("user_profile")]
        public ChatBotUserProfileDto UserProfile { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // External ChatBot Service → .NET (internal deserialization only)
    // ══════════════════════════════════════════════════════════════════════════

    public class ChatBotResponseDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("reply")]
        public string Reply { get; set; } = string.Empty;

        [JsonPropertyName("intent")]
        public string? Intent { get; set; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("is_eligible")]
        public bool? IsEligible { get; set; }

        [JsonPropertyName("wait_days")]
        public int? WaitDays { get; set; }

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();

        [JsonPropertyName("gemini_active")]
        public bool? GeminiActive { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // .NET → Frontend
    // ══════════════════════════════════════════════════════════════════════════

    public class ChatMessageResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public string? Intent { get; set; }
        public bool? IsEligible { get; set; }
        public int? WaitDays { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}
