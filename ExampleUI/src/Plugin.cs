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

            AssetLoader.LoadEmbeddedAssetBundle("resources.exampleui", (bundle) => {
                GabrielGaming = bundle.LoadAsset<AudioClip>("GabrielGaming");
            });

            Log.LogInfo("Loaded Assets from Asset Bundle");

            WideModeConfig = Config.Bind("General", "WideMode", false, "Whether or not to enbale \"Wide Mode\"");
            WidenessConfig = Config.Bind("General", "Wideness", 0.5f, "How wide \"Wide Mode\" is");
            TotalMenuOpenCount = Config.Bind("General.Stats", "TotalMenuOpenCount", 0, "The total amount of times the \"Secrets\" settings menu has been shown");

            Log.LogInfo("Loaded Config");

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            int openCountSinceGameStart = 0;
            Options.CreateOptionsMenu("Super cool settings", (menu) => {
                var gabrielPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);
                
                CustomText gamingText = UI.CreateText(gabrielPanel, "<size=50>stfu bozo</size>\n\nim gaming", width: 520, height: 75);
                
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
                var inlineTogglesGroup = UI.CreateHorizontalLayoutGroup(inlinePanel, spacing: 75);

                UI.CreateToggle(inlineTogglesGroup, "Uno", 100);
                UI.CreateToggle(inlineTogglesGroup, "Dos", 100);
                UI.CreateToggle(inlineTogglesGroup, "Tres", 100);

                menu.AddButton("Press to die ;)", () => {
                    MonoSingleton<OptionsManager>.Instance.UnPause();
                    MonoSingleton<NewMovement>.Instance.GetHurt(10000, false, 1, true);

                    // SpawnableObjectsDatabase database = (SpawnableObjectsDatabase)typeof(SandboxSaver).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Resources.FindObjectsOfTypeAll<SandboxSaver>()[0]);
                    // GameObject maurice = database.enemies[12].gameObject;

                    // NewMovement player = MonoSingleton<NewMovement>.Instance;
                    // CoroManager.InvokeNextFrame(() => player.GetHurt(10000, false, 1, true));

                    // GameObject.Instantiate(maurice/*.transform.GetChild(0).GetComponent<SpiderBody>().beamExplosion*/, player.transform.position, Quaternion.identity);
                });

                menu.AddButton("Disable UI for a bit", () => {
					UI.Canvas.gameObject.SetActive(false);
					CoroManager.InvokeAfterSeconds(3, () => {
						UI.Canvas.gameObject.SetActive(true);
					});
				});

                // This is pretty ugly, but then again, who would actually ever do this?
                CustomModal modalStack1 = null;
                CustomModal modalStack2 = null;
                CustomModal modalStack3 = null;
                CustomModal modalStack4 = null;
                CustomModal modalStack5 = null;

                modalStack1 = UI.CreateModal((modal) => {
                    UI.CreateText(modal, "Are you sure you want\nto do this?", width: 240, height: 100);
                    RectTransform buttonLayout = UI.CreateHorizontalLayoutGroup(modal);

                    UI.CreateButton(buttonLayout, "Yes", () => {
                        modal.Hide();
                        modalStack2.Show();
                    });

                    UI.CreateButton(buttonLayout, "No", () => {
                        modal.Hide();
                    });
                });

                modalStack2 = UI.CreateModal((modal) => {
                    UI.CreateText(modal, "No but, are you\nActually sure tho?", width: 240, height: 100);
                    RectTransform buttonLayout = UI.CreateHorizontalLayoutGroup(modal);

                    UI.CreateButton(buttonLayout, "I'm Sure", () => {
                        modal.Hide();
                        modalStack3.Show();
                    });

                    UI.CreateButton(buttonLayout, "On Second Thought...", () => {
                        modal.Hide();
                    }, width: 200);
                });

                modalStack3 = UI.CreateModal((modal) => {
                    UI.CreateText(modal, "But when i say \"sure\"\ni mean like \"100% sure\" levels of sure", width: 500, height: 100);
                    RectTransform buttonLayout = UI.CreateHorizontalLayoutGroup(modal);

                    UI.CreateButton(buttonLayout, "Yep! 100% Sure!", () => {
                        modal.Hide();
                        modalStack4.Show();
                    });

                    UI.CreateButton(buttonLayout, "Yeah Maybe Not...", () => {
                        modal.Hide();
                    });
                });

                modalStack4 = UI.CreateModal((modal) => {
                    UI.CreateText(modal, "<b><i><size=36>Well...</size>\n\nI tried to warn you...</i></b>", width: 300, height: 100);

                    UI.CreateButton(modal, "Just show me already!", () => {
                        modal.Hide();
                        modalStack5.Show();
                    }, width: 200);
                });

                modalStack5 = UI.CreateModal((modal) => {
                    UI.CreateImage(modal, AssetLoader.CreateSpriteFromEmbeddedTexture("Maurice.png"));
                });

				menu.AddButton("Dont click this button!!!", () => {
					modalStack1.Show();
				});

                menu.AddHeader("--Images--");
                var imagePanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                UI.CreateText(imagePanel, "Live Panopticon Reaction:", fontSize: 40);
                UI.CreateText(imagePanel, "<color=red>SPOLIERS!!!!!!!</color>", fontSize: 60, height: 50);
                // UI.CreateImage(imagePanel, AssetLoader.CreateSpriteFromEmbeddedTexture("PANOPTICON.png"));

                menu.AddHeader("--Stats--");
                var statsPanel = menu.AddOptionsPanel(TextAnchor.MiddleCenter);

                int openCountSinceLevelStart = 0;

                CustomText statsText = UI.CreateText(statsPanel, "Stats :)", height: 160);

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
