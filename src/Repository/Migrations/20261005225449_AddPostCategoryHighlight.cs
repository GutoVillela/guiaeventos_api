using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPostCategoryHighlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HighlightIcon",
                table: "post_categories",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighlightOrder",
                table: "post_categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "post_categories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_post_categories_IsHighlighted",
                table: "post_categories",
                column: "IsHighlighted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_post_categories_IsHighlighted",
                table: "post_categories");

            migrationBuilder.DropColumn(
                name: "HighlightIcon",
                table: "post_categories");

            migrationBuilder.DropColumn(
                name: "HighlightOrder",
                table: "post_categories");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "post_categories");
        }
    }
}
