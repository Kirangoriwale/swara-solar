using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Models;

namespace SolarBilling.Controllers
{
    public class AMCController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AMCController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AMC
        public async Task<IActionResult> Index()
        {
            return View(await _context.AMCs
                .Include(a => a.Customer)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync());
        }

        // GET: AMC/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amc = await _context.AMCs
                .Include(a => a.Customer)
                .Include(a => a.ServiceVisits)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (amc == null)
            {
                return NotFound();
            }

            return View(amc);
        }

        // GET: AMC/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            
            var amc = new AMC
            {
                AMCNumber = GenerateAMCNumber(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                Status = "Active"
            };
            
            return View(amc);
        }

        // POST: AMC/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AMCNumber,CustomerId,StartDate,EndDate,AMCAmount,Frequency,NumberOfVisits,Status,Description,TermsConditions,ContactPerson,ContactPhone,ContactEmail,ServiceAddress")] AMC amc)
        {
            // Clear CustomerId-related validation errors
            var customerIdKeys = ModelState.Keys.Where(k => k.Contains("CustomerId") || k.Contains("Customer")).ToList();
            foreach (var key in customerIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate CustomerId manually
            if (amc.CustomerId <= 0)
            {
                ModelState.AddModelError("CustomerId", "Please select a customer.");
            }

            // Validate AMC Number uniqueness
            if (await _context.AMCs.AnyAsync(a => a.AMCNumber == amc.AMCNumber))
            {
                ModelState.AddModelError("AMCNumber", "AMC Number already exists. Please use a different number.");
            }

            if (ModelState.IsValid)
            {
                amc.CreatedDate = DateTime.UtcNow;
                _context.Add(amc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", amc.CustomerId);
            return View(amc);
        }

        // GET: AMC/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amc = await _context.AMCs.FindAsync(id);
            if (amc == null)
            {
                return NotFound();
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", amc.CustomerId);
            return View(amc);
        }

        // POST: AMC/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AMCNumber,CustomerId,StartDate,EndDate,AMCAmount,Frequency,NumberOfVisits,Status,Description,TermsConditions,ContactPerson,ContactPhone,ContactEmail,ServiceAddress,CreatedDate")] AMC amc)
        {
            if (id != amc.Id)
            {
                return NotFound();
            }

            // Clear CustomerId-related validation errors
            var customerIdKeys = ModelState.Keys.Where(k => k.Contains("CustomerId") || k.Contains("Customer")).ToList();
            foreach (var key in customerIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate CustomerId manually
            if (amc.CustomerId <= 0)
            {
                ModelState.AddModelError("CustomerId", "Please select a customer.");
            }

            // Validate AMC Number uniqueness (excluding current record)
            if (await _context.AMCs.AnyAsync(a => a.AMCNumber == amc.AMCNumber && a.Id != amc.Id))
            {
                ModelState.AddModelError("AMCNumber", "AMC Number already exists. Please use a different number.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    amc.ModifiedDate = DateTime.UtcNow;
                    _context.Update(amc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AMCExists(amc.Id))
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

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", amc.CustomerId);
            return View(amc);
        }

        // GET: AMC/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amc = await _context.AMCs
                .Include(a => a.Customer)
                .Include(a => a.ServiceVisits)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (amc == null)
            {
                return NotFound();
            }

            return View(amc);
        }

        // POST: AMC/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var amc = await _context.AMCs
                .Include(a => a.ServiceVisits)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (amc == null)
            {
                return NotFound();
            }

            // Check if there are associated service visits
            if (amc.ServiceVisits != null && amc.ServiceVisits.Any())
            {
                // Delete all associated service visits first
                _context.ServiceVisits.RemoveRange(amc.ServiceVisits);
            }

            try
            {
                _context.AMCs.Remove(amc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // If deletion fails due to foreign key constraint, show error
                TempData["ErrorMessage"] = "Cannot delete this AMC because it has associated service visits that cannot be deleted. Please delete the service visits first.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool AMCExists(int id)
        {
            return _context.AMCs.Any(e => e.Id == id);
        }

        private string GenerateAMCNumber()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("00");
            var lastAMC = _context.AMCs
                .Where(a => a.AMCNumber.StartsWith($"AMC-{year}{month}-"))
                .OrderByDescending(a => a.AMCNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastAMC != null)
            {
                var parts = lastAMC.AMCNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"AMC-{year}{month}-{nextNumber:D4}";
        }
    }
}

