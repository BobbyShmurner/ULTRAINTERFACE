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

		public static List<string> GetListOfResourceNames() {
			return CurrentAssembly.GetManifestResourceNames().ToList();
		}

		public static Texture2D LoadEmbeddedTexture(string resourceName) {
			if (!UI.Init()) return null;

			Stream resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.{resourceName}");
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to find Embedded Texture \"{ResourceNamespace}.{resourceName}\"");
				return null;
			}

			return LoadTextureInternal(resourceStream);
		}

		internal static Texture2D LoadTextureInternal(Stream resourceStream, bool dontCloseStream = false) {
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(resourceStream.ToByteArray());

			if (!dontCloseStream) resourceStream.Close();
			return texture;
		}

		public static Sprite CreateSpriteFromEmbeddedTexture(string resourceName) {
			Texture2D texture = LoadEmbeddedTexture(resourceName);
			if (texture == null) return null;

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public static Sprite CreateSpriteFromFileDialog(string initialDirectory = "", bool restoreDirectory = true) {
			string path;
			return CreateSpriteFromFileDialog(out path, initialDirectory, restoreDirectory);
		}

		public static Sprite CreateSpriteFromFileDialog(out string path, string initialDirectory = "", bool restoreDirectory = true) {
			Texture2D texture = LoadTextureFromFileDialog(out path, initialDirectory, restoreDirectory);
			if (texture == null) return null;

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		public static Texture2D LoadTextureFromFileDialog(string initialDirectory = "", bool restoreDirectory = true) {
			string path;
			return LoadTextureFromFileDialog(out path, initialDirectory, restoreDirectory);
		}

		public static Texture2D LoadTextureFromFileDialog(out string path, string initialDirectory = "", bool restoreDirectory = true) {
			if (initialDirectory == "") initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

			Texture2D texture = null;
			FileManager.LoadFileFromFileDialog((reader) => {
				texture = LoadTextureInternal(reader.BaseStream, true);
			}, out path, initialDirectory, "Images: (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg", restoreDirectory);

			return texture;
		}

		public static void LoadEmbeddedFile(string resourceName, Action<StreamReader> action) {
			if (!UI.Init()) return;

			Stream resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.{resourceName}");
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

			Stream resourceStream = CurrentAssembly.GetManifestResourceStream($"{ResourceNamespace}.{resourceName}");
			if (resourceStream == null) {
				UI.Log.LogError($"Failed to find Embedded AssetBundle \"{ResourceNamespace}.{resourceName}\"");
				return;
			}

			LoadAssetBundleInternal(resourceStream, action);
		}

		public static void LoadAssetBundleFromDisk(string path, Action<AssetBundle> action) {
			FileManager.LoadFileFromDisk(path, (reader) => {
				LoadAssetBundleInternal(reader.BaseStream, action, true);
			});
		}

		internal static void LoadAssetBundleInternal(Stream resourceStream, Action<AssetBundle> action, bool dontCloseStream = false) {
			AssetBundle bundle = AssetBundle.LoadFromStream(resourceStream);
			action(bundle);

			if (!dontCloseStream) resourceStream.Close();
			bundle.Unload(false);
		}
	}
}