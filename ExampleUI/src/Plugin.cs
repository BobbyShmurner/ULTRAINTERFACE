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
        internal static ConfigEntry<int> TotalMenuOpenCount;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo("Created Global Logger");

            harmony = new Harmony("ExampleUI");
            harmony.PatchAll();
            Plugin.Log.LogInfo("Applied All Patches");

            var assembly = typeof(Plugin).GetTypeInfo().Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream("ExampleUI.resources.exampleui");

            var bundle = AssetBundle.LoadFromStream(resourceStream);
            GabrielGaming = bundle.LoadAsset<AudioClip>("GabrielGaming");
            bundle.Unload(false);

            Plugin.Log.LogInfo("Loaded Assets from Asset Bundle");

            WideModeConfig = Config.Bind("General", "WideMode", false, "Whether or not to enbale \"Wide Mode\"");
            TotalMenuOpenCount = Config.Bind("General.Stats", "TotalMenuOpenCount", 0, "The total amount of times the \"Secrets\" settings menu has been shown");

            Plugin.Log.LogInfo("Loaded Config");

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            int openCountSinceGameStart = 0;
            Options.CreateOptionsMenu("Super cool settings", (menu) => {
                var gabrielPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);
                
                Text gamingText = UI.CreateText(gabrielPanel, "<size=50>stfu bozo</size>\n\nim gaming", 24, 520, 75);
                
                UI.CreateButton(gabrielPanel, "Super Cool Button", () => {
                    gamingText.SetText("<size=50>stfu bozo</size>\n\nim gabriel gaming");

                    AudioSource source = new GameObject("Gabriel Gamaing Source").AddComponent<AudioSource>();
                    source.clip = GabrielGaming;
                    source.Play();

                    GameObject.Destroy(source.gameObject, 5);
                });

                var widePanel = menu.AddOptionsPanel();

                CustomToggle wideToggle = UI.CreateToggle(widePanel, "wide boi", (enabled) => {
                    foreach (Camera cam in Camera.allCameras) {
                        if (enabled) cam.aspect = 0.5f;
                        else cam.ResetAspect();
                    }
                });

                var inlinePanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                UI.CreateText(inlinePanel, "Woah, multiple toggles on one line???");
                var inlineTogglesGroup = UI.CreateHorizontalLayoutGroup(inlinePanel, 440, 20, 75);

                UI.CreateToggle(inlineTogglesGroup, "Uno", 30);
                UI.CreateToggle(inlineTogglesGroup, "Dos", 30);
                UI.CreateToggle(inlineTogglesGroup, "Tres", 30);

                menu.AddButton("Press to die ;)", () => {
                    MonoSingleton<OptionsManager>.Instance.UnPause();

                    SpawnableObjectsDatabase database = (SpawnableObjectsDatabase)typeof(SandboxSaver).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Resources.FindObjectsOfTypeAll<SandboxSaver>()[0]);
                    GameObject maurice = database.enemies[12].gameObject;

                    NewMovement v1 = Camera.main.GetComponentsInParent<NewMovement>()[0];
                    v1.InvokeNextFrame(() => v1.GetHurt(10000, false, 1, true));

                    GameObject.Instantiate(maurice.transform.GetChild(0).GetComponent<SpiderBody>().beamExplosion, v1.transform.position, Quaternion.identity);
                });

                menu.AddHeader("--Stats--");
                var statsPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                int openCountSinceLevelStart = 0;

                Text statsText = UI.CreateText(statsPanel, "Stats :)", 24, 600, 140);

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

        private void OnDestroy() {
            harmony?.UnpatchSelf();
            UI.Unload();
        }
    }
}
