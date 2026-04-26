using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Dtos
{
    // ── Response DTOs ─────────────────────────────────────────────────────────

    public class QrTokenResponseDto
    {
        public string QrToken { get; set; } = string.Empty;
        public string QrType { get; set; } = string.Empty;
        public int ReferenceId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class DonationScanResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int DonationId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PickupScanResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class ScanQrDto
    {
        [Required]
        public string QrToken { get; set; } = string.Empty;
    }
}
