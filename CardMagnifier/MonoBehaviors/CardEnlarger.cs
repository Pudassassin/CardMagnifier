using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnboundLib.GameModes;
using UnboundLib;

namespace CardMagnifier.MonoBehaviors
{
    // this mono is meant to be attached at vanilla CardBase and potentially customized ones from mods
    // Card is clone when pick card phase start and offering player, and is destroyed afterward

    public class CardEnlarger : MonoBehaviour
    {
        public static List<String> BlacklistedCards = new List<String>()
        {
            // PCE Glitch cards
            // "__PCE__Common",
            // "__PCE__Uncommon",
            // "__PCE__Rare"
        };

        public static Vector2Int defaultResolution = new Vector2Int(1920, 1080); // in case of adaptive zoom feature
        // public static float mapEmbiggenerScale = 1.0f;

        // scale 0-1, 0 = original card pos, 1 = zoom to configured pos
        public static float configPosInterpolateFactor = 1.0f;
        public static Vector3 configZoomToPos = new Vector3(0.0f, 4.5f, -10.0f);

        // whether to zoom the entire card to fixed scales or not, instead of relative to its default size when hilighted
        public static bool configZoomAbsoluteEnable = true;
        public static float configZoomScale = 1.35f;

        public static bool configReorientCardEnable = true;
        public static Vector3 configZoomRotation = Vector3.zero; // WIP

        public static float configInterpolateTime = 0.01f;

        public Vector3 cardZoomToPos, cardPreviousPos, cardPreviousRotation;
        public Vector3 cardZoomToScale, cardPreviousScale;

        // Other QoL
        public static bool configDisableDiscardEffect = false;
        public static bool configDisableCardBobbingEffect = false;
        public static bool configDisableCardFlippingEffect = false;

        // CardBar Previews
        public static Vector3 configCardBarPreviewPosition = new Vector3(25.0f, 0.0f, -10.0f);
        public static float configCardBarPreviewScale = 1.35f;

        // Discarded card effect variables
        public static float initialXSpeedMin = -2.0f;
        public static float initialXSpeedMax = 2.0f;
        public static float initialYSpeedMin = 7.5f;
        public static float initialYSpeedMax = 5.0f;
        public static float initialYTorqueMin = -180.0f;
        public static float initialYTorqueMax = 180.0f;
        public static float initialZTorqueMin = -45.0f;
        public static float initialZTorqueMax = 45.0f;
        public static float GravityMin = 7.5f;
        public static float GravityMax = 10.0f;

        public static float DeltaTimeScale = 2.5f;

        public Vector3 randomizedVelocity = Vector3.zero;
        public Vector3 randomizedTorque = Vector3.zero;
        public float randomizedGravity = 0.0f;

        public bool isCardDiscarded = false;

        public Vector3 tPosition, tRotation;

        // Picked card effect variable
        public static Vector3 finalCardPos = new Vector3(0.0f, -9.0f, 0.0f);
        public static float timeToVanish = 0.25f;

        public float vanishTimer = 0.0f;
        public Vector3 vanishingCardPos, vanishingCardScale;

        // Inner variables
        public const float processTickTime = 0.0f;
        public float processTimer = 0.0f;
        public float interpolateTimer = 0.0f;

        public bool zoomEffectEnabled = false;
        public bool discardEffectEnable = false;
        public bool pickedEffectEnable = false;

        public bool cardIsHighLighted = false;
        public bool cardPrevStateSaved = false;

        public static bool isCardPickPhase = false;

        private CardVisuals cardVisuals = null;
        private GameObject cardBaseParent = null;

        // debug
        private bool cardLifeExtended = false;
        public static float cardLifeTimeAdd = 0.0f;
        public float cardLifeExtendedTime = 0.0f; // realtime marker
        public static float realTimeToRemove = 1.5f; // realtime duration to remove

        public void Awake()
        {

        }

