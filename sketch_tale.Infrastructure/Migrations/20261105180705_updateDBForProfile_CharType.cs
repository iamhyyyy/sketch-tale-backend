using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDBForProfile_CharType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_ChildProfileId",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentManagerProfiles_Users_ContentManagerProfileId",
                table: "ContentManagerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfiles_Users_ParentProfileId",
                table: "ParentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfileSubs_Users_ParentProfileSubId",
                table: "ParentProfileSubs");

            migrationBuilder.RenameColumn(
                name: "Word",
                table: "VocabularyTemplates",
                newName: "WordVi");

            migrationBuilder.RenameColumn(
                name: "AudioUrl",
                table: "VocabularyTemplates",
                newName: "WordEn");

            migrationBuilder.RenameColumn(
                name: "ParentProfileSubId",
                table: "ParentProfileSubs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ParentProfileId",
                table: "ParentProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "EducationThemes",
                newName: "NameVi");

            migrationBuilder.RenameColumn(
                name: "ContentManagerProfileId",
                table: "ContentManagerProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ChildProfileId",
                table: "ChildProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CharTypes",
                newName: "NameVi");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CharTypes",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrlEn",
                table: "VocabularyTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrlVi",
                table: "VocabularyTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IPA",
                table: "VocabularyTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CanFly",
                table: "StoryRoleTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSwim",
                table: "StoryRoleTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "StoryRoleTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "EducationThemes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreditAfter",
                table: "CreditTransactionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreditBefore",
                table: "CreditTransactionLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FeatureKey",
                table: "CreditTransactionLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanFly",
                table: "CharTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSwim",
                table: "CharTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "CharTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "CharTypes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_Id",
                table: "ChildProfiles",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentManagerProfiles_Users_Id",
                table: "ContentManagerProfiles",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfiles_Users_Id",
                table: "ParentProfiles",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfileSubs_Users_Id",
                table: "ParentProfileSubs",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_Id",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentManagerProfiles_Users_Id",
                table: "ContentManagerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfiles_Users_Id",
                table: "ParentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentProfileSubs_Users_Id",
                table: "ParentProfileSubs");

            migrationBuilder.DropColumn(
                name: "AudioUrlEn",
                table: "VocabularyTemplates");

            migrationBuilder.DropColumn(
                name: "AudioUrlVi",
                table: "VocabularyTemplates");

            migrationBuilder.DropColumn(
                name: "IPA",
                table: "VocabularyTemplates");

            migrationBuilder.DropColumn(
                name: "CanFly",
                table: "StoryRoleTemplates");

            migrationBuilder.DropColumn(
                name: "CanSwim",
                table: "StoryRoleTemplates");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "StoryRoleTemplates");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "EducationThemes");

            migrationBuilder.DropColumn(
                name: "CreditAfter",
                table: "CreditTransactionLogs");

            migrationBuilder.DropColumn(
                name: "CreditBefore",
                table: "CreditTransactionLogs");

            migrationBuilder.DropColumn(
                name: "FeatureKey",
                table: "CreditTransactionLogs");

            migrationBuilder.DropColumn(
                name: "CanFly",
                table: "CharTypes");

            migrationBuilder.DropColumn(
                name: "CanSwim",
                table: "CharTypes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "CharTypes");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "CharTypes");

            migrationBuilder.RenameColumn(
                name: "WordVi",
                table: "VocabularyTemplates",
                newName: "Word");

            migrationBuilder.RenameColumn(
                name: "WordEn",
                table: "VocabularyTemplates",
                newName: "AudioUrl");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ParentProfileSubs",
                newName: "ParentProfileSubId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ParentProfiles",
                newName: "ParentProfileId");

            migrationBuilder.RenameColumn(
                name: "NameVi",
                table: "EducationThemes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ContentManagerProfiles",
                newName: "ContentManagerProfileId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ChildProfiles",
                newName: "ChildProfileId");

            migrationBuilder.RenameColumn(
                name: "NameVi",
                table: "CharTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "CharTypes",
                newName: "Description");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_ChildProfileId",
                table: "ChildProfiles",
                column: "ChildProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentManagerProfiles_Users_ContentManagerProfileId",
                table: "ContentManagerProfiles",
                column: "ContentManagerProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfiles_Users_ParentProfileId",
                table: "ParentProfiles",
                column: "ParentProfileId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentProfileSubs_Users_ParentProfileSubId",
                table: "ParentProfileSubs",
                column: "ParentProfileSubId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
