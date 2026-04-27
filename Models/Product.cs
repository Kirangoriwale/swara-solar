using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "HSN Code")]
        [StringLength(20)]
        public string? HSNCode { get; set; }

        [Display(Name = "MRP")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999.99)]
        public decimal? MRP { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999.99)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Unit")]
        [StringLength(20)]
        public string Unit { get; set; } = "Nos";

        [Display(Name = "Tax Rate (%)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 18.00m;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
    }
}

