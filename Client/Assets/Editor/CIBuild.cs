using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 커맨드라인 / CI 에서 플레이어 빌드를 돌리기 위한 진입점.
///   Unity.exe -batchmode -quit -nographics -projectPath &lt;path&gt; -executeMethod CIBuild.BuildWindows
/// 에디터를 열지 않고도 "이 프로젝트가 실제로 빌드되는가"를 검증할 수 있다.
/// </summary>
public static class CIBuild
{
    [MenuItem("Tools/CI/Build Windows64")]
    public static void BuildWindows()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("[CIBuild] Build Settings 에 활성화된 씬이 없습니다.");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Win64/DungeonLaders.exe",
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"[CIBuild] result={summary.result} " +
                  $"errors={summary.totalErrors} warnings={summary.totalWarnings} " +
                  $"size={summary.totalSize} bytes time={summary.totalTime}");

        if (summary.result != BuildResult.Succeeded)
            throw new Exception($"[CIBuild] 빌드 실패: {summary.result} ({summary.totalErrors} errors)");

        Debug.Log("[CIBuild] 빌드 성공: " + summary.outputPath);
    }
}
