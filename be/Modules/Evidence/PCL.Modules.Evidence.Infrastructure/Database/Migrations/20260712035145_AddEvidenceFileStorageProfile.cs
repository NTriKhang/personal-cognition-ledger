using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceFileStorageProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StorageProfileId",
                schema: "evidence",
                table: "evidence_file",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_StorageProfileId",
                schema: "evidence",
                table: "evidence_file",
                column: "StorageProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_evidence_file_storage_profile_StorageProfileId",
                schema: "evidence",
                table: "evidence_file",
                column: "StorageProfileId",
                principalSchema: "evidence",
                principalTable: "storage_profile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_evidence_file_storage_profile_StorageProfileId",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropIndex(
                name: "IX_evidence_file_StorageProfileId",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "StorageProfileId",
                schema: "evidence",
                table: "evidence_file");
        }
    }
}
