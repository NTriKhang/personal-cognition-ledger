using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evidence");

            migrationBuilder.CreateTable(
                name: "evidence_item",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_consumers",
                schema: "evidence",
                columns: table => new
                {
                    OutboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message_consumers", x => new { x.OutboxMessageId, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_item_AddedAt",
                schema: "evidence",
                table: "evidence_item",
                column: "AddedAt");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_item_OwnerId",
                schema: "evidence",
                table: "evidence_item",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_item_SessionId",
                schema: "evidence",
                table: "evidence_item",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_evidence_item_SessionId_OwnerId",
                schema: "evidence",
                table: "evidence_item",
                columns: new[] { "SessionId", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_item",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "evidence");
        }
    }
}
