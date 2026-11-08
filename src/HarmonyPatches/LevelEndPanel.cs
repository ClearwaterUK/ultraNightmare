using System.Linq;
using HarmonyLib;

using static UltraNightmare.CommonFunctions;

namespace UltraNightmare.HarmonyPatches;


// If in run and P-Rank requirements are enabled, check if the player got a P-Rank. If not, restart the current level.
[HarmonyPatch(typeof(FinalRank),"LevelChange")]
public static class LevelEndChanger
{
    [HarmonyPrefix]
    public static bool handleLevelChange(FinalRank __instance, float ___savedTime, bool force = false)
    {
        if(RunManager.isInRun)
        {
            StatsManager sman = MonoSingleton<StatsManager>.Instance;
            RunManager.runTime += sman.seconds;
            RunManager.runStylePoints += sman.stylePoints;
            
            if (RunManager.requirePRank && !RunManager.gotPRank)
            {
                MonoSingleton<OptionsMenuToManager>.Instance.RestartMissionNoConfirm();
                return false;
            }

            RunManager.gotPRank = false;
            return true;
        }
        else
        {
            return true;
        }
    }
}

// Don't submit leaderboard times if we're in a run
[HarmonyPatch(typeof(LeaderboardController),"SubmitLevelScore")]
public class preventLeaderboardSubmission
{
    [HarmonyPrefix]
    public static bool preventLeaderboardSubmissionPatch(string levelName, int difficulty, float seconds,
        int kills, int style, int restartCount, bool pRank = false)
    {
        if (RunManager.isInRun)
        {
            return false;
        }
        return true;
    }
}

// When finishing a level in a run and P-Rank requirements are enabled, display a message if player failed to P-Rank the current level
[HarmonyPatch(typeof(FinalRank),"Update")]
public class FinalRankPRankCheck
{
    [HarmonyPostfix]
    public static void sendResult(FinalRank __instance, float ___savedTime, int ___savedStyle)
    {
        if(RunManager.isInRun)
        {
            if(RunManager.requirePRank && !RunManager.hasSent)
            {
                StatsManager sman = MonoSingleton<StatsManager>.Instance;
                if(sman != null)
                {
                    RunManager.hasSent = true;
                    if(!(sman.seconds <= sman.timeRanks.Last() && sman.kills >= sman.killRanks.Last() && sman.stylePoints >= sman.styleRanks.Last() && sman.restarts == 0))
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to obtain P-Rank. This level will be restarted.");
                        RunManager.gotPRank = false;
                    }
                    else
                    {
                        RunManager.gotPRank = true;
                    }
                }
                else
                {
                    Logging.Warn("Unable to get StatsManager?");
                }
            }
        }
    }
}