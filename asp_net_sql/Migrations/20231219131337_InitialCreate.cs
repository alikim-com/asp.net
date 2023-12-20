using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_sql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableRosterEnum",
                columns: table => new
                {
                    RosterEnum = table.Column<int>(type: "int", nullable: false),
                    UniColumn = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRosterEnum", x => x.RosterEnum);
                });

            migrationBuilder.CreateTable(
                name: "TableRoster",
                columns: table => new
                {
                    RId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableRoster", x => x.RId);
                    table.ForeignKey(
                        name: "FK_TableRoster_TableRosterEnum_RId",
                        column: x => x.RId,
                        principalTable: "TableRosterEnum",
                        principalColumn: "RosterEnum",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableRosterEnum_UniColumn",
                table: "TableRosterEnum",
                column: "UniColumn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableRoster");

            migrationBuilder.DropTable(
                name: "TableRosterEnum");
        }
    }
}
