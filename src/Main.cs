using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using TMPro;
using UKEnemyIdentifier.Components;
using UltraNightmare;
using UltraNightmare.JsonStructures;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UltraNightmare.CommonFunctions;

namespace UltraNightmare;

[BepInPlugin(pluginId, pluginName, pluginVersion)]
public class Main : BaseUnityPlugin
{
    public const string pluginId = "clearwater.ultranightmare.ultranightmare";
    public const string pluginName = "UltraNIGHTMARE";
    public const string pluginVersion = "1.1.0";

    public const bool isDevelopmentBuild = false;
    
    public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    //Mod init logic
    private void Awake()
    {
        Logging.Message("--Loading UltraNIGHTMARE...--");
        AssetLoader.LoadAssets();
        
        Logging.Message("--Applying patches...--");
        Harmony harmony = new Harmony(pluginId);
        harmony.PatchAll();
        
        Logging.Message("--Loading config...--");
        RunManager.sendDeathMarkers =  Config.Bind("UltraNightmareConfig","sendDeathMarkers","true","Allow your deaths to be uploaded to the server as death markers for others to see.");
        RunManager.showDeathMarkers =  Config.Bind("UltraNightmareConfig","showDeathMarkers","true","Display death markers of other players.");

        
        Logging.Message("--Done. The torment has begun...--");
        SceneManager.sceneLoaded += onSceneLoaded;
    }
    
    public void PrepareIntermissions()
    {
        GameObject ActEndPanel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"),"PowerUpVignette"),"Panel"),"Act End Message"),"Sound 1");

        Text Description = GetGameObjectChild(ActEndPanel, "Text").GetComponent<Text>();
        Description.text =
            "ACT COMPLETE\n\n\n\n";

        if (RunManager.saveType == SaveTypeEnum.NO_SAVE)
        {
            Description.text += "THE TORMENT CONTINUES.";
        }
        else
        {
            Description.text += "YOU MAY <color=orange>CONTINUE</color> THE TORMENT\nOR YOU MAY NOW <color=orange>SAVE & EXIT</color> YOUR RUN";
        }
        
        GetGameObjectChild(ActEndPanel, "Menu").GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
        GetGameObjectChild(ActEndPanel, "Menu").GetComponent<Button>().onClick.AddListener(() =>
        {
            Logging.Info("Saving run");
            RunManager.SaveRun();
            SceneHelper.LoadScene("Main Menu");
        });
        
        Text Continue = GetGameObjectChild(GetGameObjectChild(ActEndPanel, "Insert"),"Text").GetComponentInChildren<Text>();
        Continue.text = "CONTINUE RUN";
        
        Text Quit = GetGameObjectChild(GetGameObjectChild(ActEndPanel, "Menu"),"Text").GetComponentInChildren<Text>();
        Quit.text = "SAVE RUN & EXIT";
        
        //If is set to no save, hide the Exit button
        if (RunManager.saveType == SaveTypeEnum.NO_SAVE)
        {
            GetGameObjectChild(ActEndPanel, "Menu").SetActive(false);
        }
    }

