using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class NowLive3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreamGame",
                table: "NowLiveMessages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreamTitle",
                table: "NowLiveMessages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreamGame",
                table: "NowLiveMessages");

            migrationBuilder.DropColumn(
                name: "StreamTitle",
                table: "NowLiveMessages");
        }
    }
}
