using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSupcriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_ParentIdM",
                table: "ChildProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ChildProfiles_Users_UserId",
                table: "ChildProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChildProfiles_ParentIdM",
                table: "ChildProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ChildProfiles_UserId",
                table: "ChildProfiles");

            migrationBuilder.RenameColumn(
                name: "ParentIdM",
                table: "ChildProfiles",
                newName: "ParentProfileId");

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

            migrationBuilder.AddColumn<Guid>(
                name: "ParentProfileId",
                table: "Characters",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentIdS = table.Column<Guid>(type: "uuid", nullable: true),
                    NickName = table.Column<string>(type: "text", nullable: false),
                    TargetAgeGroup = table.Column<int>(type: "integer", nullable: false),
                    DailyTimeLimit = table.Column<int>(type: "integer", nullable: false),
                    RemainingTimes = table.Column<int>(type: "integer", nullable: false),
                    DailyCharacterLimit = table.Column<int>(type: "integer", nullable: false),
                    RemainingCharacters = table.Column<int>(type: "integer", nullable: false),
                    AllowedCategoryIdsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentProfiles_Users_ParentIdS",
                        column: x => x.ParentIdS,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    TrialDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentProvider = table.Column<string>(type: "text", nullable: false),
                    TxnRef = table.Column<string>(type: "text", nullable: false),
                    TransactionNo = table.Column<string>(type: "text", nullable: true),
                    BankCode = table.Column<string>(type: "text", nullable: true),
                    ResponseCode = table.Column<string>(type: "text", nullable: true),
                    PayDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawIpnResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedStories_ParentProfileId",
                table: "GeneratedStories",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ParentProfileId",
                table: "Drawings",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ParentProfileId",
                table: "Characters",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentProfiles_ParentIdS",
                table: "ParentProfiles",
                column: "ParentIdS");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SubscriptionId",
                table: "PaymentTransactions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_ParentProfiles_ParentProfileId",
                table: "Characters",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_ParentProfiles_ParentProfileId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Drawings_ParentProfiles_ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedStories_ParentProfiles_ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropTable(
                name: "ParentProfiles");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedStories_ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Characters_ParentProfileId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "GeneratedStories");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "Drawings");

            migrationBuilder.DropColumn(
                name: "RemainingCharacters",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "RemainingTimes",
                table: "ChildProfiles");

            migrationBuilder.DropColumn(
                name: "ParentProfileId",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "ParentProfileId",
                table: "ChildProfiles",
                newName: "ParentIdM");

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfiles_ParentIdM",
                table: "ChildProfiles",
                column: "ParentIdM");

            migrationBuilder.CreateIndex(
                name: "IX_ChildProfiles_UserId",
                table: "ChildProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_ParentIdM",
                table: "ChildProfiles",
                column: "ParentIdM",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChildProfiles_Users_UserId",
                table: "ChildProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
