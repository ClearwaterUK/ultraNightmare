using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UltraNightmare.JsonStructures;
using UnityEngine;

using System.Linq;
using UltraNightmare.UIComponents;
using UnityEngine.SceneManagement;

namespace UltraNightmare;

using static UltraNightmare.CommonFunctions;

public enum SaveTypeEnum
{
    PER_LAYER,
    PER_ACT,
    NO_SAVE
}

public static class RunManager
{
    public const int ULTRANIGHTMARE_SAVE_ID = 9999;
    
    public static int previousSave;
    
    public static bool isInRun = false;
    public static bool hasRunInProgress = false;
    public static bool isUsingWhitelistedMods = true;

    //Run configuration
    public static short difficulty = 2;
    public static short level = 0;
    public static bool allowSave = false;
    public static bool requirePRank = false;
    public static int enemyRadianceLevel = 0;
    public static int unlockedMaxRadiance = 0;
    public static bool gotPRank = false;
    public static bool hasSent = false;
    public static bool saveUsesWhitelistedMods = true;

    //Run stats
    public static float runTime = 0f;
    public static int runStylePoints = 0;
    public static Vector3 currentGroundPosition = Vector3.zero;
    public static Vector3 currentGroundRotation = Vector3.zero;

    public static SaveTypeEnum saveType = SaveTypeEnum.PER_LAYER;

    public static HttpClient Client = new HttpClient();

    public static string baseDomain = "http://"+(Main.isDevelopmentBuild ? "127.0.0.1:8081/" : "clearwaterbirb.uk/");
    public static string ultranightmareBackend = "ultranightmare/";
    public static string modWhitelist = "modWhitelist/";
    public static string hallOfFame = "leaderboards/";
    public static string maxRadiance = "getMaxRadiance/";
    public static string deaths = "deaths/";
    public static string registerCompletedRun = "registerCompletedRun/";
    public static string registerDeath = "registerDeath/";

    public static string getSaveTypeString(SaveTypeEnum saveType)
    {
        switch (saveType)
        {
            case SaveTypeEnum.NO_SAVE: return "<color=red>NO SAVING</color>";
            case SaveTypeEnum.PER_ACT: return "<color=yellow>PER ACT</color>";
            case SaveTypeEnum.PER_LAYER: return "<color=green>PER LAYER</color>";
            default: return "UNKNOWN";
        }
    }

