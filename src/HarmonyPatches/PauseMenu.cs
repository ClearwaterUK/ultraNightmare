using HarmonyLib;
using TMPro;

using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

// Hide the checkpoint and restart buttons while in a run
[HarmonyPatch(typeof(PauseMenu), "OnEnable")]
public static class HidePauseRestartButtons
{
    [HarmonyPostfix]
    public static void HidePauseRestartButtonsPatch(ref PauseMenu __instance)
    {
        if (RunManager.isInRun)
        {
            GetGameObjectChild(__instance.gameObject,"Restart Checkpoint").SetActive(false);
            GetGameObjectChild(__instance.gameObject,"Restart Mission").SetActive(false);
            //If in an intermission, hide the Skip button
            if (GetGameObjectChild(__instance.gameObject, "Restart Checkpoint (1)") != null)
            {
                GetGameObjectChild(__instance.gameObject,"Restart Checkpoint (1)").SetActive(false);
            }
        }
    }
}


//Wipe the temporary save used for the run if quitting a run.
[HarmonyPatch(typeof(OptionsMenuToManager),"QuitMissionNoConfirm")]
public static class LeaveRun
{
    [HarmonyPrefix]
    public static bool LeaveRunPatch()
    {
        if(RunManager.isInRun)
        {
            RunManager.WipeSaveData();
            RunManager.isInRun = false;
        }
        return true;
    }
}

//Confirm exit to the player 
public static class PauseMenuConfirmLeave
{
    [HarmonyPatch(typeof(OptionsMenuToManager),"QuitMission")]
    public static class ConfirmExitRun
    {
        [HarmonyPrefix]
        public static bool confirmLeaveInGame(ref OptionsMenuToManager __instance)
        {
            if(RunManager.isInRun)
            {
                TextMeshProUGUI leaveText = GetGameObjectChild(GetGameObjectChild(__instance.quitDialog.gameObject,"Panel"),"Text (2)").GetComponent<TextMeshProUGUI>();
                    leaveText.text = "<color=orange>WARNING</color>: Exiting now will <color=red>end this run and wipe your progress</color>.\nAre you sure?";

                __instance.quitDialog.ShowDialog();
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}