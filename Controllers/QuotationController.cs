using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Models;
using SolarBilling.Services;
using System.Linq;
using System.Globalization;

namespace SolarBilling.Controllers
{
    public class QuotationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public QuotationController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Quotation
        public async Task<IActionResult> Index(string searchString, int? pageNumber, int pageSize = 10)
        {
            var quotations = _context.Quotations
                .Include(q => q.Customer)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                quotations = quotations.Where(q =>
                    q.QuotationNumber.Contains(searchString) ||
                    (q.Customer != null && q.Customer.Name.Contains(searchString)) ||
                    q.Status.Contains(searchString));
            }

            // Order by date descending
            quotations = quotations.OrderByDescending(q => q.QuotationDate);

            // Pagination
            var totalRecords = await quotations.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            pageNumber = pageNumber ?? 1;
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

            var paginatedQuotations = await quotations
                .Skip((pageNumber.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber.Value;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            return View(paginatedQuotations);
        }

        // GET: Quotation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotation = await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        // GET: Quotation/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            
            var quotation = new Quotation
            {
                QuotationDate = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                QuotationNumber = GenerateQuotationNumber()
            };
            
            return View(quotation);
        }

        // POST: Quotation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuotationNumber,QuotationDate,ValidUntil,CustomerId,Notes,TermsConditions,Status")] Quotation quotation, string[] productIds, string[] quantities, string[] descriptions, string[] mrps, string[] unitPrices, string[] taxRates)
        {
            // Clear ALL CustomerId-related validation errors (including from model binding)
            var customerIdKeys = ModelState.Keys.Where(k => k.Contains("CustomerId") || k.Contains("Customer")).ToList();
            foreach (var key in customerIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate CustomerId manually
            if (quotation.CustomerId <= 0)
            {
                ModelState.AddModelError("CustomerId", "Please select a customer.");
            }

            // Filter out empty product selections
            var validItems = new List<(int productId, decimal quantity, decimal? mrp, decimal unitPrice, decimal taxRate, string? description)>();
            if (productIds != null && quantities != null)
            {
                for (int i = 0; i < productIds.Length && i < quantities.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(productIds[i]) && 
                        int.TryParse(productIds[i], out int productId) && 
                        !string.IsNullOrWhiteSpace(quantities[i]) &&
                        decimal.TryParse(quantities[i], out decimal quantity) && 
                        quantity > 0)
                    {
                        // Get MRP, Unit Price, and Tax Rate from form, or use product defaults
                        var product = await _context.Products.FindAsync(productId);
                        decimal? mrp = null;
                        decimal unitPrice = product?.UnitPrice ?? 0;
                        decimal taxRate = product?.TaxRate ?? 0;

                        if (mrps != null && i < mrps.Length && !string.IsNullOrWhiteSpace(mrps[i]) && decimal.TryParse(mrps[i], out decimal mrpValue))
                        {
                            mrp = mrpValue;
                        }
                        else if (product?.MRP.HasValue == true)
                        {
                            mrp = product.MRP;
                        }

                        if (unitPrices != null && i < unitPrices.Length && !string.IsNullOrWhiteSpace(unitPrices[i]) && decimal.TryParse(unitPrices[i], out decimal price))
                        {
                            unitPrice = price;
                        }

                        if (taxRates != null && i < taxRates.Length && !string.IsNullOrWhiteSpace(taxRates[i]) && decimal.TryParse(taxRates[i], out decimal tax))
                        {
                            taxRate = tax;
                        }

                        validItems.Add((productId, quantity, mrp, unitPrice, taxRate, descriptions != null && i < descriptions.Length ? descriptions[i] : null));
                    }
                }
            }

            // Validate that we have at least one valid item
            if (validItems.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one item with a product and quantity.");
            }

            if (ModelState.IsValid && validItems.Count > 0)
            {
                try
                {
                    quotation.CreatedDate = DateTime.UtcNow;
                    _context.Add(quotation);
                    await _context.SaveChangesAsync();

                    // Add quotation items
                    decimal subtotal = 0;
                    decimal taxAmount = 0;

                    foreach (var (productId, quantity, mrp, unitPrice, taxRate, description) in validItems)
                    {
                        var item = new QuotationItem
                        {
                            QuotationId = quotation.Id,
                            ProductId = productId,
                            Quantity = quantity,
                            MRP = mrp,
                            UnitPrice = unitPrice,
                            TaxRate = taxRate,
                            Description = description
                        };
                        item.Amount = item.Quantity * item.UnitPrice;
                        item.TaxAmount = item.Amount * (item.TaxRate / 100);
                        item.TotalAmount = item.Amount + item.TaxAmount;

                        subtotal += item.Amount;
                        taxAmount += item.TaxAmount;

                        _context.Add(item);
                    }

                    quotation.Subtotal = subtotal;
                    quotation.TaxAmount = taxAmount;
                    quotation.TotalAmount = subtotal + taxAmount;
                    quotation.ModifiedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving: {ex.Message}");
                }
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", quotation.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(quotation);
        }

        // GET: Quotation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotation = await _context.Quotations
                .Include(q => q.QuotationItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quotation == null)
            {
                return NotFound();
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", quotation.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(quotation);
        }

        // POST: Quotation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuotationNumber,QuotationDate,ValidUntil,CustomerId,Notes,TermsConditions,Status,CreatedDate")] Quotation quotation, string[] productIds, string[] quantities, string[] descriptions, string[] itemIds, string[] mrps, string[] unitPrices, string[] taxRates)
        {
            if (id != quotation.Id)
            {
                return NotFound();
            }

            // Clear ALL CustomerId-related validation errors (including from model binding)
            var customerIdKeys = ModelState.Keys.Where(k => k.Contains("CustomerId") || k.Contains("Customer")).ToList();
            foreach (var key in customerIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate CustomerId manually
            if (quotation.CustomerId <= 0)
            {
                ModelState.AddModelError("CustomerId", "Please select a customer.");
            }

            // Filter out empty product selections
            var validItems = new List<(int productId, decimal quantity, decimal? mrp, decimal unitPrice, decimal taxRate, string? description)>();
            if (productIds != null && quantities != null)
            {
                for (int i = 0; i < productIds.Length && i < quantities.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(productIds[i]) && 
                        int.TryParse(productIds[i], out int productId) && 
                        !string.IsNullOrWhiteSpace(quantities[i]) &&
                        decimal.TryParse(quantities[i], out decimal quantity) && 
                        quantity > 0)
                    {
                        // Get MRP, Unit Price, and Tax Rate from form, or use product defaults
                        var product = await _context.Products.FindAsync(productId);
                        decimal? mrp = null;
                        decimal unitPrice = product?.UnitPrice ?? 0;
                        decimal taxRate = product?.TaxRate ?? 0;

                        if (mrps != null && i < mrps.Length && !string.IsNullOrWhiteSpace(mrps[i]) && decimal.TryParse(mrps[i], out decimal mrpValue))
                        {
                            mrp = mrpValue;
                        }
                        else if (product?.MRP.HasValue == true)
                        {
                            mrp = product.MRP;
                        }

                        if (unitPrices != null && i < unitPrices.Length && !string.IsNullOrWhiteSpace(unitPrices[i]) && decimal.TryParse(unitPrices[i], out decimal price))
                        {
                            unitPrice = price;
                        }

                        if (taxRates != null && i < taxRates.Length && !string.IsNullOrWhiteSpace(taxRates[i]) && decimal.TryParse(taxRates[i], out decimal tax))
                        {
                            taxRate = tax;
                        }

                        validItems.Add((productId, quantity, mrp, unitPrice, taxRate, descriptions != null && i < descriptions.Length ? descriptions[i] : null));
                    }
                }
            }

            // Validate that we have at least one valid item
            if (validItems.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one item with a product and quantity.");
            }

            if (ModelState.IsValid && validItems.Count > 0)
            {
                try
                {
                    // Remove existing items
                    var existingItems = await _context.QuotationItems.Where(item => item.QuotationId == quotation.Id).ToListAsync();
                    _context.QuotationItems.RemoveRange(existingItems);

                    // Add updated items
                    decimal subtotal = 0;
                    decimal taxAmount = 0;

                    foreach (var (productId, quantity, mrp, unitPrice, taxRate, description) in validItems)
                    {
                        var item = new QuotationItem
                        {
                            QuotationId = quotation.Id,
                            ProductId = productId,
                            Quantity = quantity,
                            MRP = mrp,
                            UnitPrice = unitPrice,
                            TaxRate = taxRate,
                            Description = description
                        };
                        item.Amount = item.Quantity * item.UnitPrice;
                        item.TaxAmount = item.Amount * (item.TaxRate / 100);
                        item.TotalAmount = item.Amount + item.TaxAmount;

                        subtotal += item.Amount;
                        taxAmount += item.TaxAmount;

                        _context.Add(item);
                    }

                    quotation.Subtotal = subtotal;
                    quotation.TaxAmount = taxAmount;
                    quotation.TotalAmount = subtotal + taxAmount;
                    quotation.ModifiedDate = DateTime.UtcNow;

                    _context.Update(quotation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuotationExists(quotation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving: {ex.Message}");
                }
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", quotation.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(quotation);
        }

        // GET: Quotation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quotation = await _context.Quotations
                .Include(q => q.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        // POST: Quotation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.QuotationItems)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quotation != null)
            {
                if (quotation.QuotationItems.Any())
                {
                    _context.QuotationItems.RemoveRange(quotation.QuotationItems);
                }
                _context.Quotations.Remove(quotation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool QuotationExists(int id)
        {
            return _context.Quotations.Any(e => e.Id == id);
        }

        // GET: Quotation/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pdfBytes = _pdfService.GenerateQuotationPdf(id.Value);
                var quotation = await _context.Quotations.FindAsync(id);
                var fileName = quotation != null ? $"{quotation.QuotationNumber}.pdf" : $"Quotation_{id}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch
            {
                return NotFound();
            }
        }

        private string GenerateQuotationNumber()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("00");
            var lastQuotation = _context.Quotations
                .Where(q => q.QuotationNumber.StartsWith($"QUO-{year}{month}-"))
                .OrderByDescending(q => q.QuotationNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastQuotation != null)
            {
                var parts = lastQuotation.QuotationNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"QUO-{year}{month}-{nextNumber:D4}";
        }
    }
}

