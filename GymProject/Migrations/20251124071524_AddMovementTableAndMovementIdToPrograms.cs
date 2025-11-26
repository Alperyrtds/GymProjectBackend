using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class AddMovementTableAndMovementIdToPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MovementName",
                table: "Customers_Program",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MovementId",
                table: "Customers_Program",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Movements",
                columns: table => new
                {
                    MovementId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MovementName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MovementDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MovementVideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movement", x => x.MovementId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Program_MovementId",
                table: "Customers_Program",
                column: "MovementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Program_Movements_MovementId",
                table: "Customers_Program",
                column: "MovementId",
                principalTable: "Movements",
                principalColumn: "MovementId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Program_Movements_MovementId",
                table: "Customers_Program");

            migrationBuilder.DropTable(
                name: "Movements");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Program_MovementId",
                table: "Customers_Program");

            migrationBuilder.DropColumn(
                name: "MovementId",
                table: "Customers_Program");

            migrationBuilder.AlterColumn<string>(
                name: "MovementName",
                table: "Customers_Program",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
