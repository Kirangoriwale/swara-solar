using Microsoft.EntityFrameworkCore;
using SolarBilling.Models;

namespace SolarBilling.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<AMC> AMCs { get; set; }
        public DbSet<ServiceVisit> ServiceVisits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for SQLite
            // SQLite doesn't have native decimal type, but EF Core handles it
            // We'll use decimal(18,2) for amounts and decimal(5,2) for percentages

            // Company configuration
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsDefault);
            });

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.HasIndex(e => e.Name);
            });

            // Invoice configuration
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => e.InvoiceDate);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Invoices)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InvoiceItem configuration
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.InvoiceItems)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.InvoiceItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Quotation configuration
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuotationNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.QuotationNumber).IsUnique();
                entity.HasIndex(e => e.QuotationDate);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Quotations)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QuotationItem configuration
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Quotation)
                    .WithMany(q => q.QuotationItems)
                    .HasForeignKey(e => e.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.QuotationItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AMC configuration
            modelBuilder.Entity<AMC>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AMCNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AMCAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.AMCNumber).IsUnique();
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.AMCs)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceVisit configuration
            modelBuilder.Entity<ServiceVisit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartsCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ServiceCharge).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.VisitDate);
                entity.HasOne(e => e.AMC)
                    .WithMany(a => a.ServiceVisits)
                    .HasForeignKey(e => e.AMCId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

