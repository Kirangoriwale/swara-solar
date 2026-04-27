using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarBilling.Models
{
    public class ServiceVisit
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "AMC")]
        public int AMCId { get; set; }

        [Required]
        [Display(Name = "Visit Date")]
        [DataType(DataType.Date)]
        public DateTime VisitDate { get; set; } = DateTime.Now;

        [Display(Name = "Visit Time")]
        [DataType(DataType.Time)]
        public TimeSpan? VisitTime { get; set; }

        [Display(Name = "Service Engineer")]
        [StringLength(200)]
        public string? ServiceEngineer { get; set; }

        [Display(Name = "Engineer Contact")]
        [StringLength(20)]
        public string? EngineerContact { get; set; }

        [Display(Name = "Work Description")]
        [StringLength(2000)]
        public string? WorkDescription { get; set; }

        [Display(Name = "Parts Replaced")]
        [StringLength(1000)]
        public string? PartsReplaced { get; set; }

        [Display(Name = "Parts Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PartsCost { get; set; }

        [Display(Name = "Service Charge")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ServiceCharge { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, In Progress, Completed, Cancelled

        [Display(Name = "Customer Feedback")]
        [StringLength(1000)]
        public string? CustomerFeedback { get; set; }

        [Display(Name = "Next Visit Date")]
        [DataType(DataType.Date)]
        public DateTime? NextVisitDate { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(2000)]
        public string? Remarks { get; set; }

        // System Information
        [Display(Name = "System Capacity (kW)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? SystemCapacity { get; set; }

        [Display(Name = "System Type")]
        [StringLength(20)]
        public string? SystemType { get; set; } // On-Grid, Off-Grid, Hybrid

        // Solar Panel Inspection
        [Display(Name = "Panel Cracks/Dust/Shading Status")]
        [StringLength(20)]
        public string? PanelCracksDustShadingStatus { get; set; } // OK, Repair Required

        [Display(Name = "Panel Cracks/Dust/Shading Observation")]
        [StringLength(500)]
        public string? PanelCracksDustShadingObservation { get; set; }

        [Display(Name = "Mounting Structure Rails Tight Status")]
        [StringLength(20)]
        public string? MountingStructureRailsTightStatus { get; set; } // OK, Repair Required

        [Display(Name = "Mounting Structure Rails Tight Observation")]
        [StringLength(500)]
        public string? MountingStructureRailsTightObservation { get; set; }

        [Display(Name = "Proper Tilt Angle Status")]
        [StringLength(10)]
        public string? ProperTiltAngleStatus { get; set; } // Yes, No

        [Display(Name = "Proper Tilt Angle Observation")]
        [StringLength(500)]
        public string? ProperTiltAngleObservation { get; set; }

        [Display(Name = "Earthing Connection Status")]
        [StringLength(20)]
        public string? EarthingConnectionStatus { get; set; } // OK, Repair Required

        [Display(Name = "Earthing Connection Observation")]
        [StringLength(500)]
        public string? EarthingConnectionObservation { get; set; }

        // Inverter Inspection
        [Display(Name = "Inverter ON/OFF Status")]
        [StringLength(20)]
        public string? InverterOnOffStatus { get; set; } // OK, Issue

        [Display(Name = "Inverter ON/OFF Observation")]
        [StringLength(500)]
        public string? InverterOnOffObservation { get; set; }

        [Display(Name = "AC/DC Connections Status")]
        [StringLength(20)]
        public string? AcDcConnectionsStatus { get; set; } // OK, Issue

        [Display(Name = "AC/DC Connections Observation")]
        [StringLength(500)]
        public string? AcDcConnectionsObservation { get; set; }

        [Display(Name = "Overheating Check Status")]
        [StringLength(20)]
        public string? OverheatingCheckStatus { get; set; } // OK, Issue

        [Display(Name = "Overheating Check Observation")]
        [StringLength(500)]
        public string? OverheatingCheckObservation { get; set; }

        [Display(Name = "Generation Data Reading")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? GenerationDataReading { get; set; }

        // Battery Inspection
        [Display(Name = "Battery Voltage")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? BatteryVoltage { get; set; }

        [Display(Name = "Terminal Connections Status")]
        [StringLength(20)]
        public string? TerminalConnectionsStatus { get; set; } // OK, Repair

        [Display(Name = "Terminal Connections Observation")]
        [StringLength(500)]
        public string? TerminalConnectionsObservation { get; set; }

        [Display(Name = "Water Level Status")]
        [StringLength(10)]
        public string? WaterLevelStatus { get; set; } // OK, Low

        [Display(Name = "Water Level Observation")]
        [StringLength(500)]
        public string? WaterLevelObservation { get; set; }

        [Display(Name = "Battery Temperature")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? BatteryTemperature { get; set; }

        // Wiring & Safety
        [Display(Name = "DC Wiring Status")]
        [StringLength(20)]
        public string? DcWiringStatus { get; set; } // OK, Repair/Replace

        [Display(Name = "DC Wiring Observation")]
        [StringLength(500)]
        public string? DcWiringObservation { get; set; }

        [Display(Name = "AC Wiring Status")]
        [StringLength(20)]
        public string? AcWiringStatus { get; set; } // OK, Repair/Replace

        [Display(Name = "AC Wiring Observation")]
        [StringLength(500)]
        public string? AcWiringObservation { get; set; }

        [Display(Name = "MCB/SPD/Isolator Status")]
        [StringLength(20)]
        public string? McbSpdIsolatorStatus { get; set; } // OK, Repair/Replace

        [Display(Name = "MCB/SPD/Isolator Observation")]
        [StringLength(500)]
        public string? McbSpdIsolatorObservation { get; set; }

        [Display(Name = "Overload/Loose Connection Status")]
        [StringLength(10)]
        public string? OverloadLooseConnectionStatus { get; set; } // Yes, No

        [Display(Name = "Overload/Loose Connection Observation")]
        [StringLength(500)]
        public string? OverloadLooseConnectionObservation { get; set; }

        // Performance Check
        [Display(Name = "Today's Generation (kWh)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? TodaysGeneration { get; set; }

        [Display(Name = "Average Daily Generation (kWh)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? AverageDailyGeneration { get; set; }

        [Display(Name = "Shading")]
        [StringLength(20)]
        public string? Shading { get; set; } // None, Partial, Heavy

        [Display(Name = "System Operating Condition")]
        [StringLength(20)]
        public string? SystemOperatingCondition { get; set; } // Excellent, Good, Average, Poor

        // Final Observation
        [Display(Name = "Required Repairs/Replacement")]
        [StringLength(2000)]
        public string? RequiredRepairsReplacement { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public AMC AMC { get; set; } = null!;
    }
}

