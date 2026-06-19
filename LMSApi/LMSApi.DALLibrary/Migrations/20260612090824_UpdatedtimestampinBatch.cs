using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedtimestampinBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timestamp without time zone 'now'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CourseBatches",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrollmentStartDate",
                table: "CourseBatches",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrollmentEndDate",
                table: "CourseBatches",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "CourseBatches",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "timestamp without time zone 'now'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(3080), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(3080) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timestamp without time zone 'now'");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrollmentStartDate",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrollmentEndDate",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "timestamp without time zone 'now'");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(3340), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(3340) });
        }
    }
}
