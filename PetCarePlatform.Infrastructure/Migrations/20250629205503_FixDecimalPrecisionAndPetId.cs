using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCarePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecisionAndPetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Pets_PetId1",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_AspNetUsers_UserId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Bookings_BookingId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PetId1",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PetId1",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_UserId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_BookingId",
                table: "Payments",
                newName: "IX_Payments_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UserId",
                table: "Payment",
                newName: "IX_Payment_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BookingId",
                table: "Payment",
                newName: "IX_Payment_BookingId");

            migrationBuilder.AddColumn<int>(
                name: "PetId1",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PetId1",
                table: "Bookings",
                column: "PetId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Pets_PetId1",
                table: "Bookings",
                column: "PetId1",
                principalTable: "Pets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ServiceProviders_ServiceProviderId",
                table: "Bookings",
                column: "ServiceProviderId",
                principalTable: "ServiceProviders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_AspNetUsers_UserId",
                table: "Payment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Bookings_BookingId",
                table: "Payment",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
