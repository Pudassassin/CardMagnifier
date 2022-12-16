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
            // "__PCE__Common",
            // "__PCE__Uncommon",
            // "__PCE__Rare"
        };

        public static Vector2Int defaultResolution = new Vector2Int(1920, 1080); // in case of adaptive zoom feature
        // public static float mapEmbiggenerScale = 1.0f;

        // scale 0-1, 0 = original card pos, 1 = zoom to configured pos
        public static float configPosInterpolateFactor = 1.0f;
        public static Vector3 configZoomToPos = new Vector3(0.0f, 3.5f, -10.0f);

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
        public static float initialYSpeedMin = 2.5f;
        public static float initialYSpeedMax = -1.5f;
        public static float initialYTorqueMin = -180.0f;
        public static float initialYTorqueMax = 180.0f;
        public static float initialZTorqueMin = -45.0f;
        public static float initialZTorqueMax = 45.0f;
        public static float GravityMin = 4.5f;
        public static float GravityMax = 10.0f;

        public static float DeltaTimeScale = 1.0f;

        public Vector3 randomizedVelocity = Vector3.zero;
        public Vector3 randomizedTorque = Vector3.zero;
        public float randomizedGravity = 0.0f;

        public bool isCardDiscarded = false;

        public Vector3 tPosition, tRotation;

        // Inner variables
        public const float processTickTime = 0.0f;
        public float processTimer = 0.0f;
        public float interpolateTimer = 0.0f;
        public bool effectEnabled = false; // should only be true during card picking phase
        public bool cardIsHighLighted = false;
        public bool cardPrevStateSaved = false;

        public static bool isCardPickPhase = false;

        private CardVisuals cardVisuals = null;
        private GameObject cardBaseParent = null;

        // debug
        private bool cardLifeExtended = false;
        public static float cardLifeTimeAdd = 0.2f;
        public float cardLifeExtendedTime = 0.0f; // realtime marker
        public static float realTimeToRemove = 1.0f; // realtime duration to remove

        public void Awake()
        {

        }

        public void Start()
        {
            cardVisuals = GetComponent<CardVisuals>();
            cardBaseParent = transform.parent.gameObject;

            Action<bool> action = GetToggleSelectionAction();
            cardVisuals.toggleSelectionAction = (Action<bool>)Delegate.Combine(cardVisuals.toggleSelectionAction, action);

            // GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
            
            // wip patch
            // if (cardVisuals.isSelected)
            // {
            //     cardBaseParent = transform.parent.gameObject;
            //     SetupCardEnlarger();
            //     effectEnabled = true;
            // }
        }

        public void LateUpdate()
        {
            // if (cardBaseParent != null && CardChoice.instance.IsPicking)
            if (CardChoice.instance.IsPicking)
            {
                if (CheckCardIsBlacklisted())
                {
                    // leave it disabled
                    // Destroy(this);
                }
                else
                {
                    // processTimer += TimeHandler.deltaTime;

                    effectEnabled = true;
                    
                    if (!cardPrevStateSaved)
                    {
                        SetupCardEnlarger();
                    }
                }
            }

            if (effectEnabled)
            {
                if (cardIsHighLighted)
                {
                    interpolateTimer += TimeHandler.deltaTime;

                    if (CardChoice.instance.IsPicking)
                    {
                        if (configDisableCardBobbingEffect)
                        {
                            transform.localScale = Vector3.one * 1.15f;
                        }

                        // wip disable card flip
                        if (configDisableCardFlippingEffect)
                        {
                            Transform canvas = gameObject.transform.Find("Canvas");
                            Vector3 temp = canvas.localEulerAngles;
                            canvas.localEulerAngles = new Vector3(temp.x, 0.0f, temp.z);

                            RectTransform rectTransform = canvas.gameObject.GetComponent<RectTransform>();
                            temp = rectTransform.localEulerAngles;
                            rectTransform.localEulerAngles = new Vector3(temp.x, 0.0f, -1 * temp.z);

                            // CurveAnimation curveAnimation = canvas.gameObject.GetComponent<CurveAnimation>();
                            // curveAnimation.stopAllAnimations = true;
                        }
                    }
                }
                else
                {
                    interpolateTimer -= TimeHandler.deltaTime;

                    if (CardChoice.instance.IsPicking)
                    {
                        if (configDisableCardBobbingEffect)
                        {
                            transform.localScale = Vector3.one * 0.9f;
                        }
                    }
                }


                interpolateTimer = Mathf.Clamp(interpolateTimer, 0.0f, configInterpolateTime);

                if (CardChoice.instance.IsPicking)
                {
                    SetCardZoom();
                }
                // not selected card discard effect fix/enhancer
                else
                {
                    if (!cardIsHighLighted)
                    {
                        if (!configDisableDiscardEffect)
                        {
                            SetupRandomizedCardDiscard();

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
                    }
                    else
                    {
                        // Destroy(this);
                    }

                    if (!cardLifeExtended)
                    {
                        RemoveAfterSeconds remover = transform.parent.GetComponent<RemoveAfterSeconds>();
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
        //         effectEnabled = false;
        //     }
        //     else
        //     {
        //         effectEnabled = true;
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
