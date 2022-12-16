﻿using System.Collections.Generic;
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
        public const string Version = "0.0.23"; //build #23 / Release 0-1-0

        public static GameObject timerUI = null;

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
            // GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);

        }

        public void Update()
        {
            
        }

        public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        {
            UnityEngine.Debug.Log("[CardMagnifier] Player Picking Started");

            CardEnlarger.isCardPickPhase = true;

            if (timerUI == null)
            {
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
                foreach (GameObject item in gameObjects)
                {
                    if (item.name == "TimerUI(Clone)")
                    {
                        timerUI = item;
                        UnityEngine.Debug.Log("[CardMagnifier] Found TimerUI(Clone)");
                        break;
                    }
                }
            }
            else
            {
                timerUI.transform.localPosition = new Vector3(0.0f, -360.0f, 0.0f);
            }
        
            yield break;
        }
        
        // public IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
        // {
        //     UnityEngine.Debug.Log("[CardMagnifier] Player Picking Ended");
        //     CardEnlarger.isCardPickPhase = false;
        // 
        //     yield break;
        // }

    }

}
