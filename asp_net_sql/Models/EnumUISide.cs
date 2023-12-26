namespace asp_net_sql.Models;

public partial class EnumUISide
{
    public int ID { get; set; }

    public string Value { get; set; } = null!;

    public virtual ICollection<Chosen> Chosens { get; set; } = new List<Chosen>();
}
