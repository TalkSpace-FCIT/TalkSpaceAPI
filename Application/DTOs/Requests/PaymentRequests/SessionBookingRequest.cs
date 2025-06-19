using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.PaymentRequests
{
    public class SessionBookingRequest
    {
        public Guid UserId { get; set; }
        public int SessionId { get; set; }
        public DateTime BookingDate { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [StringLength(3,MinimumLength =3)]
        public string Currency { get; set; } = "USD";
        public string? PaymentMethod { get; set; }
        public string? PaymentIntentId { get; internal set; }
        public string? ClientSecret { get; internal set; }
        public PaymentStatus? PaymentStatus{ get; set; }
    }
}
