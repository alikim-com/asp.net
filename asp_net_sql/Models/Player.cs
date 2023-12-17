using System.ComponentModel.DataAnnotations;

namespace asp_net_sql.Models;

internal enum Roster
{
    None,
    Human_One,
    Human_Two,
    AI_One,
    AI_Two
}

internal class Player
{
    [EnumDataType(typeof(Roster))]
    internal Roster RID { get; set; } = Roster.None;

    [Required]
    internal string Name { get; set; } = "None";
}
