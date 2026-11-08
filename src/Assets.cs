using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UltraNightmare;

public static class AssetLoader
{
    public static TMP_FontAsset GameFont;
    public static AssetBundle Assets;

    public static GameObject UNMainPanel;
    public static GameObject UNInProgressPanel;
    public static GameObject UNNonWhitelistedModsPanel;
    public static GameObject DeathMarker;
    
    public static void LoadAssets()
    {
        Assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModFolder,"un.resource"));
        GameFont = Assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        UNMainPanel = Assets.LoadAsset<GameObject>("UN-MainPanel");
        UNInProgressPanel = Assets.LoadAsset<GameObject>("UN-InProgressPanel");
        DeathMarker = Assets.LoadAsset<GameObject>("DeathMarker");
        UNNonWhitelistedModsPanel = Assets.LoadAsset<GameObject>("UN-UnallowedModsPanel");
    }
}