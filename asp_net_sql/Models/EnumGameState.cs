namespace asp_net_sql.Models;

public partial class EnumGameState
{
    public int ID { get; set; }

    public string Value { get; set; } = null!;

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
