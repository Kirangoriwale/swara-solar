using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarBilling.Data;
using SolarBilling.Models;
using SolarBilling.Services;
using System.Linq;

namespace SolarBilling.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public InvoiceController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Invoice
        public async Task<IActionResult> Index(string searchString, int? pageNumber, int pageSize = 10)
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                invoices = invoices.Where(i =>
                    i.InvoiceNumber.Contains(searchString) ||
                    (i.Customer != null && i.Customer.Name.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(i.PONumber) && i.PONumber.Contains(searchString)));
            }

            // Order by date descending
            invoices = invoices.OrderByDescending(i => i.InvoiceDate);

            // Pagination
            var totalRecords = await invoices.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            pageNumber = pageNumber ?? 1;
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

            var paginatedInvoices = await invoices
                .Skip((pageNumber.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber.Value;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = totalPages;

            return View(paginatedInvoices);
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoice/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                InvoiceNumber = GenerateInvoiceNumber()
            };
            
            return View(invoice);
        }

        // POST: Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceNumber,InvoiceDate,CustomerId,PONumber,PODate,Notes,TermsConditions")] Invoice invoice, string[] productIds, string[] quantities, string[] descriptions, string[] mrps, string[] unitPrices, string[] taxRates)
        {
            // Clear ALL CustomerId-related validation errors (including from model binding)
            var customerIdKeys = ModelState.Keys.Where(k => k.Contains("CustomerId") || k.Contains("Customer")).ToList();
            foreach (var key in customerIdKeys)
            {
                ModelState.Remove(key);
            }
            
            // Validate CustomerId manually
            if (invoice.CustomerId <= 0)
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
                    invoice.CreatedDate = DateTime.Now;
                    _context.Add(invoice);
                    await _context.SaveChangesAsync();

                    // Add invoice items
                    decimal subtotal = 0;
                    decimal taxAmount = 0;

                    foreach (var (productId, quantity, mrp, unitPrice, taxRate, description) in validItems)
                    {
                        var item = new InvoiceItem
                        {
                            InvoiceId = invoice.Id,
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

                    invoice.Subtotal = subtotal;
                    invoice.TaxAmount = taxAmount;
                    invoice.TotalAmount = subtotal + taxAmount;
                    invoice.ModifiedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving: {ex.Message}");
                }
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", invoice.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(invoice);
        }

        // GET: Invoice/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", invoice.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(invoice);
        }

        // POST: Invoice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InvoiceNumber,InvoiceDate,CustomerId,PONumber,PODate,Notes,TermsConditions,CreatedDate")] Invoice invoice, string[] productIds, string[] quantities, string[] descriptions, string[] itemIds, string[] mrps, string[] unitPrices, string[] taxRates)
        {
            if (id != invoice.Id)
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
            if (invoice.CustomerId <= 0)
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
                    var existingItems = await _context.InvoiceItems.Where(item => item.InvoiceId == invoice.Id).ToListAsync();
                    _context.InvoiceItems.RemoveRange(existingItems);

                    // Add updated items
                    decimal subtotal = 0;
                    decimal taxAmount = 0;

                    foreach (var (productId, quantity, mrp, unitPrice, taxRate, description) in validItems)
                    {
                        var item = new InvoiceItem
                        {
                            InvoiceId = invoice.Id,
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

                    invoice.Subtotal = subtotal;
                    invoice.TaxAmount = taxAmount;
                    invoice.TotalAmount = subtotal + taxAmount;
                    invoice.ModifiedDate = DateTime.Now;

                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.Id))
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

            ViewData["CustomerId"] = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", invoice.CustomerId);
            ViewData["ProductId"] = new SelectList(await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }

        // GET: Invoice/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pdfBytes = _pdfService.GenerateInvoicePdf(id.Value);
                var invoice = await _context.Invoices.FindAsync(id);
                var fileName = invoice != null ? $"{invoice.InvoiceNumber}.pdf" : $"Invoice_{id}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch
            {
                return NotFound();
            }
        }

        private string GenerateInvoiceNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("00");
            var lastInvoice = _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}{month}-"))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"INV-{year}{month}-{nextNumber:D4}";
        }
    }
}

