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