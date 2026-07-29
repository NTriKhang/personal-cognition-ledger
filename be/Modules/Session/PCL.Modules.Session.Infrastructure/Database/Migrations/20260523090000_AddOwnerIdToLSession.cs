using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Session.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToLSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                schema: "session",
                table: "lsession",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_lsession_OwnerId",
                schema: "session",
                table: "lsession",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_lsession_OwnerId_Status",
                schema: "session",
                table: "lsession",
                columns: new[] { "OwnerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lsession_OwnerId",
                schema: "session",
                table: "lsession");

            migrationBuilder.DropIndex(
                name: "IX_lsession_OwnerId_Status",
                schema: "session",
                table: "lsession");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "session",
                table: "lsession");
        }
    }
}

