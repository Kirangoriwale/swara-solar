using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SolarBilling.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AddressLine1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddressLine2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GSTIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    PAN = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CIN = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: true),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IFSCCode = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    BankBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UPIId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BillingAddressLine1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BillingAddressLine2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BillingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BillingState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BillingPinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ShippingAddressLine1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShippingAddressLine2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShippingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShippingPinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GSTIN = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    PAN = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HSNCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MRP = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AMCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AMCNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AMCAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumberOfVisits = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsConditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ServiceAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AMCs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AMCs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    PONumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PODate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsConditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuotationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TermsConditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AMCId = table.Column<int>(type: "integer", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VisitTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ServiceEngineer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EngineerContact = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WorkDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PartsReplaced = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PartsCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ServiceCharge = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerFeedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NextVisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SystemCapacity = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SystemType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PanelCracksDustShadingStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PanelCracksDustShadingObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MountingStructureRailsTightStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MountingStructureRailsTightObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProperTiltAngleStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ProperTiltAngleObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EarthingConnectionStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EarthingConnectionObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InverterOnOffStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InverterOnOffObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AcDcConnectionsStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcDcConnectionsObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OverheatingCheckStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OverheatingCheckObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GenerationDataReading = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    BatteryVoltage = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    TerminalConnectionsStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TerminalConnectionsObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WaterLevelStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    WaterLevelObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BatteryTemperature = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    DcWiringStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DcWiringObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AcWiringStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AcWiringObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    McbSpdIsolatorStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    McbSpdIsolatorObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OverloadLooseConnectionStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OverloadLooseConnectionObservation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TodaysGeneration = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    AverageDailyGeneration = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Shading = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SystemOperatingCondition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RequiredRepairsReplacement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceVisits_AMCs_AMCId",
                        column: x => x.AMCId,
                        principalTable: "AMCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MRP = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuotationId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MRP = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationItems_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AMCs_AMCNumber",
                table: "AMCs",
                column: "AMCNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AMCs_CustomerId",
                table: "AMCs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AMCs_EndDate",
                table: "AMCs",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AMCs_StartDate",
                table: "AMCs",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IsDefault",
                table: "Companies",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProductId",
                table: "InvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceDate",
                table: "Invoices",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_ProductId",
                table: "QuotationItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_QuotationId",
                table: "QuotationItems",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CustomerId",
                table: "Quotations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationDate",
                table: "Quotations",
                column: "QuotationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationNumber",
                table: "Quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVisits_AMCId",
                table: "ServiceVisits",
                column: "AMCId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVisits_VisitDate",
                table: "ServiceVisits",
                column: "VisitDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "QuotationItems");

            migrationBuilder.DropTable(
                name: "ServiceVisits");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "AMCs");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
