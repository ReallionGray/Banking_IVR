using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Banking_IVR.Persistence.Migrations
{
    public partial class InitialUssdSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USSD_Setting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    MSISDN = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USSD_Setting", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USSD_Setting_MSISDN",
                table: "USSD_Setting",
                column: "MSISDN",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USSD_Setting");
        }
    }
}
