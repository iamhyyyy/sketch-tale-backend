using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sketch_tale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions",
                column: "ParentProfileId",
                principalTable: "ParentProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ParentProfiles_ParentProfileId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_ParentProfileId",
                table: "PaymentTransactions");
        }
    }
}
