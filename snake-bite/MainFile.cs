using Godot;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace CSharpSBP.SBP_Code;
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "CSharpSBP";
    public const string ResPath = "res://CSharpSBP/";        
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } 
    = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);    
    public static void Initialize()
    {
            Harmony harmony = new(ModId);
            harmony.PatchAll();
            
            Logger.Info("CSharpSBP mod initialized with Pool attribute registration to SilentCardPool (Hunter)");
    }
}


