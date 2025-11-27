using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymProject.Migrations
{
    /// <inheritdoc />
    public partial class ContactResponseRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RespondedByUserId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ResponseDate",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "ResponseMessage",
                table: "ContactMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RespondedByUserId",
                table: "ContactMessages",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDate",
                table: "ContactMessages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseMessage",
                table: "ContactMessages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
