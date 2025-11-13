using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingMethodToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the column only if it doesn't already exist to make this migration idempotent
            migrationBuilder.Sql(@" 
IF COL_LENGTH('dbo.Orders', 'ShippingMethod') IS NULL
BEGIN
    ALTER TABLE [Orders] ADD [ShippingMethod] nvarchar(max) NOT NULL DEFAULT N'';
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                table: "Orders");
        }
    }
}
