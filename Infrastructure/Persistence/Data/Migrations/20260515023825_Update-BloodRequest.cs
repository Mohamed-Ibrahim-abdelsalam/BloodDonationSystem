using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBloodRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "neededby",
                table: "BloodRequests",
                newName: "NeededBy");

            migrationBuilder.RenameColumn(
                name: "HospitalAddress",
                table: "BloodRequests",
                newName: "HospitalLocation");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "BloodRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "BloodRequests",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "BloodRequests");

            migrationBuilder.RenameColumn(
                name: "NeededBy",
                table: "BloodRequests",
                newName: "neededby");

            migrationBuilder.RenameColumn(
                name: "HospitalLocation",
                table: "BloodRequests",
                newName: "HospitalAddress");
        }
    }
}
