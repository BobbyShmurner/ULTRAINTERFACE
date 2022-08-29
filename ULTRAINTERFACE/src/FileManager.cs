using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ULTRAINTERFACE {
	public static class FileManager {
		[DllImport("user32.dll")]
		private static extern void OpenFileDialog();

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

		public static void LoadFileFromFileDialog(Action<StreamReader> action, string initialDirectory = "", string filter = "All files (*.*)|*.*", bool restoreDirectory = true) {
			string path;
			LoadFileFromFileDialog(action, out path, initialDirectory, filter, restoreDirectory);
		}

		public static void LoadFileFromFileDialog(Action<StreamReader> action, out string path, string initialDirectory = "", string filter = "All files (*.*)|*.*", bool restoreDirectory = true) {
			if (initialDirectory == "") initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			path = "";

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = initialDirectory;
			openFileDialog.Filter = filter;
			openFileDialog.RestoreDirectory = restoreDirectory;

			if (openFileDialog.ShowDialog() != DialogResult.OK) return;
			path = openFileDialog.FileName;

			LoadFileFromDisk(path, action);
		}
	}
}