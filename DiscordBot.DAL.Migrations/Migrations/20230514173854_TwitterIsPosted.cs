using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class TwitterIsPosted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPosted",
                table: "Tweets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPosted",
                table: "Tweets");
        }
    }
}
