using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsepMVC.Migrations
{
    /// <inheritdoc />
    public partial class denemeetkinlikkatilim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventParticipations",
                columns: table => new
                {
                    EventParticipationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParticipationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipations", x => x.EventParticipationId);
                    table.ForeignKey(
                        name: "FK_EventParticipations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventParticipations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_EventId",
                table: "EventParticipations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_UserId",
                table: "EventParticipations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventParticipations");
        }
    }
}
