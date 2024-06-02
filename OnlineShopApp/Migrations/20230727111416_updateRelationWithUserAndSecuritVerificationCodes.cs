using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShopApp.Migrations
{
    /// <inheritdoc />
    public partial class updateRelationWithUserAndSecuritVerificationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SecurityVerificationCodes_UserId",
                table: "SecurityVerificationCodes");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityVerificationCodes_UserId",
                table: "SecurityVerificationCodes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SecurityVerificationCodes_UserId",
                table: "SecurityVerificationCodes");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityVerificationCodes_UserId",
                table: "SecurityVerificationCodes",
                column: "UserId",
                unique: true);
        }
    }
}
