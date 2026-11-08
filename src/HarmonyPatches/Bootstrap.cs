using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

[HarmonyPatch(typeof(Bootstrap), "Start")]
public static class SetSaveOnStart
{
    [HarmonyPrefix]
    public static bool SetSaveOnStartPatch()
    {
        //Small hack where if the player alt-f4'd out of a run, to stop it from replaying the first time boot sequence on next launch.
        //Doing this because OnApplicationQuit doesn't seem to be called when Alt+F4'ing? So we do this instead
        if (GameProgressSaver.currentSlot == RunManager.ULTRANIGHTMARE_SAVE_ID)
        {
            GameProgressSaver.SetSlot(0);
        }

        return true;
    }
}