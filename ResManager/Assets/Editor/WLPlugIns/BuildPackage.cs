using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Data;
//using Mono.Data.Sqlite;
using Mono.Data.SqliteClient;

namespace WLGame
{
class PackageData
{
    public int _id = 0;
    public int type = 1;
    public string tag = "";
    public string desc = "";
    public string appname = "";
    public string identifier = "";
}

public class BuildPackage : EditorWindow{

    public GameObject ObjectToCopy = null;
    public System.IO.DirectoryInfo dir = null;

    public List<string> _allPackageTags = new List<string>();
    
    public List<int> _allPackageDataIds = new List<int>();
    
    public List<string> _allPackageDataDesces = new List<string>();

    public List<string> _allPackageScenes = new List<string>();

    public List<string> _allPackageSceneDesces = new List<string>();

    public List<string> _allPackageDataAppNames = new List<string>();

    public List<string> _allPackageDataIdentifiers = new List<string>();

    public List<string> _allPackageDataPushKeys = new List<string>();

    public int _selectValue = 0;

    public string _iconPath = "";

    public string _oldVersion = "";

    public string _oldLuaVersion = "";

    public string _oldConfigVersion = "";

    public string _oldSvnVersion = "";

    public string _newSvnVersion = "";

    public static BuildPackage _instance = null;

    public Texture _texture = null;

    public bool _isRelease = true;

    public bool _isSign = true;

    public bool _isUpdateRes = true;

    public bool _isExported = false;

    public bool _isPortrait = true;

    public int _selectScene = 0;
    
    public static ResManager.Version PackageVersion = new ResManager.Version();
        
    private static ResManager.Version oldPackageVersion = new ResManager.Version();

    [MenuItem("Package/Show Build Package")]
    static void CreateWindow()
    {
        // Creates the window for display
        //打包编辑器
        //TestEditor.CreateWindow();
        BuildPackage editor = EditorWindow.GetWindowWithRect<BuildPackage>(new Rect(100,100,600,333),true,"打包编辑器");
        editor.InitVersion();
        editor.InitPackage();
        _instance = editor;
        _instance.Show();
    }

    public static void LoadVersion()
    {
        string versionStr = File.ReadAllText(Application.dataPath + "/Package/Version/" + ResManager.PACKAGE + ".json");
        PackageVersion = JsonMapper.ToObject<ResManager.Version>(versionStr);
        oldPackageVersion.Copy(PackageVersion);
    }

    public void InitVersion()
    {
        LoadVersion();
        int svnVersion = GetSvnVersionStatus("Assets/StreamingAssets/Resources/Resources");
        _oldSvnVersion = svnVersion.ToString();
        _newSvnVersion = (svnVersion + 1).ToString();
    }
    
    public void InitPackage()
    {
        
        string packagePath = Application.dataPath + "/Editor/WLPlugIns/package.xml";

        XmlDocument packageDoc = new XmlDocument();
        packageDoc.Load(packagePath);

        //add package data,

#if UNITY_ANDROID
        XmlNodeList androidPacakages = packageDoc.SelectNodes("/PackageList/PackageAndroidList/package");
#elif UNITY_IPHONE
        XmlNodeList androidPacakages = packageDoc.SelectNodes("/PackageList/PackageIOSList/package");
#else
        XmlNodeList androidPacakages = packageDoc.SelectNodes("/PackageList/PackageAndroidList/package");
#endif


        foreach (XmlNode node in androidPacakages)
        {
            string tag = node.Attributes["tag"].Value;
            string strId = node.Attributes["id"].Value;
            string desc = node.Attributes["desc"].Value;
            string appname = node.Attributes["app"].Value;
            string identifier = node.Attributes["identifier"].Value;
            string pushkey = node.Attributes["pushkey"].Value;
            int id = int.Parse(strId);
            _allPackageTags.Add(tag);
            _allPackageDataIds.Add(id);
            _allPackageDataDesces.Add(desc);
            _allPackageDataAppNames.Add(appname);
            _allPackageDataIdentifiers.Add(identifier);
            _allPackageDataPushKeys.Add(pushkey);
        }

        //init all scenes data
        List<string> fileList = PackUtils.GetFilesFormFolder("/Package/Scenes/", "*.unity", false);
        _allPackageScenes = fileList;
        foreach (string scene in _allPackageScenes)
        {
            _allPackageSceneDesces.Add(scene.Substring(scene.LastIndexOf("/") + 1,scene.IndexOf(".unity") - scene.LastIndexOf("/") -1));
        }
    }

