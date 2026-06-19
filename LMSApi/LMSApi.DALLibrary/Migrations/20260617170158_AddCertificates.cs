using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TemplateBackgroundUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TemplateMarkDown = table.Column<string>(type: "text", nullable: false),
                    AspectRatioWidth = table.Column<int>(type: "integer", nullable: false, defaultValue: 16),
                    AspectRatioHeight = table.Column<int>(type: "integer", nullable: false, defaultValue: 9),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CertificateTemplateId = table.Column<int>(type: "integer", nullable: false),
                    CertificateImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificateTemplates_CertificateTemplateId",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 570, DateTimeKind.Utc).AddTicks(7500), new DateTime(2026, 6, 17, 17, 1, 57, 570, DateTimeKind.Utc).AddTicks(7500) });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateId",
                table: "Certificates",
                column: "CertificateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateTemplateId",
                table: "Certificates",
                column: "CertificateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UserId_CourseId",
                table: "Certificates",
                columns: new[] { "UserId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CertificateTemplates");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6900), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(3650), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(3650) });
        }
    }
}
