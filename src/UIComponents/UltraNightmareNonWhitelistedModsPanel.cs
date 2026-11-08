using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.UIComponents;

public class UltraNightmareNonWhitelistedModsPanel
{
    public static GameObject CloseButton;

    public static TextMeshProUGUI NonWhitelistedMods;

    public static void Init(ref GameObject NonWhitelistedPanel)
    {
        GameObject panel = GetGameObjectChild(GetGameObjectChild(NonWhitelistedPanel,"Panel"),"Panel");
        NonWhitelistedMods = GetGameObjectChild(panel,"ModList").GetComponent<TextMeshProUGUI>();
        
        CloseButton = GetGameObjectChild(panel,"Ok");
        CloseButton.GetComponent<Button>().onClick.AddListener(delegate {MainMenu.NonWhitelistedModsPanel.SetActive(false);});
    }
}