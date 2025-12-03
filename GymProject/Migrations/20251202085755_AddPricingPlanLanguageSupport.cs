using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingPlanLanguageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PricingPlans",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "tr");

            migrationBuilder.AddColumn<string>(
                name: "OriginalLanguage",
                table: "PricingPlans",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PricingPlanId",
                table: "Customers_Registration",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Registration_PricingPlanId",
                table: "Customers_Registration",
                column: "PricingPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Registration_PricingPlans_PricingPlanId",
                table: "Customers_Registration",
                column: "PricingPlanId",
                principalTable: "PricingPlans",
                principalColumn: "PricingPlanId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Registration_PricingPlans_PricingPlanId",
                table: "Customers_Registration");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Registration_PricingPlanId",
                table: "Customers_Registration");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PricingPlans");

            migrationBuilder.DropColumn(
                name: "OriginalLanguage",
                table: "PricingPlans");

            migrationBuilder.DropColumn(
                name: "PricingPlanId",
                table: "Customers_Registration");
        }
    }
}
