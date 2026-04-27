using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SolarBilling.Data;
using SolarBilling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace SolarBilling.Services
{
    public class PdfService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PdfService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private string NumberToWords(decimal number)
        {
            long rupees = (long)Math.Floor(number);
            long paise = (long)Math.Round((number - rupees) * 100);

            if (rupees == 0 && paise == 0)
                return "Zero Rupees";

            string rupeesWords = ConvertToWords(rupees);
            string result = rupeesWords + (rupees == 1 ? " Rupee" : " Rupees");

            if (paise > 0)
            {
                string paiseWords = ConvertToWords(paise);
                result += " and " + paiseWords + (paise == 1 ? " Paise" : " Paise");
            }

            return result;
        }

        private string ConvertToWords(long number)
        {
            if (number == 0)
                return "Zero";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                return ones[number];

            if (number < 100)
                return tens[number / 10] + (number % 10 != 0 ? " " + ones[number % 10] : "");

            if (number < 1000)
                return ones[number / 100] + " Hundred" + (number % 100 != 0 ? " " + ConvertToWords(number % 100) : "");

            if (number < 100000)
                return ConvertToWords(number / 1000) + " Thousand" + (number % 1000 != 0 ? " " + ConvertToWords(number % 1000) : "");

            if (number < 10000000)
                return ConvertToWords(number / 100000) + " Lakh" + (number % 100000 != 0 ? " " + ConvertToWords(number % 100000) : "");

            return ConvertToWords(number / 10000000) + " Crore" + (number % 10000000 != 0 ? " " + ConvertToWords(number % 10000000) : "");
        }

        private void ComposeCompanyHeader(QuestPDF.Fluent.ColumnDescriptor column, Company? company)
        {
            if (company == null)
            {
                column.Item().Text("Solar Panel Solutions").FontSize(12).Bold();
                column.Item().Text("Please configure company details").FontSize(8);
                return;
            }

            column.Spacing(3);
            
            // Company Name
            column.Item().Text(company.Name).FontSize(11).Bold();
            
            // Address
            if (!string.IsNullOrEmpty(company.AddressLine1))
                column.Item().Text(company.AddressLine1).FontSize(8);
            if (!string.IsNullOrEmpty(company.AddressLine2))
                column.Item().Text(company.AddressLine2).FontSize(8);
            
            var cityState = "";
            if (!string.IsNullOrEmpty(company.City))
                cityState = company.City;
            if (!string.IsNullOrEmpty(company.State))
                cityState += cityState != "" ? $", {company.State}" : company.State;
            if (!string.IsNullOrEmpty(company.PinCode))
                cityState += cityState != "" ? $", {company.PinCode}" : company.PinCode;
            if (cityState != "")
                column.Item().Text(cityState).FontSize(8);
            
            // Contact Info
            if (!string.IsNullOrEmpty(company.GSTIN))
                column.Item().Text($"GSTIN: {company.GSTIN}").FontSize(8);
            if (!string.IsNullOrEmpty(company.Mobile))
                column.Item().Text($"Mobile: {company.Mobile}").FontSize(8);
            if (!string.IsNullOrEmpty(company.Email))
                column.Item().Text($"Email: {company.Email}").FontSize(8);
            if (!string.IsNullOrEmpty(company.PAN))
                column.Item().Text($"PAN: {company.PAN}").FontSize(8);
            if (!string.IsNullOrEmpty(company.Website))
                column.Item().Text($"Website: {company.Website}").FontSize(8);
        }

        private void ComposeCustomerAddress(QuestPDF.Fluent.ColumnDescriptor column, Customer customer, string title)
        {
            column.Spacing(2);
            column.Item().Text(title).FontSize(8).Bold();
            column.Item().Text(customer.Name).FontSize(8);
            
            var addressLine1 = title == "BILL TO:" ? customer.BillingAddressLine1 : customer.ShippingAddressLine1;
            var addressLine2 = title == "BILL TO:" ? customer.BillingAddressLine2 : customer.ShippingAddressLine2;
            var city = title == "BILL TO:" ? customer.BillingCity : customer.ShippingCity;
            var state = title == "BILL TO:" ? customer.BillingState : customer.ShippingState;
            var pinCode = title == "BILL TO:" ? customer.BillingPinCode : customer.ShippingPinCode;
            
            if (!string.IsNullOrEmpty(addressLine1))
                column.Item().Text(addressLine1).FontSize(8);
            if (!string.IsNullOrEmpty(addressLine2))
                column.Item().Text(addressLine2).FontSize(8);
            
            var cityState = "";
            if (!string.IsNullOrEmpty(city))
                cityState = city;
            if (!string.IsNullOrEmpty(state))
                cityState += cityState != "" ? $", {state}" : state;
            if (!string.IsNullOrEmpty(pinCode))
                cityState += cityState != "" ? $", {pinCode}" : pinCode;
            if (cityState != "")
                column.Item().Text(cityState).FontSize(8);
            
            if (!string.IsNullOrEmpty(customer.GSTIN))
                column.Item().Text($"GSTIN: {customer.GSTIN}").FontSize(8);
            if (!string.IsNullOrEmpty(customer.PAN))
                column.Item().Text($"PAN: {customer.PAN}").FontSize(8);
            if (!string.IsNullOrEmpty(state))
                column.Item().Text($"Place of Supply: {state}").FontSize(8);
        }

        public byte[] GenerateInvoicePdf(int invoiceId)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefault(i => i.Id == invoiceId);

            if (invoice == null)
                throw new Exception("Invoice not found");

            var company = _context.Companies.FirstOrDefault(c => c.IsDefault && c.IsActive) 
                ?? _context.Companies.FirstOrDefault(c => c.IsActive);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.0f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    // Header
                    page.Header()
                        .Background(Colors.White)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.ConstantItem(120).Column(column => ComposeCompanyHeader(column, company));
                            
                            // Logo in center
                            row.RelativeItem().AlignCenter().Column(column =>
                            {
                                var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
                                if (File.Exists(logoPath))
                                {
                                    try
                                    {
                                        var logoBytes = File.ReadAllBytes(logoPath);
                                        column.Item()
                                            .AlignCenter()
                                            .Width(130)
                                            .Height(78)
                                            .Image(logoBytes);
                                    }
                                    catch
                                    {
                                        // If logo fails to load, continue without it
                                    }
                                }
                            });
                            
                            row.RelativeItem().AlignRight().Column(column =>
                            {
                                column.Item()
                                    .Background(Colors.Green.Darken2)
                                    .Padding(8)
                                    .Column(col =>
                                    {
                                        col.Item().Text("INVOICE").FontSize(18).Bold().FontColor(Colors.White);
                                        col.Item().PaddingTop(3);
                                        col.Item().Text($"Invoice No.: {invoice.InvoiceNumber}").FontSize(8).FontColor(Colors.White);
                                        col.Item().Text($"Invoice Date: {invoice.InvoiceDate:dd/MM/yyyy hh:mm tt}").FontSize(8).FontColor(Colors.White);
                                        if (!string.IsNullOrEmpty(invoice.PONumber))
                                        {
                                            col.Item().Text($"PO No.: {invoice.PONumber}").FontSize(8).FontColor(Colors.White);
                                            if (invoice.PODate.HasValue)
                                                col.Item().Text($"PO Date: {invoice.PODate.Value:dd/MM/yyyy}").FontSize(8).FontColor(Colors.White);
                                        }
                                    });
                            });
                        });

                    page.Content()
                        .PaddingVertical(0.3f, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(6);

                            // Bill To and Ship To
                            column.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .Background(Colors.Green.Lighten5)
                                    .Padding(6)
                                    .Column(col => ComposeCustomerAddress(col, invoice.Customer, "BILL TO:"));
                                row.RelativeItem()
                                    .Background(Colors.Green.Lighten5)
                                    .Padding(6)
                                    .Column(col => ComposeCustomerAddress(col, invoice.Customer, "SHIP TO:"));
                            });

                            // Items Table
                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // S.NO.
                                    columns.RelativeColumn(3);   // ITEMS
                                    columns.ConstantColumn(50);   // HSN
                                    columns.ConstantColumn(50);   // QTY.
                                    columns.ConstantColumn(60);   // MRP
                                    columns.ConstantColumn(60);   // RATE
                                    columns.ConstantColumn(50);   // TAX
                                    columns.ConstantColumn(70);   // AMOUNT
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("S.NO.").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("ITEMS").Bold().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("HSN").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("QTY.").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("MRP").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("RATE").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("TAX").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("AMOUNT").Bold().AlignRight().FontColor(Colors.White);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.FontSize(8))
                                            .PaddingVertical(5)
                                            .PaddingHorizontal(3)
                                            .Background(Colors.Green.Darken1)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Green.Darken3);
                                    }
                                });

                                int sno = 1;
                                bool alternateRow = false;
                                foreach (var item in invoice.InvoiceItems)
                                {
                                    table.Cell().Element(CellStyle).Text(sno.ToString()).AlignCenter();
                                    table.Cell().Element(CellStyle).Column(col =>
                                    {
                                        col.Item().Text(item.Product.Name).FontSize(8).Bold();
                                        if (!string.IsNullOrEmpty(item.Description))
                                        {
                                            col.Item().Text(item.Description).FontSize(8).FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                    table.Cell().Element(CellStyle).Text(item.Product.HSNCode ?? "-").AlignCenter();
                                    table.Cell().Element(CellStyle).Text($"{item.Quantity:0.00} {item.Product.Unit}").AlignCenter();
                                    table.Cell().Element(CellStyle).Text($"₹{(item.MRP ?? 0):0.00}").AlignRight();
                                    table.Cell().Element(CellStyle).Text($"₹{item.UnitPrice:0.00}").AlignRight();
                                    table.Cell().Element(CellStyle).Column(col =>
                                    {
                                        col.Item().Text($"₹{item.TaxAmount:0.00}").AlignRight();
                                        col.Item().Text($"({item.TaxRate:0.00}%)").FontSize(8).FontColor(Colors.Grey.Medium).AlignRight();
                                    });
                                    table.Cell().Element(CellStyle).Text($"₹{item.TotalAmount:0.00}").AlignRight();

                                    IContainer CellStyle(IContainer container)
                                    {
                                        var style = container.BorderBottom(0.3f)
                                            .BorderColor(Colors.Green.Lighten2)
                                            .PaddingVertical(2)
                                            .PaddingHorizontal(2)
                                            .DefaultTextStyle(x => x.FontSize(8));
                                        
                                        if (alternateRow)
                                        {
                                            style = style.Background(Colors.Green.Lighten5);
                                        }
                                        
                                        return style;
                                    }
                                    alternateRow = !alternateRow;
                                    sno++;
                                }

                                // Subtotal Row
                                decimal totalQty = invoice.InvoiceItems.Sum(i => i.Quantity);
                                decimal totalTaxAmount = invoice.InvoiceItems.Sum(i => i.TaxAmount);
                                decimal totalItemAmount = invoice.InvoiceItems.Sum(i => i.TotalAmount);

                                table.Cell().Element(SubtotalCellStyle); // Empty for S.NO.
                                table.Cell().Element(SubtotalCellStyle).Text("SUBTOTAL").Bold();
                                table.Cell().Element(SubtotalCellStyle); // Empty for HSN
                                table.Cell().Element(SubtotalCellStyle).Text($"{totalQty:0.00}").Bold().AlignCenter();
                                table.Cell().Element(SubtotalCellStyle); // Empty for MRP
                                table.Cell().Element(SubtotalCellStyle); // Empty for RATE
                                table.Cell().Element(SubtotalCellStyle).Text($"₹{totalTaxAmount:0.00}").Bold().AlignRight();
                                table.Cell().Element(SubtotalCellStyle).Text($"₹{totalItemAmount:0.00}").Bold().AlignRight();

                                static IContainer SubtotalCellStyle(IContainer container)
                                {
                                    return container.BorderTop(1.5f)
                                        .BorderBottom(1.5f)
                                        .BorderColor(Colors.Green.Darken2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(2)
                                        .Background(Colors.Green.Lighten4)
                                        .DefaultTextStyle(x => x.FontSize(8));
                                }
                            });

                            // Calculate tax breakdown
                            var taxGroups = invoice.InvoiceItems
                                .GroupBy(i => Math.Round(i.TaxRate, 0))
                                .Select(g => new { TaxRate = g.Key, Amount = g.Sum(i => i.TaxAmount) })
                                .OrderBy(g => g.TaxRate)
                                .ToList();

                            decimal taxableAmount = invoice.Subtotal;
                            decimal totalTax = invoice.TaxAmount;
                            decimal totalAmount = invoice.TotalAmount;
                            decimal roundOff = Math.Round(totalAmount) - totalAmount;

                            // Terms and Conditions and Tax Summary Section
                            column.Item().Row(row =>
                            {
                                // Left side - Terms and Conditions
                                row.RelativeItem().Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(invoice.TermsConditions))
                                    {
                                        col.Item()
                                            .Background(Colors.Green.Lighten5)
                                            .Padding(6)
                                            .Column(termsCol =>
                                            {
                                                termsCol.Item().Text("Terms and Conditions:").FontSize(8).Bold().FontColor(Colors.Green.Darken3);
                                                termsCol.Item().PaddingTop(2).Text(invoice.TermsConditions).FontSize(8).FontColor(Colors.Grey.Darken1);
                                            });
                                    }
                                });

                                // Right side - Tax Breakdown
                                row.RelativeItem().Column(col =>
                                {
                                    col.Spacing(5);
                                    
                                    // Tax Breakdown
                                    foreach (var taxGroup in taxGroups)
                                    {
                                        decimal cgst = taxGroup.Amount / 2;
                                        decimal sgst = taxGroup.Amount / 2;
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text($"CGST @{taxGroup.TaxRate / 2}%:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{cgst:0.00}").FontSize(8);
                                        });
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text($"SGST @{taxGroup.TaxRate / 2}%:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{sgst:0.00}").FontSize(8);
                                        });
                                    }
                                    
                                    col.Item().Row(r =>
                                    {
                                        r.ConstantItem(100);
                                        r.RelativeItem().Text("Taxable Amount:").FontSize(8);
                                        r.ConstantItem(80).AlignRight().Text($"₹{taxableAmount:0.00}").FontSize(8);
                                    });
                                    
                                    if (Math.Abs(roundOff) > 0.01m)
                                    {
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text("Round Off:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{roundOff:0.00}").FontSize(8);
                                        });
                                    }
                                    
                                    col.Item().Row(r =>
                                    {
                                        r.ConstantItem(100);
                                        r.RelativeItem().Text("Total Amount:").FontSize(8).Bold();
                                        r.ConstantItem(80).AlignRight().Text($"₹{Math.Round(totalAmount):0.00}").FontSize(8).Bold();
                                    });
                                    
                                    col.Item().PaddingTop(2).Text($"Total Amount (in words): {NumberToWords(Math.Round(totalAmount))}").FontSize(8).Italic();
                                });
                            });

                            // Bank Details and Signature
                            column.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    if (company != null)
                                    {
                                        col.Item()
                                            .Background(Colors.Green.Lighten5)
                                            .Padding(6)
                                            .Column(bankCol =>
                                            {
                                                bankCol.Item().Text("Bank Details:").FontSize(8).Bold().FontColor(Colors.Green.Darken3);
                                                if (!string.IsNullOrEmpty(company.BankName))
                                                    bankCol.Item().Text($"Name: {company.BankName}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.IFSCCode))
                                                    bankCol.Item().Text($"IFSC Code: {company.IFSCCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.AccountNumber))
                                                    bankCol.Item().Text($"Account No: {company.AccountNumber}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.BankBranch))
                                                    bankCol.Item().Text($"Bank: {company.BankName}, {company.BankBranch}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.UPIId))
                                                {
                                                    bankCol.Item().PaddingTop(3).Text($"UPI ID: {company.UPIId}").FontSize(8).FontColor(Colors.Green.Darken2).Bold();
                                                    
                                                    // UPI QR Code
                                                    var upiQrPath = Path.Combine(_environment.WebRootPath, "images", "UPIQR.png");
                                                    if (File.Exists(upiQrPath))
                                                    {
                                                        try
                                                        {
                                                            var upiQrBytes = File.ReadAllBytes(upiQrPath);
                                                            bankCol.Item()
                                                                .PaddingTop(5)
                                                                .AlignCenter()
                                                                .Width(80)
                                                                .Height(80)
                                                                .Image(upiQrBytes);
                                                        }
                                                        catch
                                                        {
                                                            // If QR code fails to load, continue without it
                                                        }
                                                    }
                                                }
                                            });
                                    }
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                col.Item().PaddingTop(20).Text("Authorised Signature").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                
                                // Signature Image
                                var signaturePath = Path.Combine(_environment.WebRootPath, "images", "Sign.png");
                                if (File.Exists(signaturePath))
                                {
                                    try
                                    {
                                        var signatureBytes = File.ReadAllBytes(signaturePath);
                                        col.Item().PaddingTop(5)
                                            .Width(80)
                                            .Height(40)
                                            .Image(signatureBytes);
                                    }
                                    catch
                                    {
                                        // If signature fails to load, continue without it
                                    }
                                }
                                
                                col.Item().PaddingTop(15);
                                        if (company != null)
                                        col.Item().Text($"for {company.Name}").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                });
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateQuotationPdf(int quotationId)
        {
            var quotation = _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.QuotationItems)
                    .ThenInclude(item => item.Product)
                .FirstOrDefault(q => q.Id == quotationId);

            if (quotation == null)
                throw new Exception("Quotation not found");

            var company = _context.Companies.FirstOrDefault(c => c.IsDefault && c.IsActive) 
                ?? _context.Companies.FirstOrDefault(c => c.IsActive);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.0f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    // Header
                    page.Header()
                        .Background(Colors.White)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.ConstantItem(120).Column(column => ComposeCompanyHeader(column, company));
                            
                            // Logo in center
                            row.RelativeItem().AlignCenter().Column(column =>
                            {
                                var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo.png");
                                if (File.Exists(logoPath))
                                {
                                    try
                                    {
                                        var logoBytes = File.ReadAllBytes(logoPath);
                                        column.Item()
                                            .AlignCenter()
                                            .Width(130)
                                            .Height(78)
                                            .Image(logoBytes);
                                    }
                                    catch
                                    {
                                        // If logo fails to load, continue without it
                                    }
                                }
                            });
                            
                            row.RelativeItem().AlignRight().Column(column =>
                            {
                                column.Item()
                                    .Background(Colors.Green.Darken2)
                                    .Padding(8)
                                    .Column(col =>
                                    {
                                        col.Item().Text("QUOTATION").FontSize(18).Bold().FontColor(Colors.White);
                                        col.Item().PaddingTop(3);
                                        col.Item().Text($"Quotation No.: {quotation.QuotationNumber}").FontSize(8).FontColor(Colors.White);
                                        col.Item().Text($"Quotation Date: {quotation.QuotationDate:dd/MM/yyyy}").FontSize(8).FontColor(Colors.White);
                                        col.Item().Text($"Expiry Date: {quotation.ValidUntil:dd/MM/yyyy}").FontSize(8).FontColor(Colors.White);
                                    });
                            });
                        });

                    page.Content()
                        .PaddingVertical(0.3f, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(6);

                            // Bill To and Ship To
                            column.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .Background(Colors.Green.Lighten5)
                                    .Padding(6)
                                    .Column(col => ComposeCustomerAddress(col, quotation.Customer, "BILL TO:"));
                                row.RelativeItem()
                                    .Background(Colors.Green.Lighten5)
                                    .Padding(6)
                                    .Column(col => ComposeCustomerAddress(col, quotation.Customer, "SHIP TO:"));
                            });

                            // Items Table
                            column.Item().PaddingTop(2).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // S.NO.
                                    columns.RelativeColumn(3);   // ITEMS
                                    columns.ConstantColumn(50);   // HSN
                                    columns.ConstantColumn(50);   // QTY.
                                    columns.ConstantColumn(60);   // MRP
                                    columns.ConstantColumn(60);   // RATE
                                    columns.ConstantColumn(50);   // TAX
                                    columns.ConstantColumn(70);   // AMOUNT
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("S.NO.").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("ITEMS").Bold().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("HSN").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("QTY.").Bold().AlignCenter().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("MRP").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("RATE").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("TAX").Bold().AlignRight().FontColor(Colors.White);
                                    header.Cell().Element(CellStyle).Text("AMOUNT").Bold().AlignRight().FontColor(Colors.White);

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.FontSize(8))
                                            .PaddingVertical(5)
                                            .PaddingHorizontal(3)
                                            .Background(Colors.Green.Darken1)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Green.Darken3);
                                    }
                                });

                                int sno = 1;
                                bool alternateRow = false;
                                foreach (var item in quotation.QuotationItems)
                                {
                                    table.Cell().Element(CellStyle).Text(sno.ToString()).AlignCenter();
                                    table.Cell().Element(CellStyle).Column(col =>
                                    {
                                        col.Item().Text(item.Product.Name).FontSize(8).Bold();
                                        if (!string.IsNullOrEmpty(item.Description))
                                        {
                                            col.Item().Text(item.Description).FontSize(8).FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                    table.Cell().Element(CellStyle).Text(item.Product.HSNCode ?? "-").AlignCenter();
                                    table.Cell().Element(CellStyle).Text($"{item.Quantity:0.00} {item.Product.Unit}").AlignCenter();
                                    table.Cell().Element(CellStyle).Text($"₹{(item.MRP ?? 0):0.00}").AlignRight();
                                    table.Cell().Element(CellStyle).Text($"₹{item.UnitPrice:0.00}").AlignRight();
                                    table.Cell().Element(CellStyle).Column(col =>
                                    {
                                        col.Item().Text($"₹{item.TaxAmount:0.00}").AlignRight();
                                        col.Item().Text($"({item.TaxRate:0.00}%)").FontSize(8).FontColor(Colors.Grey.Medium).AlignRight();
                                    });
                                    table.Cell().Element(CellStyle).Text($"₹{item.TotalAmount:0.00}").AlignRight();

                                    IContainer CellStyle(IContainer container)
                                    {
                                        var style = container.BorderBottom(0.3f)
                                            .BorderColor(Colors.Green.Lighten2)
                                            .PaddingVertical(2)
                                            .PaddingHorizontal(2)
                                            .DefaultTextStyle(x => x.FontSize(8));
                                        
                                        if (alternateRow)
                                        {
                                            style = style.Background(Colors.Green.Lighten5);
                                        }
                                        
                                        return style;
                                    }
                                    alternateRow = !alternateRow;
                                    sno++;
                                }

                                // Subtotal Row
                                decimal totalQty = quotation.QuotationItems.Sum(i => i.Quantity);
                                decimal totalTaxAmount = quotation.QuotationItems.Sum(i => i.TaxAmount);
                                decimal totalItemAmount = quotation.QuotationItems.Sum(i => i.TotalAmount);

                                table.Cell().Element(SubtotalCellStyle); // Empty for S.NO.
                                table.Cell().Element(SubtotalCellStyle).Text("SUBTOTAL").Bold();
                                table.Cell().Element(SubtotalCellStyle); // Empty for HSN
                                table.Cell().Element(SubtotalCellStyle).Text($"{totalQty:0.00}").Bold().AlignCenter();
                                table.Cell().Element(SubtotalCellStyle); // Empty for MRP
                                table.Cell().Element(SubtotalCellStyle); // Empty for RATE
                                table.Cell().Element(SubtotalCellStyle).Text($"₹{totalTaxAmount:0.00}").Bold().AlignRight();
                                table.Cell().Element(SubtotalCellStyle).Text($"₹{totalItemAmount:0.00}").Bold().AlignRight();

                                static IContainer SubtotalCellStyle(IContainer container)
                                {
                                    return container.BorderTop(1.5f)
                                        .BorderBottom(1.5f)
                                        .BorderColor(Colors.Green.Darken2)
                                        .PaddingVertical(5)
                                        .PaddingHorizontal(2)
                                        .Background(Colors.Green.Lighten4)
                                        .DefaultTextStyle(x => x.FontSize(8));
                                }
                            });

                            // Calculate tax breakdown
                            var taxGroups = quotation.QuotationItems
                                .GroupBy(i => Math.Round(i.TaxRate, 0))
                                .Select(g => new { TaxRate = g.Key, Amount = g.Sum(i => i.TaxAmount) })
                                .OrderBy(g => g.TaxRate)
                                .ToList();

                            decimal taxableAmount = quotation.Subtotal;
                            decimal totalTax = quotation.TaxAmount;
                            decimal totalAmount = quotation.TotalAmount;
                            decimal roundOff = Math.Round(totalAmount) - totalAmount;

                            // Terms and Conditions and Tax Summary Section
                            column.Item().Row(row =>
                            {
                                // Left side - Terms and Conditions
                                row.RelativeItem().Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(quotation.TermsConditions))
                                    {
                                        col.Item()
                                            .Background(Colors.Green.Lighten5)
                                            .Padding(6)
                                            .Column(termsCol =>
                                            {
                                                termsCol.Item().Text("Terms and Conditions:").FontSize(8).Bold().FontColor(Colors.Green.Darken3);
                                                termsCol.Item().PaddingTop(2).Text(quotation.TermsConditions).FontSize(8).FontColor(Colors.Grey.Darken1);
                                            });
                                    }
                                });

                                // Right side - Tax Breakdown
                                row.RelativeItem().Column(col =>
                                {
                                    col.Spacing(5);
                                    
                                    // Tax Breakdown
                                    foreach (var taxGroup in taxGroups)
                                    {
                                        decimal cgst = taxGroup.Amount / 2;
                                        decimal sgst = taxGroup.Amount / 2;
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text($"CGST @{taxGroup.TaxRate / 2}%:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{cgst:0.00}").FontSize(8);
                                        });
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text($"SGST @{taxGroup.TaxRate / 2}%:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{sgst:0.00}").FontSize(8);
                                        });
                                    }
                                    
                                    col.Item().Row(r =>
                                    {
                                        r.ConstantItem(100);
                                        r.RelativeItem().Text("Taxable Amount:").FontSize(8);
                                        r.ConstantItem(80).AlignRight().Text($"₹{taxableAmount:0.00}").FontSize(8);
                                    });
                                    
                                    if (Math.Abs(roundOff) > 0.01m)
                                    {
                                        col.Item().Row(r =>
                                        {
                                            r.ConstantItem(100);
                                            r.RelativeItem().Text("Round Off:").FontSize(8);
                                            r.ConstantItem(80).AlignRight().Text($"₹{roundOff:0.00}").FontSize(8);
                                        });
                                    }
                                    
                                    col.Item().Row(r =>
                                    {
                                        r.ConstantItem(100);
                                        r.RelativeItem().Text("Total Amount:").FontSize(8).Bold();
                                        r.ConstantItem(80).AlignRight().Text($"₹{Math.Round(totalAmount):0.00}").FontSize(8).Bold();
                                    });
                                    
                                    col.Item().PaddingTop(2).Text($"Total Amount (in words): {NumberToWords(Math.Round(totalAmount))}").FontSize(8).Italic();
                                });
                            });

                            // Bank Details and Signature
                            column.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    if (company != null)
                                    {
                                        col.Item()
                                            .Background(Colors.Green.Lighten5)
                                            .Padding(6)
                                            .Column(bankCol =>
                                            {
                                                bankCol.Item().Text("Bank Details:").FontSize(8).Bold().FontColor(Colors.Green.Darken3);
                                                if (!string.IsNullOrEmpty(company.BankName))
                                                    bankCol.Item().Text($"Name: {company.BankName}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.IFSCCode))
                                                    bankCol.Item().Text($"IFSC Code: {company.IFSCCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.AccountNumber))
                                                    bankCol.Item().Text($"Account No: {company.AccountNumber}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.BankBranch))
                                                    bankCol.Item().Text($"Bank: {company.BankName}, {company.BankBranch}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                if (!string.IsNullOrEmpty(company.UPIId))
                                                {
                                                    bankCol.Item().PaddingTop(3).Text($"UPI ID: {company.UPIId}").FontSize(8).FontColor(Colors.Green.Darken2).Bold();
                                                    
                                                    // UPI QR Code
                                                    var upiQrPath = Path.Combine(_environment.WebRootPath, "images", "UPIQR.png");
                                                    if (File.Exists(upiQrPath))
                                                    {
                                                        try
                                                        {
                                                            var upiQrBytes = File.ReadAllBytes(upiQrPath);
                                                            bankCol.Item()
                                                                .PaddingTop(5)
                                                                .AlignCenter()
                                                                .Width(80)
                                                                .Height(80)
                                                                .Image(upiQrBytes);
                                                        }
                                                        catch
                                                        {
                                                            // If QR code fails to load, continue without it
                                                        }
                                                    }
                                                }
                                            });
                                    }
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                col.Item().PaddingTop(20).Text("Authorised Signature").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                
                                // Signature Image
                                var signaturePath = Path.Combine(_environment.WebRootPath, "images", "Sign.png");
                                if (File.Exists(signaturePath))
                                {
                                    try
                                    {
                                        var signatureBytes = File.ReadAllBytes(signaturePath);
                                        col.Item().PaddingTop(5)
                                            .Width(80)
                                            .Height(40)
                                            .Image(signatureBytes);
                                    }
                                    catch
                                    {
                                        // If signature fails to load, continue without it
                                    }
                                }
                                
                                col.Item().PaddingTop(15);
                                        if (company != null)
                                        col.Item().Text($"for {company.Name}").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                });
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
