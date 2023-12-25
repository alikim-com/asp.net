using System;
using System.Collections.Generic;

namespace asp_net_sql.Models;

public partial class Game
{
    public string ID { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? State { get; set; }

    public int TurnWheelHead { get; set; }

    public virtual EnumGameState? StateNavigation { get; set; }
}