    public static async void DisplayCompletedRun()
    {
        GameObject EAEndRoot = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Canvas"),"UnderwaterOverlay"),"Panel"),"Skippables");
        
        //Disable the social panels to free up some space - There are two children called "Panel" in the targetted gameObject so we get via index
        GameObject Panels = EAEndRoot.transform.GetChild(3).gameObject;
        Panels.SetActive(false);
        
        TextMeshProUGUI Text = GetGameObjectChild(EAEndRoot, "Text").GetComponent<TextMeshProUGUI>();
        Text.text =
            "<size=48>ULTRANIGHTMARE COMPLETED - "+ getColoredDifficultyName() + " (RADIANCE TIER "+RunManager.enemyRadianceLevel + ")</size>\n\n"
            + "You have survived the torment.\n\n\n"
            + "<align=\"left\">Run stats:\n\n"
            + "SAVE TYPE: " + RunManager.getSaveTypeString(RunManager.saveType)+"\n"
            + "TOTAL RUN TIME: <color=orange>" + getFormattedTime(RunManager.runTime) + "</color>\n"
            + "TOTAL STYLE POINTS: <color=orange>" + RunManager.runStylePoints + "</color>\n"
            + "</align>";
        
        //If save is using non-whitelisted mods, warn that run won't be publicly visible & don't unlock next radiance tier
        if (!RunManager.saveUsesWhitelistedMods || !RunManager.isUsingWhitelistedMods)
        {
            Text.text += "\n\n<color=red>NON-WHITELISTED MODS USED, UPLOADED RUN WILL BE HIDDEN BY DEFAULT</color>";
        }
        
        //If player completed a run on the current max available radiance, unlock the next tier (maxes out at radiance 10)
        if (RunManager.enemyRadianceLevel == RunManager.unlockedMaxRadiance && RunManager.unlockedMaxRadiance < 10)
        {
            RunManager.unlockedMaxRadiance++;
            Text.text += "\n\n<color=orange>RADIANCE TIER <color=red>"+RunManager.unlockedMaxRadiance+"</color> UNLOCKED FOR FUTURE RUNS</color>";
        }
        
        else if (RunManager.enemyRadianceLevel == 10)
        {
            Text.text += "\n\n<color=orange>MAX RADIANCE TIER COMPLETED! CONGRATULATIONS</color>";
        }
        
        
        GetGameObjectChild(EAEndRoot, "Quit Mission").GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
        GetGameObjectChild(EAEndRoot, "Quit Mission").GetComponent<Button>().onClick.AddListener(() =>
        {
            RunManager.WipeSaveData();
            SceneHelper.LoadScene("Main Menu");
        });
        
        //Submit run data after so we're not blocking everything before.
        await RunManager.SendCompletedRunToServer();

    }
    
    //Scene switch
    public async void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (getSceneName())
        {
            case "Main Menu":
            {
                MainMenu.PrepareMainMenu();
                break;
            }
            case "EarlyAccessEnd":
            {
                if (RunManager.isInRun)
                {
                    DisplayCompletedRun();
                }
                break;
            }
            default:
            {
                if (getSceneName().Contains("Intermission"))
                {
                    PrepareIntermissions();
                }
                
                if (getSceneName().Contains("Level ") && RunManager.isInRun)
                {
                    //Disable any alt exits for the level we're currently in.
                    CampaignPatches.Apply(getSceneName());
                    
                    RunManager.hasSent = false;
                    
                    // Fetch death markers for both the player and a random handful of other player death markers and put them in the level
                    Logging.Info("Fetching death markers for " + getSceneName());
                    if (RunManager.showDeathMarkers.Value == "true")
                    {
                        List<DeathData> deaths = await RunManager.GetDeaths();
                        foreach (DeathData death in deaths)
                        {
                            GameObject DeathMarker = GameObject.Instantiate(AssetLoader.DeathMarker);
                            GetGameObjectChild(DeathMarker, "Data").AddComponent<AlwaysLookAtCamera>();
                            GetGameObjectChild(DeathMarker,"Data").transform.localScale = new Vector3(-1f, 1f, 1f);
                            DeathMarker.AddComponent<AlwaysLookAtCamera>();
                            DeathMarker.transform.localPosition = new Vector3(death.x,death.y,death.z);
                        
                            // Display death marker data
                            GetGameObjectChild(DeathMarker, "Data").GetComponent<TextMeshPro>().text =
                                death.steamUsername + "\n" 
                                                    + DateTimeOffset.Parse(death.deathDate).ToString("yyyy-MM-dd HH:mm:ss") + "\n"
                                                    + (death.killedBy != "" ? ("Killed by: "+ death.killedBy) : "");
                        }
                    }


                    // Set up damage/death tracking (cheers Flazhik <3 )
                    EnemyIdentifierManager.Instance.OnPlayerHurt += hurtEvent =>
                    {
                        if (hurtEvent.EnemyId != null)
                        {
                            RunManager.lastHitBy = hurtEvent.EnemyId.FullName;
                        }

                        if (hurtEvent.PlayerIsKilled)
                        {
                            if (hurtEvent.EnemyId != null)
                            {
                                RunManager.lastHitBy = hurtEvent.EnemyId.FullName;
                            }
                            else
                            {
                                Logging.Warn("Unable to ascertain what killed the player! Setting death to unknown");
                                RunManager.lastHitBy = "Unknown";
                            }
                        }
                    };

                }
                break;
            }
        }
    }
}