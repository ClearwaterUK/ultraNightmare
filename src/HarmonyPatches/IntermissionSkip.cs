using HarmonyLib;
using UltraNightmare;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

[HarmonyPatch(typeof(IntermissionController), "Start")]
public class IntermissionSkip
{
    //If in a run, skip the intermission text and directly display the act end panel
    [HarmonyPrefix]
    public static bool IntermissionSkipPatch(IntermissionController __instance)
    {
        if (RunManager.isInRun)
        {
            __instance.transform.parent.gameObject.SetActive(false);
            GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"),"PowerUpVignette"),"Panel"),"Act End Message").SetActive(true);
            
            return false;
        }

        return true;
    }
}