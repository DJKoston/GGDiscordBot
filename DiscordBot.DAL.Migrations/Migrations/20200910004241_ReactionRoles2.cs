using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class ReactionRoles2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnicodeEmote",
                table: "ReactionRoles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnicodeEmote",
                table: "ReactionRoles");
        }
    }
}
