namespace asp_net_sql.Models;

public partial class EnumGameRoster
{
    public int ID { get; set; }

    public string Origin { get; set; } = null!;

    public int IDX { get; set; }

    public string Identity { get; set; } = null!;

    public virtual Chosen? Chosen { get; set; }

    public virtual ICollection<GameBoard> GameBoardCol1Navigations { get; set; } = new List<GameBoard>();

    public virtual ICollection<GameBoard> GameBoardCol2Navigations { get; set; } = new List<GameBoard>();

    public virtual ICollection<GameBoard> GameBoardCol3Navigations { get; set; } = new List<GameBoard>();
}
