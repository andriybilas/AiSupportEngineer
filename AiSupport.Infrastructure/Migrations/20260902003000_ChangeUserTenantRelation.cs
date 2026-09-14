using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AiSupport.Infrastructure.Migrations
{
    [DbContext(typeof(AiSupport.Infrastructure.DataBase.ApplicationDbContext))]
    [Migration("20260902003000_ChangeUserTenantRelation")]
    public partial class ChangeUserTenantRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the previous many-to-many join table between AppUser and AppService
            migrationBuilder.DropTable(
                name: "AppUserAppService");

            // Create new many-to-many join table between AppUser and Tenant
            migrationBuilder.CreateTable(
                name: "AppUserTenant",
                columns: table => new
                {
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate old AppUserAppService table
            migrationBuilder.DropTable(name: "AppUserTenant");

            migrationBuilder.CreateTable(
                name: "AppUserAppService",
                columns: table => new
                {
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserAppService", x => new { x.AppUserId, x.AppServiceId });
                    table.ForeignKey(
                        name: "FK_AppUserAppService_AppServices_AppServiceId",
                        column: x => x.AppServiceId,
                        principalTable: "AppServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserAppService_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserAppService_AppServiceId",
                table: "AppUserAppService",
                column: "AppServiceId");
        }
    }
}
