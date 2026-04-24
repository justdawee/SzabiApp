using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace SzabiApp.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDateToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LocalDate>(
                name: "BirthDate",
                table: "users",
                type: "date",
                nullable: false,
                defaultValue: new NodaTime.LocalDate(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "users");
        }
    }
}
