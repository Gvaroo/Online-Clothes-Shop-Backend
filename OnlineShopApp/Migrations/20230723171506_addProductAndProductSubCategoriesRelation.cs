using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShopApp.Migrations
{
	/// <inheritdoc />
	public partial class addProductAndProductSubCategoriesRelation : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "SubCategoriesId",
				table: "Product",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_Product_SubCategoriesId",
				table: "Product",
				column: "SubCategoriesId");

			migrationBuilder.AddForeignKey(
				name: "FK_Product_ProductSubCategories_SubCategoriesId",
				table: "Product",
				column: "SubCategoriesId",
				principalTable: "ProductSubCategories",
				principalColumn: "Id",
				onDelete: ReferentialAction.NoAction);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Product_ProductSubCategories_SubCategoriesId",
				table: "Product");

			migrationBuilder.DropIndex(
				name: "IX_Product_SubCategoriesId",
				table: "Product");

			migrationBuilder.DropColumn(
				name: "SubCategoriesId",
				table: "Product");
		}
	}
}
