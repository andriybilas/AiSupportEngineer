using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AiSupport.Infrastructure.Migrations
{
    [DbContext(typeof(AiSupport.Infrastructure.DataBase.ApplicationDbContext))]
    [Migration("20260902002000_AddTenant")]
    public partial class AddTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AppServices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppServices_TenantId",
                table: "AppServices",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppServices_Tenants_TenantId",
                table: "AppServices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_AppServices_Tenants_TenantId", table: "AppServices");
            migrationBuilder.DropIndex(name: "IX_AppServices_TenantId", table: "AppServices");
            migrationBuilder.DropColumn(name: "TenantId", table: "AppServices");
            migrationBuilder.DropTable(name: "Tenants");
        }
    }
}
