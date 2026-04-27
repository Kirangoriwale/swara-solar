using System.ComponentModel.DataAnnotations;

namespace SolarBilling.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        [StringLength(200)]
        public string? ContactPerson { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string? Mobile { get; set; }

        // Billing Address
        [Display(Name = "Billing Address Line 1")]
        [StringLength(500)]
        public string? BillingAddressLine1 { get; set; }

        [Display(Name = "Billing Address Line 2")]
        [StringLength(500)]
        public string? BillingAddressLine2 { get; set; }

        [Display(Name = "Billing City")]
        [StringLength(100)]
        public string? BillingCity { get; set; }

        [Display(Name = "Billing State")]
        [StringLength(100)]
        public string? BillingState { get; set; }

        [Display(Name = "Billing PIN Code")]
        [StringLength(10)]
        public string? BillingPinCode { get; set; }

        // Shipping Address
        [Display(Name = "Shipping Address Line 1")]
        [StringLength(500)]
        public string? ShippingAddressLine1 { get; set; }

        [Display(Name = "Shipping Address Line 2")]
        [StringLength(500)]
        public string? ShippingAddressLine2 { get; set; }

        [Display(Name = "Shipping City")]
        [StringLength(100)]
        public string? ShippingCity { get; set; }

        [Display(Name = "Shipping State")]
        [StringLength(100)]
        public string? ShippingState { get; set; }

        [Display(Name = "Shipping PIN Code")]
        [StringLength(10)]
        public string? ShippingPinCode { get; set; }

        [Display(Name = "GSTIN")]
        [StringLength(15)]
        public string? GSTIN { get; set; }

        [Display(Name = "PAN")]
        [StringLength(10)]
        public string? PAN { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
        public ICollection<AMC> AMCs { get; set; } = new List<AMC>();
    }
}

