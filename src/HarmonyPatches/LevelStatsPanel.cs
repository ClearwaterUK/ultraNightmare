using System.Linq;
using System.Reflection;
using HarmonyLib;

using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

//Enable tabbing to display level stats info in a run
[HarmonyPatch(typeof(LevelStatsEnabler), "Start")]
public static class LevelStatsPanelEnable
{
    [HarmonyPrefix]
    public static bool allowLevelStatsPanelInRunPatch(LevelStatsEnabler __instance)
    {
        if (RunManager.isInRun)
        {
            __instance.canAlwaysEnable = true;
        }

        return true;
    }
}

// Ensure level stats are displayed correctly when in a run
[HarmonyPatch(typeof(LevelStats), "Start")]
public static class LevelStatsEnable
{
    private static readonly FieldInfo ReadyField =
        AccessTools.Field(typeof(LevelStats), "ready");
    
    [HarmonyPostfix]
    public static void allowLevelStatsInRunPatch(LevelStats __instance)
    {
        if (RunManager.isInRun)
        {
            ReadyField.SetValue(__instance, true);
            __instance.levelName.text = StockMapInfo.Instance.assets.LargeText;
        }
    }
}