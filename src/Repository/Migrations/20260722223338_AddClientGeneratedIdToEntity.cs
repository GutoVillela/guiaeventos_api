using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddClientGeneratedIdToEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "users",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "posts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "Notifications",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "categories",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "banners",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "authors",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientGeneratedId",
                table: "advertisements",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "authors");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedId",
                table: "advertisements");
        }
    }
}
