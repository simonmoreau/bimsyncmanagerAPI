using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace bimsyncManagerAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessToken = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PowerBiSecret = table.Column<string>(nullable: true),
                    RefreshDate = table.Column<string>(nullable: true),
                    RefreshToken = table.Column<string>(nullable: true),
                    TokenExpireIn = table.Column<int>(nullable: true),
                    TokenType = table.Column<string>(nullable: true),
                    bimsync_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
