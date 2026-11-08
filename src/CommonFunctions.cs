using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltraNightmare;

public static class CommonFunctions
{
    public static string sanitiseUsername(string rawUsername)
    {
        return Regex.Replace(rawUsername,@"\p{Cs}", "");
    }
    
    public static bool checkIfLevelSaveExists(string savePath, string fileName)
    {
        string fullPath = Path.Combine(savePath, string.Format("Slot{0}/" + fileName, GameProgressSaver.currentSlot + 1));
        return File.Exists(fullPath);
    }
    
    
    public static string getSceneName()
    {
        return SceneHelper.CurrentScene;
    }
    
    public static GameObject GetInactiveRootObject(string objectName)
    {
        List<GameObject> rootList = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(rootList);
        foreach (GameObject child in rootList)
        {
            if (child.name == objectName)
            {
                return child;
            }
        }
        return null;
    }
    
    public static IEnumerator WaitforSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    
    public static GameObject GetGameObjectChild(GameObject parentObject, string childToFind)
    {
        try
        {
            GameObject childToReturn = parentObject.transform.Find(childToFind).gameObject;
            return childToReturn;
        }
        catch (Exception)
        {
            Logging.Warn("Couldn't find requested child gameObject " + childToFind + " in parent gameObject '" + parentObject.name + "'");
            return null;
        }
    }

    public static TextMeshProUGUI GetTextMeshProGUI(GameObject objectToUse)
    {
        return objectToUse.GetComponent<TextMeshProUGUI>();
    }
    
    public static Text GetTextfromGameObject(GameObject objectToUse)
    {
        return objectToUse.GetComponent<Text>();
    }
    
    public static string getFormattedTime(float time)
    {
        float secs = time;
        float mins = 0;
        while (secs >= 60f)
        {
            secs -= 60f;
            mins += 1f;
        }
        return  mins + ":" + secs.ToString("00.000");
    }

    public static string getColoredDifficultyName(int difficultyOverride=-1)
    {
        //If a difficulty is specified in argument, use that, otherwise use the currently set difficulty by the game.
        int diff = 0;
        if (difficultyOverride > -1)
        { diff = difficultyOverride; }
        else
        { diff = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0); }
            
        switch (diff)
        {
            case 0:
                return "<color=lime>HARMLESS</color>";
            case 1:
                return "<color=green>LENIENT</color>";
            case 2:
                return "<color=yellow>STANDARD</color>";
            case 3:
                return "<color=orange>VIOLENT</color>";
            case 4:
                return "<color=red>BRUTAL</color>";
            case 5:
                return "<color=red>ULTRAKILL MUST DIE</color>";
            default:
                return "UNKNOWN";
        }
    }
}