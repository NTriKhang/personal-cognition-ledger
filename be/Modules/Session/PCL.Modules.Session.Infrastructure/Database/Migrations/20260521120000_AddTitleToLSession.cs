using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Session.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToLSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "session",
                table: "lsession",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                schema: "session",
                table: "lsession");
        }
    }
}
