using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceGroupSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "device_groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "device_groups");
        }
    }
}
