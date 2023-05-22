using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class YouTubeGroundWork : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YouTubeAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelID = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    YouTubeUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastVideoDateTime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YouTubeToAnnounce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserThumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoThumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoPublishedDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoURL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouTubeToAnnounce", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YouTubeAccounts");

            migrationBuilder.DropTable(
                name: "YouTubeToAnnounce");
        }
    }
}
