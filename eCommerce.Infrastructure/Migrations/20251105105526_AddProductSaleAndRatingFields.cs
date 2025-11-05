using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSaleAndRatingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Products",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnSale",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsOnSale",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Products");
        }
    }
}
