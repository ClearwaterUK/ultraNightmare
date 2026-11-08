using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.UIComponents;

public class UltraNightmareInProgressPanel
{
    public static GameObject continueRunButton;
    public static GameObject startNewRunButton;

    public static TMP_Text runDescription;
    public static TMP_Text modNonWhitelistWarning;

    public static void SetInProgressDescription(int levelId, int difficulty, SaveTypeEnum saveType, int radianceTier, bool requirePRanks)
    {
        runDescription.text = GetMissionName.GetMission(levelId) + "\n"
                                                                 + getColoredDifficultyName(difficulty) + "\n"
                                                                 + RunManager.getSaveTypeString(saveType) +"\n";
        if (radianceTier > 0)
        {
            runDescription.text += "RADIANCE TIER " + radianceTier + "\n";
        }
        if (requirePRanks)
        {
            runDescription.text += "<color=yellow>P</color>-RANKS REQUIRED\n";
        }
    }
    
    
    public static void Init(ref GameObject inProgressPanel)
    {
        GameObject panel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(inProgressPanel,"Background"),"UN-Panel"),"Panel");
        
        runDescription = GetGameObjectChild(panel,"RunInfo").GetComponent<TextMeshProUGUI>();
        runDescription.text = GetMissionName.GetMission(RunManager.level) + "\n" +
                              GameDifficultyExtensions.GetDifficultyName((GameDifficulty)RunManager.difficulty).ToUpper()
                              + "\n";
        
        modNonWhitelistWarning = GetGameObjectChild(panel,"ModNonWhitelistWarning").GetComponent<TextMeshProUGUI>();
        
        continueRunButton = GetGameObjectChild(panel, "ContinueRun");
        startNewRunButton = GetGameObjectChild(panel, "NewRun");
        
        continueRunButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            RunManager.ContinueRun();
        });
        
        startNewRunButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            MainMenu.InProgressPanel.SetActive(false);
            MainMenu.MainPanel.SetActive(true);
        });
        
    }
}