using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaVerse.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeat_Seats_SeatId",
                table: "BookingSeat");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieGenres_Genres_GenreID",
                table: "MovieGenres");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeat_Seats_SeatId",
                table: "BookingSeat",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieGenres_Genres_GenreID",
                table: "MovieGenres",
                column: "GenreID",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingSeat_Seats_SeatId",
                table: "BookingSeat");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieGenres_Genres_GenreID",
                table: "MovieGenres");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSeat_Seats_SeatId",
                table: "BookingSeat",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieGenres_Genres_GenreID",
                table: "MovieGenres",
                column: "GenreID",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
