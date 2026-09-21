using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class editParentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityCode",
                table: "ParentProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityCode",
                table: "ParentProfiles");
        }
    }
}
