using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCL.Modules.Evidence.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceStorageConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "storage_profile",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_profile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "evidence_storage_settings",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ActiveStorageProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidence_storage_settings", x => x.Id);
                    table.CheckConstraint("CK_evidence_storage_settings_Singleton", "\"Id\" = 1");
                    table.ForeignKey(
                        name: "FK_evidence_storage_settings_storage_profile_ActiveStorageProf~",
                        column: x => x.ActiveStorageProfileId,
                        principalSchema: "evidence",
                        principalTable: "storage_profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "local_storage_profile_configuration",
                schema: "evidence",
                columns: table => new
                {
                    StorageProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RootDirectory = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_storage_profile_configuration", x => x.StorageProfileId);
                    table.ForeignKey(
                        name: "FK_local_storage_profile_configuration_storage_profile_Storage~",
                        column: x => x.StorageProfileId,
                        principalSchema: "evidence",
                        principalTable: "storage_profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "s3_storage_profile_configuration",
                schema: "evidence",
                columns: table => new
                {
                    StorageProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketName = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    Region = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_s3_storage_profile_configuration", x => x.StorageProfileId);
                    table.ForeignKey(
                        name: "FK_s3_storage_profile_configuration_storage_profile_StoragePro~",
                        column: x => x.StorageProfileId,
                        principalSchema: "evidence",
                        principalTable: "storage_profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evidence_storage_settings_ActiveStorageProfileId",
                schema: "evidence",
                table: "evidence_storage_settings",
                column: "ActiveStorageProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_profile_Name",
                schema: "evidence",
                table: "storage_profile",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_storage_profile_ProviderType",
                schema: "evidence",
                table: "storage_profile",
                column: "ProviderType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evidence_storage_settings",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "local_storage_profile_configuration",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "s3_storage_profile_configuration",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "storage_profile",
                schema: "evidence");
        }
    }
}
