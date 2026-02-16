using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDatabaseSchemaIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientBooks_ClientId_BookId",
                table: "ClientBooks");

            migrationBuilder.AddColumn<int>(
                name: "ClientBookId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientBookId",
                table: "Reservations",
                column: "ClientBookId",
                unique: true,
                filter: "[ClientBookId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBooks_ClientId",
                table: "ClientBooks",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBooks_ClientId_BookId_IsReturned",
                table: "ClientBooks",
                columns: new[] { "ClientId", "BookId", "IsReturned" },
                filter: "[IsReturned] = 0 AND [IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ClientBooks_ClientBookId",
                table: "Reservations",
                column: "ClientBookId",
                principalTable: "ClientBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ClientBooks_ClientBookId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ClientBookId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ClientBooks_ClientId",
                table: "ClientBooks");

            migrationBuilder.DropIndex(
                name: "IX_ClientBooks_ClientId_BookId_IsReturned",
                table: "ClientBooks");

            migrationBuilder.DropColumn(
                name: "ClientBookId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBooks_ClientId_BookId",
                table: "ClientBooks",
                columns: new[] { "ClientId", "BookId" },
                unique: true);
        }
    }
}
