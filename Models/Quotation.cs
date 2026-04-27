using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Quotation Number")]
        [StringLength(50)]
        public string QuotationNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quotation Date")]
        [DataType(DataType.Date)]
        public DateTime QuotationDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Valid Until")]
        [DataType(DataType.Date)]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Subtotal")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Tax Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        public string? Notes { get; set; }

        [Display(Name = "Terms & Conditions")]
        [StringLength(2000)]
        public string? TermsConditions { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Accepted, Rejected

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    }
}

