using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarParking.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parking_spaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SpaceNumber = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_spaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parking_sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleType = table.Column<int>(type: "integer", nullable: false),
                    VehicleReg = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ParkingSpaceId = table.Column<int>(type: "integer", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parking_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parking_sessions_parking_spaces_ParkingSpaceId",
                        column: x => x.ParkingSpaceId,
                        principalTable: "parking_spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parking_sessions_ParkingSpaceId",
                table: "parking_sessions",
                column: "ParkingSpaceId",
                unique: true,
                filter: "\"TimeOut\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_parking_sessions_VehicleReg",
                table: "parking_sessions",
                column: "VehicleReg",
                unique: true,
                filter: "\"TimeOut\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_parking_spaces_SpaceNumber",
                table: "parking_spaces",
                column: "SpaceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parking_sessions");

            migrationBuilder.DropTable(
                name: "parking_spaces");
        }
    }
}
