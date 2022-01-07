using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class ButtonRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ButtonRoleConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RoleId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ButtonId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButtonRoleConfigs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ButtonRoleConfigs");
        }
    }
}