    public void OnGUI()
    {
        //GUILayout.BeginArea(new Rect(0, 0, 800, 600));
        GUILayout.BeginVertical();
        //选择渠道
        //GUILayout.Label("选择渠道");
        GUILayout.BeginHorizontal();
        _selectValue = EditorGUILayout.IntPopup("选择渠道:", _selectValue, _allPackageTags.ToArray(),_allPackageDataIds.ToArray());

        //Debug.LogError(_selectValue);

        GUILayout.EndHorizontal();

        //icon
        GUILayout.BeginHorizontal();
        string selectChannel = _allPackageTags[_selectValue];
        string selectAppName = _allPackageDataAppNames[_selectValue];
        string selectIdentifier = _allPackageDataIdentifiers[_selectValue];
        string selectDesc = _allPackageDataDesces[_selectValue];
        string selectPushKey = _allPackageDataPushKeys[_selectValue];

        //部分渠道sdk已经集成dataeye，用YJSDK_NO_DATAEYE进行处理
        if(selectChannel.Equals("YJSDK_NO_DATAEYE"))
        {
            selectChannel = "YJSDK";
        }

#if UNITY_ANDROID
        string iconPath = Application.dataPath + "/../OtherSdk/" + selectChannel.ToLower() + "/Android/icon.png";
#elif UNITY_IPHONE
        string iconPath = Application.dataPath + "/../OtherSdk/" + selectChannel.ToLower() + "/IOS/ios/icon.png";
#else
        string iconPath = Application.dataPath + "/../OtherSdk/" + selectChannel.ToLower() + "/Android/icon.png";
#endif
        if (iconPath != _iconPath)
        {
            _texture = ShowIcon(iconPath);
            _iconPath = iconPath;
        }
        _texture = EditorGUILayout.ObjectField(selectChannel, _texture, typeof(Texture), false) as Texture;
        //EditorGUI.DrawPreviewTexture(EditorGUILayout.RectField(new Rect(0, 0, 64, 64)), texture);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //当前svn版本
        GUILayout.Label("svn版本：", GUILayout.Width(400));
        GUILayout.Label(_oldSvnVersion + " 升级：" + _newSvnVersion);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        //当前版本
        GUILayout.Label("base版本：", GUILayout.Width(400));
        PackageVersion.baseVersion = GUILayout.TextField(PackageVersion.baseVersion);
        GUILayout.EndHorizontal();
        //当前lua版本
        GUILayout.BeginHorizontal();
        GUILayout.Label("lua版本：", GUILayout.Width(400));
        PackageVersion.luaVersion = GUILayout.TextField(PackageVersion.luaVersion);
        GUILayout.EndHorizontal();
        //当前config版本
        GUILayout.BeginHorizontal();
        GUILayout.Label("config版本：", GUILayout.Width(400));
        PackageVersion.configVersion = GUILayout.TextField(PackageVersion.configVersion);

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        //程序名称
        GUILayout.Label("程序名称：" + selectAppName);
        //渠道报名
        GUILayout.Label("渠道包名：" + selectIdentifier);
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        _isRelease = GUILayout.Toggle(_isRelease, "正式版本");
        GUILayout.Space(10f);
        _isUpdateRes = GUILayout.Toggle(_isUpdateRes, "更新资源");
        GUILayout.Space(10f);
        _isSign = GUILayout.Toggle(_isSign, "进行签名");
        GUILayout.Space(10f);
        _isExported = GUILayout.Toggle(_isExported, "工程导出");
        GUILayout.Space(10f);
        _isPortrait = GUILayout.Toggle(_isPortrait, "竖屏");
        GUILayout.Space(10f);

        //Debug.LogError("_isRelease " + _isRelease);
        //Debug.LogError("_isSign " + _isSign);
        //Debug.LogError("_isUpdateRes " + _isUpdateRes);
        GUILayout.EndHorizontal();
            
        ChangeVersion(_isRelease,_isSign);
        //开始打包
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("构建选中渠道包", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("构建选中渠道包", "构建选中渠道包？", "确定", "取消"))
        {
            ChangeVersionCode();

			AndroidSign(_isSign,selectChannel);

            string scriptSymbols = ";" + selectDesc;

            if (_isRelease == false)
            {
                scriptSymbols += ";DEBUG_TEST";
            }

            if (_isUpdateRes == false)
            {
                scriptSymbols += ";DISABLE_UPDATE_RES";
            }

            StartBuild(selectChannel, scriptSymbols,selectAppName,selectIdentifier,_isPortrait,true,_isExported);
        }

        if (GUILayout.Button("构建所有渠道包", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("构建所有渠道包", "构建所有渠道包？", "确定", "取消"))
        {
            ChangeVersionCode();

			AndroidSign(_isSign,selectChannel);

            string scriptSymbols = ";" + selectDesc;

            if (_isRelease == false)
            {
                scriptSymbols += ";DEBUG_TEST";
            }

            if (_isUpdateRes == false)
            {
                scriptSymbols += ";DISABLE_UPDATE_RES";
            }

            for (int i = 0; i < _allPackageTags.Count;i++ )
            {
                StartBuild(_allPackageTags[i], scriptSymbols,selectAppName,_allPackageDataIdentifiers[i],_isPortrait);
            }
        }

        if (GUILayout.Button("切换选中渠道包", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("切换选中渠道包", "切换选中渠道包？", "确定", "取消"))
        {
            ChangeVersionCode();

			AndroidSign(_isSign,selectChannel);

            string scriptSymbols = ";" + selectDesc;

            if (_isRelease == false)
            {
                scriptSymbols += ";DEBUG_TEST";
            }

            if (_isUpdateRes == false)
            {
                scriptSymbols += ";DISABLE_UPDATE_RES";
            }

            StartBuild(selectChannel, scriptSymbols, selectAppName, selectIdentifier,_isPortrait,false);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("其他操作：");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("压缩纹理", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("压缩纹理", "压缩纹理？", "确定", "取消"))
        {
            Debug.LogError("压缩纹理");
            //WLGame.PackMenu.CompressTexture();
            //string inputPath = Application.dataPath + "/Editor/WLPlugIns/input.txt";//Application.dataPath + "/Editor/WLPlugIns/package.xml";
            //string outputPath = Application.dataPath + "/Editor/WLPlugIns/output.txt";//Application.dataPath + "/Editor/WLPlugIns/package.xml";
            //string inputStr = File.ReadAllText(inputPath);
            //byte[] inputBytes = System.Text.Encoding.Unicode.GetBytes("UTF-8是一种多字节编码的字符集，表示一个Unicode字符时，它可以是1个至多个字节，在表示上有规律");//File.ReadAllBytes(inputPath);
            //byte[] outputBytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode,System.Text.Encoding.GetEncoding(950),inputBytes);
            //string outputStr = System.Text.Encoding.GetEncoding(950).GetString(outputBytes);
            //File.WriteAllText(outputPath, outputStr,System.Text.Encoding.GetEncoding(950));
            //string str = TCSCConvert.TCSCConvert.S2T("UTF-8是一种多字节编码的字符集，表示一个Unicode字符时，它可以是1个至多个字节，在表示上有规律");
            //Debug.LogError(str);
        }

        if (GUILayout.Button("打包配置文件", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包配置文件", "打包配置文件？", "确定", "取消"))
        {
            Debug.LogError("打包配置文件");
            WLGame.PackMenu.PackConfigFiles();
        }

        if (GUILayout.Button("打包LuaZip", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包LuaZip", "打包LuaZip？", "确定", "取消"))
        {
            Debug.LogError("打包LuaZip");
            WLGame.PackMenu.PackLuaZip();
        }

        if (GUILayout.Button("打包Lua资源", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包Lua资源", "打包Lua资源？", "确定", "取消"))
        {
            Debug.LogError("打包LuaActive资源");
            WLGame.PackMenu.PackLuaResource();
            ////UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Package/UI/Active");
            //List<string> fileList = WLGame.PackUtils.GetFilesFormFolder("Package/UI/Active", "*.*", true);

                //if (fileList.Count == 0 || fileList.Count > 1000)
                //{
                //    Debug.LogError("fileList unusual !!!");
                //    return;
                //}

                //if (fileList.Count == 1)
                //{
                //    string bundleName = WLGame.PackUtils.GetFileNameFormPath(fileList[0]);
                //    WLGame.PackUtils.PackAsset(fileList.ToArray(), bundleName);
                //}
                //else if (fileList.Count >= 1)
                //{
                //    string bundleName = WLGame.PackUtils.GetLastFolderFormPath(fileList[0]);
                //    WLGame.PackUtils.PackAsset(fileList.ToArray(), bundleName + "_files");
                //}


                //Debug.LogError("打包Luamine资源");
                ////UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath("Package/UI/Active");
                //fileList = WLGame.PackUtils.GetFilesFormFolder("Package/UI/mine", "*.*", true);

                //if (fileList.Count == 0 || fileList.Count > 1000)
                //{
                //    Debug.LogError("fileList unusual !!!");
                //    return;
                //}

                //if (fileList.Count == 1)
                //{
                //    string bundleName = WLGame.PackUtils.GetFileNameFormPath(fileList[0]);
                //    WLGame.PackUtils.PackAsset(fileList.ToArray(), bundleName);
                //}
                //else if (fileList.Count >= 1)
                //{
                //    string bundleName = WLGame.PackUtils.GetLastFolderFormPath(fileList[0]);
                //    WLGame.PackUtils.PackAsset(fileList.ToArray(), bundleName + "_files");
                //}
            }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _selectScene = EditorGUILayout.IntPopup("选择场景:", _selectScene, _allPackageSceneDesces.ToArray(), null);
        if (GUILayout.Button("打包场景", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包场景", "打包场景？", "确定", "取消"))
        {
            string selectScene = _allPackageScenes[_selectScene];
            string bundleName = PackUtils.GetFileNameFormPath(selectScene);
            PackUtils.PackScene(selectScene, bundleName);
            Debug.LogError("打包场景 " + selectScene);
        }
        //Debug.LogError(_allPackageScenes[_selectScene]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打包全部场景", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包全部场景", "打包全部场景？", "确定", "取消"))
        {
            Debug.LogError("打包全部场景");
            //WLGame.PackMenu.PackAllScenes();
            PackUtils.PackScene();
        }

        if (GUILayout.Button("打包版本", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("打包版本", "打包版本？", "确定", "取消"))
        {
            Debug.LogError("打包版本");
            //ChangePackage(selectChannel);
            WLGame.PackMenu.PackVesionBytes();
        }

        if (GUILayout.Button("配置推送", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("配置推送", "配置推送？", "确定", "取消"))
        {
            Debug.LogError("配置推送");
            AddConfForJPush.AddConfForJushByChannel(selectChannel,selectIdentifier,selectPushKey);
        }

        if (GUILayout.Button("修改预设", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("修改预设", "修改预设？", "确定", "取消"))
        {
            Debug.LogError("修改预设");
            string[] prefabStrings = AssetDatabase.FindAssets("t:prefab");
            //Debug.LogError("prefabObjs.Length " + prefabStrings.Length);
            /*
            for(int i = 0;i < prefabStrings.Length;i++)
            {
                string guid = prefabStrings[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                //Debug.Log(assetPath);
                GameObject prefabGO = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                UISprite[] sprites = prefabGO.GetComponentsInChildren<UISprite>(true);
                if (sprites.Length > 0)
                {
                    for (int j = 0; j < sprites.Length; j++)
                    {
                        UISprite sprite = sprites[j];
                        if (sprite == null)
                            continue;
                        Debug.LogError(assetPath + "----" + sprite.name + " " + sprite.type + " " + sprite.spriteName);
                        Debug.LogError(sprite);
                    }
                }
            }
             */
        }

        if (GUILayout.Button("生成脚本", GUILayout.Width(100)) && UnityEditor.EditorUtility.DisplayDialog("生成脚本", "生成脚本？", "确定", "取消"))
        {
            //CreateITextReader(Application.dataPath + "/Package/LocalConfig/resourceAsset.xml");
            //CreateITextReader(Application.dataPath + "/Package/LocalConfig/prefabAsset.xml");
            CreateITextReader(Application.dataPath + "/Package/LocalConfig/uIResInfoAsset.bytes");
        }

        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        //GUILayout.EndArea();
    }
    //创建打包资源文件，
    public Texture ShowIcon(string iconPath)
    {
        Debug.Log("ShowIcon");
        if (File.Exists(iconPath) == false)
        {
            Debug.LogError("iconPath doesnot exits here " + iconPath);
            return null;
        }
        _iconPath = iconPath;
        //创建文件读取流
        FileStream fileStream = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        //创建Texture
        int width = 512;
        int height = 512;
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(bytes);
        return texture;
    }

    //改版本，
    public void ChangeVersion(bool isRelease,bool isSign)
    {
        string resourceVersion = _newSvnVersion;
        WLGame.BuildPackage.PackageVersion.resourceVersion = resourceVersion;
        
        
        if(oldPackageVersion.Equals(PackageVersion) == false)
        {
            string versionPath = Application.dataPath + "/Package/Version/" + ResManager.PACKAGE + ".json";
            string versionStr = JsonMapper.ToJson(WLGame.BuildPackage.PackageVersion);
            File.WriteAllText(versionPath, versionStr);
            oldPackageVersion.Copy(PackageVersion);
            AssetDatabase.Refresh();
        }
        
    }

    //修改version code,
    public void ChangeVersionCode()
    {
        string version = PackageVersion.baseVersion;
        if (string.IsNullOrEmpty(version) == true)
        {
            Debug.LogError("Invaild argumen version " + version);
            return;
        }

        string[] splits = version.Split(new string[] {"."},StringSplitOptions.None);

        string strVersionCode = "";

        for (int i = 0; i < splits.Length;i++ )
        {
            if (splits[i].Length > 1)
            {
                strVersionCode += splits[i];
                continue;
            }
            splits[i] = "0" + splits[i];
            strVersionCode += splits[i];
        }

        int intVersionCode = int.Parse(strVersionCode);

        Debug.LogError("strVersionCode " + strVersionCode);
        Debug.LogError("intVersionCode " + intVersionCode);

#if UNITY_ANDROID
        PlayerSettings.Android.bundleVersionCode = intVersionCode;
#endif
        PlayerSettings.bundleVersion = version;
    }

    private static void AndroidSign(bool isSign,string channel)
    {
            return;
#if UNITY_ANDROID
		if (isSign )
        {
			if(channel != "MOYOSDK_DANGLE")
			{
                PlayerSettings.Android.keystoreName = Application.dataPath + "/../dtwzkey/xbdntg.keystore";
	            PlayerSettings.Android.keystorePass = "xbdntg";
                PlayerSettings.Android.keyaliasName = "xbdntg";
	            PlayerSettings.Android.keyaliasPass = "xbdntg";
			}
			else if(channel == "MOYOSDK_DANGLE")
			{
				PlayerSettings.Android.keystoreName = Application.dataPath + "/../OtherSdk/moyosdk_dangle/Android/downjoy_169_AjQGuRv5PeOq9na.keystore";
				PlayerSettings.Android.keystorePass = "downjoy_169";
				PlayerSettings.Android.keyaliasName = "169";
				PlayerSettings.Android.keyaliasPass = "downjoy_169";
			}
        }

        else
        {
            PlayerSettings.Android.keystoreName = Application.dataPath + "/../dtwzkey/xbdntg.keystore";
            PlayerSettings.Android.keyaliasName = "Unsigned(debug)";
        }
#endif
    }

    //复制制定文件，
    public static void StartBuild(string channel, string scriptSymbols,string appname,string identifier,bool _isPortrait = true, bool wantBuild = true,bool isExported = false)
    {
        Debug.LogError("channel " + channel);
        Debug.LogError("scriptSymbols " + scriptSymbols);
        //设置成为横屏方向，
        PlayerSettings.allowedAutorotateToLandscapeLeft = _isPortrait != true;
        PlayerSettings.allowedAutorotateToLandscapeRight = _isPortrait != true;
        PlayerSettings.allowedAutorotateToPortrait = _isPortrait == true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = _isPortrait == true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
		//AndroidSign(true,channel);
#if UNITY_ANDROID
        Build.BulidTarget(channel, "Android", scriptSymbols,appname,identifier,wantBuild,isExported);
#elif UNITY_IPHONE
			Build.BuildIOS(channel, channel,scriptSymbols,appname,identifier,wantBuild);
#else
#endif
    }

    public static void ChangePackage(string selectChannel)
    {
        string defaultXMLPath = Application.dataPath + "/../OtherSdk/default/sdk/AndroidManifest_template.xml";
        string destXMLPath = Application.dataPath + "/../OtherSdk/" + selectChannel.ToLower() + "/Android/AndroidManifest.xml";


        XmlDocument defaultXmlDoc = new XmlDocument();
        defaultXmlDoc.Load(defaultXMLPath);

        XmlDocument destXmlDoc = new XmlDocument();
        destXmlDoc.Load(destXMLPath);

        //修改uses-permission，
        XmlNode defaultManifestNode = defaultXmlDoc.SelectSingleNode("/manifest");
        XmlNode destManifestNode = destXmlDoc.SelectSingleNode("/manifest");

        XmlNode defaultApp = defaultXmlDoc.SelectSingleNode("/manifest/application");
        XmlNode destApp = destXmlDoc.SelectSingleNode("/manifest/application");
        Debug.LogError(defaultApp.Attributes.Count);

        XmlNodeList nodeList = defaultManifestNode.SelectNodes("uses-permission");

        foreach (XmlNode node in nodeList)
        {
            XmlNode curNode = destXmlDoc.ImportNode(node, true);
            destManifestNode.AppendChild(curNode);
        }
        //修改application，
        //修改app,
        XmlNodeList defaultAddNodeList = defaultApp.ChildNodes;
        foreach (XmlNode node in defaultAddNodeList)
        {
            XmlNode curNode = destXmlDoc.ImportNode(node, true);
            destApp.AppendChild(curNode);
        }

        XmlNodeList dataNodeList = destApp.SelectNodes("meta-data");
        foreach (XmlNode node in dataNodeList)
        {
            if(node.Attributes["android:name"].Value == "ClientChId")
            {
                node.Attributes["android:value"].Value = selectChannel;
            }
        }

        XmlNodeList actNodeList = destApp.SelectNodes("activity");
        foreach (XmlNode node in actNodeList)
        {
            if (node.Attributes["android:name"].Value == "com.hdls.dgame.moyo.UnityPlayerNativeActivity")
            {
                XmlNode intentNode = node.SelectSingleNode("intent-filter");
                node.RemoveChild(intentNode);
            }
        }
        destXmlDoc.Save(destXMLPath);
        
        Build.CopyDirectory(Application.dataPath + "/../OtherSdk/default/sdk/",Application.dataPath + "/../OtherSdk/" + selectChannel.ToLower() + "/Android/");
    }
    
    //根据资源列表生成lua配置文件，
    public static void CreateResourcesLuaConfig(Dictionary<string, AssetBundleBuild> assetBundleBuilds)
    {
        string resourceStr = "[\"{0}\"] = {{name = \"{1}\",path = \"{2}\",version = \"{3}\",hash = \"{4}\"}},\n";
        string prefabStr = "[\"{0}\"] = {{name = \"{1}\",bundleName = \"{2}\"}},\n";
        string allResourceStr = "RESOURCES = \n{\n" + "[\"Resources\"] = {name = \"Resources\",path = \"Resources/Resources\",version = \"\",hash = \"\"},\n";
        string allPrefabStr = "PREFABS = \n{\n" + "[\"AssetBundleManifest\"] = {{name = \"AssetBundleManifest\",bundleName = \"Resources\"}},\n";
        List<string> keys = new List<string>(assetBundleBuilds.Keys);
        for (int i = 0;i < keys.Count;i++)
        {
            AssetBundleBuild assetBundleBuild = assetBundleBuilds[keys[i]];
            string resourceVersion = _instance._newSvnVersion;
            Hash128 hashValue;
            BuildPipeline.GetHashForAssetBundle(Application.dataPath + "/StreamingAssets/Resources/" + assetBundleBuild.assetBundleName, out hashValue);
            string curResource = string.Format(resourceStr, assetBundleBuild.assetBundleName, assetBundleBuild.assetBundleName, "Resources/" + assetBundleBuild.assetBundleName,resourceVersion,hashValue);
            allResourceStr += curResource;
            for (int j = 0;j < assetBundleBuild.assetNames.Length;j++)
            {
                string assetName = assetBundleBuild.assetNames[j];
                string curPrefab = string.Format(prefabStr,assetName,assetName,assetBundleBuild.assetBundleName);
                allPrefabStr += curPrefab;
            }
        }
        allResourceStr += "}";
        allPrefabStr += "}";
        Debug.Log(allResourceStr);
        Debug.Log(allPrefabStr);
        File.WriteAllText(Application.dataPath + "/Package/Lua/Resources.lua",allResourceStr + "\n\n\n\n" + allPrefabStr);
    }

        //根据资源列表生成xml配置文件，
    public static void CreateResourcesXmlConfig(Dictionary<string, AssetBundleBuild> assetBundleBuilds)
    {
        string resourceStr = "\t<resourceAsset id = \"{0}\" name = \"{1}\" path = \"{2}\" version = \"{3}\" hash = \"{4}\"/>\n";
        string prefabStr = "\t<prefabAsset id = \"{0}\" name = \"{1}\" bundleName = \"{2}\"/>\n";
        string allResourceStr = "<resourceAssets>\n" + "\t<resourceAsset id = \"Resources\" name = \"Resources\" path = \"Resources/Resources\" version = \"" + _instance._newSvnVersion + "\" hash = \"\"/>\n"; ;
        string allPrefabStr = "<prefabAssets>\n" + "\t<prefabAsset id = \"AssetBundleManifest\" name = \"AssetBundleManifest\" bundleName = \"Resources\"/>\n"; ;
        List<string> keys = new List<string>(assetBundleBuilds.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            AssetBundleBuild assetBundleBuild = assetBundleBuilds[keys[i]];
            string resourceVersion = _instance._newSvnVersion;
            Hash128 hashValue;
            BuildPipeline.GetHashForAssetBundle(Application.dataPath + "/StreamingAssets/Resources/" + assetBundleBuild.assetBundleName, out hashValue);
            ResourceAsset oldResourceAsset = ResourceAssetReader.Instance.FindById(assetBundleBuild.assetBundleName);
            if(oldResourceAsset != null && oldResourceAsset.hash.Equals(hashValue.ToString()))
            {
                resourceVersion = oldResourceAsset.version.ToString();
            }
            string curResource = string.Format(resourceStr, assetBundleBuild.assetBundleName, assetBundleBuild.assetBundleName, "Resources/" + assetBundleBuild.assetBundleName,resourceVersion,hashValue);
            allResourceStr += curResource;
            for (int j = 0; j < assetBundleBuild.assetNames.Length; j++)
            {
                string assetName = assetBundleBuild.assetNames[j];
                string curPrefab = string.Format(prefabStr, assetName, assetName, assetBundleBuild.assetBundleName);
                allPrefabStr += curPrefab;
            }
        }
        allResourceStr += "</resourceAssets>";
        allPrefabStr += "</prefabAssets>";
        Debug.Log(allResourceStr);
        Debug.Log(allPrefabStr);
        File.WriteAllText(Application.dataPath + "/Package/LocalConfig/resourceAsset.bytes", allResourceStr);
        File.WriteAllText(Application.dataPath + "/Package/LocalConfig/prefabAsset.bytes", allPrefabStr);
        AssetDatabase.Refresh();
    }

        //根据资源列表生成xml配置文件，
        public static void CreateUIResourceInfoXmlConfig(Dictionary<string, AssetBundleBuild> assetBundleBuilds)
        {
            /*
            string[] strs = Enum.GetNames(typeof(UI3WndPanelLayerType));
            string resourceStr = "\t<uIResInfoAsset id = \"{0}\" name = \"{1}\" path = \"{2}\" wndType = \"{3}\" layerType = \"{4}\"/>\n";
            string allResourceStr = "<uIResInfoAssets>\n";
            List<string> resPaths = PackUtils.GetFilesFormFolder("Package/UI/UIPrefab", "*.prefab", true);
            for (int i = 0; i < resPaths.Count; i++)
            {
                string resPath = resPaths[i];
                resPath = resPath.Substring(resPaths.IndexOf("/Assets") + 1);
                GameObject go = AssetDatabase.LoadAssetAtPath(resPaths[i],typeof(GameObject)) as GameObject;
                UI3Wnd wnd = go.GetComponent<UI3Wnd>();
                if(wnd == null)
                    continue;
                bool isValided = false;
                for (int j = 0;j < strs.Length;j++)
                {
                    if(wnd.panelLayerType == strs[j])
                    {
                        isValided = true;
                    }
                }
                if(isValided == false)
                {
                    wnd.panelLayerType = UI3WndPanelLayerType.DefaultLayer.ToString();
                    EditorUtility.SetDirty(go);
                }

                string curResource = string.Format(resourceStr,wnd.GetClassName(),wnd.GetClassName(),resPaths[i],(int)wnd.WndType,wnd.panelLayerType.ToString());
                allResourceStr += curResource;
            }
            allResourceStr += "</uIResInfoAssets>";
            Debug.Log(allResourceStr);
            File.WriteAllText(Application.dataPath + "/Package/LocalConfig/uIResInfoAsset.bytes", allResourceStr);
            AssetDatabase.Refresh();
             */
        }

        public void CreateITextReader(string xmlPath)
    {
        Debug.LogError(xmlPath);

        string class_define = Path.GetFileName(xmlPath);
        class_define = class_define.Substring(0,class_define.LastIndexOf("."));
        class_define = class_define.Substring(0,1).ToUpper() + class_define.Substring(1);
        string class_reader_define = class_define + "Reader";
        string class_instance_define = class_define.ToLower() + "Instance";
        string class_asset_bundle_reader_define = class_define + "AssetBundleReader";

        string int_field_define = "\tpublic int {0} = 0;\n";
        string float_field_define = "\tpublic float {0} = 0.0f;\n";
        string string_field_define = "\tpublic string {0} = \"\";\n";
        string field_define = "";

        string int_method_define = "\t\t{0}.{1} = TextUtils.XmlReadInt(xmlNode, \"{2}\", 0);\n";
        string float_method_define = "\t\t{0}.{1} = TextUtils.XmlReadFloat(xmlNode, \"{2}\", 0.0f);\n";
        string string_method_define = "\t\t{0}.{1} = TextUtils.XmlReadString(xmlNode, \"{2}\", \"\");\n";
        string method_define = "\t\t{0} {1} = new {0}();\n\n";
        string method_return_define = "\t\treturn {0};\n";
        
        string file_name = xmlPath.Substring(xmlPath.IndexOf("Assets/") + "Assets/".Length);
        string asset_bundle_name = xmlPath.Substring(xmlPath.IndexOf("LocalConfig/") + "LocalConfig/".Length);

        string class_define_token = "<CLASS_DEFINE>";
        string class_reader_define_token = "<CLASS_READER_DEFINE>";
        string class_instance_define_token = "<CLASS_INSTANCE_DEFINE>";
        string class_asset_bundle_reader_define_token = "<CLASS_ASSET_BUNDLE_READER_DEFINE>";
        string field_define_token = "<FIELD_DEFINE>";
        string method_define_token = "<METHOD_DEFINE>";
        string file_name_token = "<FILE_NAME>";
        string asset_bundle_name_token = "<ASSET_BUNDLE_NAME>";

        Debug.LogError("class_define " + class_define);

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(xmlPath);
        XmlNodeList nodeList = xmlDocument.ChildNodes[0].ChildNodes;

        if(nodeList.Count > 0)
        {
            method_define = string.Format(method_define,class_define,class_instance_define);
            XmlNode node = nodeList[0];
            XmlAttributeCollection attrs = node.Attributes;
            for(int j = 0;j < attrs.Count;j++)
            {
                XmlAttribute attr = attrs[j];
                string key = attr.Name;
                string field = key.Substring(0,1).ToLower() + key.Substring(1);
                string value = attr.Value;
                int intValue = 0;
                float floatValue = 0.0f;
                int valueType = 0;
                try
                {
                    if(int.TryParse(value,out intValue))
                    {
                        valueType = 1;
                    }
                    else if(float.TryParse(value,out floatValue))
                    {
                        valueType = 2;
                    }else
                    {
                        valueType = 3;
                    }
                }catch(Exception e)
                {
                    valueType = 3;
                    Debug.LogError(e.Data + "\n" + e.Message);
                }

                switch(valueType)
                {
                    case 1:
                        field_define += string.Format(int_field_define,field);
                        method_define += string.Format(int_method_define,class_instance_define,field,key);
                        break;
                    case 2:
                        field_define += string.Format(float_field_define,field);
                        method_define += string.Format(float_method_define,class_instance_define,field,key);
                        break;
                    case 3:
                        field_define += string.Format(string_field_define,field);
                        method_define += string.Format(string_method_define,class_instance_define,field,key);
                        break;
                    default:
                        field_define += string.Format(string_field_define,field);
                        method_define += string.Format(string_method_define, class_instance_define, field, key);
                        break;
                }
            }

            method_define += string.Format(method_return_define,class_instance_define);

            string code = File.ReadAllText(Application.dataPath + "/Editor/WLPlugIns/class.txt");
            code = code.Replace(class_define_token,class_define);
            code = code.Replace(class_reader_define_token,class_reader_define);
            code = code.Replace(class_instance_define_token, class_instance_define);
            code = code.Replace(class_asset_bundle_reader_define_token, class_asset_bundle_reader_define);
            code = code.Replace(field_define_token,field_define);
            code = code.Replace(method_define_token,method_define);
            code = code.Replace(file_name_token,file_name);
            code = code.Replace(asset_bundle_name_token,asset_bundle_name);
            string outputPath = Application.dataPath + "/Scripts/Auto/";
            if(Directory.Exists(outputPath) == false)
            {
                Directory.CreateDirectory(outputPath);
            }
            File.WriteAllText(outputPath + class_define + ".cs", code);
            AssetDatabase.Refresh();
        }
    }

    public int GetSvnVersionStatus(string filePath)
    {
        Debug.LogError("【注意】这里跳过了SVN版本检查，实际开发需要打开");
        return 0;

        UnitySVN.SVNCommand(UnitySVN.UPDATE,Application.dataPath + "/StreamingAssets/Resources/");
            string connectionString = "URI=file:" + GetSvnDbPath();
            IDbConnection dbcon = new SqliteConnection(connectionString);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            int version = 0;
            // requires a table to be created named employee
            // with columns firstname and lastname
            // such as,
            //        CREATE TABLE employee (
            //           firstname nvarchar(32),
            //           lastname nvarchar(32));
            //const string sql = "SELECT revision FROM nodes where local_relpath = 'Assets/StreamingAssets/Resources'";
            string sql = "SELECT revision FROM nodes where local_relpath like \"%" + filePath +"\"";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                string revision = reader.GetString(0);
                Debug.Log("filePath:" + filePath);
                Debug.Log("revision:" + revision);
                int.TryParse(revision,out version);
            }
            // clean up
            reader.Dispose();
            dbcmd.Dispose();
            dbcon.Close();
            return version;
    }

    public string GetSvnDbPath()
    {
        string path = Application.dataPath;
        DirectoryInfo curDirectoryInfo = new DirectoryInfo(path);
        string filePath = "";
        try
        {
            filePath = curDirectoryInfo.FullName + "/.svn/wc.db";
            while (File.Exists(filePath) == false || curDirectoryInfo.Equals(curDirectoryInfo.Root))
            {
                curDirectoryInfo = curDirectoryInfo.Parent;
                filePath = curDirectoryInfo.FullName + "/.svn/wc.db";
                Debug.Log(filePath);
            }

            if (File.Exists(filePath) == false)
            {
                path = "";
            }
            else
            {
                path = filePath;
            }
            
        }catch(Exception e)
        {
            Debug.LogError(filePath);
            Debug.LogError(e.Message);
        }
        Debug.Log(filePath);
        return path;
    }

    public void SvnUp(string path)
    {
        UnitySVN.SVNCommand(UnitySVN.UPDATE,Application.dataPath + "/" + path);
            //using (SvnClient client = new SvnClient())
            //{
            //    // Checkout the code to the specified directory
            //    client.CheckOut(new Uri("http://sharpsvn.googlecode.com/svn/trunk/"),
            //                            "c:\\sharpsvn");

            //    // Update the specified working copy path to the head revision
            //    client.Update("c:\\sharpsvn");
            //    SvnUpdateResult result;
            //    client.Update("c:\\sharpsvn", out result);


            //    client.Move("c:\\sharpsvn\\from.txt", "c:\\sharpsvn\\new.txt");

            //    // Commit the changes with the specified logmessage
            //    SvnCommitArgs ca = new SvnCommitArgs();
            //    ca.LogMessage = "Moved from.txt to new.txt";
            //    client.Commit("c:\\sharpsvn", ca);
            //}
        }
}
}