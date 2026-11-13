using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassoSeguro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurrenceAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Occurrences",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Occurrences");
        }
    }
}
