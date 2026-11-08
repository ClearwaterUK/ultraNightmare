using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNightmare.HarmonyPatches;

using static UltraNightmare.CommonFunctions;

// When dying in a run, wipe the run save data and send the death data to the server.
[HarmonyPatch(typeof(DeathSequence), "OnEnable")]
public static class PrepareDeathScreen
{
    [HarmonyPrefix]
    public static bool PrepareDeathScreenPatch()
    {
        if (RunManager.isInRun)
        {
            RunManager.WipeSaveData();
            RunManager.SendDeathToServer();
            
            GameObject deathScreen = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"), "BlackScreen"), "YouDiedText");
            //Need to disable the TextOverride component.
            Component[] test = deathScreen.GetComponents(typeof(Component));
            Behaviour bhvr = (Behaviour)test[3];
            bhvr.enabled = false;

            Text youDiedText = GetTextfromGameObject(deathScreen);
            youDiedText.text = "THE TORMENT HAS ENDED." + "\n\n\n\n\n" + "PRESS R TO EXIT";
        }

        return true;
    }
}

//Prevents the player from spamming R to restart the moment they're killed in a run (wait for the death screen)
[HarmonyPatch(typeof(StatsManager), "Restart")]
public static class PreventRestart
{
    [HarmonyPrefix]
    public static bool PreventRestartPatch(ref StatsManager __instance, ref NewMovement ___nm)
    {
        if (RunManager.isInRun && ___nm.hp <= 0)
        {
            //Wait for the "you died" screen to appear before allowing pressing R to exit
            if (GetGameObjectChild(GetInactiveRootObject("Canvas"), "BlackScreen").activeInHierarchy)
            {
                RunManager.isInRun = false;
                SceneHelper.LoadScene("Main Menu");
                return false;
            }
            return false;
        }

        return true;
    }
}

//Prevent player from opening the pause menu during death sequence
[HarmonyPatch(typeof(OptionsManager), "Pause")]
public static class PreventPauseOnDeath
{
    [HarmonyPrefix]
    public static bool PreventPauseOnDeathPatch(ref OptionsManager __instance, ref NewMovement ___nm)
    {
        if (RunManager.isInRun && ___nm != null && ___nm.hp <= 0)
        {
            return false;
        }

        return true;
    }
}