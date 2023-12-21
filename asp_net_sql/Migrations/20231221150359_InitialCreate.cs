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
                name: "ParentTable",
                columns: table => new
                {
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    UniColumn = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ParentID", x => x.ParentID);
                    table.UniqueConstraint("AK_ParentTable_UniColumn", x => x.UniColumn);
                });

            migrationBuilder.CreateTable(
                name: "ChildTable",
                columns: table => new
                {
                    ChildID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_ChildID", x => x.ChildID);
                    table.ForeignKey(
                        name: "FK_ChildID__PT_UniColumn",
                        column: x => x.ChildID,
                        principalTable: "ParentTable",
                        principalColumn: "UniColumn");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_PT_UniColumn",
                table: "ParentTable",
                column: "UniColumn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildTable");

            migrationBuilder.DropTable(
                name: "ParentTable");
        }
    }
}
