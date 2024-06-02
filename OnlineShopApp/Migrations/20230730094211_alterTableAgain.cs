using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShopApp.Migrations
{
	/// <inheritdoc />
	public partial class alterTableAgain : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "PasswordSalt",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				defaultValue: null,
				oldClrType: typeof(string),
				oldType: "nvarchar(255)",
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "PasswordHash",
				table: "Users",
				type: "nvarchar(max)",
				nullable: true,
				defaultValue: null,
				oldClrType: typeof(string),
				oldType: "nvarchar(255)",
				oldNullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "PasswordSalt",
				table: "Users",
				type: "nvarchar(255)",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)");

			migrationBuilder.AlterColumn<string>(
				name: "PasswordHash",
				table: "Users",
				type: "nvarchar(255)",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)");
		}
	}
}
