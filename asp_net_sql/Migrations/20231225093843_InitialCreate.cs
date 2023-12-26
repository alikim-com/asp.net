﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_sql.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EnumBtnMsg",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EBM_ID", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "EnumGameRoster",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false),
                Origin = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                IDX = table.Column<int>(type: "int", nullable: false),
                Identity = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EGR_ID", x => x.ID);
                table.CheckConstraint("CK_EGR_Origin", "Origin IN ('None', 'Human', 'AI')");
            });

        migrationBuilder.CreateTable(
            name: "EnumGameStates",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EGS_ID", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "EnumUISides",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EUS_ID", x => x.ID);
                table.CheckConstraint("CK_EUS_Value", "[Value] IN ('None', 'Left', 'Right')");
            });

        migrationBuilder.CreateTable(
            name: "MenuStrings",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ParentID = table.Column<int>(type: "int", nullable: true),
                IDX = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MnS_ID", x => x.ID);
                table.ForeignKey(
                    name: "FK_MnS_ID__MnS_ParentID",
                    column: x => x.ParentID,
                    principalTable: "MenuStrings",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateTable(
            name: "ResxStrings",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                Value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RxS_ID", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "GameBoard",
            columns: table => new
            {
                Row = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Col1 = table.Column<int>(type: "int", nullable: true),
                Col2 = table.Column<int>(type: "int", nullable: true),
                Col3 = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GBr_ID", x => x.Row);
                table.ForeignKey(
                    name: "FK_GBr_Col1__EGR_ID",
                    column: x => x.Col1,
                    principalTable: "EnumGameRoster",
                    principalColumn: "ID");
                table.ForeignKey(
                    name: "FK_GBr_Col2__EGR_ID",
                    column: x => x.Col2,
                    principalTable: "EnumGameRoster",
                    principalColumn: "ID");
                table.ForeignKey(
                    name: "FK_GBr_Col3__EGR_ID",
                    column: x => x.Col3,
                    principalTable: "EnumGameRoster",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                ID = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                State = table.Column<int>(type: "int", nullable: true),
                TurnWheelHead = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Gms_ID", x => x.ID);
                table.ForeignKey(
                    name: "FK_Gms_ID__EGS_Val",
                    column: x => x.State,
                    principalTable: "EnumGameStates",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateTable(
            name: "Chosen",
            columns: table => new
            {
                RosterID = table.Column<int>(type: "int", nullable: false),
                UISide = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Chn_RID", x => x.RosterID);
                table.ForeignKey(
                    name: "FK_EGR_ID__Chn_RID",
                    column: x => x.RosterID,
                    principalTable: "EnumGameRoster",
                    principalColumn: "ID");
                table.ForeignKey(
                    name: "FK_EUS_Val__Chn_USd",
                    column: x => x.UISide,
                    principalTable: "EnumUISides",
                    principalColumn: "ID");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Chosen_UISide",
            table: "Chosen",
            column: "UISide");

        migrationBuilder.CreateIndex(
            name: "UQ_EGR_Ent",
            table: "EnumGameRoster",
            columns: new[] { "Origin", "IDX" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GameBoard_Col1",
            table: "GameBoard",
            column: "Col1");

        migrationBuilder.CreateIndex(
            name: "IX_GameBoard_Col2",
            table: "GameBoard",
            column: "Col2");

        migrationBuilder.CreateIndex(
            name: "IX_GameBoard_Col3",
            table: "GameBoard",
            column: "Col3");

        migrationBuilder.CreateIndex(
            name: "IX_Games_State",
            table: "Games",
            column: "State");

        migrationBuilder.CreateIndex(
            name: "IX_MenuStrings_ParentID",
            table: "MenuStrings",
            column: "ParentID");

        SeedData(migrationBuilder);
    }

    protected partial void SeedData(MigrationBuilder migrationBuilder);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Chosen");

        migrationBuilder.DropTable(
            name: "EnumBtnMsg");

        migrationBuilder.DropTable(
            name: "GameBoard");

        migrationBuilder.DropTable(
            name: "Games");

        migrationBuilder.DropTable(
            name: "MenuStrings");

        migrationBuilder.DropTable(
            name: "ResxStrings");

        migrationBuilder.DropTable(
            name: "EnumUISides");

        migrationBuilder.DropTable(
            name: "EnumGameRoster");

        migrationBuilder.DropTable(
            name: "EnumGameStates");
    }
}