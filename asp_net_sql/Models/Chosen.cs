using System;
using System.Collections.Generic;

namespace asp_net_sql.Models;

public partial class Chosen
{
    public int RosterID { get; set; }

    public int? UISide { get; set; }

    public virtual EnumGameRoster Roster { get; set; } = null!;

    public virtual EnumUISide? UISideNavigation { get; set; }
}
