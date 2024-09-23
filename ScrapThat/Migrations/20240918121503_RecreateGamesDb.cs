using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScrapThat.Migrations
{
    /// <inheritdoc />
    public partial class RecreateGamesDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Products");
        }
    }
}
