using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTagEnableRealtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "enable_realtime",
                table: "tag_definitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "enable_realtime",
                table: "tag_definitions");
        }
    }
}
