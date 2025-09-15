using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tae_app.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsForMemberFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_PhoneNumber",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_EmiratesId",
                table: "Members",
                column: "EmiratesId",
                unique: true,
                filter: "[EmiratesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PhoneNumber",
                table: "Members",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_EmiratesId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_PhoneNumber",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PhoneNumber",
                table: "Members",
                column: "PhoneNumber");
        }
    }
}
