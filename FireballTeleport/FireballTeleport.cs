using System.Linq;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using DG.Tweening;
using UnityEngine;
using Wish;
using Object = UnityEngine.Object;

[BepInPlugin("com.76eren.FireballTeleport", "Fireball Teleport", "1.0.0")]
public class FireballTeleport : BaseUnityPlugin
{
    private static ManualLogSource log;

    private void Awake()
    {
        log = Logger;
        Harmony.CreateAndPatchAll(typeof(FireballTeleport));
        log.LogInfo("Patches applied.");
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnSuccesfulHit))]
    [HarmonyPrefix]    
    static bool Prefix(Projectile __instance)
    {
        // Make reference to the player
        var players = FindObjectsOfType<Player>()
            .Select(rb => rb.gameObject)
            .ToList();
        GameObject player = players[0];

        // Reference fireball
        GameObject gameObject = __instance.gameObject;
        
        if (gameObject.name == "Fireball(Clone)")
        {
            log.LogInfo("Teleporting player to fireball position!");
            player.transform.position = gameObject.transform.position;
        }
        
        return true;
    }
}