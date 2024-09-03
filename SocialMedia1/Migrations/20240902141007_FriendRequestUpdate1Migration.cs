using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMedia1.Migrations
{
    /// <inheritdoc />
    public partial class FriendRequestUpdate1Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendRequest",
                table: "FriendRequest");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequest_RequesterId_RequestedId",
                table: "FriendRequest");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FriendRequest");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendRequest",
                table: "FriendRequest",
                columns: new[] { "RequesterId", "RequestedId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendRequest",
                table: "FriendRequest");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FriendRequest",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendRequest",
                table: "FriendRequest",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_RequesterId_RequestedId",
                table: "FriendRequest",
                columns: new[] { "RequesterId", "RequestedId" },
                unique: true);
        }
    }
}
