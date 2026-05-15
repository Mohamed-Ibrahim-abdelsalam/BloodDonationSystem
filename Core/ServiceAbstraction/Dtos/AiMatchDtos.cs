using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ══════════════════════════════════════════════════════════════════════════
    // 1. Sent TO the AI service
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Root payload posted to POST /match/donor on the AI service.
    /// </summary>
    public class AiMatchRequestDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("donor_blood_type")]
        public string DonorBloodType { get; set; } = string.Empty;

        [JsonPropertyName("donor_latitude")]
        public double DonorLatitude { get; set; }

        [JsonPropertyName("donor_longitude")]
        public double DonorLongitude { get; set; }

        [JsonPropertyName("top_n")]
        public int TopN { get; set; } = 20;

        [JsonPropertyName("requests")]
        public List<AiRequestItemDto> Requests { get; set; } = new();
    }

    /// <summary>
    /// Single blood request item inside the AI payload.
    /// priority is sent as int: Normal=0, Emergency=1.
    /// </summary>
    public class AiRequestItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("requested_by_user_id")]
        public string RequestedByUserId { get; set; } = string.Empty;

        [JsonPropertyName("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [JsonPropertyName("hospital_address")]
        public string HospitalAddress { get; set; } = string.Empty;

        [JsonPropertyName("hospital_latitude")]
        public double HospitalLatitude { get; set; }

        [JsonPropertyName("hospital_longitude")]
        public double HospitalLongitude { get; set; }

        [JsonPropertyName("blood_type")]
        public string BloodType { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>0 = Normal, 1 = Emergency — required by AI service.</summary>
        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("needed_by")]
        public string NeededBy { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 2. Received FROM the AI service
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Root response from the AI service.</summary>
    public class AiMatchResponseDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("donor_blood_type")]
        public string DonorBloodType { get; set; } = string.Empty;

        [JsonPropertyName("total_matched")]
        public int TotalMatched { get; set; }

        [JsonPropertyName("urgent_count")]
        public int UrgentCount { get; set; }

        [JsonPropertyName("normal_count")]
        public int NormalCount { get; set; }

        [JsonPropertyName("results")]
        public List<AiMatchResultDto> Results { get; set; } = new();
    }

    /// <summary>Single ranked result returned by the AI service.</summary>
    public class AiMatchResultDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("requested_by_user_id")]
        public string RequestedByUserId { get; set; } = string.Empty;

        [JsonPropertyName("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [JsonPropertyName("hospital_address")]
        public string HospitalAddress { get; set; } = string.Empty;

        [JsonPropertyName("hospital_latitude")]
        public double HospitalLatitude { get; set; }

        [JsonPropertyName("hospital_longitude")]
        public double HospitalLongitude { get; set; }

        [JsonPropertyName("blood_type")]
        public string BloodType { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("needed_by")]
        public string NeededBy { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("distance_km")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("match_score")]
        public double MatchScore { get; set; }

        [JsonPropertyName("blood_score")]
        public double BloodScore { get; set; }

        [JsonPropertyName("distance_score")]
        public double DistanceScore { get; set; }

        [JsonPropertyName("urgency_score")]
        public double UrgencyScore { get; set; }

        [JsonPropertyName("is_exact_blood_match")]
        public bool IsExactBloodMatch { get; set; }

        [JsonPropertyName("compatibility_note")]
        public string CompatibilityNote { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 3. Sent TO the frontend (clean, user-friendly)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Final response shape returned to the frontend.</summary>
    public class FrontendMatchResponseDto
    {
        public List<FrontendMatchResultDto> Results { get; set; } = new();
    }

    /// <summary>
    /// Single matched request row shown to the user.
    /// distance_km is intentionally hidden — only the friendly label is exposed.
    /// </summary>
    public class FrontendMatchResultDto
    {
        public int RequestId { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;

        /// <summary>Loaded from DB via requested_by_user_id — never from AI response.</summary>
        public string RequesterName { get; set; } = string.Empty;

        public string HospitalName { get; set; } = string.Empty;
        public string HospitalAddress { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int Quantity { get; set; }

        /// <summary>"Normal" or "Emergency" — mapped from AI int priority.</summary>
        public string Priority { get; set; } = string.Empty;

        public string NeededBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly distance label.
        /// &lt; 5 km → "Near you" | 5–20 km → "Moderate distance" | &gt; 20 km → "Far"
        /// </summary>
        public string Distance { get; set; } = string.Empty;

        public string CompatibilityNote { get; set; } = string.Empty;
    }
}
