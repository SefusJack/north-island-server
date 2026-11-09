using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorthIsland.Server.Data.Sql.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    login_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_key = table.Column<string>(type: "text", nullable: false),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_login_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    online_seconds = table.Column<long>(type: "bigint", nullable: false),
                    mall_points = table.Column<int>(type: "integer", nullable: false),
                    saved_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.login_id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    saved_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    login_id = table.Column<int>(type: "integer", nullable: false),
                    slot = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_key = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    map_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => new { x.login_id, x.slot });
                    table.ForeignKey(
                        name: "FK_characters_accounts_login_id",
                        column: x => x.login_id,
                        principalTable: "accounts",
                        principalColumn: "login_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_name_key",
                table: "accounts",
                column: "name_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_characters_name_key",
                table: "characters",
                column: "name_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "characters");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
