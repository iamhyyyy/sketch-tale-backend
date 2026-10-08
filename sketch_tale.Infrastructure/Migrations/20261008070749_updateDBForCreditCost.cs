using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDBForCreditCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerate_CharTypes_CharTypeId",
                table: "AICharacterGenerate");

            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerate_ChildProfiles_ChildProfileId",
                table: "AICharacterGenerate");

            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerate_Drawings_DrawingId",
                table: "AICharacterGenerate");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_AICharacterGenerate_AICharacterGenerateId",
                table: "Characters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AICharacterGenerate",
                table: "AICharacterGenerate");

            migrationBuilder.RenameTable(
                name: "AICharacterGenerate",
                newName: "AICharacterGenerates");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerate_DrawingId",
                table: "AICharacterGenerates",
                newName: "IX_AICharacterGenerates_DrawingId");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerate_ChildProfileId",
                table: "AICharacterGenerates",
                newName: "IX_AICharacterGenerates_ChildProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerate_CharTypeId",
                table: "AICharacterGenerates",
                newName: "IX_AICharacterGenerates_CharTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AICharacterGenerates",
                table: "AICharacterGenerates",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CreditCosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureKey = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<int>(type: "integer", nullable: false),
                    FeatureNameVi = table.Column<string>(type: "text", nullable: false),
                    FeatureNameEn = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_ParentProfiles_ParentProfileId",
                        column: x => x.ParentProfileId,
                        principalTable: "ParentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentStoryRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoryTemplateIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStoryRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentStoryRecommendations_ChildProfiles_ChildProfileId",
                        column: x => x.ChildProfileId,
                        principalTable: "ChildProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "system", "SYSTEM" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "admin", "ADMIN" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "content manager", "CONTENT MANAGER" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "parent", "PARENT" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), null, "child", "CHILD" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ParentProfileId",
                table: "CreditTransactions",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStoryRecommendations_ChildProfileId",
                table: "ParentStoryRecommendations",
                column: "ChildProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerates_CharTypes_CharTypeId",
                table: "AICharacterGenerates",
                column: "CharTypeId",
                principalTable: "CharTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerates_ChildProfiles_ChildProfileId",
                table: "AICharacterGenerates",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerates_Drawings_DrawingId",
                table: "AICharacterGenerates",
                column: "DrawingId",
                principalTable: "Drawings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_AICharacterGenerates_AICharacterGenerateId",
                table: "Characters",
                column: "AICharacterGenerateId",
                principalTable: "AICharacterGenerates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerates_CharTypes_CharTypeId",
                table: "AICharacterGenerates");

            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerates_ChildProfiles_ChildProfileId",
                table: "AICharacterGenerates");

            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerates_Drawings_DrawingId",
                table: "AICharacterGenerates");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_AICharacterGenerates_AICharacterGenerateId",
                table: "Characters");

            migrationBuilder.DropTable(
                name: "CreditCosts");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "ParentStoryRecommendations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AICharacterGenerates",
                table: "AICharacterGenerates");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.RenameTable(
                name: "AICharacterGenerates",
                newName: "AICharacterGenerate");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerates_DrawingId",
                table: "AICharacterGenerate",
                newName: "IX_AICharacterGenerate_DrawingId");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerates_ChildProfileId",
                table: "AICharacterGenerate",
                newName: "IX_AICharacterGenerate_ChildProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AICharacterGenerates_CharTypeId",
                table: "AICharacterGenerate",
                newName: "IX_AICharacterGenerate_CharTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AICharacterGenerate",
                table: "AICharacterGenerate",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "admin", "ADMIN" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "content manager", "CONTENT MANAGER" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "parent", "PARENT" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "child", "CHILD" });

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerate_CharTypes_CharTypeId",
                table: "AICharacterGenerate",
                column: "CharTypeId",
                principalTable: "CharTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerate_ChildProfiles_ChildProfileId",
                table: "AICharacterGenerate",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AICharacterGenerate_Drawings_DrawingId",
                table: "AICharacterGenerate",
                column: "DrawingId",
                principalTable: "Drawings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_AICharacterGenerate_AICharacterGenerateId",
                table: "Characters",
                column: "AICharacterGenerateId",
                principalTable: "AICharacterGenerate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
