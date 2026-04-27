using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Invoice Number")]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "PO Number")]
        [StringLength(50)]
        public string? PONumber { get; set; }

        [Display(Name = "PO Date")]
        [DataType(DataType.Date)]
        public DateTime? PODate { get; set; }

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

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}

