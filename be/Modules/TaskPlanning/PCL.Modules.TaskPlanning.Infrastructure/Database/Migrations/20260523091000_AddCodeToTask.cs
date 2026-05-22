using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.TaskPlanning.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "TaskSq",
                schema: "task_planning",
                incrementBy: 2);

            migrationBuilder.AddColumn<int>(
                name: "Code",
                schema: "task_planning",
                table: "task",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"task_planning\".\"TaskSq\"')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                schema: "task_planning",
                table: "task");

            migrationBuilder.DropSequence(
                name: "TaskSq",
                schema: "task_planning");
        }
    }
}

