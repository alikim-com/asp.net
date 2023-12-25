using System;
using System.Collections.Generic;

namespace asp_net_sql.Models;

public partial class MenuString
{
    public int ID { get; set; }

    public int? ParentID { get; set; }

    public int IDX { get; set; }

    public string? Value { get; set; }

    public virtual ICollection<MenuString> InverseParent { get; set; } = new List<MenuString>();

    public virtual MenuString? Parent { get; set; }
}
