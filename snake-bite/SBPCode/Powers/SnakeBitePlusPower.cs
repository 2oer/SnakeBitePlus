using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using CSharpSBP.SBP_Code.Extensions;

namespace CSharpSBP.SBP_Code.Powers;

public abstract class SnakeBitePlusPower : CustomPowerModel
{
    public override string CustomPackedIconPath 
    => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".PowerImagePath();
    
    public override string CustomBigIconPath 
    => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".BigPowerImagePath();
}