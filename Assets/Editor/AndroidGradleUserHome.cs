using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Android;

/// <summary>
/// Задаёт Gradle User Home в папке проекта (только ASCII), чтобы сборка Android
/// не падала из-за кириллицы в пути пользователя (C:\Users\Андрей\.gradle).
/// Ошибка: ClassNotFoundException com.google.prefab.cli.AppKt при games-frame-pacing.
/// </summary>
[InitializeOnLoad]
public static class AndroidGradleUserHome
{
    private const string GradleHomeFolderName = "GradleUserHome";

    static AndroidGradleUserHome()
    {
        SetGradleUserHomeToProject();
    }

    [MenuItem("Edit/Android/Set Gradle User Home to project (fix Cyrillic path)")]
    public static void SetGradleUserHomeToProject()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(projectPath))
            return;

        string gradleHome = Path.Combine(projectPath, "Library", GradleHomeFolderName);
        gradleHome = Path.GetFullPath(gradleHome);

        if (!Directory.Exists(gradleHome))
        {
            try
            {
                Directory.CreateDirectory(gradleHome);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Android Gradle] Could not create Gradle home directory: {e.Message}");
                return;
            }
        }

        // Unity 6000.2+: Gradle.userHomePath; в более старых версиях: gradleUserHomePath
        AndroidExternalToolsSettings.Gradle.userHomePath = gradleHome;
        Debug.Log($"[Android Gradle] Gradle User Home set to: {gradleHome}");
    }
}
