using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Register_Login_Otp.Migrations
{
    public partial class AddregTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Verified",
                table: "Registers",
                newName: "isVerified");

            migrationBuilder.AlterColumn<string>(
                name: "OTP",
                table: "Registers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "mailTime",
                table: "Registers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mailTime",
                table: "Registers");

            migrationBuilder.RenameColumn(
                name: "isVerified",
                table: "Registers",
                newName: "Verified");

            migrationBuilder.UpdateData(
                table: "Registers",
                keyColumn: "OTP",
                keyValue: null,
                column: "OTP",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OTP",
                table: "Registers",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
