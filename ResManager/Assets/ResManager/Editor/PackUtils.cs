using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WLGame
{
    public class PackUtils
    {
        /* 函数说明：单文件打包成一份资源 */
        public static void PackAsset(string filePath, string bundleFile)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string[] filePathArray = new string[] { filePath };
            PackAsset(filePathArray, bundleFile);
        }

        /* 函数说明：多文件打包成一份资源 */
        public static void PackAsset(string[] filePathArray, string bundleFile)
        {
            if (filePathArray == null || filePathArray.Length == 0)
                return;

            if (string.IsNullOrEmpty(bundleFile))
                return;

            Dictionary<string, string> tempDict = new Dictionary<string, string>();
            for (int i = 0; i < filePathArray.Length; i++)
            {
                int startIndex = filePathArray[i].LastIndexOf('/') + 1;
                int endIndex = filePathArray[i].LastIndexOf('.');
                if (startIndex < 0 || endIndex < 0)
                    continue;

                string name = filePathArray[i].Substring(startIndex, endIndex - startIndex);
                if (tempDict.ContainsKey(name))
                {
                    Debug.LogError(string.Format("! Warning : There are repeat names ({0}) in the asset what you will pack !", name));
                    return;
                }
                tempDict[name] = filePathArray[i];
            }

            List<UnityEngine.Object> objectList = new List<UnityEngine.Object>();
            for (int i = 0; i < filePathArray.Length; i++)
            {
                string filePath = filePathArray[i];
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
                if (go != null) objectList.Add(go);
            }
            BuildAssetBundle(objectList.ToArray(), bundleFile);
            AssetDatabase.Refresh();
        }

        /* 函数说明：单场景打包成一份资源 */
        public static void PackScene(string scenePath, string bundleFile)
        {
            if (string.IsNullOrEmpty(scenePath))
                return;
            BuildSceneBundle(scenePath, bundleFile);
            AssetDatabase.Refresh();
        }

        /* 函数说明：多场景打包成一份资源 */
        public static void PackScene()
        {
            List<string> sceneList = GetFilesFormFolder("/Package/Scenes/", "*.unity", false);
            for(int i = 0;i < sceneList.Count;i++)
            {
                if(sceneList[i].IndexOf("Entry.unity") >= 0 || sceneList[i].IndexOf("Loading.unity") >= 0)
                    continue;
                PackScene(sceneList[i],Path.GetFileNameWithoutExtension(sceneList[i]));
            }
        }

        /* 函数说明：压缩纹理 */
        public static void CompressTexture(string[] texturePathArray, TextureImporterFormat format)
        {
            if (texturePathArray == null || texturePathArray.Length == 0)
                return;

            for (int i = 0; i < texturePathArray.Length; i++)
            {
                CompressTexture(texturePathArray[i], format);
            }
        }

        /* 函数说明：压缩纹理 */
        public static void CompressTexture(string texturePath, TextureImporterFormat format)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter == null)
            {
                return;
            }

            string platform = "";
#if UNITY_ANDROID
            platform = "Android";
#elif UNITY_IPHONE
            platform = "iPhone";
