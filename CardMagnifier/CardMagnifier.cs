using System.Collections.Generic;
using System;

using BepInEx;
using UnboundLib;
using UnboundLib.Cards;

using CardMagnifier.MonoBehaviors;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnityEngine;

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
        public const string Version = "0.0.3"; //build #3 / Release 0-1-0

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

            this.ExecuteAfterSeconds(2.0f, () =>
            {
                // Debug.Log("[CardMagnifier] Delegate started!");

                GameObject targetCardBase = null;
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];


                if (gameObjects != null)
                {
                    foreach (GameObject item in gameObjects)
                    {
                        // if (item == null) continue;
                        // if (item.hideFlags == HideFlags.HideAndDontSave)
                        // {
                            if (item.name == "CardBase")
                            {
                                // Debug.Log("[CardMagnifier] Found it!");
                                targetCardBase = item;
                                break;
                            }
                        // }
                    }
                }
                else
                {
                    // Debug.Log("[CardMagnifier] List is null!");
                }

                // apply mono to card base(s)
                if (targetCardBase != null)
                {
                    targetCardBase.AddComponent<CardEnlarger>();
                    // Debug.Log("[CardMagnifier] Component added!");
                }
            });
        }
    }



}
