using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asp_net_sql.Migrations;

public partial class InitialCreate
{
    /// <inheritdoc />
    protected partial void SeedData(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
        table: "EnumBtnMsg",
        columns: new[] { "ID", "Value" },
        values: new object[,] { 
            { 0, "Ready_AI" },
            { 1, "Ready_Human" },
            { 2, "Ready_Mix" },
            { 3, "Both_Missing" },
            { 4, "Left_Missing" },
            { 5, "Right_Missing" },
        });

        migrationBuilder.InsertData(
        table: "ResxStrings",
        columns: new[] { "Name", "Value" },
        values: new object[,] {
            { "Ready_AI", "I like to watch...o_o" },
            { "Ready_Human", "Fight, Mortals!" },
            { "Ready_Mix", "For the Organics!" },
            { "Both_Missing", "Choose players" },
            { "Left_Missing", "Choose left player" },
            { "Right_Missing", "Choose right player" },
        });

        migrationBuilder.InsertData(
        table: "EnumGameStates",
        columns: new[] { "ID", "Value" },
        values: new object[,] {
            { 0, "Countdown" },
            { 1, "Started" },
            { 2, "Won" },
            { 3, "Tie" },
        });

        migrationBuilder.InsertData(
        table: "EnumGameRoster",
        columns: new[] { "ID", "Origin", "IDX", "Identity" },
        values: new object[,] {
            { 0, "None", 1, "None" },
            { 1, "Human", 1, "Ironheart" },
            { 2, "Human", 2, "Silverlight" },
            { 3, "AI", 1, "Quantum" },
            { 4, "AI", 2, "Syncstorm" },
        });

        migrationBuilder.InsertData(
        table: "GameBoard",
        columns: new[] { "Col1", "Col2", "Col3" },
        values: new object[,] {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
        });

        migrationBuilder.InsertData(
        table: "MenuStrings",
        columns: new[] { "ParentID", "IDX", "Value" },
        values: new object[,] {
            { null, 1, "Load" },
            { null, 2, "Save" },
            { null, 3, "Help" },
            { null, 4, "Game name:" },

            { 1, 1, "Open saved game..." },
            { 1, 2, "Saved games" },

            { 2, 1, "Save game as..." },
            { 3, 1, "About" },
        });

        migrationBuilder.InsertData(
        table: "ResxStrings",
        columns: new[] { "Name", "Value" },
        values: new object[,] {
            { "Title", "Tic-Tac-Toe" },
            { "DefaultGameName", "Default" },
            { "MenuHelpAboutButton", "Leave the narrative" },
            { "MenuHelpAboutContent",
"""
About
***
Boop The Snoop For Fun And Profit is an open - source initiative aimed at finding Uncle Serge a job. We believe in the power of collaboration and community-driven development.


Open Source
***
This project is released under an open-source MIT license.
This means that the source code is freely available for inspection, modification, and distribution.You can use it, contribute to it, and even fork it to create your own version.


Disclaimer
***
While we strive to maintain the quality and security of our software, it is important to note that the software is provided "as is," without warranty of any kind.Users are encouraged to use it at their own risk.We do not make any guarantees regarding its fitness for a particular purpose, and we are not liable for any physical or mental damage or loss incurred through its use.

We welcome contributions from the community to help improve this project.
If you choose to contribute, please contact us on
https://github.com/alikim-com/tafe 
for more information.

Thank you for being part of our open - source community!
"""
            },
        });

    }
}