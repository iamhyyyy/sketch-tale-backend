using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDbForNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerates_CharTypes_CharTypeId",
                table: "AICharacterGenerates");

            migrationBuilder.DropForeignKey(
                name: "FK_AICharacterGenerates_ChildProfiles_ChildProfileId",
                table: "AICharacterGenerates");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ReadingLogs_ChildProfiles_ChildProfileId",
                table: "ReadingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ReadingLogs_GeneratedStories_GeneratedStoryId",
                table: "ReadingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStoryCharacterMappings_Characters_CharacterId",
                table: "UserStoryCharacterMappings");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropIndex(
                name: "IX_UserStoryCharacterMappings_CharacterId",
                table: "UserStoryCharacterMappings");

            migrationBuilder.DropIndex(
                name: "IX_ReadingLogs_ChildProfileId",
                table: "ReadingLogs");

            migrationBuilder.DropIndex(
                name: "IX_ReadingLogs_GeneratedStoryId",
                table: "ReadingLogs");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ParentProfileId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AICharacterGenerates_CharTypeId",
                table: "AICharacterGenerates");

            migrationBuilder.DropIndex(
                name: "IX_AICharacterGenerates_ChildProfileId",
                table: "AICharacterGenerates");

            migrationBuilder.DropColumn(
                name: "RawImageUrl",
                table: "AICharacterGenerates");

            migrationBuilder.CreateTable(
                name: "CreditTransactionLogs",
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
                    table.PrimaryKey("PK_CreditTransactionLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditTransactionLogs");

            migrationBuilder.AddColumn<string>(
                name: "RawImageUrl",
                table: "AICharacterGenerates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_UserStoryCharacterMappings_CharacterId",
                table: "UserStoryCharacterMappings",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingLogs_ChildProfileId",
                table: "ReadingLogs",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingLogs_GeneratedStoryId",
                table: "ReadingLogs",
                column: "GeneratedStoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AICharacterGenerates_CharTypeId",
                table: "AICharacterGenerates",
                column: "CharTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AICharacterGenerates_ChildProfileId",
                table: "AICharacterGenerates",
                column: "ChildProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ParentProfileId",
                table: "CreditTransactions",
                column: "ParentProfileId");

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
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingLogs_ChildProfiles_ChildProfileId",
                table: "ReadingLogs",
                column: "ChildProfileId",
                principalTable: "ChildProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingLogs_GeneratedStories_GeneratedStoryId",
                table: "ReadingLogs",
                column: "GeneratedStoryId",
                principalTable: "GeneratedStories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStoryCharacterMappings_Characters_CharacterId",
                table: "UserStoryCharacterMappings",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
