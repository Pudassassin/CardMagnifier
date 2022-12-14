using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnboundLib.GameModes;

namespace CardMagnifier.MonoBehaviors
{
    // this mono is meant to be attached at vanilla CardBase and potentially customized ones from mods
    // Card is clone when pick card phase start and offering player, and is destroyed afterward

    public class CardEnlarger : MonoBehaviour
    {
        public static List<String> BlacklistedCards = new List<String>()
        {
            // PCE Glitch cards
            "__PCE__Common",
            "__PCE__Uncommon",
            "__PCE__Rare"
        };

        public static Vector2Int defaultResolution = new Vector2Int(1920, 1080); // in case of adaptive zoom feature

        // scale 0-1, 0 = original card pos, 1 = zoom to configured pos
        public static float configPosInterpolateFactor = 0.95f;
        public static Vector3 configZoomToPos = new Vector3(0.0f, 3.0f, -10.0f);

        // whether to zoom the entire card to fixed scales or not, instead of relative to its default size when hilighted
        public static bool configZoomAbsoluteEnable = true;
        public static float configZoomScale = 1.35f;

        public static bool configReorientCardEnable = true;
        public static Vector3 configZoomRotation = Vector3.zero; // WIP

        public static float configInterpolateTime = 0.2f;

        public Vector3 cardZoomToPos, cardPreviousPos, cardPreviousRotation;
        public Vector3 cardZoomToScale, cardPreviousScale;

        public const float processTickTime = 0.0f;
        public float processTimer = 0.0f;
        public float interpolateTimer = 0.0f;
        public bool effectEnabled = false; // should only be true during card picking phase
        public bool cardIsHighLighted = false;
        public bool cardPrevStateSaved = false;
        public bool cardIsPicked = false;

        private CardVisuals cardVisuals = null;
        private GameObject cardBaseParent = null;

        public void Awake()
        {

        }

        public void Start()
        {
            cardVisuals = GetComponent<CardVisuals>();
            cardBaseParent = transform.parent.gameObject;

            Action<bool> action = GetToggleSelectionAction();
            cardVisuals.toggleSelectionAction = (Action<bool>)Delegate.Combine(cardVisuals.toggleSelectionAction, action);
        }

        public void Update()
        {
            if (cardBaseParent != null && CardChoice.instance.IsPicking)
            {
                if (CheckCardIsBlacklisted())
                {
                    Destroy(this);
                }
                else
                {
                    processTimer += TimeHandler.deltaTime;
                    // if (processTimer >= 3.0f)
                    // {
                    effectEnabled = true;
                    
                    if (!cardPrevStateSaved)
                    {
                        SetupCardEnlarger();
                    }
                    // }
                }
            }

            if (effectEnabled)
            {
                if (cardIsHighLighted)
                {
                    interpolateTimer += TimeHandler.deltaTime;
                }
                else
                {
                    interpolateTimer -= TimeHandler.deltaTime;
                }

                interpolateTimer = Mathf.Clamp(interpolateTimer, 0.0f, configInterpolateTime);

                if (!cardIsPicked)
                {
                    SetCardZoom(interpolateTimer);
                }
            }
        }

        public void SetupCardEnlarger()
        {
            cardPreviousPos = transform.parent.transform.position; //world space position
            cardPreviousRotation = transform.parent.transform.localEulerAngles;
            cardPreviousScale = transform.parent.transform.localScale;

            cardZoomToPos = Vector3.Lerp(cardPreviousPos, configZoomToPos, configPosInterpolateFactor);
            if (configZoomAbsoluteEnable)
            {
                cardZoomToScale = Vector3.one * configZoomScale;
            }
            else
            {
                cardZoomToScale = cardPreviousScale * configZoomScale;
            }

            cardPrevStateSaved = true;
        }

        private void SetCardZoom(float time)
        {
            float iValue = time / configInterpolateTime;
            iValue = BinomialToLerpMapping(Mathf.Clamp01(iValue));

            cardBaseParent.transform.localPosition = Vector3.Lerp(cardPreviousPos, cardZoomToPos, iValue);
            cardBaseParent.transform.localScale = Vector3.Lerp(cardPreviousScale, cardZoomToScale, iValue);
            if (configReorientCardEnable)
            {
                cardBaseParent.transform.localEulerAngles = V3RotationalLerp(cardPreviousRotation, configZoomRotation, iValue);
            }
        }

        private Vector3 V3RotationalLerp(Vector3 start, Vector3 end, float t)
        {
            float tC = Mathf.Clamp01(t);
            return new Vector3
            (
                Mathf.LerpAngle(start.x, end.x, tC),
                Mathf.LerpAngle(start.y, end.y, tC),
                Mathf.LerpAngle(start.z, end.z, tC)
            );
        }

        private float BinomialToLerpMapping(float x)
        {
            // formula
            float xC = Mathf.Clamp01(x);
            return (-1.0f * xC * xC) + (2 * xC);
        }

        public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        {
            if (CheckCardIsBlacklisted())
            {
                effectEnabled = false;
            }
            else
            {
                effectEnabled = true;
            }
            yield break;
        }

        public IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
        {
            cardIsPicked = true;
            yield break;
        }

        public bool CheckCardIsBlacklisted()
        {
            foreach (String item in BlacklistedCards)
            {
                if (transform.parent.name.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public Action<bool> GetToggleSelectionAction()
        {
            return delegate (bool highLightState)
            {
                if (effectEnabled)
                {
                    if (cardBaseParent == null)
                    {
                        cardBaseParent = transform.parent.gameObject;
                        return;
                    }

                    if (!cardPrevStateSaved)
                    {
                        SetupCardEnlarger();
                    }

                    cardIsHighLighted = highLightState;
                }
            };
        }

        // debug
        public void ResetTimer()
        {
            interpolateTimer = 0.0f;
        }
    }
}
