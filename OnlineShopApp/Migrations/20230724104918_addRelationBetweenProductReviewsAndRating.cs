using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShopApp.Migrations
{
	/// <inheritdoc />
	public partial class addRelationBetweenProductReviewsAndRating : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "ProductRatingId",
				table: "ProductReviews",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateIndex(
				name: "IX_ProductReviews_ProductRatingId",
				table: "ProductReviews",
				column: "ProductRatingId",
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_ProductReviews_ProductRating_ProductRatingId",
				table: "ProductReviews",
				column: "ProductRatingId",
				principalTable: "ProductRating",
				principalColumn: "Id",
				onDelete: ReferentialAction.NoAction);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_ProductReviews_ProductRating_ProductRatingId",
				table: "ProductReviews");

			migrationBuilder.DropIndex(
				name: "IX_ProductReviews_ProductRatingId",
				table: "ProductReviews");

			migrationBuilder.DropColumn(
				name: "ProductRatingId",
				table: "ProductReviews");
		}
	}
}
