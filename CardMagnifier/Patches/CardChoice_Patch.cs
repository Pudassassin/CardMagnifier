using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using UnboundLib;

using CardMagnifier.MonoBehaviors;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(CardChoice))]
    class CardChoice_Patch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("SpawnUniqueCard")]
        static void AddComponentToSpawnedCard(ref GameObject __result)
        {
            CardVisuals cardVisuals = __result.transform.GetComponentInChildren<CardVisuals>();
            cardVisuals.gameObject.AddComponent<CardEnlarger>();
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("DoPick")]
        static bool RemoveComponentToPickedCard(ref List<GameObject> ___spawnedCards, int picksToSet)
        {
            if (picksToSet < 0 || picksToSet >= ___spawnedCards.Count)
            {
                return true;
            }
            GameObject targetCard = ___spawnedCards[picksToSet];
            CardEnlarger cardEnlarger = targetCard.transform.GetComponentInChildren<CardEnlarger>();
            cardEnlarger.effectEnabled = false;

            return true;
        }

        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.Last)]
        // [HarmonyPatch("DoPick")]
        // static void SignalCardIsPicked()
        // {
        //     UnityEngine.Debug.Log("[CardMagnifier] SignalCardIsPicked fired");
        //     CardEnlarger.isCardPickPhase = false;
        // }

        // static void ApplyHealMultiplier(Player ___player, ref float healAmount)
        // {
        //     float healMuliplier = 1.0f;
        // 
        //     TacticalScannerStatus scannerStatus = ___player.GetComponent<TacticalScannerStatus>();
        //     if (scannerStatus != null)
        //     {
        //         healMuliplier *= scannerStatus.GetHealMultiplier();
        //     }
        // 
        //     HollowLifeEffect hollowLifeEffect = ___player.GetComponent<HollowLifeEffect>();
        //     if (hollowLifeEffect != null)
        //     {
        //         healMuliplier *= hollowLifeEffect.GetHealMultiplier();
        //     }
        // }
    }
}