using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using ULTRAINTERFACE;

using System.IO;
using System.Reflection;

namespace ExampleUI
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static AudioClip GabrielGaming;

        internal static ConfigEntry<bool> WideModeConfig;
        internal static ConfigEntry<float> WidenessConfig;
        internal static ConfigEntry<int> TotalMenuOpenCount;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Log = base.Logger;
            Log.LogInfo("Created Global Logger");

            harmony = new Harmony("ExampleUI");
            harmony.PatchAll();
            Log.LogInfo("Applied All Patches");

            var assembly = typeof(Plugin).GetTypeInfo().Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream("ExampleUI.resources.exampleui");

            var bundle = AssetBundle.LoadFromStream(resourceStream);
            GabrielGaming = bundle.LoadAsset<AudioClip>("GabrielGaming");
            resourceStream.Close();
            bundle.Unload(false);

            Log.LogInfo("Loaded Assets from Asset Bundle");

            WideModeConfig = Config.Bind("General", "WideMode", false, "Whether or not to enbale \"Wide Mode\"");
            WidenessConfig = Config.Bind("General", "Wideness", 0.5f, "How wide \"Wide Mode\" is");
            TotalMenuOpenCount = Config.Bind("General.Stats", "TotalMenuOpenCount", 0, "The total amount of times the \"Secrets\" settings menu has been shown");

            Log.LogInfo("Loaded Config");

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            int openCountSinceGameStart = 0;
            Options.CreateOptionsMenu("Super cool settings", (menu) => {
                var gabrielPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);
                
                CustomText gamingText = UI.CreateText(gabrielPanel, "<size=50>stfu bozo</size>\n\nim gaming", 24, 520, 75);
                
                UI.CreateButton(gabrielPanel, "Super Cool Button", () => {
                    gamingText.SetText("<size=50>stfu bozo</size>\n\nim gabriel gaming");

                    AudioSource source = new GameObject("Gabriel Gamaing Source").AddComponent<AudioSource>();
                    source.clip = GabrielGaming;
                    source.Play();

                    GameObject.Destroy(source.gameObject, 5);
                });

                var widePanel = menu.AddOptionsPanel();
                CustomSlider wideSlider = null;

                CustomToggle wideToggle = UI.CreateToggle(widePanel, "wide boi", (enabled) => {
                    WideModeConfig.Value = enabled;
                    wideSlider.gameObject.SetActive(enabled);

                    Camera.main.ResetAspect();
                    float defaultAspect = Camera.main.aspect;
                    float newAspect = CalculateAspect(defaultAspect);

                    foreach (Camera cam in Resources.FindObjectsOfTypeAll<Camera>()) {
                        if (enabled) cam.aspect = newAspect;
                        else cam.ResetAspect();
                    }

                    menu.Rebuild();
                });

                wideSlider = UI.CreateSlider(widePanel, "WIDENESS", new SliderSettings(0, 100, Color.clear, Color.red, DecimalType.NoDecimals, "%", "thin", "THICC >:)"), (value) => {
                    if (!WideModeConfig.Value) return;

                    Camera.main.ResetAspect();
                    WidenessConfig.Value = value * 0.01f;

                    float defaultAspect = Camera.main.aspect;
                    float newAspect = CalculateAspect(defaultAspect);

                    foreach (Camera cam in Resources.FindObjectsOfTypeAll<Camera>()) {
                        cam.aspect = newAspect;
                    }
                }, forceCaps: false);

                wideSlider.gameObject.SetActive(WideModeConfig.Value);
                wideSlider.SetValue(WidenessConfig.Value * 100);
                wideToggle.SetValue(WideModeConfig.Value);

                var inlinePanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                UI.CreateText(inlinePanel, "Woah, multiple toggles on one line???");
                var inlineTogglesGroup = UI.CreateHorizontalLayoutGroup(inlinePanel, 440, 20, 75);

                UI.CreateToggle(inlineTogglesGroup, "Uno", 100);
                UI.CreateToggle(inlineTogglesGroup, "Dos", 100);
                UI.CreateToggle(inlineTogglesGroup, "Tres", 100);

                menu.AddButton("Press to die ;)", () => {
                    MonoSingleton<OptionsManager>.Instance.UnPause();

                    SpawnableObjectsDatabase database = (SpawnableObjectsDatabase)typeof(SandboxSaver).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Resources.FindObjectsOfTypeAll<SandboxSaver>()[0]);
                    GameObject maurice = database.enemies[12].gameObject;

                    NewMovement v1 = Camera.main.GetComponentsInParent<NewMovement>()[0];
                    CoroManager.InvokeNextFrame(() => v1.GetHurt(10000, false, 1, true));

                    GameObject.Instantiate(maurice.transform.GetChild(0).GetComponent<SpiderBody>().beamExplosion, v1.transform.position, Quaternion.identity);
                });

                menu.AddHeader("--Stats--");
                var statsPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                int openCountSinceLevelStart = 0;

                CustomText statsText = UI.CreateText(statsPanel, "Stats :)", 24, 160, 160);

                // Reset Gabriel Gaming Text
                menu.OnShown.Add((menu) => {
                    openCountSinceGameStart++;
                    openCountSinceLevelStart++;
                    TotalMenuOpenCount.Value++;

                    statsText.SetText($"<size=40>This menu has been shown:</size>\n\n{openCountSinceLevelStart} {(openCountSinceLevelStart == 1 ? "time" : "times")} this level\n{openCountSinceGameStart} {(openCountSinceGameStart == 1 ? "time" : "times")} since the game opened\n{TotalMenuOpenCount.Value} {(TotalMenuOpenCount.Value == 1 ? "time" : "times")} in total");
                    gamingText.SetText("<size=50>stfu bozo</size>\n\nim gaming");
                });
            }, "Secrets");
        }

        private float CalculateAspect(float defaultAspect) {
            return Mathf.Lerp(defaultAspect, 0.3f, WidenessConfig.Value);
        }

        private void OnDestroy() {
            harmony?.UnpatchSelf();
            UI.Unload();
        }
    }
}
