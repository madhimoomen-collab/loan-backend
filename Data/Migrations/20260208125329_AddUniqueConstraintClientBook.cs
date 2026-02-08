using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintClientBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientBooks_ClientId",
                table: "ClientBooks");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBooks_ClientId_BookId",
                table: "ClientBooks",
                columns: new[] { "ClientId", "BookId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientBooks_ClientId_BookId",
                table: "ClientBooks");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBooks_ClientId",
                table: "ClientBooks",
                column: "ClientId");
        }
    }
}
