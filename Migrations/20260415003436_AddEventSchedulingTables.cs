using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plan2Gather.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSchedulingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTimeSlots_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeSlotId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttendances_EventTimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "EventTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAttendances_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventAttendances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_EventId",
                table: "EventAttendances",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_TimeSlotId",
                table: "EventAttendances",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendances_UserId",
                table: "EventAttendances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTimeSlots_EventId",
                table: "EventTimeSlots",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAttendances");

            migrationBuilder.DropTable(
                name: "EventTimeSlots");
        }
    }
}
