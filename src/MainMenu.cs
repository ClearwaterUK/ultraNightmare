using System.Collections.Generic;
using BepInEx.Bootstrap;
using TMPro;
using UltraNightmare.UIComponents;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNightmare;

using static UltraNightmare.CommonFunctions;

public static class MainMenu
{
    public static GameObject MainPanel = null;
    public static GameObject InProgressPanel = null;
    public static GameObject NonWhitelistedModsPanel = null;
    

    public static async void PrepareMainMenu()
    {
        if(InProgressPanel == null)
        {
            InProgressPanel = GameObject.Instantiate(AssetLoader.UNInProgressPanel, GetInactiveRootObject("Canvas").transform);
            UltraNightmareInProgressPanel.Init(ref InProgressPanel);
        }        
        
        //Check to see if we have a mid-run save
        RunManager.CheckSaveData();
        
        //Check what the current max radiance the player has access to
        await RunManager.CheckMaxRadiance();
        
        if (MainPanel == null)
        {
            MainPanel = GameObject.Instantiate(AssetLoader.UNMainPanel, GetInactiveRootObject("Canvas").transform);
            UltraNightmareMainPanel.Init(ref MainPanel);
        }


        if (NonWhitelistedModsPanel == null)
        {
            NonWhitelistedModsPanel = GameObject.Instantiate(AssetLoader.UNNonWhitelistedModsPanel, GetInactiveRootObject("Canvas").transform);
            UltraNightmareNonWhitelistedModsPanel.Init(ref NonWhitelistedModsPanel);
        }

        
        GameObject originalPlayButton = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"),"Main Menu (1)"),"LeftSide"),"Continue");
        
        GameObject ultranightmarePlayButton = GameObject.Instantiate(originalPlayButton,originalPlayButton.transform.parent);
        ultranightmarePlayButton.transform.localPosition += new Vector3(0f, 80f, 0f);
        ultranightmarePlayButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
        ultranightmarePlayButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (RunManager.hasRunInProgress)
            {
                InProgressPanel.SetActive(true);
            }
            else
            {
                MainPanel.SetActive(true);
            }
        });
        GetGameObjectChild(ultranightmarePlayButton, "Text").GetComponent<TextMeshProUGUI>().text = "ULTRA-NIGHTMARE";
        ultranightmarePlayButton.SetActive(true);
        
        //Check if the user is running whitelisted mods. If not, flag completed runs to not be uploaded and warn user
        Logging.Message("Validating current modlist...");
        Dictionary<string, string> modList = new Dictionary<string, string>();
        foreach (var plugin in Chainloader.PluginInfos)
        {
            //Make sure we're not adding our own mod to the checklist
            if (plugin.Key != Main.pluginId)
            {
                string pluginNameWithoutVersion = plugin.Value.ToString().Split(' ')[0];
                modList.Add(plugin.Key,pluginNameWithoutVersion);
            }
        }

        await RunManager.CheckModWhitelist(modList);
    }
}