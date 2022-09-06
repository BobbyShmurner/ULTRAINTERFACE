using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ULTRAINTERFACE {
	public static class FileManager {
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

		// public static void LoadFileFromFileDialog(Action<StreamReader> action, string initialDirectory = "", string filter = "All files (*.*)|*.*", bool restoreDirectory = true) {
		// 	string path;
		// 	LoadFileFromFileDialog(action, out path, initialDirectory, filter, restoreDirectory);
		// }

		// public static void LoadFileFromFileDialog(Action<StreamReader> action, out string path, string initialDirectory = "", string filter = "All files (*.*)|*.*", bool restoreDirectory = true) {
		// 	if (initialDirectory == "") initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		// 	path = "";

		// 	// --- UNMANAGED CODE WARNING!!!! ---

		// 	unsafe {
		// 		int hr = UnmanagedFunctions.CoInitializeEx(IntPtr.Zero, COINIT.COINIT_APARTMENTTHREADED | COINIT.COINIT_DISABLE_OLE1DDE);
		// 		if (hr < 0) return;

		// 		Guid CLSID_FileOpenDialog = new Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7");
		// 		Guid IID_IFileOpenDialog = new Guid("d57c7288-d4ad-4768-be02-9d969532d960");
		// 		Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

		// 		IntPtr pFileOpen;

		// 		uint returnVal = UnmanagedFunctions.CoCreateInstance(ref CLSID_FileOpenDialog, IntPtr.Zero, (uint)(CLSCTX.CLSCTX_INPROC_SERVER | CLSCTX.CLSCTX_LOCAL_SERVER | CLSCTX.CLSCTX_REMOTE_SERVER), ref IID_IUnknown, &pFileOpen);

		// 		UI.Log.LogInfo($"pFileOpen: {pFileOpen}");
		// 		UI.Log.LogInfo($"Marshaled Type: {Marshal.GetObjectForIUnknown(pFileOpen)}");

		// 		UnmanagedFunctions.CoUninitialize();
		// 	}

		// 	LoadFileFromDisk(path, action);
		// }
	}
}