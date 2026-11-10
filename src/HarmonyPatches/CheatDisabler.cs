using HarmonyLib;
using UltraNightmare;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

    [HarmonyPatch(typeof(CheatsController), "ProcessInput")]
    public class CheatCombinationDisabler
    {
        [HarmonyPostfix]
        //Prevent cheats from being enabled if the player inputs the Konami code
        public static void disableCheatingAttempts(GameObject ___consentScreen, int ___sequenceIndex)
        {
            if(RunManager.isInRun)
            {
                if(___consentScreen.activeSelf && !Main.isDevelopmentBuild)
                {
                    MonoSingleton<OptionsManager>.Instance.UnPause();
                    ___consentScreen.SetActive(false);
                }
            }
        }
    }
    
    
    [HarmonyPatch(typeof(CheatsController), "Start")]
    public class ForceDisableKeepEnabledCheat
    {
        [HarmonyPrefix]
        public static bool forceDisableKeepEnabeldCheat()
        {
            if (RunManager.isInRun)
            {
                MonoSingleton<PrefsManager>.Instance.SetBool("cheat.ultrakill.keep-enabled", false);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CheatsManager), "Start")]
    public class RemoveCheatFlagInRun
    {
        [HarmonyPostfix]
        public static void disableCheatFlagInBingo(CheatsManager __instance)
        {
            if(RunManager.isInRun)
            {
                MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
            }
        }
    }