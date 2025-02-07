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
        
        // Access private members using reflection
        FieldInfo projectileTypeField = typeof(Wish.Projectile).GetField("_projectileType", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo pierceCountField = typeof(Wish.Projectile).GetField("_pierceCount", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo numPiercesField = typeof(Wish.Projectile).GetField("_numPierces", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo hasExplodeAnimationField = typeof(Wish.Projectile).GetField("hasExplodeAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo delayField = typeof(Wish.Projectile).GetField("delay", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo animField = typeof(Wish.Projectile).GetField("anim", BindingFlags.NonPublic | BindingFlags.Instance);
            
        // Access the current values of the private members
        ProjectileType projectileType = (Wish.ProjectileType)projectileTypeField.GetValue(__instance);
        int pierceCount = (int)pierceCountField.GetValue(__instance);
        int numPierces = (int)numPiercesField.GetValue(__instance);
        bool hasExplodeAnimation = (bool)hasExplodeAnimationField.GetValue(__instance);
        float delay = (float)delayField.GetValue(__instance);
        Animator anim = (Animator)animField.GetValue(__instance);

        GameObject gameObject = __instance.gameObject;
        
        switch (projectileType)
        {
            case ProjectileType.Piercing:
                pierceCount++;
                pierceCountField.SetValue(__instance, pierceCount);
                if (pierceCount > numPierces)
                {
                    Destroy(gameObject);
                    return false; 
                }
                break;

            case ProjectileType.InfinitePierce:
                break;

            default:
                // Teleport player to the fireball position
                if (gameObject.name == "Fireball(Clone)")
                {
                    log.LogInfo("Teleporting player to fireball position!");
                    player.transform.position = gameObject.transform.position;
                }
                    
                if (hasExplodeAnimation)
                {
                    anim.Play("Explosion");
                    DOVirtual.DelayedCall(delay, () => Object.Destroy(gameObject));
                }
                else
                {
                    Destroy(gameObject);
                }
                break;
        }
        
        return false; 
        
    }
}