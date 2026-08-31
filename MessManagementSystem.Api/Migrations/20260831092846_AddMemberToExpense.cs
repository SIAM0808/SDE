using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessManagementSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_MemberId",
                table: "Expenses",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Members_MemberId",
                table: "Expenses",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Members_MemberId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_MemberId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Expenses");
        }
    }
}
