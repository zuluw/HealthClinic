using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoClinic.Services.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("8869b323-92ce-4113-a606-d9c157cc4ff4"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("ea05bd73-4ae5-4980-846d-0ab3290b867d"));

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Category", "IsActive", "Price", "ServiceName", "SpecializationId" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, true, 150.00m, "Consultation with cardiologist", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 3, true, 20.00m, "Blood Test", new Guid("11111111-1111-1111-1111-111111111111") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "Category", "IsActive", "Price", "ServiceName", "SpecializationId" },
                values: new object[,]
                {
                    { new Guid("8869b323-92ce-4113-a606-d9c157cc4ff4"), 1, true, 150.00m, "Consultation with cardiologist", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("ea05bd73-4ae5-4980-846d-0ab3290b867d"), 3, true, 20.00m, "Blood Test", new Guid("11111111-1111-1111-1111-111111111111") }
                });
        }
    }
}
