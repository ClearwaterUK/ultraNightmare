using System;
using System.Collections.Generic;
using Discord;
using TMPro;
using UltraNightmare.Components;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.UIComponents;

public static class UltraNightmareMainPanel
{
    public static GameObject closeButton;

    public static GameObject startRunButton;
    public static GameObject flaggedModsButton;
    public static GameObject hallOfFameButton;
    
    public static GameObject saveType;
    public static GameObject requirePank;
        
    public static TMP_Dropdown Difficulty;
    public static TMP_Dropdown SaveType;
    public static Toggle RequirePRank;
    public static Slider RadianceLevel;

    public static TextMeshProUGUI Descriptor;
    public static TextMeshProUGUI NonWhitelistedModsWarning;
    
    public static void Init(ref GameObject mainPanel)
    {
        GameObject panel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(mainPanel,"Background"),"UN-Panel"),"Panel");
        
        startRunButton = GetGameObjectChild(panel, "BeginRun");

        startRunButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            RunManager.StartRun();
        });
        
        flaggedModsButton = GetGameObjectChild(panel, "FlaggedModsButton");
        flaggedModsButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            MainMenu.NonWhitelistedModsPanel.SetActive(true);
        });
        
        hallOfFameButton = GetGameObjectChild(panel, "HallOfFameButton");
        hallOfFameButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.OpenURL(RunManager.baseDomain+RunManager.ultranightmareBackend+RunManager.hallOfFame);
        });
        
        Difficulty = GetGameObjectChild(GetGameObjectChild(panel,"Difficulty"),"Value").GetComponent<TMP_Dropdown>();
        Difficulty.transform.parent.gameObject.AddComponent<Descriptor>();
        Difficulty.transform.parent.gameObject.gameObject.GetComponent<Descriptor>().descriptorText = "Game difficulty.";
        Difficulty.onValueChanged.AddListener(delegate(int arg0)
        {
            RunManager.difficulty = (short)Difficulty.value;
        });

        SaveType = GetGameObjectChild(GetGameObjectChild(panel,"SaveType"),"Value").GetComponent<TMP_Dropdown>();
        SaveType.transform.parent.gameObject.AddComponent<Descriptor>();
        SaveType.transform.parent.gameObject.GetComponent<Descriptor>().descriptorText = "Type of save.";
        SaveType.onValueChanged.AddListener(delegate(int arg0)
        {
            RunManager.saveType = (SaveTypeEnum)SaveType.value;
        });
        
        RequirePRank = GetGameObjectChild(GetGameObjectChild(panel,"RequirePRank"),"Value").GetComponent<Toggle>();
        RequirePRank.transform.parent.gameObject.AddComponent<Descriptor>();
        RequirePRank.transform.parent.gameObject.GetComponent<Descriptor>().descriptorText = "Levels must be P-Ranked to progress.\nCompleting a level, but failing to obtain P-Rank will restart the current level.";
        RequirePRank.onValueChanged.AddListener(delegate { RunManager.requirePRank = RequirePRank.isOn; });
        

        RadianceLevel = GetGameObjectChild(GetGameObjectChild(panel,"Radiance"),"Value").GetComponent<Slider>();
        //If player hasn't completed a non-radiance run, hide the option
        if (RunManager.unlockedMaxRadiance <= 0)
        {
            RadianceLevel.transform.parent.gameObject.SetActive(false);
        }
        RadianceLevel.maxValue = Mathf.Clamp(RunManager.unlockedMaxRadiance,1,10);
        RadianceLevel.onValueChanged.AddListener(delegate
        {
            GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(panel, "Radiance"), "Value"), "Text")
                    .GetComponent<TextMeshProUGUI>().text =
                RadianceLevel.value.ToString();
            RunManager.enemyRadianceLevel = (int)RadianceLevel.value;
        });
        
        Descriptor = GetGameObjectChild(panel,"Tooltip").GetComponent<TextMeshProUGUI>();
        Descriptor.gameObject.SetActive(false);
        
        NonWhitelistedModsWarning = GetGameObjectChild(panel,"FlaggedMods").GetComponent<TextMeshProUGUI>();
        NonWhitelistedModsWarning.gameObject.SetActive(false);

    }
}