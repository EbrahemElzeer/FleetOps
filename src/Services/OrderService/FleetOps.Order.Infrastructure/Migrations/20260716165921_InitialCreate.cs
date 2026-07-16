using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOps.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PickupCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupGovernorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PickupStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PickupBuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PickupLandmark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PickupLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    PickupLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DeliveryCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryGovernorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeliveryBuildingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryLandmark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DeliveryLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryFailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<int>(type: "int", nullable: true),
                    DeliveryFailureNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DriverId",
                table: "Orders",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TrackingNumber",
                table: "Orders",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
