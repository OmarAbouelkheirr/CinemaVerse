using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaVerse.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Movies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Movies");
        }
    }
}
