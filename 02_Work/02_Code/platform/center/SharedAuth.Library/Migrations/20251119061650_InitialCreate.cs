using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    resource_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    request_body = table.Column<string>(type: "text", nullable: true),
                    response_status = table.Column<int>(type: "integer", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "edge_nodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    node_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    node_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    platform = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    port = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    platform_config = table.Column<string>(type: "jsonb", nullable: false),
                    resource_limits = table.Column<string>(type: "jsonb", nullable: false),
                    os_info = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    hardware_info = table.Column<string>(type: "jsonb", nullable: true),
                    install_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    last_heartbeat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_edge_nodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    menu_type = table.Column<int>(type: "integer", nullable: false),
                    menu_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    route_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    route_path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    component = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    i18n_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    hide_in_menu = table.Column<bool>(type: "boolean", nullable: false),
                    active_menu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    multi_tab = table.Column<bool>(type: "boolean", nullable: false),
                    fixed_index_in_tab = table.Column<int>(type: "integer", nullable: true),
                    query = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    buttons = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_token_jti = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_session", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nick_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    password_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    device_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    device_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    protocol_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    edge_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_config = table.Column<string>(type: "jsonb", nullable: false),
                    protocol_config = table.Column<string>(type: "jsonb", nullable: false),
                    connection_status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    last_connect_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    tags_config = table.Column<string>(type: "jsonb", nullable: false),
                    vendor = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    firmware_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    table.PrimaryKey("pk_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_devices_edge_nodes_edge_node_id",
                        column: x => x.edge_node_id,
                        principalTable: "edge_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_button",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    button_code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_button", x => new { x.role_id, x.button_code });
                    table.ForeignKey(
                        name: "fk_role_button_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_menu",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_menu", x => new { x.role_id, x.menu_id });
                    table.ForeignKey(
                        name: "fk_role_menu_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_menu_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tag_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tag_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    data_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    collection_interval = table.Column<int>(type: "integer", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    min_value = table.Column<decimal>(type: "numeric", nullable: true),
                    max_value = table.Column<decimal>(type: "numeric", nullable: true),
                    scaling_factor = table.Column<decimal>(type: "numeric", nullable: false),
                    offset = table.Column<decimal>(type: "numeric", nullable: false),
                    access_mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    deadband = table.Column<decimal>(type: "numeric", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_flag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "t1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag_definitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_tag_definitions_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_action_created_at",
                table: "audit_log",
                columns: new[] { "action", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_tenant_id_created_at",
                table: "audit_log",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_id_created_at",
                table: "audit_log",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_devices_device_id",
                table: "devices",
                column: "device_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devices_edge_node_id_connection_status",
                table: "devices",
                columns: new[] { "edge_node_id", "connection_status" });

            migrationBuilder.CreateIndex(
                name: "ix_devices_tenant_id",
                table: "devices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_edge_nodes_node_id",
                table: "edge_nodes",
                column: "node_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_edge_nodes_platform_status",
                table: "edge_nodes",
                columns: new[] { "platform", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_edge_nodes_tenant_id",
                table: "edge_nodes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_parent_id",
                table: "menu",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_tenant_id_route_name",
                table: "menu",
                columns: new[] { "tenant_id", "route_name" },
                unique: true,
                filter: "\"deleted_flag\" = false");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id_token",
                table: "refresh_token",
                columns: new[] { "user_id", "token" });

            migrationBuilder.CreateIndex(
                name: "ix_role_tenant_id_code",
                table: "role",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "\"deleted_flag\" = false");

            migrationBuilder.CreateIndex(
                name: "ix_role_button_role_id",
                table: "role_button",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_menu_menu_id",
                table: "role_menu",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix_session_access_token_jti",
                table: "session",
                column: "access_token_jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_refresh_token_hash",
                table: "session",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_session_user_id_expires_at",
                table: "session",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tag_definitions_device_id_enabled",
                table: "tag_definitions",
                columns: new[] { "device_id", "enabled" });

            migrationBuilder.CreateIndex(
                name: "ix_tag_definitions_device_id_tag_id",
                table: "tag_definitions",
                columns: new[] { "device_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tag_definitions_tenant_id",
                table: "tag_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tenant_id_user_name",
                table: "user",
                columns: new[] { "tenant_id", "user_name" },
                unique: true,
                filter: "\"deleted_flag\" = false");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                table: "user_role",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "role_button");

            migrationBuilder.DropTable(
                name: "role_menu");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "tag_definitions");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "edge_nodes");
        }
    }
}
