using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Models;

namespace SolarBilling.Controllers
{
    public class ServiceVisitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceVisitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceVisit
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiceVisits
                .Include(s => s.AMC)
                    .ThenInclude(a => a.Customer)
                .OrderByDescending(s => s.VisitDate)
                .ToListAsync());
        }

        // GET: ServiceVisit/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            var today = DateTime.Today;
            // Only show Scheduled visits (future visits that haven't been completed yet)
            var upcomingVisits = await _context.ServiceVisits
                .Include(s => s.AMC)
                    .ThenInclude(a => a.Customer)
                .Where(s => s.VisitDate >= today && 
                           s.Status == "Scheduled") // Only show Scheduled visits
                .ToListAsync();

            // Order on client side since SQLite doesn't support TimeSpan in ORDER BY
            var orderedVisits = upcomingVisits
                .OrderBy(s => s.VisitDate)
                .ThenBy(s => s.VisitTime ?? TimeSpan.MaxValue)
                .ToList();

            return View(orderedVisits);
        }

        // GET: ServiceVisit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceVisit = await _context.ServiceVisits
                .Include(s => s.AMC)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceVisit == null)
            {
                return NotFound();
            }

            return View(serviceVisit);
        }

        // GET: ServiceVisit/Create
        public async Task<IActionResult> Create(int? amcId)
        {
            var amcs = await _context.AMCs
                .Include(a => a.Customer)
                .OrderBy(a => a.AMCNumber)
                .ToListAsync();
            
            ViewData["AMCId"] = new SelectList(amcs, "Id", "AMCNumber", amcId);
            ViewBag.AMCs = amcs; // For displaying customer name in view
            
            // Default to tomorrow (future date) for new service visits
            var serviceVisit = new ServiceVisit
            {
                VisitDate = DateTime.Today.AddDays(1), // Tomorrow by default
                VisitTime = new TimeSpan(9, 0, 0), // 9:00 AM default
                Status = "Scheduled"
            };
            
            if (amcId.HasValue)
            {
                serviceVisit.AMCId = amcId.Value;
            }
            
            return View(serviceVisit);
        }

        // POST: ServiceVisit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AMCId,VisitDate,VisitTime,ServiceEngineer,EngineerContact,WorkDescription,PartsReplaced,PartsCost,ServiceCharge,TotalAmount,Status,CustomerFeedback,NextVisitDate,Remarks,SystemCapacity,SystemType,PanelCracksDustShadingStatus,PanelCracksDustShadingObservation,MountingStructureRailsTightStatus,MountingStructureRailsTightObservation,ProperTiltAngleStatus,ProperTiltAngleObservation,EarthingConnectionStatus,EarthingConnectionObservation,InverterOnOffStatus,InverterOnOffObservation,AcDcConnectionsStatus,AcDcConnectionsObservation,OverheatingCheckStatus,OverheatingCheckObservation,GenerationDataReading,BatteryVoltage,TerminalConnectionsStatus,TerminalConnectionsObservation,WaterLevelStatus,WaterLevelObservation,BatteryTemperature,DcWiringStatus,DcWiringObservation,AcWiringStatus,AcWiringObservation,McbSpdIsolatorStatus,McbSpdIsolatorObservation,OverloadLooseConnectionStatus,OverloadLooseConnectionObservation,TodaysGeneration,AverageDailyGeneration,Shading,SystemOperatingCondition,RequiredRepairsReplacement")] ServiceVisit serviceVisit)
        {
            // Clear AMCId-related validation errors
            var amcIdKeys = ModelState.Keys.Where(k => k.Contains("AMCId") || k.Contains("AMC")).ToList();
            foreach (var key in amcIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate AMCId manually
            if (serviceVisit.AMCId <= 0)
            {
                ModelState.AddModelError("AMCId", "Please select an AMC.");
            }

            // Validate VisitDate is in the future (for new service visits)
            if (serviceVisit.VisitDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("VisitDate", "Visit date must be today or a future date.");
            }

            // Ensure status is Scheduled for new visits
            serviceVisit.Status = "Scheduled";

            // Calculate Total Amount if Parts Cost or Service Charge is provided
            if (serviceVisit.PartsCost.HasValue || serviceVisit.ServiceCharge.HasValue)
            {
                serviceVisit.TotalAmount = (serviceVisit.PartsCost ?? 0) + (serviceVisit.ServiceCharge ?? 0);
            }

            if (ModelState.IsValid)
            {
                serviceVisit.CreatedDate = DateTime.UtcNow;
                _context.Add(serviceVisit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var amcs = await _context.AMCs
                .Include(a => a.Customer)
                .OrderBy(a => a.AMCNumber)
                .ToListAsync();
            
            ViewData["AMCId"] = new SelectList(amcs, "Id", "AMCNumber", serviceVisit.AMCId);
            ViewBag.AMCs = amcs; // For displaying customer name in view
            return View(serviceVisit);
        }

        // GET: ServiceVisit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceVisit = await _context.ServiceVisits.FindAsync(id);
            if (serviceVisit == null)
            {
                return NotFound();
            }

            var amcs = await _context.AMCs
                .Include(a => a.Customer)
                .OrderBy(a => a.AMCNumber)
                .ToListAsync();
            
            ViewData["AMCId"] = new SelectList(amcs, "Id", "AMCNumber", serviceVisit.AMCId);
            ViewBag.AMCs = amcs; // For displaying customer name in view
            return View(serviceVisit);
        }

        // POST: ServiceVisit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AMCId,VisitDate,VisitTime,ServiceEngineer,EngineerContact,WorkDescription,PartsReplaced,PartsCost,ServiceCharge,TotalAmount,Status,CustomerFeedback,NextVisitDate,Remarks,CreatedDate,SystemCapacity,SystemType,PanelCracksDustShadingStatus,PanelCracksDustShadingObservation,MountingStructureRailsTightStatus,MountingStructureRailsTightObservation,ProperTiltAngleStatus,ProperTiltAngleObservation,EarthingConnectionStatus,EarthingConnectionObservation,InverterOnOffStatus,InverterOnOffObservation,AcDcConnectionsStatus,AcDcConnectionsObservation,OverheatingCheckStatus,OverheatingCheckObservation,GenerationDataReading,BatteryVoltage,TerminalConnectionsStatus,TerminalConnectionsObservation,WaterLevelStatus,WaterLevelObservation,BatteryTemperature,DcWiringStatus,DcWiringObservation,AcWiringStatus,AcWiringObservation,McbSpdIsolatorStatus,McbSpdIsolatorObservation,OverloadLooseConnectionStatus,OverloadLooseConnectionObservation,TodaysGeneration,AverageDailyGeneration,Shading,SystemOperatingCondition,RequiredRepairsReplacement")] ServiceVisit serviceVisit)
        {
            if (id != serviceVisit.Id)
            {
                return NotFound();
            }

            // Clear AMCId-related validation errors
            var amcIdKeys = ModelState.Keys.Where(k => k.Contains("AMCId") || k.Contains("AMC")).ToList();
            foreach (var key in amcIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate AMCId manually
            if (serviceVisit.AMCId <= 0)
            {
                ModelState.AddModelError("AMCId", "Please select an AMC.");
            }

            // Calculate Total Amount if Parts Cost or Service Charge is provided
            if (serviceVisit.PartsCost.HasValue || serviceVisit.ServiceCharge.HasValue)
            {
                serviceVisit.TotalAmount = (serviceVisit.PartsCost ?? 0) + (serviceVisit.ServiceCharge ?? 0);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original visit to check if status changed to Completed
                    var originalVisit = await _context.ServiceVisits.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == serviceVisit.Id);
                    
                    var statusChangedToCompleted = originalVisit != null && 
                                                   originalVisit.Status != "Completed" && 
                                                   serviceVisit.Status == "Completed";

                    serviceVisit.ModifiedDate = DateTime.UtcNow;
                    _context.Update(serviceVisit);
                    await _context.SaveChangesAsync();

                    // If status changed to Completed and NextVisitDate is set, create a new Service Visit
                    if (statusChangedToCompleted && serviceVisit.NextVisitDate.HasValue)
                    {
                        var nextVisit = new ServiceVisit
                        {
                            AMCId = serviceVisit.AMCId,
                            VisitDate = serviceVisit.NextVisitDate.Value,
                            VisitTime = serviceVisit.VisitTime, // Use same time or default
                            Status = "Scheduled",
                            ServiceEngineer = serviceVisit.ServiceEngineer, // Copy engineer info
                            EngineerContact = serviceVisit.EngineerContact,
                            CreatedDate = DateTime.UtcNow,
                            Remarks = $"Auto-created from completed visit #{serviceVisit.Id}"
                        };
                        
                        _context.Add(nextVisit);
                        await _context.SaveChangesAsync();
                        
                        TempData["SuccessMessage"] = $"Service visit completed successfully. A new service visit has been scheduled for {nextVisit.VisitDate.ToString("dd MMM yyyy")}.";
                    }
                    else if (statusChangedToCompleted)
                    {
                        TempData["SuccessMessage"] = "Service visit marked as completed.";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceVisitExists(serviceVisit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var amcs = await _context.AMCs
                .Include(a => a.Customer)
                .OrderBy(a => a.AMCNumber)
                .ToListAsync();
            
            ViewData["AMCId"] = new SelectList(amcs, "Id", "AMCNumber", serviceVisit.AMCId);
            ViewBag.AMCs = amcs; // For displaying customer name in view
            return View(serviceVisit);
        }

        // GET: ServiceVisit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceVisit = await _context.ServiceVisits
                .Include(s => s.AMC)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceVisit == null)
            {
                return NotFound();
            }

            return View(serviceVisit);
        }

        // POST: ServiceVisit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceVisit = await _context.ServiceVisits.FindAsync(id);
            if (serviceVisit != null)
            {
                _context.ServiceVisits.Remove(serviceVisit);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceVisitExists(int id)
        {
            return _context.ServiceVisits.Any(e => e.Id == id);
        }
    }
}

