using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceFileUploadLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_evidence_file_FileSizeBytes",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusChangedAt",
                schema: "evidence",
                table: "evidence_file",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadAttemptId",
                schema: "evidence",
                table: "evidence_file",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE evidence.evidence_file
                SET "UploadAttemptId" = "EvidenceItemId",
                    "StatusChangedAt" = "CreatedAt";
                """);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StatusChangedAt",
                schema: "evidence",
                table: "evidence_file",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UploadAttemptId",
                schema: "evidence",
                table: "evidence_file",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidence_file_UploadAttemptId",
                schema: "evidence",
                table: "evidence_file",
                column: "UploadAttemptId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_evidence_file_FileSizeBytes",
                schema: "evidence",
                table: "evidence_file",
                sql: "\"FileSizeBytes\" > 0 AND \"FileSizeBytes\" <= 26214400");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_evidence_file_UploadAttemptId",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropCheckConstraint(
                name: "CK_evidence_file_FileSizeBytes",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.DropColumn(
                name: "UploadAttemptId",
                schema: "evidence",
                table: "evidence_file");

            migrationBuilder.AddCheckConstraint(
                name: "CK_evidence_file_FileSizeBytes",
                schema: "evidence",
                table: "evidence_file",
                sql: "\"FileSizeBytes\" >= 0");
        }
    }
}
