using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDBForChildProfile_Theme_Story : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_StoryTemplates_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_EducationThemes_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.CreateTable(
                name: "ChildProfileEducationTheme",
                columns: table => new
                {
                    AllowedThemesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildProfileEducationTheme", x => new { x.AllowedThemesId, x.ChildProfileId });
                    table.ForeignKey(
                        name: "FK_ChildProfileEducationTheme_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildProfileEducationTheme_EducationThemes_AllowedThemesId",
                        column: x => x.AllowedThemesId,
                        principalTable: "EducationThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChildProfileStoryTemplate",
                columns: table => new
                {
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoryTemplateRecommendsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildProfileStoryTemplate", x => new { x.ChildProfileId, x.StoryTemplateRecommendsId });
                    table.ForeignKey(
                        name: "FK_ChildProfileStoryTemplate_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildProfileStoryTemplate_StoryTemplates_StoryTemplateRecom~",
                        column: x => x.StoryTemplateRecommendsId,
                        principalTable: "StoryTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfileEducationTheme_ChildProfileId",
                table: "ChildProfileEducationTheme",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfileStoryTemplate_StoryTemplateRecommendsId",
                table: "ChildProfileStoryTemplate",
                column: "StoryTemplateRecommendsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildProfileEducationTheme");

            migrationBuilder.DropTable(
                name: "ChildProfileStoryTemplate");

            migrationBuilder.AddColumn<Guid>(
                name: "ChildProfileId",
                table: "StoryTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChildProfileId",
                table: "EducationThemes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryTemplates_ChildProfileId",
                table: "StoryTemplates",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationThemes_ChildProfileId",
                table: "EducationThemes",
                column: "ChildProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id");
        }
    }
}
