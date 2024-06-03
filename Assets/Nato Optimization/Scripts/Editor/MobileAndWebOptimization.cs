using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MobileAndWebOptimization : EditorWindow
{
    private enum Tab
    {
        ABOUT,
        TEXTURE,
        AUDIO_CLIPS,
        MODELS,
        MOBILE_BUILD,
        WEBGL_BUILD,
        ASSETS_SIZE,
    }

    private Tab currentTab = Tab.ABOUT;
    private List<TextureInfo> textureList = new List<TextureInfo>();
    private List<AudioClipInfo> audioClipList = new List<AudioClipInfo>();
    private List<ModelInfo> modelList = new List<ModelInfo>();
    private List<AssetSizeInfo> buildLogList = new List<AssetSizeInfo>();

    private Vector2 scrollPosition = Vector2.zero;

    private const int visibleItems = 15; 

    private struct TextureInfo
    {
        public string name;
        public string type;
        public int width;
        public int height;
        public int maxSize;
        public TextureImporterCompression compression;
        public bool crunchCompression;
        public int crunchCompQuality;
        public string assetPath;
    }

    private struct AudioClipInfo
    {
        public string name;
        public string loadType;
        public AudioClipLoadType audioClipLoadType;
        public bool forceToMono;
        public string quality;
        public string assetPath;
    }

    private struct ModelInfo
    {
        public string name;
        public bool readWriteEnabled;
        public bool polygonsOptimized;
        public bool verticesOptimized;
        public ModelImporterMeshCompression meshCompression;
        public ModelImporterAnimationCompression animationCompression;
        public string assetPath;
    }


    private class AssetSizeInfo
    {
        public string name;
        public float size;
        public float sizePercentage;
        public string assetPath;
    }

    [MenuItem("Window/Nato Optimization/Mobile and WebGL Optimization")]
    private static void Init()
    {
        MobileAndWebOptimization window = (MobileAndWebOptimization)EditorWindow.GetWindow(typeof(MobileAndWebOptimization));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("ABOUT"))
        {
            currentTab = Tab.ABOUT;
        }
        if (GUILayout.Button("TEXTURES"))
        {
            currentTab = Tab.TEXTURE;
            LoadTextureInfo();
        }

        if (GUILayout.Button("AUDIO CLIPS"))
        {
            currentTab = Tab.AUDIO_CLIPS;
            LoadAudioClipInfo();
        }
        if (GUILayout.Button("MODELS"))
        {
            currentTab = Tab.MODELS;
            LoadModelInfo();
        }
        if (GUILayout.Button("MOBILE BUILD"))
        {
            currentTab = Tab.MOBILE_BUILD;
        }
        if (GUILayout.Button("WEBGL BUILD"))
        {
            currentTab = Tab.WEBGL_BUILD;
        }
        if (GUILayout.Button("ASSETS SIZE"))
        {
            currentTab = Tab.ASSETS_SIZE;
            LoadBuildLogInfo(); 
        }
       
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        switch (currentTab)
        {
            case Tab.ABOUT:
                DisplayAbout();
                break;
            case Tab.TEXTURE:
                DisplayTextureAnalysis();
                break;
            case Tab.AUDIO_CLIPS:
                DisplayAudioClipAnalysis();
                break;
            case Tab.MODELS:
                DisplayModelAnalysis();
                break;
            case Tab.MOBILE_BUILD:
                DisplayMobileBuildAnalyses();
                break;
            case Tab.WEBGL_BUILD:
                DisplayWebGLBuildSettings();
                break;
            case Tab.ASSETS_SIZE:
                DisplayBuildLogAnalysis();
                break;
        }
    }

    private void DisplayAbout()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("This package provides optimization tips for mobile and webgl development.");
        GUILayout.Label("Developed by NATO Game Studio.\n");


        GUIStyle linkStyle = new GUIStyle(GUI.skin.label);
        linkStyle.stretchWidth = false;

        GUILayout.Label("https://renatomarcacini.github.io/natogamestudio/", linkStyle);

        Rect labelRect = GUILayoutUtility.GetLastRect();

        EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);

        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
        {
            Application.OpenURL("https://renatomarcacini.github.io/natogamestudio/");
        }

    }

    private void DisplayTextureAnalysis()
    {
        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * visibleItems;

        GUILayout.BeginVertical("box");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));

        GUIStyle greenStyle = new GUIStyle(GUI.skin.label);
        greenStyle.normal.textColor = Color.green;

        GUIStyle redStyle = new GUIStyle(GUI.skin.label);
        redStyle.normal.textColor = Color.red;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Texture Name", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Texture Size", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Max Size", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Compression", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Crunch Compression", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Crunch Comp Quality", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        foreach (var textureInfo in textureList)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(textureInfo.name, GUILayout.Width(150)))
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Texture>(textureInfo.assetPath));

            GUILayout.Label(textureInfo.type, GUILayout.Width(100));
            GUILayout.Label($"{textureInfo.width}x{textureInfo.height}", GUILayout.Width(100));

            int idealMaxSize = GetIdealMaxSize(textureInfo.width, textureInfo.height);
            GUILayout.Label(textureInfo.maxSize.ToString(), textureInfo.maxSize == idealMaxSize ? greenStyle : redStyle, GUILayout.Width(100));

            string compression = "Normal";
            switch (textureInfo.compression)
            {
                case TextureImporterCompression.Uncompressed:
                    compression = "None";
                    break;
                case TextureImporterCompression.Compressed:
                    compression = "Normal";
                    break;
                case TextureImporterCompression.CompressedHQ:
                    compression = "High";
                    break;
                case TextureImporterCompression.CompressedLQ:
                    compression = "Low";
                    break;
            }
            GUILayout.Label(compression, GUILayout.Width(100));

            GUILayout.Label(textureInfo.crunchCompression ? "Yes" : "No", GUILayout.Width(150));
            GUILayout.Label(textureInfo.crunchCompQuality.ToString(), GUILayout.Width(150));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            LoadTextureInfo();
        }


        string maxSizeDescription = "Use the minimum settings that produce visually acepptable results.\n";
        string compressionDescription = "Determines how texture data is compressed to occupy less storage space. Different compression levels, such as \"Normal\" or \"Low\", can be selected in Unity. Lower compression may reduce visual quality but decrease file size.\n";
        string crunchDescription = "Unity's additional compression method that compresses textures using the Crunch algorithm, resulting in smaller file sizes when enabled. Useful for mobile devices where file size is critical.\n";
        string crunchQuality = "Sets the quality of the Crunch compression applied to textures. Higher quality results in larger texture files but better visual quality. Adjusting this value balances file size and visual quality based on project needs and storage constraints.\n";

        GUILayout.BeginVertical();
        GUILayout.Label("Max Size", EditorStyles.boldLabel);
        GUILayout.Label(maxSizeDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Compression", EditorStyles.boldLabel);
        GUILayout.Label(compressionDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Crunch Compression", EditorStyles.boldLabel);
        GUILayout.Label(crunchDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Crunch Comp. Quality", EditorStyles.boldLabel);
        GUILayout.Label(crunchQuality, EditorStyles.wordWrappedLabel);
        GUILayout.EndVertical();

    }

    private int GetIdealMaxSize(int width, int height)
    {
        int largestDimension = Mathf.Max(width, height);
        int[] maxSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

        foreach (int maxSize in maxSizes)
        {
            if (largestDimension <= maxSize)
            {
                return maxSize;
            }
        }
        return 16384;
    }

    private void LoadTextureInfo()
    {
        textureList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:texture2D");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith("Assets/"))
                continue;

            if (path.StartsWith("Packages/"))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                TextureInfo info = new TextureInfo();
                info.name = Path.GetFileName(path);
                info.type = importer.textureType.ToString();
                info.width = texture.width;
                info.height = texture.height;
                info.maxSize = importer.maxTextureSize;
                info.compression = importer.textureCompression;
                info.crunchCompression = importer.crunchedCompression;
                info.crunchCompQuality = importer.compressionQuality;
                info.assetPath = path;

                textureList.Add(info);
            }
        }
    }

    private void DisplayAudioClipAnalysis()
    {
        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * visibleItems;

        GUIStyle greenStyle = new GUIStyle(GUI.skin.label);
        greenStyle.normal.textColor = Color.green;

        GUIStyle redStyle = new GUIStyle(GUI.skin.label);
        redStyle.normal.textColor = Color.red;

        GUILayout.BeginVertical("box");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Audio Clip Name", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Load Type", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Force to Mono", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Quality", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        foreach (var audioClipInfo in audioClipList)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(audioClipInfo.name, GUILayout.Width(150)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AudioClip>(audioClipInfo.assetPath));
            }

            GUILayout.Label(audioClipInfo.loadType, GUILayout.Width(150));
            GUILayout.Label(audioClipInfo.forceToMono ? "Enabled":"Disabled", audioClipInfo.forceToMono ? greenStyle : redStyle ,GUILayout.Width(150));
            GUILayout.Label(audioClipInfo.quality, GUILayout.Width(150));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();


        GUILayout.EndVertical();
        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            LoadAudioClipInfo();
        }

        string info = "Compress audio files to formats like OGG to reduce the total bundle size.";
        string loadTypeDescription = "Determines how the audio clip data is loaded into memory during runtime. 'Decompress on Load' is better for audio effects (small clips). 'Compressed in Memory' is better for backgrounds audio clips (medium and large clips).\n";
        string forceToMonoDescription = "Always enable force to mono to reduce the filze size, use less memory.\n";
        string qualityDescription = "Sets the audio clip compression quality. Higher quality may result in larger file sizes but better sound fidelity. Adjust this value based on project requirements and device constraints.\n";

        GUILayout.BeginVertical();
        GUILayout.Label(info, EditorStyles.boldLabel);

        GUILayout.Label("Load Type", EditorStyles.boldLabel);
        GUILayout.Label(loadTypeDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Force to mono", EditorStyles.boldLabel);
        GUILayout.Label(forceToMonoDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Quality", EditorStyles.boldLabel);
        GUILayout.Label(qualityDescription, EditorStyles.wordWrappedLabel);
        GUILayout.EndVertical();
    }

    private void LoadAudioClipInfo()
    {
        audioClipList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:audioClip");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith("Assets/"))
                continue;

            if (path.StartsWith("Packages/"))
                continue;

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;

            if (importer != null)
            {
                AudioClipInfo info = new AudioClipInfo();
                info.name = Path.GetFileName(path);
                info.loadType = importer.defaultSampleSettings.loadType.ToString();
                info.audioClipLoadType = importer.defaultSampleSettings.loadType;
                info.forceToMono = importer.forceToMono;
                info.quality = (importer.defaultSampleSettings.quality * 100).ToString();
                info.assetPath = path;

                audioClipList.Add(info);
            }
        }
    }

    private void DisplayModelAnalysis()
    {
        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * visibleItems;

        GUILayout.BeginVertical("box");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Model Name", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("R/W enabled", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Polygons optimized", EditorStyles.boldLabel, GUILayout.Width(120));
        GUILayout.Label("Vertices optimized", EditorStyles.boldLabel, GUILayout.Width(120));
        GUILayout.Label("Mesh compression", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Animation compression", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        foreach (var modelInfo in modelList)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(modelInfo.name, GUILayout.Width(150)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(modelInfo.assetPath));
            }

            GUILayout.Label(modelInfo.readWriteEnabled ? "Yes" : "No", GUILayout.Width(100));
            GUILayout.Label(modelInfo.polygonsOptimized ? "Yes" : "No", GUILayout.Width(120));
            GUILayout.Label(modelInfo.verticesOptimized ? "Yes" : "No", GUILayout.Width(120));
            GUILayout.Label(modelInfo.meshCompression.ToString(), GUILayout.Width(150));
            GUILayout.Label(modelInfo.animationCompression.ToString(), GUILayout.Width(150));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();


        GUILayout.EndVertical();
        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            LoadModelInfo();
        }

        string readWriteDescription = "Enabling Read/Write option duplicates the mesh in memory. Use with caution as it can increase memory usage. In most cases, you should disable this options to save runtime memory usage.\n";
        string polygonsOptimizedDescription = "Improves rendering performance by rearranging the order of polygons in the mesh to make better use of the GPU's internal caches.\n";
        string verticesOptimizedDescription = "Improves rendering performance by rearranging the order of vertices in the mesh to make better use of the GPU's internal caches.\n";
        string meshCompressionDescription = "Agressive compression can reduce disk space. Note that the agressive compression can result in inacucuracy, so experiment with compression levels to see what works for your models.\n";
        string animationCompressionDescription = "Change properties can reduces the final build size but may introduce errors in animations.\n";


        GUILayout.BeginVertical();

        GUILayout.Label("R/W enabled", EditorStyles.boldLabel);
        GUILayout.Label(readWriteDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Polygons Optimized", EditorStyles.boldLabel);
        GUILayout.Label(polygonsOptimizedDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Vertices Optimized", EditorStyles.boldLabel);
        GUILayout.Label(verticesOptimizedDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Mesh Compression", EditorStyles.boldLabel);
        GUILayout.Label(meshCompressionDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Animation Compression", EditorStyles.boldLabel);
        GUILayout.Label(animationCompressionDescription, EditorStyles.wordWrappedLabel);

        GUILayout.EndVertical();
    }

    private void LoadModelInfo()
    {
        modelList.Clear();

        string[] guids = AssetDatabase.FindAssets("t:model");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith("Assets/"))
                continue;

            if (path.StartsWith("Packages/"))
                continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

            if (importer != null)
            {
                ModelInfo info = new ModelInfo();
                info.name = Path.GetFileName(path);
                info.readWriteEnabled = importer.isReadable;
                info.polygonsOptimized = importer.optimizeMeshPolygons;
                info.verticesOptimized = importer.optimizeGameObjects;
                info.meshCompression = importer.meshCompression;
                info.animationCompression = importer.animationCompression;
                info.assetPath = path;

                modelList.Add(info);
            }
        }
    }

    private void DisplayBuildLogAnalysis()
    {
        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * visibleItems;

        GUILayout.BeginVertical("box");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Size", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Size (%)", EditorStyles.boldLabel, GUILayout.Width(150));
        GUILayout.Label("Path", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        buildLogList = buildLogList.OrderByDescending(b => b.sizePercentage).ToList();
        foreach (var buildLogInfo in buildLogList)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(buildLogInfo.name, GUILayout.Width(150)))
            {
                Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(buildLogInfo.assetPath);
                EditorGUIUtility.PingObject(assetObject);
            }
            
            GUILayout.Label(FormatBytes(buildLogInfo.size), GUILayout.Width(150));
            GUILayout.Label(buildLogInfo.sizePercentage.ToString("0.00") + "%", GUILayout.Width(150));
            GUILayout.Label(buildLogInfo.assetPath);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();


        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            LoadModelInfo();
        }
    }

    private void LoadBuildLogInfo()
    {
        buildLogList.Clear();

        float totalSize = 0;

        List<string> validExtensions = new List<string> { ".prefab", ".fbx", ".dae", ".obj", ".png", ".jpg", ".mp3", ".wav", ".ogg" }; // Adicione mais extensões conforme necessário
        List<string> guids = new List<string>();

        foreach (string extension in validExtensions)
        {
            guids.AddRange(AssetDatabase.FindAssets("t:Object", new[] { "Assets" }).Where(guid => AssetDatabase.GUIDToAssetPath(guid).EndsWith(extension)));
        }

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith("Assets/") || path.StartsWith("Packages/"))
                continue;

            if (AssetDatabase.IsValidFolder(path))
                continue;

            long fileSizeBytes = new FileInfo(path).Length;

            AssetSizeInfo info = new AssetSizeInfo();
            info.size = fileSizeBytes;
            info.assetPath = path;
            info.name = Path.GetFileName(path);

            buildLogList.Add(info);

            totalSize += fileSizeBytes;
        }

        foreach (var info in buildLogList)
        {
            info.sizePercentage = (info.size / totalSize) * 100;
        }
    }

    private string FormatBytes(float bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return string.Format("{0:0.##} {1}", size, suffixes[suffixIndex]);
    }

    private void DisplayMobileBuildAnalyses()
    {
        GUILayout.BeginVertical("box");

        GUIStyle greenStyle = new GUIStyle(GUI.skin.label);
        greenStyle.normal.textColor = Color.green;

        GUIStyle redStyle = new GUIStyle(GUI.skin.label);
        redStyle.normal.textColor = Color.red;

        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 6;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));

        GUILayout.BeginHorizontal();
        GUILayout.Label("Setting", EditorStyles.boldLabel, GUILayout.Width(200));
        GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Incremental GC", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.gcIncremental ? "Yes" : "No", PlayerSettings.gcIncremental == true ? greenStyle : redStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scripting Backend (Android)", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP ? "IL2CPP" : "Mono", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scripting Backend (iOS)", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS) == ScriptingImplementation.IL2CPP ? "IL2CPP" : "Mono", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Color Space", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.colorSpace.ToString(), GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Vsync", GUILayout.Width(200));
        GUILayout.Label(GetVSyncLabel(QualitySettings.vSyncCount), GetVSyncLabel(QualitySettings.vSyncCount) == "Don't Sync" ? greenStyle : redStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();


        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            DisplayMobileBuildAnalyses();
        }

        string incrementalGCDescription = "Divides garbage collection into smaller steps, potentially improving performance, always leave it enabled. \n";
        string scriptingBackendDescription = "By default it is Mono and it already works well. IL2CPP offers better performance but increases binary size and there may be specific crashes, IL2CPP is required for IOS.\n";
        string colorSpaceDescription = "Gamma has better performance (~10-30%) than Linear. However, the display is a bit worse.\n";
        string vSyncDescription = "Enable Vsync in mobile game might cause lag, faster battery drain and also limit the frame rate to the monitor´s refresh rate. Leave it on \"Don't Sync\"\n";

        GUILayout.BeginVertical();

        GUILayout.Label("Incremental GC", EditorStyles.boldLabel);
        GUILayout.Label(incrementalGCDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Scripting Backend", EditorStyles.boldLabel);
        GUILayout.Label(scriptingBackendDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Color Space", EditorStyles.boldLabel);
        GUILayout.Label(colorSpaceDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Vsync", EditorStyles.boldLabel);
        GUILayout.Label(vSyncDescription, EditorStyles.wordWrappedLabel);


        GUILayout.EndVertical();
    }

    private string GetVSyncLabel(int vSyncCount)
    {
        switch (vSyncCount)
        {
            case 0:
                return "Don't Sync";
            case 1:
                return "Every V Blank";
            case 2:
                return "Every Second V Blank";
            default:
                return "Unknown";
        }
    }

    private void DisplayWebGLBuildSettings()
    {
        GUILayout.BeginVertical("box");
        int scrollViewHeight = (int)(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 7;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));
        GUIStyle greenStyle = new GUIStyle(GUI.skin.label);
        greenStyle.normal.textColor = Color.green;

        GUIStyle redStyle = new GUIStyle(GUI.skin.label);
        redStyle.normal.textColor = Color.red;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Setting", EditorStyles.boldLabel, GUILayout.Width(200));
        GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Brotli Compression", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli ? "Enabled" : "Disabled",
            PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli ? greenStyle : redStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name file as hashes", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.WebGL.nameFilesAsHashes ? "Enabled" : "Disabled",
            PlayerSettings.WebGL.nameFilesAsHashes ? greenStyle : redStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Exception Support", GUILayout.Width(200));
        string exceptionSupport = PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.None
            ? "None"
            : (PlayerSettings.WebGL.exceptionSupport == WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly ? "Explicitly thrown exceptions only" : "Full With Stacktrace");
        GUILayout.Label(exceptionSupport, GUILayout.Width(250));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Strip engine code", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.WebGL) == ManagedStrippingLevel.Disabled
            ? "Disabled"
            : (PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.WebGL) == ManagedStrippingLevel.Low ? "Low"
            : (PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.WebGL) == ManagedStrippingLevel.Medium ? "Medium" : "High")), GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Color Space", GUILayout.Width(200));
        GUILayout.Label(PlayerSettings.colorSpace.ToString(), GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Vsync", GUILayout.Width(200));
        GUILayout.Label(GetVSyncLabel(QualitySettings.vSyncCount), GetVSyncLabel(QualitySettings.vSyncCount) == "Don't Sync" ? greenStyle : redStyle, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(10);
        if (GUILayout.Button("Analyze"))
        {
            DisplayMobileBuildAnalyses();
        }

        string brotliCompressionDescription = "Utilizes Brotli compression to reduce file sizes, improving loading times.\n";
        string nameFileHashesDescriptions = "Names files based on hashes, aiding browser caching for improved performance.\n";
        string exceptionSupportDescription = "Controls exception handling during runtime. \"Explicitly thrown exceptions only\" may enhance performance.\n";
        string stripEngineCodeDescription = "To decrese the bundle size even more, you can select Medium or High stripping from Player Settings.\n";
        string colorSpaceDescription = "Gamma has better performance (~10-30%) than Linear. However, the display is a bit worse.\n";
        string vSyncDescription = "Enable Vsync in webgl game might cause lag, faster battery drain and also limit the frame rate to the monitor´s refresh rate. Leave it on \"Don't Sync\"\n";

        GUILayout.BeginVertical();

        GUILayout.Label("Brotli Compression", EditorStyles.boldLabel);
        GUILayout.Label(brotliCompressionDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Name file as hashes", EditorStyles.boldLabel);
        GUILayout.Label(nameFileHashesDescriptions, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Exception Support", EditorStyles.boldLabel);
        GUILayout.Label(exceptionSupportDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Strip engine code", EditorStyles.boldLabel);
        GUILayout.Label(stripEngineCodeDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Color Space", EditorStyles.boldLabel);
        GUILayout.Label(colorSpaceDescription, EditorStyles.wordWrappedLabel);

        GUILayout.Label("Vsync", EditorStyles.boldLabel);
        GUILayout.Label(vSyncDescription, EditorStyles.wordWrappedLabel);

        GUILayout.EndVertical();
    }


}
