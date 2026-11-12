using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassoSeguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusTimelineIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Occurrences_Status_CreatedAt",
                table: "Occurrences",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Occurrences_Status_CreatedAt",
                table: "Occurrences");
        }
    }
}
