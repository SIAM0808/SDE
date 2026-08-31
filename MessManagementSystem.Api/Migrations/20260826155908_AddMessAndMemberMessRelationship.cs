using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessManagementSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMessAndMemberMessRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MessId",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Messes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessCode = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminMemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messes_Members_AdminMemberId",
                        column: x => x.AdminMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MessId",
                table: "Members",
                column: "MessId");

            migrationBuilder.CreateIndex(
                name: "IX_Messes_AdminMemberId",
                table: "Messes",
                column: "AdminMemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messes_MessCode",
                table: "Messes",
                column: "MessCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Messes_MessId",
                table: "Members",
                column: "MessId",
                principalTable: "Messes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Messes_MessId",
                table: "Members");

            migrationBuilder.DropTable(
                name: "Messes");

            migrationBuilder.DropIndex(
                name: "IX_Members_MessId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MessId",
                table: "Members");
        }
    }
}
