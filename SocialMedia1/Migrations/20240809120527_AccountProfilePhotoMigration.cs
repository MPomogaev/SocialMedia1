using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia1.Migrations
{
    /// <inheritdoc />
    public partial class AccountProfilePhotoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePhoto",
                table: "Account",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "Account");
        }
    }
}
