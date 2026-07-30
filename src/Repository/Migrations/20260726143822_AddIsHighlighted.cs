using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHighlighted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "posts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "advertisements",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_posts_IsHighlighted",
                table: "posts",
                column: "IsHighlighted");

            migrationBuilder.CreateIndex(
                name: "IX_advertisements_IsHighlighted",
                table: "advertisements",
                column: "IsHighlighted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_posts_IsHighlighted",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_advertisements_IsHighlighted",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "advertisements");
        }
    }
}
