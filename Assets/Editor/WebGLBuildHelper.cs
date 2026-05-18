using UnityEditor;
using UnityEngine;

public class WebGLBuildHelper : EditorWindow
{
    [MenuItem("Build/Quick Setup WebGL")]
    public static void SetupWebGL()
    {
        // 1. Chuyển platform sang WebGL
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        // 2. Cấu hình Player Settings tối ưu
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        
        // Cấu hình tên file build không có symbols để nhẹ hơn
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        
        Debug.Log("<color=green>WebGL Setup Complete! Bạn có thể nhấn Build trong Build Settings ngay bây giờ.</color>");
    }

    [MenuItem("Build/Build WebGL (To Desktop)")]
    public static void BuildWebGL()
    {
        SetupWebGL();

        string buildPath = "Builds/WebGL";
        string[] scenes = GetScenes();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("<color=cyan>WebGL Build Finished at: " + buildPath + "</color>");
    }

    private static string[] GetScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        string[] scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }
        return scenePaths;
    }
}
