using HarmonyLib;
using TMPro;

using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

// Hide the checkpoint and restart buttons while in a run
[HarmonyPatch(typeof(NewMovement), "Update")]
public static class PositionTracker
{
    [HarmonyPostfix]
    public static void PositionTrackerPostfix(NewMovement __instance)
    {
        if (RunManager.isInRun)
        {
            if (!__instance.falling && !__instance.jumping && __instance.hp > 0)
            {
                RunManager.currentGroundPosition = __instance.gameObject.transform.position;
                RunManager.currentGroundRotation = __instance.gameObject.transform.forward;
            }
        }
    }
}