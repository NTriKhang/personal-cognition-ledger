using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReliableEvidenceUploadLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CleanupAttempts",
                schema: "evidence",
                table: "evidence_file",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CleanupNextAttemptAt",
                schema: "evidence",
                table: "evidence_file",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PhysicalDeletedAt",
                schema: "evidence",
                table: "evidence_file",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "evidence",
                table: "evidence_file",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "evidence_file_initialization",
                schema: "evidence",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EvidenceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_file_initialization", x => new { x.OwnerId, x.IdempotencyKey });
                    table.ForeignKey(
                        name: "FK_evidence_file_initialization_evidence_item_EvidenceItemId",
                        column: x => x.EvidenceItemId,
                        principalSchema: "evidence",
                        principalTable: "evidence_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evidence_file_upload_attempt",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PhysicalDeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CleanupAttempts = table.Column<int>(type: "integer", nullable: false),
                    CleanupNextAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_file_upload_attempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_evidence_file_upload_attempt_evidence_item_EvidenceItemId",
                        column: x => x.EvidenceItemId,
                        principalSchema: "evidence",
                        principalTable: "evidence_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evidence_file_upload_attempt_storage_profile_StorageProfile~",
                        column: x => x.StorageProfileId,
                        principalSchema: "evidence",
                        principalTable: "storage_profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_initialization_EvidenceItemId",
                schema: "evidence",
                table: "evidence_file_initialization",
                column: "EvidenceItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_upload_attempt_EvidenceItemId",
                schema: "evidence",
                table: "evidence_file_upload_attempt",
                column: "EvidenceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_upload_attempt_ObjectKey",
                schema: "evidence",
                table: "evidence_file_upload_attempt",
                column: "ObjectKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_upload_attempt_StatusChangedAt_PhysicalDelete~",
                schema: "evidence",
                table: "evidence_file_upload_attempt",
                columns: new[] { "StatusChangedAt", "PhysicalDeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_upload_attempt_StorageProfileId",
                schema: "evidence",
                table: "evidence_file_upload_attempt",
                column: "StorageProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_file_initialization",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "evidence_file_upload_attempt",
                schema: "evidence");

            migrationBuilder.DropColumn(
                name: "CleanupAttempts",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "CleanupNextAttemptAt",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "PhysicalDeletedAt",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "evidence",
                table: "evidence_file");
        }
    }
}
