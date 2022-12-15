using System.Collections.Generic;
using System;

using BepInEx;
using UnboundLib;
using UnboundLib.Cards;

using CardMagnifier.MonoBehaviors;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnityEngine;
using UnboundLib.GameModes;
using System.Collections;

namespace CardMagnifier
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class CardMagnifier : BaseUnityPlugin
    {
        private const string ModId = "com.pudassassin.rounds.CardMagnifier";
        private const string ModName = "CardMagnifier";
        public const string Version = "0.0.7"; //build #7 / Release 0-1-0

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            // CustomCard.BuildCard<MyCardName>();
            Unbound.RegisterClientSideMod(ModId);

            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart);

            this.ExecuteAfterFrames(5, () =>
            {
                // Debug.Log("[CardMagnifier] Delegate started!");

                // GameObject targetCardBase = null;
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];


                if (gameObjects != null)
                {
                    foreach (GameObject item in gameObjects)
                    {
                        // Attach to all 'card bases' that contain CardVisual
                        CardVisuals cardVisuals = item.GetComponent<CardVisuals>();
                        if (cardVisuals != null)
                        {
                            // apply mono to card base(s)
                            // Debug.Log("[CardMagnifier] Found it!");

                            item.AddComponent<CardEnlarger>();

                            // Debug.Log("[CardMagnifier] Component added to xxx!");

                            // targetCardBase = item;
                            // break;
                        }

                    }
                }
                else
                {
                    // Debug.Log("[CardMagnifier] List is null!");
                }
                
            });
        }

        public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        {
            GameObject timerUI = GameObject.Find("TimerUI");
            if (timerUI != null)
            {
                timerUI.transform.localPosition = new Vector3(0.0f, -360.0f, 0.0f);
            }

            yield break;
        }

    }

}
