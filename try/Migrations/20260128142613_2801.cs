using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarkShop.Migrations
{
    /// <inheritdoc />
    public partial class _2801 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Vector",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vector",
                table: "Products");
        }
    }
}
