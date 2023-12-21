namespace asp_net_sql.Models;

public partial class ChildTable
{
    public int ChildID { get; set; }

    public virtual ParentTable Child { get; set; } = null!;
}
