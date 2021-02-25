using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerApi.Migrations
{
    public partial class Migr6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUserText",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUser",
                table: "Messages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverUser",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverUserText",
                table: "Messages",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
