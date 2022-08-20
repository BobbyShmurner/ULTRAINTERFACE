using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ULTRAINTERFACE {
	public static class EmbedManager {
		public static string ResourceNamespace { get; internal set; }
		public static Assembly CurrentAssembly { get; internal set;}

		public static List<string> GetListOfResourceNames() {
			return CurrentAssembly.GetManifestResourceNames().ToList();
		}

		public static void LoadEmbeddedAssetBundle(string name, Action<AssetBundle> action) {
			if (!UI.Init()) return;

			Stream resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.{name}");
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to find Embedded AssetBundle \"{ResourceNamespace}.{name}\"");
				return;
			}

			LoadAssetBundleInternal(resourceStream, action);
		}

		public static void LoadAssetBundleFromDisk(string path, Action<AssetBundle> action) {
			if (!UI.Init()) return;

			if (!File.Exists(path)) {
				UI.Log.LogError($"AssetBundle \"{path}\" does not exist");
				return;
			}

			Stream resourceStream = File.OpenRead(path);
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to open AssetBundle \"{path}\"");
				return;
			}

			LoadAssetBundleInternal(resourceStream, action);
		}

		internal static void LoadAssetBundleInternal(Stream resourceStream, Action<AssetBundle> action) {
			AssetBundle bundle = AssetBundle.LoadFromStream(resourceStream);
			action(bundle);

			resourceStream.Close();
			bundle.Unload(false);
		}
	}
}