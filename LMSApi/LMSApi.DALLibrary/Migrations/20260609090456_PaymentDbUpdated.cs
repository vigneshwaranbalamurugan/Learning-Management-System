using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class PaymentDbUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RazorpayPaymentId",
                table: "Payments",
                newName: "ProviderPaymentId");

            migrationBuilder.RenameColumn(
                name: "RazorpayOrderId",
                table: "Payments",
                newName: "ProviderOrderId");

            migrationBuilder.AddColumn<int>(
                name: "AttachmentType",
                table: "AssignmentSubmissions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Review = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishLists_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishLists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(4220), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(4220) });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CourseId",
                table: "Reviews",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_CourseId",
                table: "Reviews",
                columns: new[] { "UserId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishLists_CourseId",
                table: "WishLists",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_WishLists_UserId_CourseId",
                table: "WishLists",
                columns: new[] { "UserId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "WishLists");

            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "AssignmentSubmissions");

            migrationBuilder.RenameColumn(
                name: "ProviderPaymentId",
                table: "Payments",
                newName: "RazorpayPaymentId");

            migrationBuilder.RenameColumn(
                name: "ProviderOrderId",
                table: "Payments",
                newName: "RazorpayOrderId");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 493, DateTimeKind.Utc).AddTicks(8630), new DateTime(2026, 6, 8, 9, 13, 28, 493, DateTimeKind.Utc).AddTicks(8630) });
        }
    }
}
