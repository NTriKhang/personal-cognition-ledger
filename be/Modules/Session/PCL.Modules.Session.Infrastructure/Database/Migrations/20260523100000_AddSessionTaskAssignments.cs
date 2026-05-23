using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Session.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTaskAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "session_task_assignments",
                schema: "session",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_task_assignments", x => new { x.SessionId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_session_task_assignments_lsession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "session",
                        principalTable: "lsession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_session_task_assignments_TaskId",
                schema: "session",
                table: "session_task_assignments",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_task_assignments",
                schema: "session");
        }
    }
}
