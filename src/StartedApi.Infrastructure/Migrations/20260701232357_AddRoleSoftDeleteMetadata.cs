using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSoftDeleteMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAtUtc",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeactivatedByUserId",
                table: "AspNetRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "AspNetRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_IsActive",
                table: "AspNetRoles",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_IsActive",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "DeactivatedAtUtc",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "DeactivatedByUserId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AspNetRoles");
        }
    }
}
