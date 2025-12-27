using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceExtensionsAndGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "enabled",
                table: "devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "group_id",
                table: "devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "devices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "device_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_groups_device_groups_parent_id",
                        column: x => x.parent_id,
                        principalTable: "device_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devices_enabled",
                table: "devices",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "ix_devices_group_id",
                table: "devices",
                column: "group_id",
                filter: "\"group_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_device_groups_parent_id",
                table: "device_groups",
                column: "parent_id",
                filter: "\"parent_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_device_groups_tenant_id",
                table: "device_groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_groups_tenant_id_parent_id_name",
                table: "device_groups",
                columns: new[] { "tenant_id", "parent_id", "name" },
                unique: true,
                filter: "\"deleted_flag\" = false");

            migrationBuilder.AddForeignKey(
                name: "fk_devices_device_groups_group_id",
                table: "devices",
                column: "group_id",
                principalTable: "device_groups",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_devices_device_groups_group_id",
                table: "devices");

            migrationBuilder.DropTable(
                name: "device_groups");

            migrationBuilder.DropIndex(
                name: "ix_devices_enabled",
                table: "devices");

            migrationBuilder.DropIndex(
                name: "ix_devices_group_id",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "enabled",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "location",
                table: "devices");
        }
    }
}
