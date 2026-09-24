using BaseLib.Abstracts;
using Godot;

namespace CSharpSBP.SBP_Code;

public class SBPCardPool : CustomCardPoolModel
{
    public override string Title => MainFile.ModId;
    public override Color DeckEntryCardColor => Colors.Green;
    public override bool IsColorless => false;
}

