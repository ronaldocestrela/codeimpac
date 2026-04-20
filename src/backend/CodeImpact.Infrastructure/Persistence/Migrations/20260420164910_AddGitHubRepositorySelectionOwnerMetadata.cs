using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeImpact.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubRepositorySelectionOwnerMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerLogin",
                table: "GitHubRepositorySelections",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                table: "GitHubRepositorySelections",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerLogin",
                table: "GitHubRepositorySelections");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "GitHubRepositorySelections");
        }
    }
}
