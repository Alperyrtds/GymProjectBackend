using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramMovementsAndWorkoutSessionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogs_Customers_CustomerId",
                table: "WorkoutLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogs_Trainers_TrainerId",
                table: "WorkoutLogs");

            migrationBuilder.AddColumn<string>(
                name: "WorkoutSessionId",
                table: "WorkoutLogs",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Customers_Program",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Customers_Program",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramName",
                table: "Customers_Program",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProgramMovements",
                columns: table => new
                {
                    ProgramMovementId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerProgramId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MovementId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MovementName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SetCount = table.Column<int>(type: "integer", nullable: true),
                    Reps = table.Column<int>(type: "integer", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramMovement", x => x.ProgramMovementId);
                    table.ForeignKey(
                        name: "FK_ProgramMovements_Customers_Program_CustomerProgramId",
                        column: x => x.CustomerProgramId,
                        principalTable: "Customers_Program",
                        principalColumn: "CustomerProgramId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramMovements_Movements_MovementId",
                        column: x => x.MovementId,
                        principalTable: "Movements",
                        principalColumn: "MovementId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    WorkoutSessionId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WorkoutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TotalDuration = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TrainerId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSession", x => x.WorkoutSessionId);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_WorkoutSessionId",
                table: "WorkoutLogs",
                column: "WorkoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramMovements_CustomerProgramId",
                table: "ProgramMovements",
                column: "CustomerProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramMovements_MovementId",
                table: "ProgramMovements",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_CustomerId",
                table: "WorkoutSessions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_TrainerId",
                table: "WorkoutSessions",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogs_Customers_CustomerId",
                table: "WorkoutLogs",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogs_Trainers_TrainerId",
                table: "WorkoutLogs",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogs_WorkoutSessions_WorkoutSessionId",
                table: "WorkoutLogs",
                column: "WorkoutSessionId",
                principalTable: "WorkoutSessions",
                principalColumn: "WorkoutSessionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogs_Customers_CustomerId",
                table: "WorkoutLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogs_Trainers_TrainerId",
                table: "WorkoutLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutLogs_WorkoutSessions_WorkoutSessionId",
                table: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "ProgramMovements");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutLogs_WorkoutSessionId",
                table: "WorkoutLogs");

            migrationBuilder.DropColumn(
                name: "WorkoutSessionId",
                table: "WorkoutLogs");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Customers_Program");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Customers_Program");

            migrationBuilder.DropColumn(
                name: "ProgramName",
                table: "Customers_Program");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogs_Customers_CustomerId",
                table: "WorkoutLogs",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutLogs_Trainers_TrainerId",
                table: "WorkoutLogs",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