        public void Start()
        {
            cardVisuals = transform.GetComponentInChildren<CardVisuals>();
            // cardBaseParent = transform.parent.gameObject;
            cardBaseParent = transform.gameObject;

            Action<bool> action = GetToggleSelectionAction();
            cardVisuals.toggleSelectionAction = (Action<bool>)Delegate.Combine(cardVisuals.toggleSelectionAction, action);

            // GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
            
            // wip patch
            // if (cardVisuals.isSelected)
            // {
            //     cardBaseParent = transform.parent.gameObject;
            //     SetupCardEnlarger();
            //     zoomEffectEnabled = true;
            // }
        }

        public void Update()
        {
            if (cardVisuals == null)
            {
                cardVisuals = transform.GetComponentInChildren<CardVisuals>();
            }
        }

        public void LateUpdate()
        {
            // if (cardBaseParent != null && CardChoice.instance.IsPicking)
            // if (CardChoice.instance.IsPicking)
            // {
            //     if (CheckCardIsBlacklisted())
            //     {
            //         // leave it disabled
            //         // Destroy(this);
            //     }
            //     else
            //     {
            //         // processTimer += TimeHandler.deltaTime;
            // 
            //         zoomEffectEnabled = true;
            //         
            //         if (!cardPrevStateSaved)
            //         {
            //             SetupCardEnlarger();
            //         }
            //     }
            // }

            if (zoomEffectEnabled)
            {
                if (cardIsHighLighted)
                {
                    interpolateTimer += TimeHandler.deltaTime;

                    if (configDisableCardBobbingEffect)
                    {
                        cardVisuals.transform.localScale = Vector3.one * 1.15f;
                    }

                    // wip disable card flip
                    if (configDisableCardFlippingEffect)
                    {
                        Transform canvas = cardVisuals.transform.Find("Canvas");
                        Vector3 temp = canvas.localEulerAngles;
                        canvas.localEulerAngles = new Vector3(temp.x, 0.0f, temp.z);

                        RectTransform rectTransform = canvas.gameObject.GetComponent<RectTransform>();
                        temp = rectTransform.localEulerAngles;
                        rectTransform.localEulerAngles = new Vector3(temp.x, 0.0f, -1 * temp.z);

                        // CurveAnimation curveAnimation = canvas.gameObject.GetComponent<CurveAnimation>();
                        // curveAnimation.stopAllAnimations = true;
                    }
                }
                else
                {
                    interpolateTimer -= TimeHandler.deltaTime;

                    if (configDisableCardBobbingEffect)
                    {
                        transform.localScale = Vector3.one * 0.9f;
                    }
                }


                interpolateTimer = Mathf.Clamp(interpolateTimer, 0.0f, configInterpolateTime);
                SetCardZoom();
            }

            // picked card effect
            else if (pickedEffectEnable)
            {
                vanishTimer += TimeHandler.deltaTime;

                cardBaseParent.transform.localPosition = Vector3.Lerp(vanishingCardPos, finalCardPos, vanishTimer / timeToVanish);
                cardBaseParent.transform.localScale = Vector3.Lerp(vanishingCardScale, Vector3.zero, vanishTimer / timeToVanish);
            }

            // not picked card discard effect fix/enhancer
            else if (discardEffectEnable && !configDisableDiscardEffect)
            {
                if (!cardIsHighLighted)
                {
                    // tPosition += randomizedVelocity * TimeHandler.deltaTime;
                    // tRotation += randomizedTorque * TimeHandler.deltaTime;
                    // randomizedVelocity.y -= randomizedGravity * TimeHandler.deltaTime;
                    // 
                    // cardBaseParent.transform.position = tPosition;
                    // cardBaseParent.transform.localEulerAngles = tRotation;

                    cardBaseParent.transform.position += randomizedVelocity * TimeHandler.deltaTime * DeltaTimeScale;
                    cardBaseParent.transform.localEulerAngles += randomizedTorque * TimeHandler.deltaTime * DeltaTimeScale;
                    randomizedVelocity.y -= randomizedGravity * TimeHandler.deltaTime * DeltaTimeScale;
                }

                if (!cardLifeExtended)
                {
                    RemoveAfterSeconds remover = cardBaseParent.GetComponent<RemoveAfterSeconds>();
                    if (remover != null)
                    {
                        remover.seconds += cardLifeTimeAdd;
                        cardLifeExtended = true;
                        cardLifeExtendedTime = Time.time;
                    }
                }
                else
                {
                    if (Time.time > cardLifeExtendedTime + realTimeToRemove)
                    {
                        Destroy(cardBaseParent);
                    }
                }
            }
            // else
            // {
            //     setCardDiscarded();
            // }
        }

