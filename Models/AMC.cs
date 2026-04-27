using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class AMC
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "AMC Number")]
        [StringLength(50)]
        public string AMCNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddYears(1);

        [Display(Name = "AMC Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AMCAmount { get; set; }

        [Display(Name = "Frequency")]
        [StringLength(50)]
        public string? Frequency { get; set; } // Monthly, Quarterly, Half-Yearly, Yearly

        [Display(Name = "No. of Visits")]
        public int? NumberOfVisits { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Cancelled

        [Display(Name = "Description")]
        [StringLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Terms & Conditions")]
        [StringLength(2000)]
        public string? TermsConditions { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(200)]
        public string? ContactPerson { get; set; }

        [Display(Name = "Contact Phone")]
        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [Display(Name = "Contact Email")]
        [EmailAddress]
        [StringLength(200)]
        public string? ContactEmail { get; set; }

        [Display(Name = "Service Address")]
        [StringLength(500)]
        public string? ServiceAddress { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<ServiceVisit> ServiceVisits { get; set; } = new List<ServiceVisit>();
    }
}

