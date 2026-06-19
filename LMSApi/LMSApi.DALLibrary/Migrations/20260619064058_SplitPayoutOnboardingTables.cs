using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SplitPayoutOnboardingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstructorLinkedAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorId = table.Column<int>(type: "integer", nullable: false),
                    RazorpayAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LegalBusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Street1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Street2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Pan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Gst = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    ProfileCategory = table.Column<string>(type: "text", nullable: false),
                    ProfileSubcategory = table.Column<string>(type: "text", nullable: false),
                    AccountStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "created"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorLinkedAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorLinkedAccounts_Users_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstructorPayoutProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorLinkedAccountId = table.Column<int>(type: "integer", nullable: false),
                    RazorpayProductId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IfscCode = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    BeneficiaryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TncAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    ProductStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "requested"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorPayoutProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorPayoutProducts_InstructorLinkedAccounts_Instructo~",
                        column: x => x.InstructorLinkedAccountId,
                        principalTable: "InstructorLinkedAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstructorStakeholders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorLinkedAccountId = table.Column<int>(type: "integer", nullable: false),
                    RazorpayStakeholderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorStakeholders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorStakeholders_InstructorLinkedAccounts_InstructorL~",
                        column: x => x.InstructorLinkedAccountId,
                        principalTable: "InstructorLinkedAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9880), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(6570), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(6570) });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorLinkedAccounts_InstructorId",
                table: "InstructorLinkedAccounts",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorLinkedAccounts_RazorpayAccountId",
                table: "InstructorLinkedAccounts",
                column: "RazorpayAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayoutProducts_InstructorLinkedAccountId",
                table: "InstructorPayoutProducts",
                column: "InstructorLinkedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayoutProducts_RazorpayProductId",
                table: "InstructorPayoutProducts",
                column: "RazorpayProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorStakeholders_InstructorLinkedAccountId",
                table: "InstructorStakeholders",
                column: "InstructorLinkedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorStakeholders_RazorpayStakeholderId",
                table: "InstructorStakeholders",
                column: "RazorpayStakeholderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstructorPayoutProducts");

            migrationBuilder.DropTable(
                name: "InstructorStakeholders");

            migrationBuilder.DropTable(
                name: "InstructorLinkedAccounts");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5340), new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5340) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5350), new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5350) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5350), new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(5350) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(2000), new DateTime(2026, 6, 18, 16, 2, 38, 152, DateTimeKind.Utc).AddTicks(2010) });
        }
    }
}
