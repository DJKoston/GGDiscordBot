using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class ButtonRole2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiveRemove",
                table: "ButtonRoleConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiveRemove",
                table: "ButtonRoleConfigs");
        }
    }
}
