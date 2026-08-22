using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHighlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HighlightColor",
                table: "categories",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HighlightLink",
                table: "categories",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighlightOrder",
                table: "categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "categories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_categories_IsHighlighted",
                table: "categories",
                column: "IsHighlighted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_IsHighlighted",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "HighlightColor",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "HighlightLink",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "HighlightOrder",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "categories");
        }
    }
}
