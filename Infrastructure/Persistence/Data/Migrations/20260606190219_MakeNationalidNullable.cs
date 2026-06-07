using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeNationalidNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NationalId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NationalId",
                table: "AspNetUsers",
                column: "NationalId",
                unique: true,
                filter: "[NationalId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NationalId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NationalId",
                table: "AspNetUsers",
                column: "NationalId",
                unique: true);
        }
    }
}
