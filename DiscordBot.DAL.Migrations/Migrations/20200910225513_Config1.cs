using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class Config1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NitroBoosterConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildId = table.Column<decimal>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NitroBoosterConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WelcomeMessageConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeMessageConfigs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NitroBoosterConfigs");

            migrationBuilder.DropTable(
                name: "WelcomeMessageConfigs");
        }
    }
}
