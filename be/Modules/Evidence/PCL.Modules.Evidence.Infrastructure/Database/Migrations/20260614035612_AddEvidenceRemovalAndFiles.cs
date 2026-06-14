using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceRemovalAndFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RemovalReason",
                schema: "evidence",
                table: "evidence_item",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RemovedAt",
                schema: "evidence",
                table: "evidence_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RemovedBy",
                schema: "evidence",
                table: "evidence_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "evidence_file",
                schema: "evidence",
                columns: table => new
                {
                    EvidenceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChecksumAlgorithm = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ChecksumValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    VersionId = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UploadExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_file", x => x.EvidenceItemId);
                    table.CheckConstraint("CK_evidence_file_FileSizeBytes", "\"FileSizeBytes\" >= 0");
                    table.ForeignKey(
                        name: "FK_evidence_file_evidence_item_EvidenceItemId",
                        column: x => x.EvidenceItemId,
                        principalSchema: "evidence",
                        principalTable: "evidence_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_item_RemovedAt",
                schema: "evidence",
                table: "evidence_item",
                column: "RemovedAt");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_ObjectKey",
                schema: "evidence",
                table: "evidence_file",
                column: "ObjectKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_UploadStatus_UploadExpiresAt",
                schema: "evidence",
                table: "evidence_file",
                columns: new[] { "UploadStatus", "UploadExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_file",
                schema: "evidence");

            migrationBuilder.DropIndex(
                name: "IX_evidence_item_RemovedAt",
                schema: "evidence",
                table: "evidence_item");

            migrationBuilder.DropColumn(
                name: "RemovalReason",
                schema: "evidence",
                table: "evidence_item");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                schema: "evidence",
                table: "evidence_item");

            migrationBuilder.DropColumn(
                name: "RemovedBy",
                schema: "evidence",
                table: "evidence_item");
        }
    }
}
