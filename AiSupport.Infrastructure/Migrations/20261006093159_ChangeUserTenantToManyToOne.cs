using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiSupport.Infrastructure.Migrations
{
    /// <summary>
    /// Changes AppUser-AppTenant from many-to-many (AppUserTenant join table) to many-to-one
    /// (AspNetUsers.TenantId), preserving existing memberships:
    /// - a user with several tenants keeps the earliest-created one (ties: Name, Id);
    /// - a user with no tenant is assigned to the earliest-created tenant;
    /// - if users exist but no tenant exists at all, the migration fails instead of dropping data.
    /// </summary>
    public partial class ChangeUserTenantToManyToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "AspNetUsers" AS u
                SET "TenantId" = picked."TenantId"
                FROM (
                    SELECT DISTINCT ON (ut."AppUserId") ut."AppUserId", ut."TenantId"
                    FROM "AppUserTenant" AS ut
                    INNER JOIN "Tenants" AS t ON t."Id" = ut."TenantId"
                    ORDER BY ut."AppUserId", t."CreatedDateTime", t."Name", t."Id"
                ) AS picked
                WHERE u."Id" = picked."AppUserId";
                """);

            migrationBuilder.Sql(
                """
                UPDATE "AspNetUsers"
                SET "TenantId" = (
                    SELECT t."Id"
                    FROM "Tenants" AS t
                    ORDER BY t."CreatedDateTime", t."Name", t."Id"
                    LIMIT 1)
                WHERE "TenantId" IS NULL;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "TenantId" IS NULL) THEN
                        RAISE EXCEPTION 'ChangeUserTenantToManyToOne: users exist but there is no tenant to assign them to. Create a tenant first.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "AppUserTenant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUserTenant",
                columns: table => new
                {
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserTenant", x => new { x.AppUserId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_AppUserTenant_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserTenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserTenant_TenantId",
                table: "AppUserTenant",
                column: "TenantId");

            migrationBuilder.Sql(
                """
                INSERT INTO "AppUserTenant" ("AppUserId", "TenantId")
                SELECT u."Id", u."TenantId"
                FROM "AspNetUsers" AS u;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");
        }
    }
}
