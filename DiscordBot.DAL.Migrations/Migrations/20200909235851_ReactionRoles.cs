﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.DAL.Migrations.Migrations
{
    public partial class ReactionRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReactionRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    MessageId = table.Column<decimal>(nullable: false),
                    RoleId = table.Column<decimal>(nullable: false),
                    EmoteId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionRoles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReactionRoles");
        }
    }
}
