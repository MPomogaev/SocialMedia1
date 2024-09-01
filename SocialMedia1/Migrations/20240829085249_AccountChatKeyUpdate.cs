using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia1.Migrations
{
    /// <inheritdoc />
    public partial class AccountChatKeyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatAccount",
                table: "ChatAccount");

            migrationBuilder.DropIndex(
                name: "IX_ChatAccount_ChatId",
                table: "ChatAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatAccount",
                table: "ChatAccount",
                columns: new[] { "ChatId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAccount_AccountId",
                table: "ChatAccount",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatAccount",
                table: "ChatAccount");

            migrationBuilder.DropIndex(
                name: "IX_ChatAccount_AccountId",
                table: "ChatAccount");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatAccount",
                table: "ChatAccount",
                columns: new[] { "AccountId", "ChatId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAccount_ChatId",
                table: "ChatAccount",
                column: "ChatId");
        }
    }
}
