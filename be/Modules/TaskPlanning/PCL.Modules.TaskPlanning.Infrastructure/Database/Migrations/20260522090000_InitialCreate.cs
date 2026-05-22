using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.TaskPlanning.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "task_planning");

            migrationBuilder.CreateTable(
                name: "task",
                schema: "task_planning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_Category",
                schema: "task_planning",
                table: "task",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_task_CreatedAt",
                schema: "task_planning",
                table: "task",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_task_OwnerId",
                schema: "task_planning",
                table: "task",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_task_OwnerId_Status",
                schema: "task_planning",
                table: "task",
                columns: new[] { "OwnerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_task_Priority",
                schema: "task_planning",
                table: "task",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_task_Status",
                schema: "task_planning",
                table: "task",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task",
                schema: "task_planning");
        }
    }
}

