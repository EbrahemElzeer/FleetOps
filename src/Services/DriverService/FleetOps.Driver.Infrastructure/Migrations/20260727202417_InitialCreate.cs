using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOps.Driver.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    VehiclePlateNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WentOnlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WentOfflineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TripStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturningStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspensionReason = table.Column<int>(type: "int", nullable: true),
                    SuspensionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_PhoneNumber",
                table: "Drivers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_VehiclePlateNumber",
                table: "Drivers",
                column: "VehiclePlateNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivers");
        }
    }
}
