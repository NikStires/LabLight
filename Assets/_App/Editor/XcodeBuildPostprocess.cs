using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class XcodeBuildPostprocess
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.VisionOS)
        {
            Debug.Log("Build target is not visionOS, skipping resource copy");
            return;
        }

        Debug.Log($"Starting visionOS build post-process at path: {buildPath}");

        // Get the correct path for visionOS project
        string projectPath = Path.Combine(buildPath, "Unity-VisionOS.xcodeproj/project.pbxproj");
        Debug.Log($"Looking for PBX project at: {projectPath}");

        if (!File.Exists(projectPath))
        {
            Debug.LogError($"Could not find Xcode project at: {projectPath}");
            return;
        }

        var project = new PBXProject();
        project.ReadFromFile(projectPath);

        string targetGuid = project.GetUnityFrameworkTargetGuid();
        Debug.Log($"Target GUID: {targetGuid}");

        // Copy resources from Unity Assets to Xcode project
        string resourcesPath = "Assets/Resources/Protocol";
        Debug.Log($"Looking for resources in: {resourcesPath}");

        if (!Directory.Exists(resourcesPath))
        {
            Debug.LogError($"Resources directory not found: {resourcesPath}");
            return;
        }

        string[] files = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
        Debug.Log($"Found {files.Length} files to process");

        // Define allowed file extensions
        string[] allowedExtensions = new[] { 
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", 
            ".mp4", ".mov", ".avi", ".wmv", ".m4v",
            ".pdf"
        };

        // Keep track of processed filenames to avoid duplicates
        var processedFileNames = new HashSet<string>();

        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                Debug.Log($"Skipping meta file: {file}");
                continue;
            }

            string extension = Path.GetExtension(file).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                Debug.Log($"Skipping non-media file: {file}");
                continue;
            }

            string fileName = Path.GetFileName(file);
            if (processedFileNames.Contains(fileName))
            {
                Debug.LogWarning($"Skipping duplicate file name: {fileName}");
                continue;
            }
            processedFileNames.Add(fileName);

            string relativePath = Path.GetRelativePath(resourcesPath, file);
            string destination = Path.Combine(buildPath, "Data/Resources", relativePath);
            
            Debug.Log($"Processing file: {relativePath}");
            Debug.Log($"From: {file}");
            Debug.Log($"To: {destination}");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
                Debug.Log($"Successfully copied: {relativePath}");

                string xcodeRelativePath = Path.Combine("Data/Resources", relativePath);
                string fileGuid = project.AddFile(xcodeRelativePath, xcodeRelativePath);
                project.AddFileToBuild(targetGuid, fileGuid);
                Debug.Log($"Added to Xcode project with GUID: {fileGuid}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error copying file {relativePath}: {e.Message}");
            }
        }

        try
        {
            project.WriteToFile(projectPath);
            Debug.Log("Successfully wrote changes to Xcode project");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving Xcode project: {e.Message}");
        }

        // Verify the files exist in the build directory
        foreach (string file in files)
        {
            if (file.EndsWith(".meta")) continue;
            
            string relativePath = Path.GetRelativePath(resourcesPath, file);
            string destination = Path.Combine(buildPath, "Data/Resources", relativePath);
            
            if (File.Exists(destination))
            {
                Debug.Log($"✅ Verified file exists in build: {destination}");
                Debug.Log($"File size: {new FileInfo(destination).Length} bytes");
            }
            else
            {
                Debug.LogError($"❌ File not found in build: {destination}");
            }
        }
    }
}