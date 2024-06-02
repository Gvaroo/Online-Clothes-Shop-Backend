using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShopApp.Migrations
{
	/// <inheritdoc />
	public partial class addProductAndUserRelation : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "UserId",
				table: "Product",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_Product_UserId",
				table: "Product",
				column: "UserId");

			migrationBuilder.AddForeignKey(
				name: "FK_Product_Users_UserId",
				table: "Product",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.NoAction);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Product_Users_UserId",
				table: "Product");

			migrationBuilder.DropIndex(
				name: "IX_Product_UserId",
				table: "Product");

			migrationBuilder.DropColumn(
				name: "UserId",
				table: "Product");
		}
	}
}
