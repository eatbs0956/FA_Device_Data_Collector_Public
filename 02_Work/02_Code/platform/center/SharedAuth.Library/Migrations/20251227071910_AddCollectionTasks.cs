using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collection_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    task_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    default_interval = table.Column<int>(type: "integer", nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "collection_task_devices",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_task_devices", x => new { x.task_id, x.device_id });
                    table.ForeignKey(
                        name: "fk_collection_task_devices_collection_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "collection_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_collection_task_devices_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_collection_task_devices_device_id",
                table: "collection_task_devices",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_tasks_tenant_id_code",
                table: "collection_tasks",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "\"deleted_flag\" = false AND \"code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_collection_tasks_tenant_id_name",
                table: "collection_tasks",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "\"deleted_flag\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_task_devices");

            migrationBuilder.DropTable(
                name: "collection_tasks");
        }
    }
}
