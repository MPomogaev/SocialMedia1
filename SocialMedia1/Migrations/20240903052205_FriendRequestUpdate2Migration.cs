using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia1.Migrations
{
    /// <inheritdoc />
    public partial class FriendRequestUpdate2Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FriendRequest_RequestedId",
                table: "FriendRequest");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_RequestedId",
                table: "FriendRequest",
                column: "RequestedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FriendRequest_RequestedId",
                table: "FriendRequest");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_RequestedId",
                table: "FriendRequest",
                column: "RequestedId",
                unique: true);
        }
    }
}
