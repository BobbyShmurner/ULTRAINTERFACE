using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles() {
        string assetBundleDirectory = "Assets/StreamingAssets";

        File.Create("build.lock");

        if (!Directory.Exists(Application.streamingAssetsPath)) {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        File.Delete("build.lock");
        Debug.Log("Finished Building Asset Bundles");
	}
}