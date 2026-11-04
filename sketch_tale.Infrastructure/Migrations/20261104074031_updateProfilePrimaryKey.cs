using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateProfilePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_ChildProfiles_ChildProfileId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildDailyUsageLogs_ChildProfiles_ChildProfileId",
                table: "ChildDailyUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildQuizAnswers_ChildProfiles_ChildProfileId",
                table: "ChildQuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildVocabularyProgresses_ChildProfiles_ChildProfileId",
                table: "ChildVocabularyProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_ChildProfiles_ChildProfileId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedStories_ChildProfiles_ChildProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfileSubs_ParentProfiles_ParentProfileId",
                table: "ParentProfileSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentProfileSubs",
                table: "ParentProfileSubs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentProfiles",
                table: "ParentProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentManagerProfiles",
                table: "ContentManagerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChildProfiles",
                table: "ChildProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Characters_DrawingId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContentManagerProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "DrawingId",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ParentProfileSubs",
                newName: "ParentProfileSubId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ParentProfiles",
                newName: "ParentProfileId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ContentManagerProfiles",
                newName: "ContentManagerProfileId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ChildProfiles",
                newName: "ChildProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentProfileSubs",
                table: "ParentProfileSubs",
                column: "ParentProfileSubId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentProfiles",
                table: "ParentProfiles",
                column: "ParentProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentManagerProfiles",
                table: "ContentManagerProfiles",
                column: "ContentManagerProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChildProfiles",
                table: "ChildProfiles",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentProfileSubs_ParentProfileId",
                table: "ParentProfileSubs",
                column: "ParentProfileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_ChildProfiles_ChildProfileId",
                table: "Characters",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildDailyUsageLogs_ChildProfiles_ChildProfileId",
                table: "ChildDailyUsageLogs",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "ParentProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_ChildProfileId",
                table: "ChildProfiles",
                column: "ChildProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildQuizAnswers_ChildProfiles_ChildProfileId",
                table: "ChildQuizAnswers",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildVocabularyProgresses_ChildProfiles_ChildProfileId",
                table: "ChildVocabularyProgresses",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentManagerProfiles_Users_ContentManagerProfileId",
                table: "ContentManagerProfiles",
                column: "ContentManagerProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_ChildProfiles_ChildProfileId",
                table: "Drawings",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedStories_ChildProfiles_ChildProfileId",
                table: "GeneratedStories",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfiles_Users_ParentProfileId",
                table: "ParentProfiles",
                column: "ParentProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfileSubs_ParentProfiles_ParentProfileId",
                table: "ParentProfileSubs",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "ParentProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfileSubs_Users_ParentProfileSubId",
                table: "ParentProfileSubs",
                column: "ParentProfileSubId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "ParentProfileId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "ChildProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_ChildProfiles_ChildProfileId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildDailyUsageLogs_ChildProfiles_ChildProfileId",
                table: "ChildDailyUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_ChildProfileId",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildQuizAnswers_ChildProfiles_ChildProfileId",
                table: "ChildQuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildVocabularyProgresses_ChildProfiles_ChildProfileId",
                table: "ChildVocabularyProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentManagerProfiles_Users_ContentManagerProfileId",
                table: "ContentManagerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_ChildProfiles_ChildProfileId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedStories_ChildProfiles_ChildProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfiles_Users_ParentProfileId",
                table: "ParentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfileSubs_ParentProfiles_ParentProfileId",
                table: "ParentProfileSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfileSubs_Users_ParentProfileSubId",
                table: "ParentProfileSubs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentProfileSubs",
                table: "ParentProfileSubs");

            migrationBuilder.DropIndex(
                name: "IX_ParentProfileSubs_ParentProfileId",
                table: "ParentProfileSubs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParentProfiles",
                table: "ParentProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentManagerProfiles",
                table: "ContentManagerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChildProfiles",
                table: "ChildProfiles");

            migrationBuilder.RenameColumn(
                name: "ParentProfileSubId",
                table: "ParentProfileSubs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ParentProfileId",
                table: "ParentProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ContentManagerProfileId",
                table: "ContentManagerProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ChildProfileId",
                table: "ChildProfiles",
                newName: "UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ParentProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ContentManagerProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ChildProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DrawingId",
                table: "Characters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentProfileSubs",
                table: "ParentProfileSubs",
                column: "ParentProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParentProfiles",
                table: "ParentProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentManagerProfiles",
                table: "ContentManagerProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChildProfiles",
                table: "ChildProfiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_DrawingId",
                table: "Characters",
                column: "DrawingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_ChildProfiles_ChildProfileId",
                table: "Characters",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters",
                column: "DrawingId",
                principalTable: "Drawings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildDailyUsageLogs_ChildProfiles_ChildProfileId",
                table: "ChildDailyUsageLogs",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_ParentProfiles_ParentProfileId",
                table: "ChildProfiles",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildQuizAnswers_ChildProfiles_ChildProfileId",
                table: "ChildQuizAnswers",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildVocabularyProgresses_ChildProfiles_ChildProfileId",
                table: "ChildVocabularyProgresses",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drawings_ChildProfiles_ChildProfileId",
                table: "Drawings",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedStories_ChildProfiles_ChildProfileId",
                table: "GeneratedStories",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfileSubs_ParentProfiles_ParentProfileId",
                table: "ParentProfileSubs",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id");
        }
    }
}
