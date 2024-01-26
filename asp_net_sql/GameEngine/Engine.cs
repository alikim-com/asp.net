
using asp_net_sql.Models;

namespace asp_net_sql.GameEngine;

public enum Side
{
    None,
    Left,
    Right
}

internal enum Roster
{
    None,
    Human_1,
    Human_2,
    AI_1,
    AI_2
}

internal class Engine
{
    public static List<ChoiceItem> roster = [];

    /// <summary>
    /// Called from Game/Index.cshtml.cs.OnGet 
    /// after retrieving the list from the DB
    /// </summary>
    public static void SetRoster(List<EnumGameRoster> _AsyncDbSetItems)
    {
        roster.Clear();

        foreach(var itm in _AsyncDbSetItems)
        {
            var rosterName = itm.Origin != "None" ? $"{itm.Origin}_{itm.IDX}" : "None";
            if (!Enum.TryParse(typeof(Roster), rosterName, out object? rosterId))
                throw new Exception($"Engine.SetRoster : Roster.'{rosterName}' not found ");

            roster.Add(new ChoiceItem(
                (Roster)rosterId,
                Side.None,
                itm.Origin,
                itm.Identity,
                itm.ID));
        }
    }
}



class ChoiceItem
{
    internal bool chosen;

    public Roster RosterId { get; set; } = Roster.None;
    public string IdentityName { get; set; } = "";
    public Side SideId { get; set; } = Side.None;
    public string OriginType { get; set; } = "";

    internal int id;

    internal ChoiceItem(
        Roster _rosterId, 
        Side _side, 
        string _origin, 
        string _identity,
        int _id = 0)
    {
        chosen = false;
        RosterId = _rosterId;
        SideId = _side;
        OriginType = _origin;
        IdentityName = _identity;
        id = _id;
    }

    public ChoiceItem()
    {
        // for JsonSerializer.Deserialize<P>(input);
    }

}
