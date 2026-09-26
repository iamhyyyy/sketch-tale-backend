using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDBForAILog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_Categories_CategoryId",
                table: "StoryTemplates");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropColumn(
                name: "RemainingCharacters",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "RemainingTimes",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "AIDetectedTagsJson",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ProcessedSpriteUrl",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "StoryTemplates",
                newName: "EducationThemeId");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTemplates_CategoryId",
                table: "StoryTemplates",
                newName: "IX_StoryTemplates_EducationThemeId");

            migrationBuilder.RenameColumn(
                name: "AllowedCategoryIdsJson",
                table: "ChildProfiles",
                newName: "AllowedThemeIdsJson");

            migrationBuilder.AddColumn<int>(
                name: "ParentApprovalStatus",
                table: "UserStoryCharacterMappings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "DrawingId",
                table: "Characters",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "CharTypeId",
                table: "Characters",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AICharacterGenerateId",
                table: "Characters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FinalImageUrl",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AICharacterGenerate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrawingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawImageUrl = table.Column<string>(type: "text", nullable: false),
                    AIPromptUsed = table.Column<string>(type: "text", nullable: false),
                    GenerateImageUrl = table.Column<string>(type: "text", nullable: true),
                    AIDetectedTagsJson = table.Column<string>(type: "text", nullable: true),
                    AIGenStatus = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICharacterGenerate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICharacterGenerate_CharTypes_CharTypeId",
                        column: x => x.CharTypeId,
                        principalTable: "CharTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AICharacterGenerate_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AICharacterGenerate_Drawings_DrawingId",
                        column: x => x.DrawingId,
                        principalTable: "Drawings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    TokenUsed = table.Column<int>(type: "integer", nullable: false),
                    CostAmount = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIUsageLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChildDailyUsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationSecond = table.Column<int>(type: "integer", nullable: false),
                    CharacterCreatedCount = table.Column<int>(type: "integer", nullable: false),
                    StoryReadCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildDailyUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildDailyUsageLogs_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EducationThemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationThemes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_AICharacterGenerateId",
                table: "Characters",
                column: "AICharacterGenerateId");

            migrationBuilder.CreateIndex(
                name: "IX_AICharacterGenerate_CharTypeId",
                table: "AICharacterGenerate",
                column: "CharTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AICharacterGenerate_ChildProfileId",
                table: "AICharacterGenerate",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AICharacterGenerate_DrawingId",
                table: "AICharacterGenerate",
                column: "DrawingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChildDailyUsageLogs_ChildProfileId",
                table: "ChildDailyUsageLogs",
                column: "ChildProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_AICharacterGenerate_AICharacterGenerateId",
                table: "Characters",
                column: "AICharacterGenerateId",
                principalTable: "AICharacterGenerate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters",
                column: "CharTypeId",
                principalTable: "CharTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters",
                column: "DrawingId",
                principalTable: "Drawings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_EducationThemes_EducationThemeId",
                table: "StoryTemplates",
                column: "EducationThemeId",
                principalTable: "EducationThemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_AICharacterGenerate_AICharacterGenerateId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryTemplates_EducationThemes_EducationThemeId",
                table: "StoryTemplates");

            migrationBuilder.DropTable(
                name: "AICharacterGenerate");

            migrationBuilder.DropTable(
                name: "AIUsageLogs");

            migrationBuilder.DropTable(
                name: "ChildDailyUsageLogs");

            migrationBuilder.DropTable(
                name: "EducationThemes");

            migrationBuilder.DropIndex(
                name: "IX_Characters_AICharacterGenerateId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ParentApprovalStatus",
                table: "UserStoryCharacterMappings");

            migrationBuilder.DropColumn(
                name: "AICharacterGenerateId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "FinalImageUrl",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "EducationThemeId",
                table: "StoryTemplates",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_StoryTemplates_EducationThemeId",
                table: "StoryTemplates",
                newName: "IX_StoryTemplates_CategoryId");

            migrationBuilder.RenameColumn(
                name: "AllowedThemeIdsJson",
                table: "ChildProfiles",
                newName: "AllowedCategoryIdsJson");

            migrationBuilder.AddColumn<int>(
                name: "RemainingCharacters",
                table: "ChildProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemainingTimes",
                table: "ChildProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "DrawingId",
                table: "Characters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CharTypeId",
                table: "Characters",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AIDetectedTagsJson",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedSpriteUrl",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_CharTypes_CharTypeId",
                table: "Characters",
                column: "CharTypeId",
                principalTable: "CharTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Drawings_DrawingId",
                table: "Characters",
                column: "DrawingId",
                principalTable: "Drawings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryTemplates_Categories_CategoryId",
                table: "StoryTemplates",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
