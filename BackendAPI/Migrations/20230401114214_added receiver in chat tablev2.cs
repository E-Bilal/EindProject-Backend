using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class addedreceiverinchattablev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationFriendId",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ApplicationFriendId",
                table: "Chats",
                column: "ApplicationFriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_ApplicationFriendId",
                table: "Chats",
                column: "ApplicationFriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_ApplicationFriendId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_ApplicationFriendId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ApplicationFriendId",
                table: "Chats");
        }
    }
}
