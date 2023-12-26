namespace asp_net_sql.Models;

public partial class GameBoard
{
    public int Row { get; set; }

    public int? Col1 { get; set; }

    public int? Col2 { get; set; }

    public int? Col3 { get; set; }

    public virtual EnumGameRoster? Col1Navigation { get; set; }

    public virtual EnumGameRoster? Col2Navigation { get; set; }

    public virtual EnumGameRoster? Col3Navigation { get; set; }
}
