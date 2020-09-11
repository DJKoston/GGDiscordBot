using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class Config2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeaveImage",
                table: "WelcomeMessageConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveMessage",
                table: "WelcomeMessageConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeImage",
                table: "WelcomeMessageConfigs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "WelcomeMessageConfigs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaveImage",
                table: "WelcomeMessageConfigs");

            migrationBuilder.DropColumn(
                name: "LeaveMessage",
                table: "WelcomeMessageConfigs");

            migrationBuilder.DropColumn(
                name: "WelcomeImage",
                table: "WelcomeMessageConfigs");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "WelcomeMessageConfigs");
        }
    }
}
