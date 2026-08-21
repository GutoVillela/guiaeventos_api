using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class MakeSlugRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE advertisements SET Slug = CONCAT('anuncio-', CAST(Id AS CHAR)) WHERE Slug IS NULL OR Slug = ''");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "advertisements",
                type: "varchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(400)",
                oldMaxLength: 400,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "advertisements",
                type: "varchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(400)",
                oldMaxLength: 400);
        }
    }
}
