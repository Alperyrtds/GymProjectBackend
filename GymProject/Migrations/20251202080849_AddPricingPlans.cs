using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingPlans",
                columns: table => new
                {
                    PricingPlanId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DurationInMonths = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPlan", x => x.PricingPlanId);
                });

            migrationBuilder.CreateTable(
                name: "PricingPlanFeatures",
                columns: table => new
                {
                    PricingPlanFeatureId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PricingPlanId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FeatureText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PricingPlanId1 = table.Column<string>(type: "character varying(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPlanFeature", x => x.PricingPlanFeatureId);
                    table.ForeignKey(
                        name: "FK_PricingPlanFeatures_PricingPlans_PricingPlanId",
                        column: x => x.PricingPlanId,
                        principalTable: "PricingPlans",
                        principalColumn: "PricingPlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PricingPlanFeatures_PricingPlans_PricingPlanId1",
                        column: x => x.PricingPlanId1,
                        principalTable: "PricingPlans",
                        principalColumn: "PricingPlanId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlanFeatures_PricingPlanId",
                table: "PricingPlanFeatures",
                column: "PricingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingPlanFeatures_PricingPlanId1",
                table: "PricingPlanFeatures",
                column: "PricingPlanId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPlanFeatures");

            migrationBuilder.DropTable(
                name: "PricingPlans");
        }
    }
}
