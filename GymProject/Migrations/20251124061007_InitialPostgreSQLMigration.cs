using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQLMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    AdministratorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdministratorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdministratorSurname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrator", x => x.AdministratorId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerSurname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerIdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerEmail = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Customers_Program",
                columns: table => new
                {
                    CustomerProgramId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MovementName = table.Column<string>(type: "text", nullable: true),
                    SetCount = table.Column<int>(type: "integer", nullable: true),
                    Reps = table.Column<int>(type: "integer", nullable: true),
                    ProgramStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProgramEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeftValidity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers_Program", x => x.CustomerProgramId);
                });

            migrationBuilder.CreateTable(
                name: "Customers_Registration",
                columns: table => new
                {
                    CustomerRegistrationId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CustomerRegistrationStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CustomerRegistrationFinishDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRegistratiom", x => x.CustomerRegistrationId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    UserPassword = table.Column<string>(type: "text", nullable: true),
                    CustomerBool = table.Column<int>(type: "integer", nullable: true),
                    AdministratorBool = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AdminastorId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Customers_Program");

            migrationBuilder.DropTable(
                name: "Customers_Registration");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