    public static async Task<string> CheckModWhitelist(Dictionary<string,string> currentMods)
    {
        try
        {
            string responseRaw = await Client.GetStringAsync(RunManager.baseDomain+RunManager.modWhitelist);
            
            List<ModWhitelist> modWhitelist = JsonConvert.DeserializeObject<List<ModWhitelist>>(responseRaw);
            List<string> modWhitelistIds = modWhitelist.Select(mod => mod.modId).ToList();

            List<string> nonWhitelistedMods = new List<string>();

            foreach (var item in currentMods)
            {
                if (!modWhitelistIds.Contains(item.Key))
                {
                    Logging.Warn(item.Value + " not in whitelist");
                    nonWhitelistedMods.Add(item.Value);
                }
            }

            if (nonWhitelistedMods.Count > 0)
            {
                isUsingWhitelistedMods = false;
                UltraNightmareNonWhitelistedModsPanel.NonWhitelistedMods.text = string.Join("\n",nonWhitelistedMods);
                UltraNightmareMainPanel.NonWhitelistedModsWarning.gameObject.SetActive(true);
                UltraNightmareMainPanel.flaggedModsButton.gameObject.SetActive(true);
            }
            
            return responseRaw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static async Task<string> CheckMaxRadiance()
    {
        string getDeathsUrl = baseDomain + ultranightmareBackend + maxRadiance + Steamworks.SteamClient.SteamId;
        try
        {
            string responseRaw = await Client.GetStringAsync(getDeathsUrl);
            MaxRadianceData mad = JsonConvert.DeserializeObject<MaxRadianceData>(responseRaw);
            Logging.Info("Current max radiance: "+mad.maxRadiance);
            if (mad.maxRadiance == -1)//Radiance not unlocked
            {
                unlockedMaxRadiance = 0;
            }
            else if (mad.maxRadiance >= 0)
            {
                unlockedMaxRadiance = Math.Min(mad.maxRadiance + 1,10);
            }
            else
            {
                unlockedMaxRadiance = 0;
            }
            return responseRaw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public static void ResetStats()
    {
        runTime = 0f;
        runStylePoints = 0;
        currentGroundPosition = Vector3.zero;
        currentGroundRotation = Vector3.zero;
    }
    
    public static void StartRun()
    {
        PrepareSaveData();
        ResetStats();
        
        isInRun = true;
        SceneHelper.LoadScene("Level 0-1");
    }

    public static void ContinueRun()
    {
        isInRun = true;
        GameProgressSaver.SetSlot(ULTRANIGHTMARE_SAVE_ID);
        string encodedJson = File.ReadAllText(Path.Combine(GameProgressSaver.SavePath, "unsave.un"));
        var base64EncodedBytes = System.Convert.FromBase64String(encodedJson);
        string decoded = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        
        RunSettingsData rsd =  JsonConvert.DeserializeObject<RunSettingsData>(decoded);
        saveType = rsd.SaveType;
        requirePRank = rsd.RequirePRanks;
        isInRun = true;
        saveUsesWhitelistedMods = rsd.IsUsingWhitelistedMods;
        
        
        SceneHelper.LoadScene(GetMissionName.GetSceneName(level));
    }

    public static void CheckSaveData()
    {
        
        previousSave = GameProgressSaver.currentSlot;
        GameProgressSaver.currentSlot = ULTRANIGHTMARE_SAVE_ID;
        //On game boot, check if a save in the UltraNightmare slot exists.
        if (Directory.Exists(GameProgressSaver.SavePath))
        {
            if (File.Exists(Path.Combine(GameProgressSaver.SavePath, "unsave.un")))
            {
                Logging.Info("Midrun directory exists, checking data");
                GameProgressSaver.GetGeneralProgress();
            
                //Slighty hacky way due to private methods inside of GameProgressSaver...
                //Check from Brutal first down to Harmless
                for (int i = 5; i >= 0; i--)
                {
                    if (GameProgressSaver.GetProgress(i) > 1)
                    {
                        Logging.Info("Midrun data found for difficulty "+i+" on level ID "+ GameProgressSaver.GetProgress(i));
                        difficulty = (short)i;
                        level = (short)GameProgressSaver.GetProgress(i);
                        hasRunInProgress = true;
                        
                        //Check if the save was flagged with non-whitelisted mods
                        string encodedJson = File.ReadAllText(Path.Combine(GameProgressSaver.SavePath, "unsave.un"));
                        var base64EncodedBytes = System.Convert.FromBase64String(encodedJson);
                        string decoded = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        
                        RunSettingsData rsd =  JsonConvert.DeserializeObject<RunSettingsData>(decoded);
                        if (!rsd.IsUsingWhitelistedMods)
                        {
                            Logging.Warn("Save used non-whitelisted mods, flagging");
                            saveUsesWhitelistedMods = false;
                            UltraNightmareInProgressPanel.modNonWhitelistWarning.gameObject.SetActive(true);
                        }

                        difficulty = (short)rsd.difficulty;
                        level = (short)rsd.levelId;
                        enemyRadianceLevel = rsd.radianceTier;
                        saveType = rsd.SaveType;
                        
                        GameProgressSaver.currentSlot = previousSave;
                        UltraNightmareInProgressPanel.SetInProgressDescription(level,difficulty,saveType,enemyRadianceLevel,requirePRank);
                        return;
                    }
                }
                Logging.Info("No midrun data found for any difficulties, assuming no save data");
                hasRunInProgress = false;
            }
            else
            {
                Logging.Warn("unsave.un does not exist! Wiping save");
                WipeSaveData();
                hasRunInProgress = false;
            }
        }
        else
        {
            Logging.Info("No current run save for UltraNightmare");
            hasRunInProgress = false;
        }

        GameProgressSaver.currentSlot = previousSave;
    }
    
    public static void PrepareSaveData()
    {
        previousSave = GameProgressSaver.currentSlot;
        GameProgressSaver.WipeSlot(ULTRANIGHTMARE_SAVE_ID);
        GameProgressSaver.SetSlot(ULTRANIGHTMARE_SAVE_ID);
        
        MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", false);
        MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", difficulty);
    }

    public static void SaveRun()
    {
        //Create an extra file with the base save that contains data on run settings (Save type & P-Rank required)
        RunSettingsData rsd = new RunSettingsData();
        
        // If save type is per layer, get level ID that the player was on
        if (saveType == SaveTypeEnum.PER_LAYER)
        {
            rsd.levelId = SceneHelper.CurrentLevelNumber;
        }
        else if (saveType == SaveTypeEnum.PER_ACT)
        {
            //SceneHelper.GetGetLevelIndexAfterIntermission isn't static so can't be accessed directly. So we'll just do a hack.
            if (SceneHelper.CurrentScene == "Intermission1")
            {
                rsd.levelId = 16; //4-1
            }
            else if (SceneHelper.CurrentScene == "Intermission2")
            {
                rsd.levelId = 26; //7-1
            }
        }
        
        rsd.difficulty = difficulty;
        rsd.radianceTier = enemyRadianceLevel;
        rsd.SaveType = saveType;
        rsd.RequirePRanks = requirePRank;
        //If we had resumed from a previous save that was started with non-whitelisted mods, resave the flag
        if (!saveUsesWhitelistedMods)
        {
            rsd.IsUsingWhitelistedMods = false;
        }
        else
        {
            rsd.IsUsingWhitelistedMods = isUsingWhitelistedMods;
        }
        
        
        byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rsd));
        string encodedJson = System.Convert.ToBase64String(encodedBytes);
        File.WriteAllText(Path.Combine(GameProgressSaver.SavePath, "unsave.un"),encodedJson);
        isInRun = false;
        
        //Switch back to the previous save.
        GameProgressSaver.SetSlot(previousSave);

    }

    public static async Task<List<DeathData>> GetDeaths()
    {
        string deathDataRaw = await FetchDeathsFromServer();
        Console.WriteLine(deathDataRaw);

        List<DeathData> deaths = JsonConvert.DeserializeObject<List<DeathData>>(deathDataRaw);

        return deaths;
    }

    public static async Task<string> FetchDeathsFromServer()
    {
        string getDeathsUrl = baseDomain + ultranightmareBackend + deaths + Steamworks.SteamClient.SteamId + "/" + getSceneName() + "/" + MonoSingleton<PrefsManager>.Instance.GetInt("difficulty") + "/" + enemyRadianceLevel;
        try
        {
            string responseRaw = await Client.GetStringAsync(getDeathsUrl);
            return responseRaw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    public static async Task<string> SendCompletedRunToServer()
    {
        try
        {
            DateTime currentDate = new DateTime();
            string deathUrl = baseDomain + ultranightmareBackend + registerCompletedRun;
            
            Dictionary<string, string> content = new Dictionary<string, string>()
            {
                { "steamId", Steamworks.SteamClient.SteamId.ToString() },
                { "difficulty", MonoSingleton<PrefsManager>.Instance.GetInt("difficulty").ToString()},
                { "time", runTime.ToString("00.000") },
                { "style", runStylePoints.ToString() },
                { "radiance", enemyRadianceLevel.ToString()},
                { "saveType", saveType.ToString()},
                { "pRanksRequired", requirePRank.ToString()},
                { "usedWhitelistedMods", (saveUsesWhitelistedMods && isUsingWhitelistedMods).ToString()},
                { "completionDate", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()}
            };
            var encodedContent = new FormUrlEncodedContent(content);
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            HttpResponseMessage response = await Client.PostAsync(deathUrl,encodedContent);
            return "ok";
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.ToString();
        }
    }

    public static async Task<string> SendDeathToServer()
    {
        DateTime currentDate = new DateTime();
        string deathUrl = baseDomain + ultranightmareBackend + registerDeath;
        try
        {
            Dictionary<string, string> content = new Dictionary<string, string>()
            {
                { "X", currentGroundPosition.x.ToString("F3") },
                { "Y", currentGroundPosition.y.ToString("F3") },
                { "Z", currentGroundPosition.z.ToString("F3") },
                { "steamId", Steamworks.SteamClient.SteamId.ToString() },
                { "levelId", SceneHelper.CurrentScene },
                { "deathDate", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()},
                { "difficulty", MonoSingleton<PrefsManager>.Instance.GetInt("difficulty").ToString()},
                { "radiance", enemyRadianceLevel.ToString()}
            };
            var encodedContent = new FormUrlEncodedContent(content);
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            HttpResponseMessage response = await Client.PostAsync(deathUrl,encodedContent);
            return "ok";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.ToString();
        }
    }
    

    public static void WipeSaveData()
    {
        GameProgressSaver.WipeSlot(ULTRANIGHTMARE_SAVE_ID);
        GameProgressSaver.SetSlot(previousSave);
        File.Delete(Path.Combine(GameProgressSaver.SavePath, "unsave.un"));
    }
}