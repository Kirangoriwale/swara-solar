using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 999999.99)]
        public decimal Quantity { get; set; }

        [Display(Name = "MRP")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MRP { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Tax Rate (%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; }

        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Tax Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public Invoice Invoice { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}

