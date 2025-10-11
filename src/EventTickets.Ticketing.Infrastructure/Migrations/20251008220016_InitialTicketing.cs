using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventTickets.Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTicketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ticketing");

            migrationBuilder.CreateTable(
                name: "performance_inventory",
                schema: "ticketing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Reserved = table.Column<int>(type: "integer", nullable: false),
                    Sold = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_inventory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "ticketing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservations_PerformanceId",
                schema: "ticketing",
                table: "reservations",
                column: "PerformanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "performance_inventory",
                schema: "ticketing");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "ticketing");
        }
    }
}
