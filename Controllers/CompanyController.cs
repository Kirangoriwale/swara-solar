using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Models;

namespace SolarBilling.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Company
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies.OrderBy(c => c.Name).ToListAsync());
        }

        // GET: Company/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Company/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TradeName,AddressLine1,AddressLine2,City,State,PinCode,Phone,Mobile,Email,Website,GSTIN,PAN,CIN,BankName,AccountNumber,IFSCCode,BankBranch,UPIId,IsActive,IsDefault")] Company company)
        {
            if (ModelState.IsValid)
            {
                // If this is set as default, unset other defaults
                if (company.IsDefault)
                {
                    var existingDefaults = await _context.Companies.Where(c => c.IsDefault).ToListAsync();
                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                    }
                }

                company.CreatedDate = DateTime.Now;
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Company/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Company/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TradeName,AddressLine1,AddressLine2,City,State,PinCode,Phone,Mobile,Email,Website,GSTIN,PAN,CIN,BankName,AccountNumber,IFSCCode,BankBranch,UPIId,IsActive,IsDefault,CreatedDate")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // If this is set as default, unset other defaults
                    if (company.IsDefault)
                    {
                        var existingDefaults = await _context.Companies.Where(c => c.IsDefault && c.Id != company.Id).ToListAsync();
                        foreach (var existing in existingDefaults)
                        {
                            existing.IsDefault = false;
                        }
                    }

                    company.ModifiedDate = DateTime.Now;
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Company/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}

