using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class InitialLiveDatabaseCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "XP",
                table: "Profiles",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Gold",
                table: "Profiles",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "XP",
                table: "Profiles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<long>(
                name: "Gold",
                table: "Profiles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
