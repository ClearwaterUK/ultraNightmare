using System;
using HarmonyLib;
using TMPro;
using UltraNightmare;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;

// Modify buttons on the shop in a run to prevent the player from going to CG/sandbox
[HarmonyPatch(typeof(ShopZone),"TurnOn")]
public class ShopAddLevelInfo
{
    [HarmonyPostfix]
    public static void AddLevelInfo(ShopZone __instance, Canvas ___shopCanvas)
    {
        if(RunManager.isInRun && ___shopCanvas != null && !___shopCanvas.gameObject.name.Contains("Shop"))
        {
            try
            {
                //Hide the CG and sandbox buttons
                GameObject shopObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject,"Background"),"Main Panel"),"Main Menu");
                
                GameObject cgButton = GetGameObjectChild(GetGameObjectChild(shopObject,"Buttons"),"CyberGrindButton");
                cgButton.SetActive(false);
                
                GameObject sandboxButton = GetGameObjectChild(GetGameObjectChild(shopObject,"Buttons"),"SandboxButton");

                // If per layer saving is on and we're on the first level of a layer, change the sandbox button to save run & quit
                if (RunManager.saveType == SaveTypeEnum.PER_LAYER && getSceneName().Contains("-1"))
                {
                    GetGameObjectChild(sandboxButton, "Text").GetComponent<TextMeshProUGUI>().text =
                        "SAVE RUN & EXIT";
                }
                // Otherwise just hide the button
                else
                {
                    sandboxButton.SetActive(false);
                }
            }

            catch (Exception e)
            {
                Logging.Warn("This shop isn't vanilla or an error occured");
                Logging.Error(e.ToString());
            }
        }
    }
}

[HarmonyPatch(typeof(ShopButton), "OnPointerClick")]
public class ShopButtonSaveAndQuit
{
    [HarmonyPrefix]
    public static bool SaveAndQuitPatch(ShopButton __instance)
    {
        if (RunManager.saveType == SaveTypeEnum.PER_LAYER && getSceneName().Contains("-1") && __instance.gameObject.name == "SandboxButton")
        {
            // If clicking on the save run original button GameObject is called SandboxButton, save the run and exit.
            Logging.Info("Saving run");
            RunManager.SaveRun();
            SceneHelper.LoadScene("Main Menu");
            return false;
        }

        return true;

    }
}