using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessManagementSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixMessMemberRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MessId1",
                table: "MessJoinRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessJoinRequests_MessId1",
                table: "MessJoinRequests",
                column: "MessId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MessJoinRequests_Messes_MessId1",
                table: "MessJoinRequests",
                column: "MessId1",
                principalTable: "Messes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessJoinRequests_Messes_MessId1",
                table: "MessJoinRequests");

            migrationBuilder.DropIndex(
                name: "IX_MessJoinRequests_MessId1",
                table: "MessJoinRequests");

            migrationBuilder.DropColumn(
                name: "MessId1",
                table: "MessJoinRequests");
        }
    }
}
