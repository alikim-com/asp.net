namespace asp_net_sql.Models;

public partial class ParentTable
{
    public int ParentID { get; set; }

    public int UniColumn { get; set; }

    public virtual ChildTable? ChildTable { get; set; }
}
