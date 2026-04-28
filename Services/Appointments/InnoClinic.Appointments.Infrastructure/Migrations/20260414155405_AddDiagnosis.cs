using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoClinic.Appointments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Results",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Results");
        }
    }
}
