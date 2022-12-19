using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using UnboundLib;

using CardMagnifier.MonoBehaviors;

namespace CardMagnifier.Patches
{
    [HarmonyPatch(typeof(CardBar))]
    class CardBar_Patch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("OnHover")]
        static void OverrideCardBarPreviewTransform(GameObject ___currentCard)
        {
            float rescale = CardEnlarger.mapEmbiggenerScale * CardEnlarger.screenResolutionScale;
            ___currentCard.transform.position = CardEnlarger.configCardBarPreviewPosition * rescale;
            ___currentCard.transform.localScale = Vector3.one * CardEnlarger.configCardBarPreviewScale * rescale;
        }
    }

    [HarmonyPatch(typeof(CardInfo))]
    class CardInfo_Patch
    {
        public static GameObject selectedCard;

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("RPCA_ChangeSelected")]
        static void AddCardEnlarger(CardInfo __instance, bool setSelected)
        {
            if (setSelected == false)
            {
                return;
            }
        
            selectedCard = __instance.transform.root.gameObject;
            // UnityEngine.Debug.Log("[CardInfo_Patch] RPCA_ChangedSelected: " + selectedCard.name);
        
            CardEnlarger cardEnlarger;
            cardEnlarger = selectedCard.GetComponent<CardEnlarger>();
        
            if (cardEnlarger == null)
            {
                cardEnlarger = selectedCard.AddComponent<CardEnlarger>();
                cardEnlarger.EnableCardZoom();
            }
        }

    }

    [HarmonyPatch(typeof(CardChoice))]
    class CardChoice_Patch
    {
        // darn it CardChoice.SpawnUniqueCard only be executed by the PICKING player and sync to other players...???

        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.Last)]
        // [HarmonyPatch("SpawnUniqueCard")]
        

        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.Last)]
        // [HarmonyPatch("Spawn")]
        // static void AddComponentToSpawnedCard(ref GameObject __result)
        // {
        //     CardVisuals cardVisuals = __result.transform.GetComponentInChildren<CardVisuals>();
        //     CardEnlarger pickedCardEnlarger = cardVisuals.gameObject.transform.parent.gameObject.AddComponent<CardEnlarger>();
        //     pickedCardEnlarger.EnableCardZoom();
        // 
        //     CardEnlarger.isCardPickPhase = true;
        //     UnityEngine.Debug.Log("[CardChoice_Patch] AddComponentToSpawnedCard Done");
        // }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("IDoEndPick")]
        static bool RemoveComponentToPickedCard(ref List<GameObject> ___spawnedCards, GameObject pickedCard, int theInt)
        {
            // if (theInt < 0 || theInt >= ___spawnedCards.Count)
            // {
            //     return true;
            // }
            // GameObject pickedCard = ___spawnedCards[theInt];

            CardEnlarger pickedCardEnlarger = pickedCard.transform.GetComponentInChildren<CardEnlarger>();
            foreach (GameObject item in ___spawnedCards)
            {
                if (item == pickedCard)
                {
                    if (pickedCardEnlarger != null)
                    {
                        pickedCardEnlarger.SetCardPicked();
                    }
                    continue;
                }
                else
                {
                    CardEnlarger cardEnlarger = item.transform.GetComponentInChildren<CardEnlarger>();
                    if (cardEnlarger != null)
                    {
                        cardEnlarger.setCardDiscarded();
                    }
                }
            }

            // pickedCardEnlarger.zoomEffectEnabled = false;

            CardEnlarger.isCardPickPhase = false;
            // UnityEngine.Debug.Log("[CardChoice_Patch] RemoveComponentToPickedCard Done");
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