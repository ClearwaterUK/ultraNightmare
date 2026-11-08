using System.Collections;
using HarmonyLib;
using Sandbox;
using UnityEngine;
using UnityEngine.UI;

namespace UltraNightmare.HarmonyPatches;

using static UltraNightmare.CommonFunctions;

[HarmonyPatch(typeof(EnemySpawnableInstance), "OnEnable")]
public static class SetEnemyRadiance
{
    [HarmonyPostfix]
    public static void SetEnemyRadiancePatch(EnemySpawnableInstance __instance)
    {
        if (RunManager.isInRun && RunManager.enemyRadianceLevel > 0)
        {
            //Wait for the enemy object to finish spawning in before applying radiance
            __instance.StartCoroutine(RadianceAfterTime(0.25f, __instance));
        }
    }
    
    public static IEnumerator WaitforSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
        
    public static IEnumerator RadianceAfterTime(float time, EnemySpawnableInstance instance)
    {
        yield return new WaitForSeconds(time);
        instance.radiance.enabled = true;
        instance.radiance.tier = RunManager.enemyRadianceLevel;
        instance.UpdateRadiance();
    }
}