using System.ComponentModel.DataAnnotations;

namespace SolarBilling.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Trade Name")]
        [StringLength(200)]
        public string? TradeName { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(500)]
        public string? AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(500)]
        public string? AddressLine2 { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "State")]
        [StringLength(100)]
        public string? State { get; set; }

        [Display(Name = "PIN Code")]
        [StringLength(10)]
        public string? PinCode { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string? Mobile { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Display(Name = "Website")]
        [StringLength(200)]
        public string? Website { get; set; }

        [Display(Name = "GSTIN")]
        [StringLength(15)]
        public string? GSTIN { get; set; }

        [Display(Name = "PAN")]
        [StringLength(10)]
        public string? PAN { get; set; }

        [Display(Name = "CIN")]
        [StringLength(21)]
        public string? CIN { get; set; }

        [Display(Name = "Bank Name")]
        [StringLength(200)]
        public string? BankName { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [Display(Name = "IFSC Code")]
        [StringLength(11)]
        public string? IFSCCode { get; set; }

        [Display(Name = "Bank Branch")]
        [StringLength(200)]
        public string? BankBranch { get; set; }

        [Display(Name = "UPI ID")]
        [StringLength(200)]
        public string? UPIId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}

