using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "advertisements",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "advertisements",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RejectedAt",
                table: "advertisements",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "advertisements",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "advertisements",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "advertisements");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "advertisements");
        }
    }
}
