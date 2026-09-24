using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using CSharpSBP.SBP_Code.Events;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSBP.SBP_Code.Patches;

[HarmonyPatch]
public static class EventPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Underdocks), nameof(Underdocks.AllEvents), MethodType.Getter)]
    public static void AddSnakeAltarEventToUnderdocks(ref IEnumerable<EventModel> __result)
    {
        var snakeAltarEvent = ModelDb.Event<SnakeAltarEvent>();
        __result = new[] { snakeAltarEvent }.Concat(__result);
    }
}
