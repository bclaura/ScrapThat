using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScrapThat.Migrations
{
    /// <inheritdoc />
    public partial class DbRecreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPriceHistories_Products_ProductId",
                table: "ProductPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProductPriceHistories_ProductId",
                table: "ProductPriceHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductId",
                table: "ProductPriceHistories",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPriceHistories_Products_ProductId",
                table: "ProductPriceHistories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
