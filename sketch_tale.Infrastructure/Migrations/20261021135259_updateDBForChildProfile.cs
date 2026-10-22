using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateDBForChildProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParentStoryRecommendations");

            migrationBuilder.DropColumn(
                name: "AllowedThemeIdsJson",
                table: "ChildProfiles");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedThemeIds",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "StoryTemplateRecommendIds",
                table: "ChildProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AllowedThemeIdsJson",
                table: "ChildProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParentStoryRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StoryTemplateIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_ParentStoryRecommendations_ChildProfileId",
                table: "ParentStoryRecommendations",
                column: "ChildProfileId");
        }
    }
}
