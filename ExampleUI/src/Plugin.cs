using BepInEx;
using BepInEx.Logging;

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

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Options.CreateOptionsMenu("Super cool settings", (menu) => {
                var gabrielPanel = menu.AddOptionsPanel();
                
                Text gamingText = UI.CreateText(gabrielPanel, "<size=50>stfu bozo</size>\n\nim gaming", 24, 700, 75);
                
                UI.CreateButton(gabrielPanel, "Super Cool Button", () => {
                    gamingText.SetText("<size=50>stfu bozo</size>\n\nim gabriel gaming");

                    AudioSource source = new GameObject("Gabriel Gamaing Source").AddComponent<AudioSource>();
                    source.clip = GabrielGaming;
                    source.Play();

                    GameObject.Destroy(source.gameObject, 5);
                });

                var widePanel = menu.AddOptionsPanel();

                UI.CreateButton(widePanel, "wide boi", () => {
                    foreach (Camera cam in Camera.allCameras) {
                        cam.aspect = 0.5f;
                    }
                    
                    MonoSingleton<OptionsManager>.Instance.UnPause();
                }, 580, 40);

                var diePanel = menu.AddOptionsPanel();
                
                UI.CreateButton(diePanel, "Press to die ;)", () => {
                    MonoSingleton<OptionsManager>.Instance.UnPause();

                    SpawnableObjectsDatabase database = (SpawnableObjectsDatabase)typeof(SandboxSaver).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Resources.FindObjectsOfTypeAll<SandboxSaver>()[0]);
                    GameObject maurice = database.enemies[12].gameObject;

                    NewMovement v1 = Camera.main.GetComponentsInParent<NewMovement>()[0];
                    v1.InvokeNextFrame(() => v1.GetHurt(10000, false, 1, true));

                    GameObject.Instantiate(maurice.transform.GetChild(0).GetComponent<SpiderBody>().beamExplosion, v1.transform.position, Quaternion.identity);
                }, 400);


                // Reset Gabriel Gaming Text
                menu.OnShown.Add((menu) => {
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
