using TMPro;
using UltraNightmare;
using UnityEngine;
using UnityEngine.UI;

using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.UI_Elements;

public static class UIHelper
{
    public static GameObject CreateText(string textString="Text", int fontSize=32,string idName="Text")
    { 
        GameObject text = new GameObject();
        text.name = idName;
        text.AddComponent<RectTransform>();
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
        text.AddComponent<CanvasRenderer>();
        text.AddComponent<TextMeshProUGUI>();
        text.GetComponent<TextMeshProUGUI>().text = textString;
        text.GetComponent<TextMeshProUGUI>().font = AssetLoader.GameFont;
        text.GetComponent<TextMeshProUGUI>().fontSize = fontSize;
        text.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        text.GetComponent<TextMeshProUGUI>().color = Color.white;
        text.GetComponent<TextMeshProUGUI>().enableWordWrapping = false;
        return text;
    }
}