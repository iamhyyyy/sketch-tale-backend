using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addParentProfileSubTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_ParentProfiles_ParentProfileId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_ParentIdS",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_ParentProfiles_ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedStories_ParentProfiles_ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfiles_Users_ParentIdS",
                table: "ParentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ParentProfiles_ParentIdS",
                table: "ParentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedStories_ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_ChildProfiles_ParentIdS",
                table: "ChildProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Characters_ParentProfileId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "AllowedCategoryIdsJson",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "ParentIdS",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropColumn(
                name: "ParentIdS",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "TargetAgeGroup",
                table: "ParentProfiles",
                newName: "RemainingChild");

            migrationBuilder.RenameColumn(
                name: "RemainingTimes",
                table: "ParentProfiles",
                newName: "ExportStoryLimit");

            migrationBuilder.RenameColumn(
                name: "DailyTimeLimit",
                table: "ParentProfiles",
                newName: "ChildProfileLimit");

            migrationBuilder.RenameColumn(
                name: "DailyCharacterLimit",
                table: "ParentProfiles",
                newName: "CharacterLimit");

            migrationBuilder.AddColumn<bool>(
                name: "AccessFullStories",
                table: "ParentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RemainingExport",
                table: "ParentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ParentProfileSubs",
                columns: table => new
                {
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentProfileSubs", x => x.ParentProfileId);
                    table.ForeignKey(
                        name: "FK_ParentProfileSubs_ParentProfiles_ParentProfileId",
                        column: x => x.ParentProfileId,
                        principalTable: "ParentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentProfileSubs");

            migrationBuilder.DropColumn(
                name: "AccessFullStories",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "RemainingExport",
                table: "ParentProfiles");

            migrationBuilder.RenameColumn(
                name: "RemainingChild",
                table: "ParentProfiles",
                newName: "TargetAgeGroup");

            migrationBuilder.RenameColumn(
                name: "ExportStoryLimit",
                table: "ParentProfiles",
                newName: "RemainingTimes");

            migrationBuilder.RenameColumn(
                name: "ChildProfileLimit",
                table: "ParentProfiles",
                newName: "DailyTimeLimit");

            migrationBuilder.RenameColumn(
                name: "CharacterLimit",
                table: "ParentProfiles",
                newName: "DailyCharacterLimit");

            migrationBuilder.AddColumn<string>(
                name: "AllowedCategoryIdsJson",
                table: "ParentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChildProfileId",
                table: "ParentProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "ParentProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentIdS",
                table: "ParentProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentProfileId",
                table: "GeneratedStories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentProfileId",
                table: "Drawings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentIdS",
                table: "ChildProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentProfileId",
                table: "Characters",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentProfiles_ParentIdS",
                table: "ParentProfiles",
                column: "ParentIdS");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedStories_ParentProfileId",
                table: "GeneratedStories",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ParentProfileId",
                table: "Drawings",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfiles_ParentIdS",
                table: "ChildProfiles",
                column: "ParentIdS");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ParentProfileId",
                table: "Characters",
                column: "ParentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_ParentProfiles_ParentProfileId",
                table: "Characters",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_ParentIdS",
                table: "ChildProfiles",
                column: "ParentIdS",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_ParentProfiles_ParentProfileId",
                table: "Drawings",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedStories_ParentProfiles_ParentProfileId",
                table: "GeneratedStories",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfiles_Users_ParentIdS",
                table: "ParentProfiles",
                column: "ParentIdS",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
