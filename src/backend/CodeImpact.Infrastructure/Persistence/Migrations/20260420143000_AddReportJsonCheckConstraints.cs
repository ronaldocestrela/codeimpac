using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeImpact.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportJsonCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_RepositoriesJson_IsJson",
                table: "Reports",
                sql: "ISJSON([RepositoriesJson]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_HighlightsJson_IsJson",
                table: "Reports",
                sql: "ISJSON([HighlightsJson]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_RisksJson_IsJson",
                table: "Reports",
                sql: "ISJSON([RisksJson]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reports_EvidenceJson_IsJson",
                table: "Reports",
                sql: "ISJSON([EvidenceJson]) = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_RepositoriesJson_IsJson",
                table: "Reports");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_HighlightsJson_IsJson",
                table: "Reports");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_RisksJson_IsJson",
                table: "Reports");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reports_EvidenceJson_IsJson",
                table: "Reports");
        }
    }
}
