using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarImagensResponsivasBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileImage_AltText",
                table: "banners",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileImage_Url",
                table: "banners",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TabletImage_AltText",
                table: "banners",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TabletImage_Url",
                table: "banners",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileImage_AltText",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "MobileImage_Url",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "TabletImage_AltText",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "TabletImage_Url",
                table: "banners");
        }
    }
}
