using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaya.Migrations
{
    /// <inheritdoc />
    public partial class AddBiographyAndExperienceToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "yearsExperience",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "yearsExperience",
                table: "Doctors");
        }
    }
}
