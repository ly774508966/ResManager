using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace WLGame
{
    public class PackMenu : AssetPostprocessor
    {
        [MenuItem("Assets/资源打包工具/【A1】压缩纹理")]
        public static void CompressTexture()
        {
            string[] compressFolderArray = 
            {
                "/Resources/Textrue/",
                "/Package/UI/",
            };

            for (int i = 0; i < compressFolderArray.Length; i++)
            {
                List<string> fileList = PackUtils.GetFilesFormFolder(compressFolderArray[i], "*.tga|*.psd|*.bmp|*.png|*.jpg", true);
#if UNITY_ANDROID
                PackUtils.CompressTexture(fileList.ToArray(), TextureImporterFormat.DXT5);
#elif UNITY_IPHONE
                PackUtils.CompressTexture(fileList.ToArray(), TextureImporterFormat.PVRTC_RGBA4);
#endif
            }
        }

        [MenuItem("Assets/资源打包工具/【A2】打包配置文件")]
        public static void PackConfigFiles()
        {
            string inputFolder = Application.dataPath + "/Package/LocalConfig/";
            string zipPath = Application.dataPath + "/Package/" + ResManager.CONFIG_PATH + ".bytes";
            ZipHelper.ZipDir(inputFolder, zipPath, 0);
            AssetDatabase.Refresh();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/Package/" + ResManager.CONFIG_PATH + ".bytes", typeof(TextAsset));
            BuildPipeline.BuildAssetBundle(asset, null, Application.dataPath + "/StreamingAssets/" + ResManager.CONFIG_PATH + ".unity3d", BuildAssetBundleOptions.ChunkBasedCompression, PackUtils.GetBuildTarget());
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/资源打包工具/【A3】打包全部场景")]
        public static void PackAllScenes()
        {
            List<string> fileList = PackUtils.GetFilesFormFolder("/Package/Scenes/", "*.unity", false);
            for (int i = 0; i < fileList.Count; i++)
            {
                string bundleName = PackUtils.GetFileNameFormPath(fileList[i]);
                //PackUtils.PackScene(fileList[i], "Scenes/" + bundleName);
            }
        }

        [MenuItem("Assets/资源打包工具/【A4】生成本地Assets MD5")]
        public static void GenAssetsMD5()
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlDeclaration xmldecl = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            XmlElement xmlRootElem = xmldoc.CreateElement("", "AssetsMD5", "");
            xmldoc.AppendChild(xmlRootElem);

            List<string> fileList = PackUtils.GetFilesFormFolder("/StreamingAssets/", "*.*", true);
            for (int i = 0; i < fileList.Count; i++)
            {
                string filePath = fileList[i].Replace("Assets/StreamingAssets/", "");
                string fileMD5 = ResUtils.GetFileMD5(Application.dataPath + "/StreamingAssets/" + filePath);

                XmlNode root = xmldoc.SelectSingleNode("AssetsMD5");
                XmlElement xe1 = xmldoc.CreateElement("File");
                xe1.SetAttribute("path", filePath);
                xe1.SetAttribute("md5", fileMD5);
                root.AppendChild(xe1);
            }

            //保存创建好的XML文档
            string xmlFilePath = Application.dataPath + "/Resources/AssetsMD5.xml";
            xmldoc.Save(xmlFilePath);
        }

        [MenuItem("Assets/资源打包工具/【A5】打包LuaZip")]
        public static void PackLuaZip()
        {
            string luaFolder = Application.dataPath + "/Package/Lua/";
            string zipPath = Application.dataPath + "/Package/" + ResManager.LUA_PATH + ".bytes";
            ZipHelper.ZipDir(luaFolder, zipPath, 0);
            AssetDatabase.Refresh();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/Package/" + ResManager.LUA_PATH + ".bytes", typeof(TextAsset));
            BuildPipeline.BuildAssetBundle(asset, null, Application.dataPath + "/StreamingAssets/" + ResManager.LUA_PATH + ".unity3d", BuildAssetBundleOptions.ChunkBasedCompression,PackUtils.GetBuildTarget());
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/资源打包工具/【S1】打包选中场景")]
        public static void PackSelectScene()
        {
            UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if (selection.Length <= 0)
                return;

            List<string> pathList = PackUtils.GetFilesFormObjects(selection, ".unity");
            for (int i = 0; i < pathList.Count; i++)
            {
                string bundleName = PackUtils.GetFileNameFormPath(pathList[i]);
                PackUtils.PackScene(pathList[i], bundleName);
            }
        }

        [MenuItem("Assets/资源打包工具/【S2】打包选中资源")]
        public static void PackSelectAsset()
        {
            UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if (selection.Length == 0 || selection.Length > 1000)
            {
                Debug.LogError("selection unusual !!!");
                return;
            }

            List<string> pathList = PackUtils.GetFilesFormObjects(selection, "*.*");
            if (pathList.Count == 1)
            {
                string bundleName = PackUtils.GetFileNameFormPath(pathList[0]);
                PackUtils.PackAsset(pathList.ToArray(), bundleName);
            }
            else if (pathList.Count >= 1)
            {
                string bundleName = PackUtils.GetLastFolderFormPath(pathList[0]);
                PackUtils.PackAsset(pathList.ToArray(), bundleName + "_files");
            }
        }

        [MenuItem("Assets/资源打包工具/【A6】打包版本")]
        public static void PackVesionBytes()
        {
            string zipPath = Application.dataPath + "/Package/" + ResManager.VERSIONS_PATH + ".bytes";
            ZipHelper.ZipDir(Application.dataPath + "/Package/Version/", zipPath, 0);
            AssetDatabase.Refresh();
            UnityEngine.Object[] assets = new UnityEngine.Object[2];
            assets[0] = AssetDatabase.LoadAssetAtPath("Assets/Package/Version/" + ResManager.PACKAGE + ".json", typeof(TextAsset));
            assets[1] = AssetDatabase.LoadAssetAtPath("Assets/Package/" + ResManager.VERSIONS_PATH + ".bytes", typeof(TextAsset));
            BuildPipeline.BuildAssetBundle(assets[0], assets, Application.dataPath + "/StreamingAssets/" + ResManager.VERSIONS_PATH + ".unity3d", BuildAssetBundleOptions.ChunkBasedCompression, PackUtils.GetBuildTarget());
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/资源打包工具/【A7】打包资源")]
        public static void PackLuaResource()
        {
            Dictionary<string, AssetBundleBuild> assetBundleBuilds = new Dictionary<string, AssetBundleBuild>();
            //PackUtils.PackEffect(ref assetBundleBuilds);
            PackUtils.PackAtlas(ref assetBundleBuilds);
            PackUtils.PackUIPrefab(ref assetBundleBuilds);
            PackUtils.PackMode(ref assetBundleBuilds);
            if (assetBundleBuilds.Count <= 0)
            {
                return;
            }
            if (System.IO.Directory.Exists(PackUtils.GetBundlePath("Resources")) == false)
            {
                System.IO.Directory.CreateDirectory(PackUtils.GetBundlePath("Resources"));
            }
            BuildPipeline.BuildAssetBundles(PackUtils.GetBundlePath("/Resources"), new List<AssetBundleBuild>(assetBundleBuilds.Values).ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, PackUtils.GetBuildTarget());
            AssetDatabase.Refresh();
            WLGame.BuildPackage.CreateResourcesLuaConfig(assetBundleBuilds);
            WLGame.BuildPackage.CreateResourcesXmlConfig(assetBundleBuilds);
            WLGame.BuildPackage.CreateUIResourceInfoXmlConfig(assetBundleBuilds);
            string resourceFolder = PackUtils.GetBundlePath("Resources");
            string zipPath = Application.dataPath + "/Package/" + ResManager.RESOURCE_PATH + ".bytes";
            ZipHelper.ZipDir(resourceFolder, zipPath, 0);
            AssetDatabase.Refresh();
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/Package/" + ResManager.RESOURCE_PATH + ".bytes", typeof(TextAsset));
            BuildPipeline.BuildAssetBundle(asset, null, ResManager.RESOURCE_EDITOR_PATH, BuildAssetBundleOptions.ChunkBasedCompression, PackUtils.GetBuildTarget());
            AssetDatabase.Refresh();
        }

        void OnPostprocessAssetbundleNameChanged(string path,
                string previous, string next)
        {
            Debug.LogError("AB: " + path + " old: " + previous + " new: " + next);
        }
    }
}