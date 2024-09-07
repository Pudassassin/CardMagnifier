using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using BepInEx;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Cards;
using UnboundLib.Utils.UI;
using HarmonyLib;

using CardMagnifier.MonoBehaviors;

namespace CardMagnifier
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class CardMagnifier : BaseUnityPlugin
    {
        public const string ModId = "com.pudassassin.rounds.CardMagnifier";
        public const string ModName = "Card Magnifier";
        public const string Version = "0.2.1"; //build #38 / Release 0-2-1

        public const string CompatibilityModName = "CardMagnifier";

        public static GameObject timerUI = null;
        public static Vector3 timerUIPos = new Vector3(-200.0f, -400.0f, 0.0f);

        // config part

        private static Color easyChangeColor = new Color(0.521f, 1f, 0.521f, 1f);
        private static Color hardChangeColor = new Color(1f, 0.521f, 0.521f, 1f);


        internal static string ConfigKey(string name)
        {
            return $"{CardMagnifier.CompatibilityModName}_{name.ToLower()}";
        }
        internal static bool GetBool(string name, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue ? 1 : 0) == 1;
        }
        internal static void SetBool(string name, bool value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value ? 1 : 0);
        }
        internal static int GetInt(string name, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(ConfigKey(name), defaultValue);
        }
        internal static void SetInt(string name, int value)
        {
            PlayerPrefs.SetInt(ConfigKey(name), value);
        }
        internal static float GetFloat(string name, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(ConfigKey(name), defaultValue);
        }
        internal static void SetFloat(string name, float value)
        {
            PlayerPrefs.SetFloat(ConfigKey(name), value);
        }


        public static float CardToZoomPointFactor
        {
            get
            {
                return GetFloat("CardToZoomPointFactor", 0.6f);
            }
            set
            {
                SetFloat("CardToZoomPointFactor", value);
            }
        }
        public static float ZoomPointXOffset
        {
            get
            {
                return GetFloat("ZoomPointXOffset", 0.0f);
            }
            set
            {
                SetFloat("ZoomPointXOffset", value);
            }
        }
        public static float ZoomPointYOffset
        {
            get
            {
                return GetFloat("ZoomPointYOffset", -8.0f);
            }
            set
            {
                SetFloat("ZoomPointYOffset", value);
            }
        }
        public static float ZoomScale
        {
            get
            {
                return GetFloat("ZoomScale", 1.35f);
            }
            set
            {
                SetFloat("ZoomScale", value);
            }
        }
        public static bool ZoomToAbsoluteSize
        { 
            get
            {
                return GetBool("ZoomToAbsoluteSize", true);
            }
            set
            {
                SetBool("ZoomToAbsoluteSize", value);
            }
        }
        public static bool ReorientCard
        {
            get
            {
                return GetBool("ReorientCard", false);
            }
            set
            {
                SetBool("ReorientCard", value);
            }
        }
        public static float ZoomTime
        {
            get
            {
                return GetFloat("ZoomTime", 0.3f);
            }
            set
            {
                SetFloat("ZoomTime", value);
            }
        }
        public static bool DecreaseDiscardMotions
        {
            get
            {
                return GetBool("DecreaseDiscardMotions", false);
            }
            set
            {
                SetBool("DecreaseDiscardMotions", value);
            }
        }
        public static bool DisableCardBobbingEffect
        {
            get
            {
                return GetBool("DisableCardBobbingEffect", false);
            }
            set
            {
                SetBool("DisableCardBobbingEffect", value);
            }
        }
        public static bool DisableCardFlippingEffect
        {
            get
            {
                return GetBool("DisableCardFlippingEffect", false);
            }
            set
            {
                SetBool("DisableCardFlippingEffect", value);
            }
        }
        public static float CardBarPreviewXOveride
        {
            get
            {
                return GetFloat("CardBarPreviewXOveride", 25.0f);
            }
            set
            {
                SetFloat("CardBarPreviewXOveride", value);
            }
        }
        public static float CardBarPreviewYOveride
        {
            get
            {
                return GetFloat("CardBarPreviewYOveride", 0.0f);
            }
            set
            {
                SetFloat("CardBarPreviewYOveride", value);
            }
        }
        public static float CardBarPreviewScaleOveride
        {
            get
            {
                return GetFloat("CardBarPreviewScaleOveride", 1.35f);
            }
            set
            {
                SetFloat("CardBarPreviewScaleOveride", value);
            }
        }

        public const float RealtimeToRefresh = 0.05f;
        public static float RealtimeLastRefreshed;

        public static bool CardZoomDemoMode = false;
        public static bool CardBarPreviewDemoMode = false;

        public static int PickTimerSearchCount = 5;

        internal static Dictionary<string, List<Toggle>> TogglesToSync = new Dictionary<string, List<Toggle>>();
        internal static Dictionary<string, List<Slider>> SlidersToSync = new Dictionary<string, List<Slider>>();

        private static void VanillaPlusPreset()
        {
            CardToZoomPointFactor = 0.6f;
            ZoomPointXOffset = 0.0f;
            ZoomPointYOffset = -8.0f;

            ZoomScale = 1.35f;
            ZoomToAbsoluteSize = true;
            ReorientCard = false;

            ZoomTime = 0.2f;
            DecreaseDiscardMotions = false;
            DisableCardBobbingEffect = false;
            DisableCardFlippingEffect = false;

            CardEnlargerDemo.DemoCardZoomRefresh();
        }
        private static void FancyZoomPreset()
        {
            CardToZoomPointFactor = 1.0f;
            ZoomPointXOffset = 0.0f;
            ZoomPointYOffset = 3.25f;

            ZoomScale = 1.4f;
            ZoomToAbsoluteSize = true;
            ReorientCard = true;

            ZoomTime = 0.3f;
            DecreaseDiscardMotions = false;
            DisableCardBobbingEffect = false;
            DisableCardFlippingEffect = false;

            CardEnlargerDemo.DemoCardZoomRefresh();
        }
        private static void InstantZoomPreset()
        {
            CardToZoomPointFactor = 1.0f;
            ZoomPointXOffset = 0.0f;
            ZoomPointYOffset = 3.25f;

            ZoomScale = 1.4f;
            ZoomToAbsoluteSize = true;
            ReorientCard = true;

            ZoomTime = 0.001f;
            DecreaseDiscardMotions = false;
            DisableCardBobbingEffect = false;
            DisableCardFlippingEffect = false;

            CardEnlargerDemo.DemoCardZoomRefresh();
        }
        private static void ReduceMotionOverride()
        {
            ZoomTime = 0.001f;
            DecreaseDiscardMotions = true;
            DisableCardBobbingEffect = true;
            DisableCardFlippingEffect = true;

            CardEnlargerDemo.DemoCardZoomRefresh();
        }

        private static void InitializeOptionsDictionaries()
        {
            // if (!TogglesToSync.Keys.Contains("DisableCardParticleAnimations")) { TogglesToSync["DisableCardParticleAnimations"] = new List<Toggle>() { }; }
            // if (!SlidersToSync.Keys.Contains("NumberOfGeneralParticles")){ SlidersToSync["NumberOfGeneralParticles"] = new List<Slider>(){};}

            if (!SlidersToSync.Keys.Contains("CardToZoomPointFactor")) { SlidersToSync["CardToZoomPointFactor"] = new List<Slider>() { }; }
            if (!SlidersToSync.Keys.Contains("ZoomPointXOffset")) { SlidersToSync["ZoomPointXOffset"] = new List<Slider>() { }; }
            if (!SlidersToSync.Keys.Contains("ZoomPointYOffset")) { SlidersToSync["ZoomPointYOffset"] = new List<Slider>() { }; }

            if (!SlidersToSync.Keys.Contains("ZoomScale")) { SlidersToSync["ZoomScale"] = new List<Slider>() { }; }
            if (!TogglesToSync.Keys.Contains("ZoomToAbsoluteSize")) { TogglesToSync["ZoomToAbsoluteSize"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("ReorientCard")) { TogglesToSync["ReorientCard"] = new List<Toggle>() { }; }

            if (!SlidersToSync.Keys.Contains("ZoomTime")) { SlidersToSync["ZoomTime"] = new List<Slider>() { }; }
            if (!TogglesToSync.Keys.Contains("DecreaseDiscardMotions")) { TogglesToSync["DecreaseDiscardMotions"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("DisableCardBobbingEffect")) { TogglesToSync["DisableCardBobbingEffect"] = new List<Toggle>() { }; }
            if (!TogglesToSync.Keys.Contains("DisableCardFlippingEffect")) { TogglesToSync["DisableCardFlippingEffect"] = new List<Toggle>() { }; }


            if (!SlidersToSync.Keys.Contains("CardBarPreviewXOveride")) { SlidersToSync["CardBarPreviewXOveride"] = new List<Slider>() { }; }
            if (!SlidersToSync.Keys.Contains("CardBarPreviewYOveride")) { SlidersToSync["CardBarPreviewYOveride"] = new List<Slider>() { }; }
            if (!SlidersToSync.Keys.Contains("CardBarPreviewScaleOveride")) { SlidersToSync["CardBarPreviewScaleOveride"] = new List<Slider>() { }; }
        }
        private static void SyncOptionsMenus(int recurse = 3)
        {
            // foreach (Toggle toggle in TogglesToSync["DisableCardParticleAnimations"]) { toggle.isOn = DisableCardParticleAnimations; }
            // foreach (Slider slider in SlidersToSync["NumberOfGeneralParticles"]) { slider.value = NumberOfGeneralParticles; }

            foreach (Slider slider in SlidersToSync["CardToZoomPointFactor"]) { slider.value = CardToZoomPointFactor; }
            foreach (Slider slider in SlidersToSync["ZoomPointXOffset"]) { slider.value = ZoomPointXOffset; }
            foreach (Slider slider in SlidersToSync["ZoomPointYOffset"]) { slider.value = ZoomPointYOffset; }

            foreach (Slider slider in SlidersToSync["ZoomScale"]) { slider.value = ZoomScale; }
            foreach (Toggle toggle in TogglesToSync["ZoomToAbsoluteSize"]) { toggle.isOn = ZoomToAbsoluteSize; }
            foreach (Toggle toggle in TogglesToSync["ReorientCard"]) { toggle.isOn = ReorientCard; }

            foreach (Slider slider in SlidersToSync["ZoomTime"]) { slider.value = ZoomTime; }
            foreach (Toggle toggle in TogglesToSync["DecreaseDiscardMotions"]) { toggle.isOn = DecreaseDiscardMotions; }
            foreach (Toggle toggle in TogglesToSync["DisableCardBobbingEffect"]) { toggle.isOn = DisableCardBobbingEffect; }
            foreach (Toggle toggle in TogglesToSync["DisableCardFlippingEffect"]) { toggle.isOn = DisableCardFlippingEffect; }


            foreach (Slider slider in SlidersToSync["CardBarPreviewXOveride"]) { slider.value = CardBarPreviewXOveride; }
            foreach (Slider slider in SlidersToSync["CardBarPreviewYOveride"]) { slider.value = CardBarPreviewYOveride; }
            foreach (Slider slider in SlidersToSync["CardBarPreviewScaleOveride"]) { slider.value = CardBarPreviewScaleOveride; }

            if (recurse > 0) { SyncOptionsMenus(recurse - 1); }
        }

        private static bool RefreshCheck()
        {
            if (Time.time > RealtimeLastRefreshed + RealtimeToRefresh)
            {
                RealtimeLastRefreshed = Time.time;
                return true;
            }

            return false;
        }
        private static void UpdateAndRefreshCardZoom()
        {
            if (!RefreshCheck()) return;

            CardEnlargerDemo.DemoCardZoomRefresh();
            // SyncOptionsMenus();
        }
        private static void UpdateAndRefreshCardBar()
        {
            if (!RefreshCheck()) return;

            CardEnlargerDemo.DemoCardBarPreviewRefresh();
            // SyncOptionsMenus();
        }

        private static void NewGUI(GameObject menu)
        {
            InitializeOptionsDictionaries();

            MenuHandler.CreateText("<b>" + ModName + " Options</b>", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            GameObject presetMenu = MenuHandler.CreateMenu("Presets", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            PresetsMenu(presetMenu);

            GameObject cardZoomSetting = MenuHandler.CreateMenu("Card Zoom Setting", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            CardZoomSetting(cardZoomSetting);

            GameObject cardBarSetting = MenuHandler.CreateMenu("CardBar Preview Setting", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            CardBarSetting(cardBarSetting);

            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
        private static void PresetsMenu(GameObject menu)
        {
            MenuHandler.CreateButton("Vanilla+", menu, VanillaPlusPreset, 60, color: easyChangeColor);
            MenuHandler.CreateButton("Fancy Zoom*", menu, FancyZoomPreset, 60, color: easyChangeColor);
            MenuHandler.CreateButton("Insta-Zoom!", menu, InstantZoomPreset, 60, color: easyChangeColor);
            MenuHandler.CreateText("* motion-sickness warning", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 20);

            MenuHandler.CreateButton("Reduce Card Motions", menu, ReduceMotionOverride, 60, color: easyChangeColor);
            MenuHandler.CreateText("(disable motion-sickness inducing motions in the current setting)", menu, out TextMeshProUGUI _, 20);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            void ToggleDemo()
            {
                CardZoomDemoMode = !CardZoomDemoMode;
            }
            MenuHandler.CreateButton("Toggle Demo", menu, ToggleDemo, 30);
            MenuHandler.CreateText("RMB-drag to move demo card around, NUMPAD+/- to scale \'starting size\', NUMPAD* to reset to default scale.", menu, out TextMeshProUGUI _, 20);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            // 'go back' events
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(delegate ()
            {
                CardZoomDemoMode = false;
                SyncOptionsMenus();
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                CardZoomDemoMode = false;
                SyncOptionsMenus();
            });
        }
        private static void CardZoomSetting(GameObject menu)
        {
            MenuHandler.CreateText("Card Zoom Setting", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            void CardToZoomPointFactorChanged(float val)
            {
                CardToZoomPointFactor = (float)val;
                UpdateAndRefreshCardZoom();
            }
            MenuHandler.CreateSlider("Card-to-ZoomPoint Position", menu, 30, 0.0f, 1.0f, CardToZoomPointFactor, CardToZoomPointFactorChanged, out Slider slider1, false, color: hardChangeColor);
            SlidersToSync["CardToZoomPointFactor"].Add(slider1);

            MenuHandler.CreateText("ZoomPoint Position:", menu, out TextMeshProUGUI _, 30);

            void ZoomPointXOffsetChanged(float val)
            {
                ZoomPointXOffset = (float)val;
                UpdateAndRefreshCardZoom();
            }
            MenuHandler.CreateSlider("X From Center", menu, 30, -50.0f, 50.0f, ZoomPointXOffset, ZoomPointXOffsetChanged, out Slider slider2, false, color: hardChangeColor);
            SlidersToSync["ZoomPointXOffset"].Add(slider2);

            void ZoomPointYOffsetChanged(float val)
            {
                ZoomPointYOffset = (float)val;
                UpdateAndRefreshCardZoom();
            }
            MenuHandler.CreateSlider("Y From Center", menu, 30, -25.0f, 25.0f, ZoomPointYOffset, ZoomPointYOffsetChanged, out Slider slider3, false, color: hardChangeColor);
            SlidersToSync["ZoomPointYOffset"].Add(slider3);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            void ZoomScaleChanged(float val)
            {
                ZoomScale = (float)val;
                UpdateAndRefreshCardZoom();
            }
            MenuHandler.CreateSlider("Card Zoom Scale", menu, 30, 0.5f, 5.0f, ZoomScale, ZoomScaleChanged, out Slider slider4, false, color: easyChangeColor);
            SlidersToSync["ZoomScale"].Add(slider4);

            void ZoomToAbsoluteSizeChanged(bool val)
            {
                ZoomToAbsoluteSize = (bool)val;
                UpdateAndRefreshCardZoom();
            }
            Toggle toggle1 = MenuHandler.CreateToggle(ZoomToAbsoluteSize, "Zoom Card to Fixed Size", menu, ZoomToAbsoluteSizeChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            TogglesToSync["ZoomToAbsoluteSize"].Add(toggle1);

            void ReorientCardChanged(bool val)
            {
                ReorientCard = (bool)val;
                UpdateAndRefreshCardZoom();
            }
            Toggle toggle2 = MenuHandler.CreateToggle(ReorientCard, "Reorient Zoomed Card Upright", menu, ReorientCardChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            TogglesToSync["ReorientCard"].Add(toggle2);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            void ZoomTimeChanged(float val)
            {
                ZoomTime = (float)val;
                UpdateAndRefreshCardZoom();
            }
            MenuHandler.CreateSlider("Card Zoom Animation Time", menu, 30, 0.001f, 1.0f, ZoomTime, ZoomTimeChanged, out Slider slider5, false, color: easyChangeColor);
            SlidersToSync["ZoomTime"].Add(slider5);

            void DecreaseDiscardMotionsChanged(bool val)
            {
                DecreaseDiscardMotions = (bool)val;
                UpdateAndRefreshCardZoom();
            }
            Toggle toggle3 = MenuHandler.CreateToggle(DecreaseDiscardMotions, "Decrease Discard Card Motions", menu, DecreaseDiscardMotionsChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            TogglesToSync["DecreaseDiscardMotions"].Add(toggle3);

            void DisableCardBobbingEffectChanged(bool val)
            {
                DisableCardBobbingEffect = (bool)val;
                UpdateAndRefreshCardZoom();
            }
            Toggle toggle4 = MenuHandler.CreateToggle(DisableCardBobbingEffect, "Disable Card Bobbing when Highlighted", menu, DisableCardBobbingEffectChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            TogglesToSync["DisableCardBobbingEffect"].Add(toggle4);

            void DisableCardFlippingEffectChanged(bool val)
            {
                DisableCardFlippingEffect = (bool)val;
                UpdateAndRefreshCardZoom();
            }
            Toggle toggle5 = MenuHandler.CreateToggle(DisableCardFlippingEffect, "Disable Card Flipping when Revealed", menu, DisableCardFlippingEffectChanged, 20, color: easyChangeColor).GetComponent<Toggle>();
            TogglesToSync["DisableCardFlippingEffect"].Add(toggle5);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            void ToggleDemo()
            {
                CardZoomDemoMode = !CardZoomDemoMode;
            }
            MenuHandler.CreateButton("Toggle Demo", menu, ToggleDemo, 30);
            MenuHandler.CreateText("RMB-drag to move demo card around, NUMPAD+/- to scale \'starting size\', NUMPAD* to reset to default scale.", menu, out TextMeshProUGUI _, 20);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);

            // 'go back' events
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(delegate ()
            {
                CardZoomDemoMode = false;
                SyncOptionsMenus();
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                CardZoomDemoMode = false;
                SyncOptionsMenus();
            });

        }
        private static void CardBarSetting(GameObject menu)
        {
            MenuHandler.CreateText("CardBar Preview Setting", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            MenuHandler.CreateText("Preview Position:", menu, out TextMeshProUGUI _, 30);

            void CardBarPreviewXOverideChanged(float val)
            {
                CardBarPreviewXOveride = (float)val;
                UpdateAndRefreshCardBar();
            }
            MenuHandler.CreateSlider("X From Center", menu, 30, -50.0f, 50.0f, CardBarPreviewXOveride, CardBarPreviewXOverideChanged, out Slider slider1, false, color: hardChangeColor);
            SlidersToSync["CardBarPreviewXOveride"].Add(slider1);

            void CardBarPreviewYOverideChanged(float val)
            {
                CardBarPreviewYOveride = (float)val;
                UpdateAndRefreshCardBar();
            }
            MenuHandler.CreateSlider("Y From Center", menu, 30, -50.0f, 50.0f, CardBarPreviewYOveride, CardBarPreviewYOverideChanged, out Slider slider2, false, color: hardChangeColor);
            SlidersToSync["CardBarPreviewYOveride"].Add(slider2);

            void CardBarPreviewScaleOverideChanged(float val)
            {
                CardBarPreviewScaleOveride = (float)val;
                UpdateAndRefreshCardBar();
            }
            MenuHandler.CreateSlider("CardBar Preview Size", menu, 30, 0.5f, 5.0f, CardBarPreviewScaleOveride, CardBarPreviewScaleOverideChanged, out Slider slider3, false, color: hardChangeColor);
            SlidersToSync["CardBarPreviewScaleOveride"].Add(slider3);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);


            void ToggleDemo()
            {
                CardBarPreviewDemoMode = !CardBarPreviewDemoMode;
            }
            MenuHandler.CreateButton("Toggle Demo", menu, ToggleDemo, 30);

            // 'go back' events
            menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(delegate ()
            {
                CardBarPreviewDemoMode = false;
                SyncOptionsMenus();
            });
            menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                CardBarPreviewDemoMode = false;
                SyncOptionsMenus();
            });
        }

        // methods

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

            Unbound.RegisterCredits
            (
                ModName,
                new string[] { "Pudassassin, Creator of GearUp Cards", "Willuwontu (coding guide)", "[Root] (UX testing and suggestion)" },
                new string[] { "github" },
                new string[] { "https://github.com/Pudassassin/CardMagnifier" }
            );

            // add GUI to modoptions menu
            Unbound.RegisterMenu
            (
                ModName,
                delegate ()
                { 
                    
                },
                NewGUI,
                showInPauseMenu: true
            );

            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart);
            // GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);

            gameObject.AddComponent<CardEnlargerDemo>();
        }

        public void Update()
        {
            CardBarPreviewRescaler.UpdateCurrentZoom();
        }

        public IEnumerator OnPlayerPickStart(IGameModeHandler gm)
        {
            // UnityEngine.Debug.Log("[CardMagnifier] Player Picking Started");
            CardEnlarger.UpdateConfigs();
            CardEnlarger.mapEmbiggenerScale = 1.0f;

            CardEnlarger.SetCameraPosCardPick();
            CardEnlarger.isCardPickPhase = true;

            if (timerUI == null && PickTimerSearchCount > 0)
            {
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
                foreach (GameObject item in gameObjects)
                {
                    if (item.name == "TimerUI(Clone)")
                    {
                        timerUI = item;
                        UnityEngine.Debug.Log("[CardMagnifier] Found TimerUI(Clone)");
                        timerUI.transform.localPosition = new Vector3(0.0f, -390.0f, 0.0f);

                        PickTimerSearchCount = 0;
                        break;
                    }
                }

                PickTimerSearchCount--;
            }
            else if (timerUI != null)
            {
                timerUI.transform.localPosition = timerUIPos;
            }
        
            yield break;
        }

        public IEnumerator OnPointStart(IGameModeHandler gm)
        {
            // UnityEngine.Debug.Log("[CardMagnifier] Point Start");
            CardEnlarger.SetCameraPosGameplay();
        
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
