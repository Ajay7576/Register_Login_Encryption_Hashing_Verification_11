using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Register_Login_Otp.Migrations
{
    public partial class abc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OTP",
                table: "Registers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTP",
                table: "Registers");
        }
    }
}
