using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class YouTubeGroundWork4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "YouTubeToAnnounce",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "YouTubeToAnnounce");
        }
    }
}
