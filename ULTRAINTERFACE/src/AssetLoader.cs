using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ULTRAINTERFACE {
	public static class AssetLoader {
		public static string ResourceNamespace { get; internal set; }
		public static Assembly CurrentAssembly { get; internal set;}

		public static string GetUltrakillInstallPath() {
			return Path.GetFullPath(UnityEngine.Application.dataPath + "/../");
		}

		public static void LoadFileFromDisk(string path, Action<StreamReader> action) {
			if (!UI.Init()) return;

			if (!File.Exists(path)) {
				UI.Log.LogError($"File \"{path}\" does not exist");
				return;
			}

			Stream resourceStream = File.OpenRead(path);
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to open File \"{path}\"");
				return;
			}

			StreamReader reader = new StreamReader(resourceStream);
			action(reader);

			reader.Close();
			resourceStream.Close();
		}

		public static List<string> GetListOfResourceNames() {
			return CurrentAssembly.GetManifestResourceNames().ToList();
		}

		public static Texture2D LoadEmbeddedTexture(string resourceName) {
			if (!UI.Init()) return null;
			Texture2D texture = null;

			LoadEmbeddedFile(resourceName, (reader) => {
				texture = LoadTextureInternal(reader.BaseStream);
			});

			return texture;
		}

		public static Texture2D LoadTextureFromDisk(string path) {
			if (!UI.Init()) return null;
			Texture2D texture = null;

			LoadFileFromDisk(path, (reader) => {
				texture = LoadTextureInternal(reader.BaseStream);
			});

			return texture;
		}

		internal static Texture2D LoadTextureInternal(Stream resourceStream) {
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(resourceStream.ToByteArray());

			resourceStream.Close();
			return texture;
		}

		public static Sprite CreateSpriteFromEmbeddedTexture(string resourceName) {
			Texture2D texture = LoadEmbeddedTexture(resourceName);
			if (texture == null) return null;

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public static Sprite CreateSpriteFromDisk(string path) {
			Texture2D texture = LoadTextureFromDisk(path);
			if (texture == null) return null;

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public static void LoadEmbeddedFile(string resourceName, Action<StreamReader> action) {
			if (!UI.Init()) return;

			Stream resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.{resourceName}");
			if (resourceStream == null) resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.resources.{resourceName}");
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to find Embedded File \"{ResourceNamespace}.{resourceName}\"");
				return;
			}

			StreamReader reader = new StreamReader(resourceStream);
			action(reader);

			reader.Close();
			resourceStream.Close();
		}

		public static void LoadEmbeddedAssetBundle(string resourceName, Action<AssetBundle> action) {
			if (!UI.Init()) return;

			LoadEmbeddedFile(resourceName, (reader) => {
				LoadAssetBundleInternal(reader.BaseStream, action);
			});
		}

		public static void LoadAssetBundleFromDisk(string path, Action<AssetBundle> action) {
			LoadFileFromDisk(path, (reader) => {
				LoadAssetBundleInternal(reader.BaseStream, action);
			});
		}

		internal static void LoadAssetBundleInternal(Stream resourceStream, Action<AssetBundle> action) {
			AssetBundle bundle = AssetBundle.LoadFromStream(resourceStream);
			action(bundle);

			resourceStream.Close();
			bundle.Unload(false);
		}
	}
}