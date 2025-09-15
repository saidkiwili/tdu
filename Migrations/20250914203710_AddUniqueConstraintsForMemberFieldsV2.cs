using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tae_app.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsForMemberFieldsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_EmiratesId",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_EmiratesId",
                table: "Members",
                column: "EmiratesId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_EmiratesId",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_EmiratesId",
                table: "Members",
                column: "EmiratesId",
                unique: true,
                filter: "[EmiratesId] IS NOT NULL");
        }
    }
}
