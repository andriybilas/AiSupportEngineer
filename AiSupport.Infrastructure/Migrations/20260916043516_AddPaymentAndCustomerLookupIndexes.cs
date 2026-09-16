using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiSupport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndCustomerLookupIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_ProviderTransactionId",
                table: "PaymentAttempts",
                column: "ProviderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ExternalId",
                table: "Customers",
                column: "ExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_ProviderTransactionId",
                table: "PaymentAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ExternalId",
                table: "Customers");
        }
    }
}
