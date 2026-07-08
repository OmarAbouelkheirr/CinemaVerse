using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaVerse.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameBranchsToBranchesAndMovieRatingPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Halls_Branchs_BranchId",
                table: "Halls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branchs",
                table: "Branchs");

            migrationBuilder.RenameTable(
                name: "Branchs",
                newName: "Branches");

            migrationBuilder.AlterColumn<decimal>(
                name: "MovieRating",
                table: "Movies",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,1)",
                oldPrecision: 2,
                oldScale: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branches",
                table: "Branches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Halls_Branches_BranchId",
                table: "Halls",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Halls_Branches_BranchId",
                table: "Halls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branches",
                table: "Branches");

            migrationBuilder.RenameTable(
                name: "Branches",
                newName: "Branchs");

            migrationBuilder.AlterColumn<decimal>(
                name: "MovieRating",
                table: "Movies",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldPrecision: 3,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branchs",
                table: "Branchs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Halls_Branchs_BranchId",
                table: "Halls",
                column: "BranchId",
                principalTable: "Branchs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
