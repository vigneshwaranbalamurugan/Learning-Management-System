using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminApprovalAndFinalizeRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Courses_CourseId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderPaymentId",
                table: "Payments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderOrderId",
                table: "Payments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "INR",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "FeeTypeSnapshot",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeValueSnapshot",
                table: "Payments",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InstructorAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeeAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PlatformFeeConfigId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "Enrollments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstructorPayoutAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstructorId = table.Column<int>(type: "integer", nullable: false),
                    RazorpayLinkedAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IfscCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BankName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorPayoutAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorPayoutAccounts_Users_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformFeeConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeeCategory = table.Column<int>(type: "integer", nullable: false),
                    FeeType = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformFeeConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformFeeConfigs_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstructorPayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    InstructorId = table.Column<int>(type: "integer", nullable: false),
                    InstructorPayoutAccountId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RazorpayPayoutId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RazorpayFundAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorPayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorPayouts_InstructorPayoutAccounts_InstructorPayout~",
                        column: x => x.InstructorPayoutAccountId,
                        principalTable: "InstructorPayoutAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstructorPayouts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstructorPayouts_Users_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(490), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(490) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PlatformFeeConfigId",
                table: "Payments",
                column: "PlatformFeeConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderOrderId",
                table: "Payments",
                column: "ProviderOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_PaymentId",
                table: "Enrollments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayoutAccounts_InstructorId_IsActive",
                table: "InstructorPayoutAccounts",
                columns: new[] { "InstructorId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayouts_InstructorId",
                table: "InstructorPayouts",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayouts_InstructorPayoutAccountId",
                table: "InstructorPayouts",
                column: "InstructorPayoutAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayouts_PaymentId",
                table: "InstructorPayouts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorPayouts_RazorpayPayoutId",
                table: "InstructorPayouts",
                column: "RazorpayPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFeeConfigs_CreatedByAdminId",
                table: "PlatformFeeConfigs",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformFeeConfigs_FeeCategory_EffectiveFrom",
                table: "PlatformFeeConfigs",
                columns: new[] { "FeeCategory", "EffectiveFrom" });

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Payments_PaymentId",
                table: "Enrollments",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Courses_CourseId",
                table: "Payments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PlatformFeeConfigs_PlatformFeeConfigId",
                table: "Payments",
                column: "PlatformFeeConfigId",
                principalTable: "PlatformFeeConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Payments_PaymentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Courses_CourseId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PlatformFeeConfigs_PlatformFeeConfigId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "InstructorPayouts");

            migrationBuilder.DropTable(
                name: "PlatformFeeConfigs");

            migrationBuilder.DropTable(
                name: "InstructorPayoutAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PlatformFeeConfigId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ProviderOrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_PaymentId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "FeeTypeSnapshot",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FeeValueSnapshot",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InstructorAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformFeeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformFeeConfigId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Enrollments");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderPaymentId",
                table: "Payments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderOrderId",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "INR");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(6180), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(6180) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Courses_CourseId",
                table: "Payments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
