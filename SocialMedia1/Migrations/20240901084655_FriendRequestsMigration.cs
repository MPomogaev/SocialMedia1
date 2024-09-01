using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SocialMedia1.Migrations
{
    /// <inheritdoc />
    public partial class FriendRequestsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Account_FriendId",
                table: "Friends");

            migrationBuilder.CreateTable(
                name: "FriendRequestStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequestStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FriendRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    RequestedId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendRequest_Account_RequestedId",
                        column: x => x.RequestedId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendRequest_Account_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendRequest_FriendRequestStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FriendRequestStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FriendRequestStatus",
                columns: new[] { "Id", "Status" },
                values: new object[,]
                {
                    { 1, "unanswered" },
                    { 2, "rejected" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_RequestedId",
                table: "FriendRequest",
                column: "RequestedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_RequesterId_RequestedId",
                table: "FriendRequest",
                columns: new[] { "RequesterId", "RequestedId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_StatusId",
                table: "FriendRequest",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Account_FriendId",
                table: "Friends",
                column: "FriendId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Account_FriendId",
                table: "Friends");

            migrationBuilder.DropTable(
                name: "FriendRequest");

            migrationBuilder.DropTable(
                name: "FriendRequestStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Account_FriendId",
                table: "Friends",
                column: "FriendId",
                principalTable: "Account",
                principalColumn: "Id");
        }
    }
}
