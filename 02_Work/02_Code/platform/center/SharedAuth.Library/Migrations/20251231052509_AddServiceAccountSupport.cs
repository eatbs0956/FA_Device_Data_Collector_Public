using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAccountSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_type",
                table: "user",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "user");
            
            // 将现有用户的 user_type 设置为 'user'
            migrationBuilder.Sql("UPDATE \"user\" SET user_type = 'user' WHERE user_type = '' OR user_type IS NULL");

            migrationBuilder.AddColumn<Guid>(
                name: "service_user_id",
                table: "edge_nodes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_edge_nodes_service_user_id",
                table: "edge_nodes",
                column: "service_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_edge_nodes_user_service_user_id",
                table: "edge_nodes",
                column: "service_user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_edge_nodes_user_service_user_id",
                table: "edge_nodes");

            migrationBuilder.DropIndex(
                name: "ix_edge_nodes_service_user_id",
                table: "edge_nodes");

            migrationBuilder.DropColumn(
                name: "user_type",
                table: "user");

            migrationBuilder.DropColumn(
                name: "service_user_id",
                table: "edge_nodes");
        }
    }
}
