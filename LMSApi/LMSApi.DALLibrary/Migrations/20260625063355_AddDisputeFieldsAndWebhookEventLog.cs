using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeFieldsAndWebhookEventLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisputeId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisputeStatus",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WebhookEventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    RawPayload = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessingError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEventLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5930), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(1930), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(1930) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookEventLogs");

            migrationBuilder.DropColumn(
                name: "DisputeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DisputeStatus",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(4400), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(4400) });
        }
    }
}
