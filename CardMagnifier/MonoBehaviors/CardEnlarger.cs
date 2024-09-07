using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnboundLib.GameModes;
using UnboundLib;
using HarmonyLib;
using UnityEngine.UI;

namespace CardMagnifier.MonoBehaviors
{
    // mono to attach to card spawned during pick phase

    public class CardEnlarger : MonoBehaviour
    {
        public static List<string> BlacklistedCards = new List<string>()
        {
            // PCE Glitch cards
            // "__PCE__Common",
            // "__PCE__Uncommon",
            // "__PCE__Rare"
            "__CMR__Jack"
        };

        // in case of adaptive zoom feature
        public static Vector2Int defaultResolution = new Vector2Int(1920, 1080);
        public static Vector2Int currentResolution;
        public static float resolutionScalingFactor = 0.80f;

        public const float defaultCameraOrthoSize = 20.0f;
        public static float cameraOrthoSizeCardPick, cameraOrthoSizeGameplay, currentCameraOrthoSize;

        public static float screenResolutionScale = 1.0f;
        public static float mapEmbiggenerScale = 1.0f;

        public static float defaultScaleShake_TargetScale = 1.15f;


        // scale 0-1, 0 = original card pos, 1 = zoom to configured pos
        public static float configPosInterpolateFactor = 1.0f;
        public static Vector3 configZoomToPos = new Vector3(0.0f, 4.0f, -10.0f);

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
        public static float timeToVanish = 0.20f;

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
        public ScaleShake scaleShake = null;

        // debug
        private bool cardLifeExtended = false;
        public static float cardLifeTimeAdd = 0.0f;
        public float cardLifeExtendedTime = 0.0f; // realtime marker
        public static float realTimeToRemove = 1.5f; // realtime duration to remove

        public static void UpdateConfigs()
        {
            configPosInterpolateFactor = CardMagnifier.CardToZoomPointFactor;
            configZoomToPos.x = CardMagnifier.ZoomPointXOffset;
            configZoomToPos.y = CardMagnifier.ZoomPointYOffset;

            configZoomScale = CardMagnifier.ZoomScale;
            configZoomAbsoluteEnable = CardMagnifier.ZoomToAbsoluteSize;
            configReorientCardEnable = CardMagnifier.ReorientCard;

            configInterpolateTime = CardMagnifier.ZoomTime;
            configDisableDiscardEffect = CardMagnifier.DecreaseDiscardMotions;
            configDisableCardBobbingEffect = CardMagnifier.DisableCardBobbingEffect;
            configDisableCardFlippingEffect = CardMagnifier.DisableCardFlippingEffect;


            configCardBarPreviewPosition.x = CardMagnifier.CardBarPreviewXOveride;
            configCardBarPreviewPosition.y = CardMagnifier.CardBarPreviewYOveride;
            configCardBarPreviewScale = CardMagnifier.CardBarPreviewScaleOveride;
        }

        public void Awake()
        {

        }

        public void Start()
        {
            cardVisuals = transform.GetComponentInChildren<CardVisuals>();
            scaleShake = transform.GetComponentInChildren<ScaleShake>();
            // cardBaseParent = transform.parent.gameObject;
            cardBaseParent = transform.gameObject;

            Action<bool> action = GetToggleSelectionAction();
            cardVisuals.toggleSelectionAction = (Action<bool>)Delegate.Combine(cardVisuals.toggleSelectionAction, action);

        }

        public void Update()
        {
            currentCameraOrthoSize = MainCam.instance.cam.orthographicSize;

            if (cardVisuals == null)
            {
                cardVisuals = transform.GetComponentInChildren<CardVisuals>();
            }

            if (scaleShake == null)
            {
                scaleShake = transform.GetComponentInChildren<ScaleShake>();
            }

            if (configDisableCardBobbingEffect)
            {
                ScaleShake scaleShake = gameObject.GetComponentInChildren<ScaleShake>();
                scaleShake.enabled = false;
            }
        }

