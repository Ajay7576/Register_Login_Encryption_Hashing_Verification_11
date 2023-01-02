using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Register_Login_Otp.Migrations
{
    public partial class addOtptimeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OtpTime",
                table: "Registers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpTime",
                table: "Registers");
        }
    }
}
