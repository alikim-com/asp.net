using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace asp_net_sql.Models;

public enum Roster
{
    None,
    One,
    Two,
    Three,
    Four
}

[Index(nameof(UniColumn), IsUnique = true)]
internal class RowRosterEnum
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [EnumDataType(typeof(Roster))]
    public Roster RosterEnum { get; set; }

    public Roster UniColumn { get; set; }
}

internal class RowRoster
{
    [Key]
    [ForeignKey(nameof(RowRosterEnum))]
    public Roster RId { get; set; }

    public string? Name { get; set; }

    public RowRosterEnum RowRosterEnum { get; set; }

}