        public void LateUpdate()
        {
            if (CheckCardIsBlacklisted())
            {
                return;
            }

            if (zoomEffectEnabled)
            {
                if (cardIsHighLighted)
                {
                    interpolateTimer += TimeHandler.deltaTime;

                    if (configDisableCardBobbingEffect)
                    {
                        cardVisuals.transform.localScale = Vector3.one * 1.15f;
                    }
                    else
                    {
                        scaleShake.targetScale = 1.15f;
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
                    {
                        scaleShake.targetScale = 0.9f;
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
        }

        public bool CheckIsInTheCardDraw()
        {
            List<GameObject> spawnedCards = (List<GameObject>)Traverse.Create(CardChoice.instance).Field("spawnedCards").GetValue();
            if (spawnedCards == null)
            {
                return false;
            }
            else if (spawnedCards.Count == 0)
            {
                return false;
            }
            else if (spawnedCards.Contains(cardBaseParent))
            {
                return true;
            }
            
            return false;
        }

        public void SetupCardEnlarger()
        {
            if (cardPrevStateSaved) return;

            cardPreviousPos = cardBaseParent.transform.position; //world space position
            cardPreviousRotation = cardBaseParent.transform.localEulerAngles;
            cardPreviousScale = cardBaseParent.transform.localScale;

            CardEnlarger.currentResolution = new Vector2Int
            (
                MainCam.instance.cam.pixelWidth,
                MainCam.instance.cam.pixelHeight
            );
            CardEnlarger.SetResolutionScale();

            cardZoomToPos = Vector3.Lerp(cardPreviousPos, configZoomToPos, configPosInterpolateFactor) * screenResolutionScale;
            if (configZoomAbsoluteEnable)
            {
                cardZoomToScale = Vector3.one * configZoomScale * screenResolutionScale;
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

            float cameraScale = currentCameraOrthoSize / defaultCameraOrthoSize;

            cardBaseParent.transform.localPosition = Vector3.Lerp(cardPreviousPos, cardZoomToPos, iValue);
            cardBaseParent.transform.localScale = Vector3.Lerp(cardPreviousScale, cardZoomToScale * cameraScale, iValue);
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

        public bool CheckCardIsBlacklisted()
        {
            foreach (string item in BlacklistedCards)
            {
                if (cardBaseParent.name.Equals(item))
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
            if (cardBaseParent == null)
            {
                cardBaseParent = gameObject;
            }

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

        // debug + demo
        public void ResetTimer()
        {
            interpolateTimer = 0.0f;
        }

        public void DemoOveride()
        {
            // UnityEngine.Debug.Log("[CardMagnifier] DemoOveride A");
            cardBaseParent.transform.position = CardEnlargerDemo.demoCardStartPos;
            cardBaseParent.transform.localEulerAngles = new Vector3
            (
                0.0f,
                0.0f,
                CardEnlargerDemo.demoCardEdgeTilt * cardBaseParent.transform.position.x
            );
            cardBaseParent.transform.localScale = CardEnlargerDemo.demoCardStartScale;

            cardVisuals.transform.localScale = Vector3.one * 0.9f;

            // UnityEngine.Debug.Log("[CardMagnifier] DemoOveride B");
            cardPrevStateSaved = false;
            SetupCardEnlarger();

            // UnityEngine.Debug.Log("[CardMagnifier] DemoOveride C");
            cardIsHighLighted = true;
            ResetTimer();

            // UnityEngine.Debug.Log("[CardMagnifier] DemoOveride D");
        }

        // adaptive scaling
        public static void SetResolutionScale()
        {
            screenResolutionScale = Mathf.Min
            (
                (float)currentResolution.x / (float)defaultResolution.x,
                (float)currentResolution.y / (float)defaultResolution.y
            );

            screenResolutionScale = 1 + (screenResolutionScale - 1) * resolutionScalingFactor;
        }

        public static void SetCameraPosCardPick()
        {
            Camera gameCamera = GameObject.Find("SpotlightCam").GetComponent<Camera>();

            if (gameCamera != null)
            {
                cameraOrthoSizeCardPick = gameCamera.orthographicSize;
                // UnityEngine.Debug.Log("SpotlightCam ortho size: " + cameraOrthoSizeCardPick);
            }
        }

        public static void SetCameraPosGameplay()
        {
            // Camera gameCamera = GameObject.Find("SpotlightCam").GetComponent<Camera>();
            Camera gameCamera = GameObject.Find("MainCamera").GetComponent<Camera>();

            if (gameCamera != null)
            {
                cameraOrthoSizeGameplay = gameCamera.orthographicSize;
                // UnityEngine.Debug.Log("SpotlightCam ortho size: " + cameraOrthoSizeGameplay);

                currentResolution = new Vector2Int
                (
                    gameCamera.pixelWidth,
                    gameCamera.pixelHeight
                );

                mapEmbiggenerScale = cameraOrthoSizeGameplay / cameraOrthoSizeCardPick;
                SetResolutionScale();
            }

        }
    }

    public class CardEnlargerDemo : MonoBehaviour
    {
        // asset for visualizing previews

        public static CardInfo demoCardInfo;
        public static GameObject demoCardObject, demoStartPosObj, demoZoomPointObj; // link to parent of CardBase

        public static float demoCardEdgeTilt = -15.0f / 35.0f; // value at right edge of screen?

        public static Vector3 demoCardStartPos = new Vector3(-31.5f, 0.0185f, -5.0f);
        public static Vector3 demoCardStartRotation = Vector3.zero;
        public static Vector3 demoCardStartScale = Vector3.one * 0.6f;

        public static bool isDemoCardZoomMode = false;
        public static bool isDemoCardBarPreviewMode = false;
        public static bool demoCardIsDragged = false;
        public static Vector3 dragDisplacement = Vector3.zero, dragStart;
        public static float demoCardScale = 0.9f;

        public const float RealtimeToRefresh = 0.1f;
        public static float RealtimeLastRefreshed;

        public void Update()
        {
            isDemoCardZoomMode = CardMagnifier.CardZoomDemoMode;
            isDemoCardBarPreviewMode = CardMagnifier.CardBarPreviewDemoMode;

            if (isDemoCardZoomMode)
            {
                if (demoCardObject == null)
                {
                    DemoCardZoomInstantiate();
                }

                if (Input.GetMouseButtonDown(1) && demoCardIsDragged == false)
                {
                    // drag start
                    dragStart = MainCam.instance.cam.ScreenToWorldPoint(Input.mousePosition);
                    demoCardIsDragged = true;
                }
                else if (Input.GetMouseButtonUp(1) && demoCardIsDragged == true)
                {
                    // drag end
                    demoCardStartPos += dragDisplacement;
                    demoCardStartRotation = new Vector3
                    (
                        0.0f,
                        0.0f,
                        CardEnlargerDemo.demoCardEdgeTilt * demoCardStartPos.x
                    );
                    DemoCardZoomRefresh();

                    demoCardIsDragged = false;
                }

                if (Input.GetKeyDown(KeyCode.KeypadMultiply))
                {
                    //reset card starting scale
                    demoCardScale = 0.9f;
                    demoCardStartScale = Vector3.one * demoCardScale;

                    demoStartPosObj.transform.localScale = demoCardStartScale;
                    DemoCardZoomRefresh();
                }
                else if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    demoCardScale += 0.05f;
                    demoCardScale = Mathf.Clamp(demoCardScale, 0.25f, 2.0f);
                    demoCardStartScale = Vector3.one * demoCardScale;

                    demoStartPosObj.transform.localScale = demoCardStartScale;
                    DemoCardZoomRefresh();
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    demoCardScale -= 0.05f;
                    demoCardScale = Mathf.Clamp(demoCardScale, 0.25f, 2.0f);
                    demoCardStartScale = Vector3.one * demoCardScale;

                    demoStartPosObj.transform.localScale = demoCardStartScale;
                    DemoCardZoomRefresh();
                }

                // if (Input.mouseScrollDelta.y != 0.0f)
                // {
                //     UnityEngine.Debug.Log("Input.mouseScrollDelta.y : " + Input.mouseScrollDelta.y);
                // }

                if (demoCardIsDragged)
                {
                    dragDisplacement = MainCam.instance.cam.ScreenToWorldPoint(Input.mousePosition) - dragStart;
                    dragDisplacement.z = 0.0f;

                    demoStartPosObj.transform.position = demoCardStartPos + dragDisplacement;
                    demoStartPosObj.transform.localEulerAngles = new Vector3
                    (
                        0.0f,
                        0.0f,
                        CardEnlargerDemo.demoCardEdgeTilt * (demoCardStartPos.x + dragDisplacement.x)
                    );

                    // demoZoomPointObj.transform.position = CardEnlarger.configZoomToPos;
                }
            }
            else if (isDemoCardBarPreviewMode)
            {
                if (demoCardObject == null)
                {
                    DemoCardBarInstantiate();
                }
            }
            else
            {
                if (demoCardObject != null)
                {
                    GameObject.Destroy(demoCardObject);
                }
                if (demoStartPosObj != null)
                {
                    GameObject.Destroy(demoStartPosObj);
                }
            }
        }

        public void DemoCardZoomInstantiate()
        {
            // pick random CardInfo
            demoCardInfo = GetRandomCardInfo();

            // spawn full package card object
            SpawnPreviewCard(demoCardInfo);

            demoCardObject.transform.position = demoCardStartPos;
            demoCardObject.transform.localEulerAngles = new Vector3
            (
                0.0f,
                0.0f,
                CardEnlargerDemo.demoCardEdgeTilt * demoCardObject.transform.position.x
            );
            demoCardObject.transform.localScale = demoCardStartScale;

            CardEnlarger.currentResolution = new Vector2Int
            (
                MainCam.instance.cam.pixelWidth,
                MainCam.instance.cam.pixelHeight
            );
            CardEnlarger.SetResolutionScale();

            // temp starting point
            if (demoStartPosObj != null)
            {
                GameObject.Destroy(demoStartPosObj);
            }
            else
            {
                demoStartPosObj = GameObject.Instantiate(demoCardObject);
            }

            // attach CardEnlarger
            CardEnlarger cardEnlarger = demoCardObject.AddComponent<CardEnlarger>();
            this.ExecuteAfterFrames(2, delegate ()
            {
                cardEnlarger.EnableCardZoom();
                cardEnlarger.DemoOveride();
            });
        }

        public void DemoCardBarInstantiate()
        {
            demoCardInfo = GetRandomCardInfo();
            SpawnPreviewCard(demoCardInfo);

            CardEnlarger.currentResolution = new Vector2Int
            (
                MainCam.instance.cam.pixelWidth,
                MainCam.instance.cam.pixelHeight
            );
            CardEnlarger.SetResolutionScale();

            demoCardObject.transform.position = CardEnlarger.configCardBarPreviewPosition * CardEnlarger.screenResolutionScale;
            demoCardObject.transform.localEulerAngles = Vector3.zero;
            demoCardObject.transform.localScale = Vector3.one * CardEnlarger.configCardBarPreviewScale * CardEnlarger.screenResolutionScale;
        }

        public static CardInfo GetRandomCardInfo()
        {
            int roll = Mathf.RoundToInt(UnityEngine.Random.Range(0.0f, CardChoice.instance.cards.Length));
            roll = Mathf.Clamp(roll, 0, CardChoice.instance.cards.Length - 1);

            CardInfo cardInfo = CardChoice.instance.cards[roll];

            return cardInfo;
        }

        public static void SpawnPreviewCard(CardInfo cardInfo)
        {
            if (demoCardObject != null)
            {
                GameObject.Destroy(demoCardObject);
            }

            demoCardObject = CardChoice.instance.AddCardVisual(cardInfo, Vector3.zero);
            Collider2D[] componentsInChildren = demoCardObject.transform.root.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].enabled = false;
            }

            demoCardObject.GetComponentInChildren<Canvas>().sortingLayerName = "MostFront";
            demoCardObject.GetComponentInChildren<GraphicRaycaster>().enabled = false;
            demoCardObject.GetComponentInChildren<SetScaleToZero>().enabled = false;
            demoCardObject.GetComponentInChildren<SetScaleToZero>().transform.localScale = Vector3.one * 1.15f;
        }

        public static void DemoCardZoomRefresh()
        {
            if (Time.time > RealtimeLastRefreshed + RealtimeToRefresh)
            {
                RealtimeLastRefreshed = Time.time;
            }
            else return;

            CardEnlarger.UpdateConfigs();

            if (isDemoCardZoomMode)
            {
                CardEnlarger cardEnlarger = demoCardObject.GetComponent<CardEnlarger>();
                cardEnlarger.DemoOveride();
            }
        }

        public static void DemoCardBarPreviewRefresh()
        {
            if (Time.time > RealtimeLastRefreshed + RealtimeToRefresh)
            {
                RealtimeLastRefreshed = Time.time;
            }
            else return;

            CardEnlarger.UpdateConfigs();

            if (isDemoCardBarPreviewMode)
            {
                demoCardObject.transform.position = CardEnlarger.configCardBarPreviewPosition;
                demoCardObject.transform.localEulerAngles = Vector3.zero;
                demoCardObject.transform.localScale = Vector3.one * CardEnlarger.configCardBarPreviewScale * CardEnlarger.screenResolutionScale;
            }
        }
    }

    public class CardBarPreviewRescaler : MonoBehaviour
    {
        public static Camera gameCamera = null;
        public static float currentOrthoSize = 20.0f;

        public float rescale = 1.0f;
        public float targetScaleCheck = 1.0f;

        public void Awake()
        {
            RescalePreview();
        }

        public void Update()
        {
            RescalePreview();
        }

        public void RescalePreview()
        {
            rescale = (currentOrthoSize / CardEnlarger.defaultCameraOrthoSize) * CardEnlarger.screenResolutionScale;
            gameObject.transform.position = CardEnlarger.configCardBarPreviewPosition * rescale;
            gameObject.transform.localScale = Vector3.one * CardEnlarger.configCardBarPreviewScale * rescale;

            // this will reduce CardMagnifier's changes if there's something else changing the preview's scaling
            // MapExtended
            targetScaleCheck = gameObject.GetComponentInChildren<ScaleShake>().targetScale / CardEnlarger.defaultScaleShake_TargetScale;
            gameObject.transform.localScale /= targetScaleCheck;
        }

        public static void UpdateCurrentZoom()
        {
            if (gameCamera == null)
            {
                GameObject cameraObject = GameObject.Find("SpotlightCam");
                if (cameraObject != null)
                {
                    gameCamera = cameraObject.GetComponent<Camera>();
                }
                else
                {
                    currentOrthoSize = CardEnlarger.defaultCameraOrthoSize;
                    return;
                }

                if (gameCamera == null)
                {
                    currentOrthoSize = CardEnlarger.defaultCameraOrthoSize;
                    return;
                }

            }
            
            currentOrthoSize = gameCamera.orthographicSize;
        }
    }
}
