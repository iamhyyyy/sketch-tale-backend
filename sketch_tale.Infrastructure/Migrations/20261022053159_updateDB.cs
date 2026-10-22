using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Characters_CharTypeId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "AccessFullStories",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "CanExportStory",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "ChildProfileLimit",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "MonthlyCreditLimit",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "SecurityCode",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "AllowedThemeIds",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "StoryTemplateRecommendIds",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "CharTypeId",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "SubscriptionTrialPlusPlan",
                table: "ParentProfiles",
                newName: "TrialPlusPlan");

            migrationBuilder.RenameColumn(
                name: "RemainingMonthlyCreditLimit",
                table: "ParentProfiles",
                newName: "RemainingMonthlyCredit");

            migrationBuilder.RenameColumn(
                name: "RemainingChildProfileLimit",
                table: "ParentProfiles",
                newName: "RemainingChildProfile");

            migrationBuilder.AddColumn<Guid>(
                name: "ChildProfileId",
                table: "StoryTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "ParentProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "IX_ParentProfiles_SubscriptionPlanId",
                table: "ParentProfiles",
                column: "SubscriptionPlanId");

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
                name: "FK_ParentProfiles_SubscriptionPlans_SubscriptionPlanId",
                table: "ParentProfiles",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationThemes_ChildProfiles_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfiles_SubscriptionPlans_SubscriptionPlanId",
                table: "ParentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_ChildProfiles_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_StoryTemplates_ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ParentProfiles_SubscriptionPlanId",
                table: "ParentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_EducationThemes_ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "StoryTemplates");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "ParentProfiles");

            migrationBuilder.DropColumn(
                name: "ChildProfileId",
                table: "EducationThemes");

            migrationBuilder.RenameColumn(
                name: "TrialPlusPlan",
                table: "ParentProfiles",
                newName: "SubscriptionTrialPlusPlan");

            migrationBuilder.RenameColumn(
                name: "RemainingMonthlyCredit",
                table: "ParentProfiles",
                newName: "RemainingMonthlyCreditLimit");

            migrationBuilder.RenameColumn(
                name: "RemainingChildProfile",
                table: "ParentProfiles",
                newName: "RemainingChildProfileLimit");

            migrationBuilder.AddColumn<bool>(
                name: "AccessFullStories",
                table: "ParentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanExportStory",
                table: "ParentProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ChildProfileLimit",
                table: "ParentProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyCreditLimit",
                table: "ParentProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecurityCode",
                table: "ParentProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "AllowedThemeIds",
                table: "ChildProfiles",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "StoryTemplateRecommendIds",
                table: "ChildProfiles",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CharTypeId",
                table: "Characters",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_CharTypeId",
                table: "Characters",
                column: "CharTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters",
                column: "CharTypeId",
                principalTable: "CharTypes",
                principalColumn: "Id");
        }
    }
}
