using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateParentPro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FreeTrialDays",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfiles_ParentProfileId",
                table: "ChildProfiles",
                column: "ParentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChildProfiles_ParentProfileId",
                table: "ChildProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "FreeTrialDays",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
