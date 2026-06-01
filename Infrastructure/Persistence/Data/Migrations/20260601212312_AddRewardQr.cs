using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "UserRewards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserRewardId",
                table: "QrTokens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrTokens_UserRewardId",
                table: "QrTokens",
                column: "UserRewardId");

            migrationBuilder.AddForeignKey(
                name: "FK_QrTokens_UserRewards_UserRewardId",
                table: "QrTokens",
                column: "UserRewardId",
                principalTable: "UserRewards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QrTokens_UserRewards_UserRewardId",
                table: "QrTokens");

            migrationBuilder.DropIndex(
                name: "IX_QrTokens_UserRewardId",
                table: "QrTokens");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "UserRewards");

            migrationBuilder.DropColumn(
                name: "UserRewardId",
                table: "QrTokens");
        }
    }
}