#endif
            if (string.IsNullOrEmpty(platform))
            {
                return;
            }
            textureImporter.textureType = TextureImporterType.Advanced;
            textureImporter.generateCubemap = TextureImporterGenerateCubemap.None;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.anisoLevel = 2;
            textureImporter.isReadable = false;
            textureImporter.textureFormat = format;
            textureImporter.SetPlatformTextureSettings(platform, textureImporter.maxTextureSize, format);
            AssetDatabase.ImportAsset(texturePath);
        }

        /* 函数说明：获取(Assets目录下)某目录里的文件列表 */
        public static List<string> GetFilesFormFolder(string folder, string type, bool allDir)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(type))
                return result;

            string folderPath = Application.dataPath;
            if (folder[0] != '/')
                folderPath += "/";
            if (folder[folder.Length - 1] != '/')
                folder += "/";
            folderPath += folder;

            SearchOption searchOption = (allDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            string[] srcFileArray = Directory.GetFiles(folderPath, "*.*", searchOption);
            for (int i = 0; i < srcFileArray.Length; i++)
            {
                string filePath = srcFileArray[i];
                if (IsFileOfType(filePath, type))
                {
                    filePath = filePath.Replace("\\", "/");
                    filePath = filePath.Substring(filePath.IndexOf("Assets/"));
                    result.Add(filePath);
                }
            }
            return result;
        }

        /* 函数说明：从对象数组中 获取属于某类型的文件 */
        public static List<string> GetFilesFormObjects(UnityEngine.Object[] objects, string type)
        {
            List<string> result = new List<string>();
            if (objects == null || objects.Length == 0 || string.IsNullOrEmpty(type))
                return result;

            for (int i = 0; i < objects.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(objects[i]);
                if (!string.IsNullOrEmpty(path))
                {
                    if (IsFileOfType(path, type))
                    {
                        result.Add(path);
                    }
                }
            }
            return result;
        }

        /* 函数说明：从文件数组中 获取属于某类型的文件 */
        public static List<string> GetFilesFormFiles(string[] files, string type)
        {
            List<string> result = new List<string>();
            if (files == null || files.Length == 0 || string.IsNullOrEmpty(type))
                return result;

            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                if (!string.IsNullOrEmpty(path))
                {
                    if (IsFileOfType(path, type))
                    {
                        result.Add(path);
                    }
                }
            }
            return result;
        }

        /* 函数说明：从文件路径中获取文件名(不包含后缀名) */
        public static string GetFileNameFormPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            int startIndex = path.LastIndexOf("/") + 1;
            if (startIndex > path.Length)
                return "";

            int endIndex = path.LastIndexOf(".");
            int length = endIndex - startIndex;
            if (length <= 0)
                return "";

            string result = path.Substring(startIndex, length);
            return result;
        }

        /* 函数说明：从文件路径中获取最后的目录名 */
        public static string GetLastFolderFormPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            int endIndex = path.LastIndexOf("/");
            string result = path.Substring(0, endIndex);
            int startIndex = result.LastIndexOf("/") + 1;
            result = result.Substring(startIndex, endIndex - startIndex);
            return result;
        }

        /* 函数说明：CheckAssetBundleBuild用于创建打包资源列表 */
        public static void CheckAssetBundleBuild(AssetImporter assetImporter,ref Dictionary<string, AssetBundleBuild> assetBundleBuilds)
        {
            string assetBundleName = assetImporter.assetBundleName;
            if(assetBundleBuilds.ContainsKey(assetBundleName))
            {
                AssetBundleBuild haveAssetBundleBuild = assetBundleBuilds[assetBundleName];
                for(int i = 0;i < haveAssetBundleBuild.assetNames.Length;i++)
                {
                    if (assetImporter.assetPath.Equals(haveAssetBundleBuild.assetNames[i]))
                    {
                        return;
                    }
                }
                string[] assetsNames = new string[haveAssetBundleBuild.assetNames.Length + 1];
                haveAssetBundleBuild.assetNames.CopyTo(assetsNames, 0);
                assetsNames[assetsNames.Length - 1] = assetImporter.assetPath;
                haveAssetBundleBuild.assetNames = assetsNames;
                assetBundleBuilds[assetBundleName] = haveAssetBundleBuild;
            }
            else
            {
                AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                string[] assetsNames = new string[1];
                assetsNames[0] = assetImporter.assetPath;
                assetBundleBuild.assetNames = assetsNames;
                assetBundleBuild.assetBundleName = assetBundleName;
                assetBundleBuilds.Add(assetBundleName, assetBundleBuild);
            }
        }

        /* 函数说明：PackAtlas用于创建需要打包图集列表 */
        public static void PackAtlas(ref Dictionary<string,AssetBundleBuild> assetBundleBuilds)
        {
            Debug.LogError("导入NGUI，恢复下面的代码");
            /*
            List<string> atlasPathList = GetFilesFormFolder("Package/UI/UIAtlas","*.prefab",true);
            for(int i = 0; i < atlasPathList.Count; i++)
            {
                string curPath = atlasPathList[i];
                UIAtlas atlas = AssetDatabase.LoadAssetAtPath<UIAtlas>(curPath);
                if (atlas == null)
                    continue;
                AssetImporter assetImporter = AssetImporter.GetAtPath(curPath);
                if (assetImporter.assetPath.IndexOf("Assets/Package/UI/UIAtlas/") < 0)
                    continue;
                string assetBundleName = assetImporter.assetPath.Substring(0, assetImporter.assetPath.LastIndexOf("/"));
                assetImporter.assetBundleName = assetBundleName;
                CheckAssetBundleBuild(assetImporter,ref assetBundleBuilds);
                string[] dependencies = AssetDatabase.GetDependencies(assetImporter.assetPath);
                for(int j = 0; j < dependencies.Length; j++)
                {
                    if (dependencies[j].EndsWith(".png") == false && dependencies[j].EndsWith(".mat") == false)
                        continue;
                    AssetImporter dependencyImporter = AssetImporter.GetAtPath(dependencies[j]);
                    dependencyImporter.assetBundleName = assetImporter.assetBundleName;
                    CheckAssetBundleBuild(dependencyImporter, ref assetBundleBuilds);
                }
            }
             */
        }

        /* 函数说明：PackEffect */
        public static void PackEffect(ref Dictionary<string, AssetBundleBuild> assetBundleBuilds)
        {
            List<string> effectPathList = GetFilesFormFolder("/Package/Effect/QTE", "*.prefab",true);
            //List<string> effectPathList = GetFilesFormFolder("/Effect/Texture/lizi/Materials/", "*.mat", true);
            for (int i = 0; i < effectPathList.Count; i++)
            {
                string curPath = effectPathList[i];
                AssetImporter assetImporter = AssetImporter.GetAtPath(curPath);
                Debug.LogError("effectPathList[" + i + "] " + curPath);
                //if(assetImporter.assetPath.IndexOf("Assets/Package/Effect/") < 0)
                //    continue;
                string assetBundleName = assetImporter.assetPath;
                assetImporter.assetBundleName = assetBundleName;
                //CheckAssetBundleBuild(assetImporter, ref assetBundleBuilds);
                string[] dependencies = AssetDatabase.GetDependencies(assetImporter.assetPath);
                for(int j = 0; j < dependencies.Length; j++)
                {
                    Debug.LogError("dependencies[" + j + "] " + dependencies[j]);
                    continue;
                    if(dependencies[j].EndsWith(".png") == false && dependencies[j].EndsWith(".mat") == false)
                        continue;
                    AssetImporter dependencyImporter = AssetImporter.GetAtPath(dependencies[j]);
                    dependencyImporter.assetBundleName = assetImporter.assetBundleName;
                    CheckAssetBundleBuild(dependencyImporter, ref assetBundleBuilds);
                }
            }
        }

        /* 函数说明：PackMode */
        public static void PackMode(ref Dictionary<string, AssetBundleBuild> assetBundleBuilds)
        {
            List<string> modePathList = GetFilesFormFolder("Package/Player", "*.prefab",true);
            string outputLog = "";
            for(int i = 0; i < modePathList.Count; i++)
            {
                string curPath = modePathList [i];
                AssetImporter assetImporter = AssetImporter.GetAtPath(curPath);
                outputLog += "modePathList[" + i + "] " + curPath + "\n";
                string assetBundleName = assetImporter.assetPath.Substring(assetImporter.assetPath.LastIndexOf("/Player/") + 1,assetImporter.assetPath.LastIndexOf(".") - assetImporter.assetPath.LastIndexOf("/Player/") - 1);
                assetImporter.assetBundleName = assetBundleName;
                CheckAssetBundleBuild(assetImporter, ref assetBundleBuilds);
                string[] dependencies = AssetDatabase.GetDependencies(assetImporter.assetPath);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    outputLog += "dependencies[" + j + "] " + dependencies[j] + "\n";
                }
            }

            modePathList = GetFilesFormFolder("Package/Monsters", "*.prefab", true);
            for (int i = 0; i < modePathList.Count; i++)
            {
                string curPath = modePathList[i];
                AssetImporter assetImporter = AssetImporter.GetAtPath(curPath);
                outputLog += "modePathList[" + i + "] " + curPath + "\n";
                string assetBundleName = assetImporter.assetPath.Substring(assetImporter.assetPath.LastIndexOf("/Monsters/") + 1, assetImporter.assetPath.LastIndexOf(".") - assetImporter.assetPath.LastIndexOf("/Monsters/") - 1);
                assetImporter.assetBundleName = assetBundleName;
                outputLog += "modePathList[" + i + "] " + assetBundleName + "\n";
                CheckAssetBundleBuild(assetImporter, ref assetBundleBuilds);
            }
            File.WriteAllText(Application.dataPath + "/Package/outputLog.txt",outputLog);
        }

        /* 函数说明：PackUIPrefab用于创建需要打包预设列表 */
        public static void PackUIPrefab(ref Dictionary<string, AssetBundleBuild> assetBundleBuilds)
        {
            List<string> prefabPathList = GetFilesFormFolder("Package/UI/UIPrefab", "*.prefab", true);
            for (int i = 0; i < prefabPathList.Count; i++)
            {
                string curPath = prefabPathList[i];
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(curPath);
                if (go == null)
                    continue;
                AssetImporter assetImporter = AssetImporter.GetAtPath(curPath);
                if (assetImporter.assetPath.IndexOf("Assets/Package/UI/UIPrefab/") < 0)
                    continue;
                string assetBundleName = assetImporter.assetPath.Substring(0,assetImporter.assetPath.LastIndexOf("/"));
                //assetBundleName = assetBundleName.Substring(assetBundleName.LastIndexOf("/") + 1, assetBundleName.Length - assetBundleName.LastIndexOf("/") - 1);
                assetImporter.assetBundleName = assetBundleName;
                CheckAssetBundleBuild(assetImporter, ref assetBundleBuilds);
                string[] dependencies = AssetDatabase.GetDependencies(assetImporter.assetPath);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    if (dependencies[j].EndsWith(".png") == false && dependencies[j].EndsWith(".jpg") == false)
                        continue;
                    if (dependencies[j].ToLower().IndexOf("atlas") >= 0)
                        continue;
                    AssetImporter dependencyImporter = AssetImporter.GetAtPath(dependencies[j]);
                    dependencyImporter.assetBundleName = dependencies[j].Substring(0, dependencies[j].LastIndexOf("."));
                    CheckAssetBundleBuild(dependencyImporter, ref assetBundleBuilds);
                }
            }
        }
        // ----------------------------------------------------------
        // -------------------- 以下是内部函数 ----------------------
        // ----------------------------------------------------------

        // 是否是指定类型的文件
        private static bool IsFileOfType(string fileName, string type)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(type))
                return false;

            if (fileName.EndsWith(".meta"))
                return false;

            string[] typeArray = type.Split('|');
            for (int i = 0; i < typeArray.Length; i++)
            {
                if (typeArray[i] == "*.*")
                    return true;

                typeArray[i] = typeArray[i].Substring(1, typeArray[i].Length - 1);
            }

            for (int i = 0; i < typeArray.Length; i++)
            {
                if (fileName.EndsWith(typeArray[i]))
                    return true;
            }

            return false;
        }

        // 转换成bundle路径
        public static string GetBundlePath(string fileName)
        {
            return Application.dataPath + "/StreamingAssets/" + fileName;
        }

        // 获取目标平台
        public static BuildTarget GetBuildTarget()
        {
            BuildTarget target = BuildTarget.StandaloneWindows;
#if UNITY_ANDROID
            target = BuildTarget.Android;
#endif
#if UNITY_IPHONE
            target = BuildTarget.iPhone;
#endif
#if UNITY_WEBPLAYER
            target = BuildTarget.WebPlayer;
#endif
#if UNITY_STANDALONE_WIN
            target = BuildTarget.StandaloneWindows;
#endif
#if UNITY_FLASH
            target = BuildTarget.FlashPlayer;
#endif
            return target;
        }

        // 将某些对象打包成一个Bundle
        private static void BuildAssetBundle(UnityEngine.Object[] objects, string bundleName)
        {
            if (objects == null || objects.Length == 0)
                return;

            if (!bundleName.EndsWith(".unity3d"))
            {
                bundleName += ".unity3d";
            }
            string filePath = GetBundlePath(bundleName);
            BuildTarget target = GetBuildTarget();
            BuildPipeline.BuildAssetBundle(objects[0], objects, filePath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle, target);
            EncodeFile(filePath);

            Debug.Log(string.Format("CreateAssetBundle Succeed: {0}", filePath));
        }

        // 将某些场景打包成一个Bundle
        private static void BuildSceneBundle(string sceneFile, string bundleName)
        {
            if (!bundleName.EndsWith(".unity3d"))
            {
                bundleName += ".unity3d";
            }
            if(Directory.Exists(GetBundlePath("Scenes/")) == false)
            {
                Directory.CreateDirectory(GetBundlePath("Scenes/"));
            }
            BuildPipeline.BuildPlayer(new string[] { sceneFile },GetBundlePath("Scenes/" + bundleName),GetBuildTarget(),BuildOptions.BuildAdditionalStreamedScenes);
            //BuildPipeline.BuildStreamedSceneAssetBundle(new string[] { sceneFile }, GetBundlePath("Scenes/" + bundleName), GetBuildTarget(),BuildOptions.UncompressedAssetBundle,BuildOptions.UncompressedAssetBundle);
        }

        private static bool EncodeFile(string file)
        {
            FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite);
            if (fs == null || fs.Length == 0)
            {
                Debug.LogError("EncodeFile Error : can not open '" + file + "' !!!");
                return false;
            }

            byte[] fileBuff = new byte[fs.Length];
            fs.Read(fileBuff, 0, (int)fs.Length);

            if (GameEncoder.EncodeBytes(ref fileBuff, 0, fs.Length, "WLGame", 2014) == true)
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(fileBuff, 0, fileBuff.Length);
            }
            fs.Close();

            return true;
        }
    }
}