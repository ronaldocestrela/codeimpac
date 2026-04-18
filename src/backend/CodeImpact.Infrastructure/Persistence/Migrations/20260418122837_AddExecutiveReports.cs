using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeImpact.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutiveReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepositoryId = table.Column<long>(type: "bigint", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeveloperScope = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RepositoriesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CommitCount = table.Column<int>(type: "int", nullable: false),
                    PullRequestOpenCount = table.Column<int>(type: "int", nullable: false),
                    PullRequestClosedCount = table.Column<int>(type: "int", nullable: false),
                    PullRequestMergedCount = table.Column<int>(type: "int", nullable: false),
                    PullRequestApprovedCount = table.Column<int>(type: "int", nullable: false),
                    AverageMergeLeadTimeHours = table.Column<double>(type: "float", nullable: true),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    HighlightsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    RisksJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    EvidenceJson = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId_GeneratedAt",
                table: "Reports",
                columns: new[] { "UserId", "GeneratedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
