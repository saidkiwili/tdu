using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace tae_app.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.CreateTable(
                name: "AdminSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SiteDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "boolean", nullable: false),
                    NidaServicesEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NidaIndividualFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NidaFamilyFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NidaProcessingTime = table.Column<int>(type: "integer", nullable: false),
                    NidaMaxApplications = table.Column<int>(type: "integer", nullable: false),
                    JobPortalEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EventsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RequireStrongPassword = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordExpiration = table.Column<bool>(type: "boolean", nullable: false),
                    SessionTimeout = table.Column<int>(type: "integer", nullable: false),
                    MaxLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    NotifyNewRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyNidaApplication = table.Column<bool>(type: "boolean", nullable: false),
                    AlertSystemErrors = table.Column<bool>(type: "boolean", nullable: false),
                    AlertSecurityThreats = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminSettings");

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });
        }
    }
}
