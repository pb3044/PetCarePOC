using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCarePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceProviderId1Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ServiceProviderId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceProviderId",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceProviderId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ServiceProviderId",
                table: "Bookings",
                column: "ServiceProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
