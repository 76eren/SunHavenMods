using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using I2.Loc;
using UnityEngine.Events;
using Wish;

[BepInPlugin("com.76eren.SleepWhenever", "Sleep Whenever", "1.0.0")]
public class SleepWhenever : BaseUnityPlugin
{
    private static ManualLogSource log;
    private Harmony harmony;

    private void Awake()
    {
        log = Logger;
        Harmony.CreateAndPatchAll(typeof(SleepWhenever));
        log.LogInfo("Patches applied.");
    }

    [HarmonyPatch(typeof(Player), nameof(Player.RequestSleep))]
    [HarmonyPrefix]
    static bool Prefix(
        Bed bed,
        bool isMarriageBed,
        MarriageOvernightCutscene marriageOvernightCutscene,
        bool isCutsceneComplete,
        Player __instance
    )
    {
        log.LogInfo("Running custom code!");
        // Player can sleep whenever they want (game starts at 6am)
        if (!isMarriageBed)
        {
            DialogueController.Instance.SetDefaultBox();
            DialogueController.Instance.PushDialogue(new DialogueNode()
            {
                dialogueText = new List<string>()
                {
                    ScriptLocalization.SleepRequest
                },
                responses = new Dictionary<int, Response>()
                {
                    {
                        0,
                        new Response()
                        {
                            responseText = (() => ScriptLocalization.Yes),
                            action = () => { StartSleep(bed, __instance); }
                        }
                    },
                    {
                        1,
                        new Response()
                        {
                            responseText = (StringFunction)(() => ScriptLocalization.No),
                            action = (UnityAction)(() =>
                            {
                                DialogueController.Instance.CancelDialogue(true, (UnityAction)null, true);
                                __instance.Sleeping = false;
                            })
                        }
                    }
                }
            });
        }
        else if (!isCutsceneComplete)
        {
            DialogueController.Instance.SetDefaultBox();
            DialogueController.Instance.PushDialogue(new DialogueNode()
            {
                dialogueText = new List<string>()
                {
                    ScriptLocalization.SleepRequestSpouse
                },
                responses = new Dictionary<int, Response>()
                {
                    {
                        0,
                        new Response()
                        {
                            responseText = (StringFunction)(() => ScriptLocalization.Yes),
                            action = (UnityAction)(() => marriageOvernightCutscene.Begin())
                        }
                    },
                    {
                        1,
                        new Response()
                        {
                            responseText = (StringFunction)(() => ScriptLocalization.No),
                            action = (UnityAction)(() =>
                            {
                                DialogueController.Instance.CancelDialogue(true, (UnityAction)null, true);
                                __instance.Sleeping = false;
                            })
                        }
                    }
                }
            });
        }
        else
        { 
            StartSleep(bed, __instance);
        }
        
        FieldInfo pausedField = typeof(Player).GetField("_paused", BindingFlags.Instance | BindingFlags.NonPublic);
        pausedField.SetValue(__instance, true);

        __instance.OnUnpausePlayer += (UnityAction)(() =>
        {
            DialogueController.Instance.CancelDialogue();
            __instance.Sleeping = false;
        });

        return false;
    }


    public static void StartSleep(Bed bed, Player __instance)
    {
        MethodInfo startSleepMethod =
            typeof(Player).GetMethod("StartSleep", BindingFlags.Instance | BindingFlags.NonPublic);
        startSleepMethod.Invoke(__instance, new object[] { bed });
    }
}