        public void SetupCardEnlarger()
        {
            if (cardPrevStateSaved) return;

            cardPreviousPos = cardBaseParent.transform.position; //world space position
            cardPreviousRotation = cardBaseParent.transform.localEulerAngles;
            cardPreviousScale = cardBaseParent.transform.localScale;

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

        private void SetCardZoom()
        {
            float iValue;
            if (configInterpolateTime <= 0.0f)
            {
                iValue = 1.0f;
            }
            else
            {
                iValue = interpolateTimer / configInterpolateTime;
                iValue = BinomialToLerpMapping(Mathf.Clamp01(iValue));
            }

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

        public void SetupRandomizedCardDiscard()
        {
            if (isCardDiscarded) return;

            randomizedGravity = UnityEngine.Random.Range(GravityMin, GravityMax);
            randomizedVelocity = new Vector3
            (
                UnityEngine.Random.Range(initialXSpeedMin, initialXSpeedMax),
                UnityEngine.Random.Range(initialYSpeedMin, initialYSpeedMax),
                0.0f
            );
            randomizedTorque = new Vector3
            (
                0.0f,
                UnityEngine.Random.Range(initialYTorqueMin, initialYTorqueMax),
                UnityEngine.Random.Range(initialZTorqueMin, initialZTorqueMax)
            );

            tPosition = cardBaseParent.transform.position;
            tRotation = cardBaseParent.transform.localEulerAngles;

            isCardDiscarded = true;
        }

        // public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        // {
        //     if (CheckCardIsBlacklisted())
        //     {
        //         zoomEffectEnabled = false;
        //     }
        //     else
        //     {
        //         zoomEffectEnabled = true;
        //     }
        //     yield break;
        // }

        // public IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
        // {
        //     // cardPickEnded = true;
        // 
        //     if (!cardIsHighLighted)
        //     {
        //         SetupRandomizedCardDiscard();
        //     }
        //     
        //     yield break;
        // }

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
                // hack solution
                // CardEnlarger.isCardPickPhase = true;

                if (zoomEffectEnabled)
                {
                    if (cardBaseParent == null)
                    {
                        cardBaseParent = transform.parent.gameObject;
                        return;
                    }

                    SetupCardEnlarger();

                    cardIsHighLighted = highLightState;
                }
            };
        }

        public void EnableCardZoom()
        {
            this.ExecuteAfterFrames(5, () =>
            {
                zoomEffectEnabled = true;
                pickedEffectEnable = false;
                discardEffectEnable = false;

                SetupCardEnlarger();

                if (cardVisuals != null)
                {
                    if (cardVisuals.isSelected)
                    {
                        cardIsHighLighted = true;
                    }
                }
            });
        }

        public void SetCardPicked()
        {
            zoomEffectEnabled = false;
            pickedEffectEnable = true;
            discardEffectEnable = false;

            vanishingCardPos = cardBaseParent.transform.localPosition;
            vanishingCardScale = cardBaseParent.transform.localScale;
        }

        public void setCardDiscarded()
        {
            if (pickedEffectEnable) return;
            zoomEffectEnabled = false;
            discardEffectEnable = true;

            SetupRandomizedCardDiscard();
        }

        public void OnDestroy()
        {
            // GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
        }

        // debug
        public void ResetTimer()
        {
            interpolateTimer = 0.0f;
        }
    }
}
