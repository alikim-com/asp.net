
using System.Drawing;
using asp_net_sql.GameEnginePort;

namespace asp_net_sql.GameEngine;

partial class SetupForm
{
    // ENGINE PORT

    Control panelLeft = new();
    Control panelRight = new();
    Label identityLeft;
    Label allegLeft;
    Label headerLeft;
    Label toolStripLabel;
    Size ClientSize;

    // ---------

    static readonly UIColors.ColorTheme theme = UIColors.Steel;

    readonly Color foreLeft, foreRight, foreLeftDim, foreRightDim;

    static internal readonly List<ChoiceItem> roster = new();

    readonly ButtonToolStripRenderer buttonRenderer;

    enum BtnMessage
    {
        Ready_AI,
        Ready_Human,
        Ready_Mix,
        Both_Missing,
        Left_Missing,
        Right_Missing,
    }

    static Label AddLabel(Control parent, Point loc, string name, int tab, string text)
    {
        var label = new Label
        {
            AutoSize = true,
            Location = loc,
            Name = name,
            Size = new Size(62, 15),
            TabIndex = tab,
            Text = text
        };

        parent.Controls.Add(label);

        return label;
    }

    void CreateChoiceLists()
    {
        // left side

        int tab = -1;
        Point locId = identityLeft.Location;
        int allegX = allegLeft.Location.X + allegLeft.Width;
        locId.Y += 15;

        foreach (var (rostItem, identity) in Game.rosterIdentity)
        {
            tab++;
            locId.Y += 25;

            var labName = AddLabel(panelLeft, locId, $"{identity}{tab}", tab, identity);
            labName.Font = UIFonts.regular;
            labName.Cursor = Cursors.Hand;

            var alleg = rostItem.ToString().Split('_')[0];
            var labAlleg = AddLabel(panelLeft, Point.Empty, $"{alleg}{tab}", tab, alleg);
            labAlleg.Font = UIFonts.tiny;

            labAlleg.Location = new Point(
                allegX - labAlleg.Width,
                labName.Location.Y + (labName.Height - labAlleg.Height) / 2
            );

            roster.Add(new(rostItem, Side.Left, labAlleg.Text, labName.Text));
        }

        // right side

        static Label AddMirrorLabel(Control parent, Label src, string name, int tab, string text)
        {
            var lab = AddLabel(parent, Point.Empty, name, tab, text);
            lab.Font = src.Font;

            lab.Location = new Point(
                parent.Width - lab.Width - src.Location.X,
                src.Location.Y
            );

            lab.Cursor = src.Cursor;

            return lab;
        }

        tab = 0;
        AddMirrorLabel(panelRight, headerLeft, "headerRight", tab++, "Right Player");
        AddMirrorLabel(panelRight, allegLeft, "allegRight", tab++, "Origin");
        AddMirrorLabel(panelRight, identityLeft, "identityRight", tab++, "Identity");

        var rosterLeft = roster.ToList();
        foreach (var choiceItem in rosterLeft)
        {
           // var (origLab, identLab) = choiceItem;
          //  var _origLab = AddMirrorLabel(panelRight, origLab, "Right" + origLab.Name, tab++, origLab.Text);
         //   var _identLab = AddMirrorLabel(panelRight, identLab, identLab.Name.Replace("Left", "Right"), tab++, identLab.Text);

          //  roster.Add(new(choiceItem.RosterId, Side.Right, _origLab.Text, _identLab.Text));
        }

        foreach (var rec in roster)
            MakeOnClickHandler(rec, roster);
    }

    void MakeOnClickHandler(ChoiceItem choiceItem, List<ChoiceItem> roster)
    {
        EventHandler handler = (object? sender, EventArgs e) =>
        {
            var side = choiceItem.SideId;
            var rosterThisSide = roster.Where(itm => itm.SideId == side && itm != choiceItem);
            var rosterOtherSide = roster.Where(itm => itm.SideId != side);

            var (panelThisSide, panelOtherSide) = side == Side.Left ? (panelLeft, panelRight) : (panelRight, panelLeft);

            panelThisSide.BackgroundImage = GetBackgroundImage(choiceItem.RosterId, side);

            choiceItem.chosen = true;
           // choiceItem.Activate();

            foreach (var rec in rosterThisSide)
            {
                rec.chosen = false;
             //   rec.Deactivate();
            }

            var rosterId = choiceItem.RosterId;

            var mirrorItem = rosterOtherSide.FirstOrDefault(itm => itm.chosen && itm.RosterId == rosterId);

            if (mirrorItem != null)
            {
                panelOtherSide.BackgroundImage = null;

                mirrorItem.chosen = false;
               // foreach (var rec in rosterOtherSide) rec.Activate();
            }

            // update button messages

            var thisChosen = choiceItem;
            var otherChosen = rosterOtherSide.FirstOrDefault(itm => itm.chosen);

            if (thisChosen != null && otherChosen != null)
            {
                if (thisChosen.OriginType != otherChosen.OriginType)
                {
                    UpdateButton(BtnMessage.Ready_Mix);

                } else if (Enum.TryParse($"Ready_{thisChosen.OriginType}", out BtnMessage msg))
                {
                    UpdateButton(msg);
                }

            } else
            {
                var missingSide = side == Side.Left ? BtnMessage.Right_Missing : BtnMessage.Left_Missing;
                UpdateButton(missingSide);
            }

        };

      //  choiceItem.SetOnClickHandler(handler);
    }

    void UpdateButton(BtnMessage msg)
    {
        Dictionary<BtnMessage, string> buttonMessages = new() {
            { BtnMessage.Ready_AI, "I like to watch...o_o" },
            { BtnMessage.Ready_Human, "Fight, Mortals!" },
            { BtnMessage.Ready_Mix, "For the Organics!" },
            { BtnMessage.Both_Missing, "Choose players" },
            { BtnMessage.Left_Missing, "Choose left player" },
            { BtnMessage.Right_Missing, "Choose right player" },
        };

        toolStripLabel.Text = buttonMessages[msg];
        buttonRenderer.Disabled = msg.ToString().Contains("Missing");
    }

    static Image? GetBackgroundImage(Roster rosterId, Side side)
    {
        var imageName = $"{rosterId}_{side}";
        return (Image?)Resource.ResourceManager.GetObject(imageName);
    }

    private void ToolStrip_SizeChanged(object sender, EventArgs e)
    {
        if (sender is Control ctrl) ctrl.Location = new((ClientSize.Width - ctrl.Width) / 2, ctrl.Location.Y);
    }

    private void ToolStrip_Click(object sender, EventArgs e)
    {
        if (buttonRenderer.Disabled) return;
        //DialogResult = DialogResult.OK;
    }
    private void ToolStrip_MouseEnter(object sender, EventArgs e) => buttonRenderer.SetOverState(sender, true);
    private void ToolStrip_MouseLeave(object sender, EventArgs e) => buttonRenderer.SetOverState(sender, false);
}


