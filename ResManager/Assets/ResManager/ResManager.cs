using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class ResManager : MonoBehaviour
{
    // 当前场景的名称。
    // 之所以定义这个变量，是想通过该变量在游戏运行时能够在编辑器的Hierarchy视图中看到当前场景的名称。
    [SerializeField]
    string CurrentScene;

    public enum BundleStatus
    {
        loading = 0, finish = 1, destroy = 2,
    }

    public enum BundleType
    {
        resource = 0, config = 1, lua = 2, versions = 3,
    }

    public enum VersionType
    {
        package = 0, update = 1, unzip = 2,
    }

    // 配置当前资源列表，
    public class ResInfo
    {
        public string assetName;
        public string assetPath;
        public string bundleName;
        public string bundlePath;
        public string version;
        public List<Action<object>> loadedCallbackList = new List<Action<object>>();
    }
    //缓存已经加载的AssetBundle
    public class BundleInfo
    {
        public string bundleName;
        public string bundlePath;
        public AssetBundle assetBundle = null;
        public BundleStatus bundleStatus = BundleStatus.loading;
        public List<Action<object>> loadedCallbackList = new List<Action<object>>();
    }

    public class Version
    {
        //大版本号，
        public string baseVersion;
        //resource版本resourceVersion，这个是根据svn revision，比较两个资源版本，需要用baseVersion:resourceVersion尽心比较，
        public string resourceVersion;
        //配置版本configVersion，
        public string configVersion;
        //lua版本luaVersion，
        public string luaVersion;
        //
        public string ToString()
        {
            return baseVersion + "," + configVersion + "," + luaVersion + "," + resourceVersion;
        }
        //
        public bool Equals(Version version)
        {
            return ToString().Equals(version.ToString());
        }
        //
        public void Copy(Version version)
        {
            if(version == null)
            {
                return;
            }
            baseVersion = version.baseVersion;
            configVersion = version.configVersion;
            luaVersion = version.luaVersion;
            resourceVersion = version.resourceVersion;
        }
    }

    private static ResManager instance = null;
    private Dictionary<string, ResInfo> resInfoDict = new Dictionary<string, ResInfo>();
    private Dictionary<string, BundleInfo> bundleInfoDict = new Dictionary<string, BundleInfo>();

    //持久化存储路径，
    private static string persistentDataPath = "";
    //当前StreamingAssets路径，
    private static string streamingAssetsPath = "";
    //
    private static string editorPackagePath = Application.dataPath + "/Package/";
    //上一次更新的版本updateVersion，
    private static Version updateVersion;
    //打包时候包体版本packageVersion，
    private static Version packageVersion;
    //打包时候包体版本unzipVersion，
    private static Version unzipVersion;

    //是否在编辑器模式下开启更新模式，用于测试，
    private static bool enableUpdate = false;
    
    public const string LUA_PATH = "Luazip";

    public const string CONFIG_PATH = "Configzip";

    public const string VERSIONS_PATH = "Versions";

    public const string RESOURCE_PATH = "Resourcezip";

    public const string PACKAGE = "package";

    public const string UPDATE = "update";

    public const string UNZIP = "unzip";

    public const string LOCAL_LUA_PATH = "Lua/";

    public const string LOCAL_CONFIG_PATH = "LocalConfig/";

    public const string LOCAL_RESOURCE_PATH = "Resource";

    public static string RESOURCE_EDITOR_PATH = editorPackagePath + RESOURCE_PATH + ".unity3d";

    public string[] BUNDLE_PATHS = { RESOURCE_PATH, CONFIG_PATH, LUA_PATH };
    ResInfo manifestResInfo = null;
    AssetBundleManifest manifest = null;
    public static ResManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject res = GameObject.Find("ResManager");
                if (res != null)
                {
                    GameObject.Destroy(res);
                }
                instance = (new GameObject("ResManager")).AddComponent<ResManager>();
                instance.Init();
                if(Application.isPlaying)
                {
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    private void Init()
    {

    }

    public ResInfo FindResInfo(string resName)
    {
        if(resInfoDict == null || resInfoDict.Count <= 0 || resInfoDict.ContainsKey(resName) == false)
            return null;

        return resInfoDict[resName];
    }

    public ResInfo AddResInfo(ResInfo resInfo)
    {
        if (resInfoDict == null || resInfo == null)
            return null;

        return resInfoDict[resInfo.bundleName] = resInfo;
    }

    public void DeleteResInfo(string resName)
    {
        if (resInfoDict == null || resInfoDict.Count <= 0 || resInfoDict.ContainsKey(resName) == false)
            return;

        resInfoDict.Remove(resName);
    }
    
    public BundleInfo FindBundleInfo(string bundleName)
    {
        if (bundleInfoDict == null || bundleInfoDict.Count <= 0 || bundleInfoDict.ContainsKey(bundleName) == false)
            return null;

        return bundleInfoDict[bundleName];
    }

    public BundleInfo AddBundleInfo(BundleInfo bundleInfo)
    {
        if (bundleInfoDict == null || bundleInfo == null)
            return null;

        return bundleInfoDict[bundleInfo.bundleName] = bundleInfo;
    }

    public void DeleteBundleInfo(string bundleName)
    {
        BundleInfo bundleInfo = FindBundleInfo(bundleName);
        if(bundleInfo != null)
        {
            if(bundleInfo.assetBundle != null)
            {
                bundleInfo.assetBundle.Unload(true);
            }
            bundleInfoDict.Remove(bundleName);
        }
    }

    /*****************************************
    * 函数说明: 从指定的包里的获取一个资源
    * 返 回 值: T
    * 参数说明: assetName 资源名
    * 参数说明: T 资源T
    * 注意事项: 该函数采样同步方式LoadFromFile()获取已经加载好的资源，不涉及加载
    *****************************************/
    public T LoadResource<T>(string resName) where T : UnityEngine.Object
    {
        T retT = default(T);
        if (string.IsNullOrEmpty(resName))
        {
            return retT;
        }

        AssetBundle assetBundle = LoadBundle(resName);

        if (assetBundle == null)
        {
            Debug.LogError("assetBundle is null, resName:"+resName);
            return retT;
        }

        return retT = assetBundle.LoadAsset<T>(resName);
    }
    /*****************************************
    * 函数说明: 从指定的包里的获取一个资源
    * 返 回 值: T
    * 参数说明: assetName 资源名
    * 参数说明: T 资源T
    * 注意事项: 该函数采样同步方式LoadFromFile()获取已经加载好的资源，不涉及加载
    *****************************************/
    private AssetBundle LoadBundle(string resName)
    {
        ResInfo resInfo;
        if (resInfoDict.TryGetValue(resName, out resInfo) == false)
        {
            return null;
        }

        if (string.IsNullOrEmpty(resInfo.bundlePath) == true)
        {
            return null;
        }

        BundleInfo bundleInfo = FindBundleInfo(resInfo.bundleName);
        AssetBundle assetBundle = null;
        if (bundleInfo != null && bundleInfo.assetBundle != null)
        {
            assetBundle = bundleInfo.assetBundle;
        }
        else
        {
            if(manifest != null)
            {
                string[] dependencies = manifest.GetAllDependencies(resInfo.bundleName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string dependency = dependencies[i];
                    LoadBundle(dependency);
                }
            }
            string loadPath = GetLoadResPath(resInfo.bundlePath, resInfo.version);
            Debug.Log("LoadBundle bundlePath " + loadPath);
            assetBundle = AssetBundle.LoadFromFile(loadPath);
            if (bundleInfo == null)
            {
                bundleInfo = new BundleInfo();
            }
            bundleInfo.assetBundle = assetBundle;
            bundleInfo.bundleName = resInfo.bundleName;
            bundleInfo.bundlePath = resInfo.bundlePath;
            //bundleInfoDict[resInfo.bundlePath] = bundleInfo;
            AddBundleInfo(bundleInfo);
        }
        return assetBundle;
    }
    /*****************************************
    * 函数说明: 从指定的包里的获取一个config资源
    *****************************************/
    public byte[] LoadConfig(string configName)
    {
        string configPath = configName + ".bytes";
        if(Application.isMobilePlatform || EnableUpdate)
        {
            configPath = PersistentDataPath + configPath;
        }
        else
        {
            configPath = EditorPackagePath + configPath;
        }

        if (File.Exists(configPath) == false)
        {
            Debug.LogError("cannot load the config " + configPath);
            return null;
        }

        return File.ReadAllBytes(configPath);
    }

    /*****************************************
    * 函数说明: 从指定的包里的获取一个prefab资源
    *****************************************/
    public UnityEngine.GameObject LoadPrefab(string prefabName)
    {
        return LoadResource<UnityEngine.GameObject>(prefabName);
    }

    /*****************************************
    * 函数说明: 从指定的包里的获取一个Component资源
    *****************************************/
    public T LoadComponent<T>(string prefabName)
    {
        GameObject go = LoadPrefab(prefabName);
        if(go == null)
            return default(T);
        T t = go.GetComponent<T>();
        return t;
    }

    /*****************************************
    * 函数说明: 从指定的包里的获取一个Component资源
    *****************************************/
    /*
    public UIAtlas LoadAtlas(string prefabName)
    {
        return LoadComponent<UIAtlas>(prefabName);
    }
     */

    /*****************************************
    * 函数说明: 从指定的包里的获取一个Prefab资源，同时添加到go上，
    *****************************************/
    public GameObject AddPrefab(string prefabName,GameObject go)
    {
        if(go == null)
            return null;
        GameObject prefab = LoadPrefab(prefabName);
        if(prefab == null)
            return null;
        GameObject curGO = GameObject.Instantiate(prefab) as GameObject;
        if(curGO == null)
            return null;
        curGO.transform.parent = go.transform;
        curGO.transform.localPosition = Vector3.zero;
        curGO.transform.localRotation = Quaternion.identity;
        curGO.transform.localScale = Vector3.one;
        curGO.layer = go.layer;
        return curGO;
    }

    /*****************************************
    * 函数说明: 从指定的包里的获取一个scene资源
    *****************************************/
    public void LoadScene(string sceneName)
    {
        BundleInfo bundleInfo = FindBundleInfo(sceneName);
        AssetBundle assetBundle = null;
        if (bundleInfo != null && bundleInfo.assetBundle != null)
        {
            assetBundle = bundleInfo.assetBundle;
        }else
        {
            assetBundle = AssetBundle.LoadFromFile(StreamingAssetsPath + "Scenes/" + sceneName + ".unity3d");
            if (bundleInfo == null)
            {
                bundleInfo = new BundleInfo();
            }
            bundleInfo.assetBundle = assetBundle;
            bundleInfo.bundleName = sceneName;
            bundleInfo.bundlePath = sceneName;
            //bundleInfoDict[resInfo.bundlePath] = bundleInfo;
            AddBundleInfo(bundleInfo);
        }
        if(assetBundle != null)
        {
            SceneManager.LoadScene(sceneName);
            //assetBundle.Unload(true);
            //Application.LoadLevel(sceneName);
        }
    }

    public void UnloadScene(string sceneName)
    {
        DeleteBundleInfo(sceneName);
        SceneManager.UnloadScene(sceneName);
    }

    public GameObject LoadMode(string name)
    {
        name = "Assets/Package/" + name + ".prefab";
        return LoadPrefab(name);
    }
    /*****************************************
    * 函数说明: 异步加载多个资源包，全部加载完毕回调
    * 返 回 值: void
    * 参数说明: pkgNames 包名数组
    * 参数说明: isGlobal 是否标记为全局
    * 参数说明: callback 回调函数
    * 注意事项: 该函数采样异步加载，不会立即返回
    *****************************************/
    public string GetLoadResPath(string bundlePath,string version)
    {
        string retPath = "";

        if(Application.isMobilePlatform)
        {
            if(version.CompareTo(PackageVersion.resourceVersion) > 0)
            {
                retPath += PersistentDataPath + bundlePath;
            }else
            {
                retPath += StreamingAssetsPath + bundlePath;
            }
        }else
        {
            if(EnableUpdate)
            {
                retPath += PersistentDataPath + bundlePath;
            }else
            {
                retPath += StreamingAssetsPath + bundlePath;
            }
        }
        return retPath;
    }

    public static string PersistentDataPath
    {
        get
        {
            if(string.IsNullOrEmpty(persistentDataPath))
            {
                persistentDataPath = Application.persistentDataPath + "/";
            }
            return persistentDataPath;
        }
    }

    public static string StreamingAssetsPath
    {
        get
        {
            if(string.IsNullOrEmpty(streamingAssetsPath))
            {
                if(Application.isMobilePlatform)
                {
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        streamingAssetsPath = Application.dataPath + "!assets/";
                    }else
                    {
                        streamingAssetsPath = Application.streamingAssetsPath;
                    }
                }else
                {
                    streamingAssetsPath = Application.streamingAssetsPath + "/";
                }
            }
            return streamingAssetsPath;
        }
    }

    public static string EditorPackagePath
    {
        get
        {
            return editorPackagePath;
        }
    }

    public Version UpdateVersion
    {
        get
        {
            if(updateVersion == null)
            {
                InitialVersions();
            }
            return updateVersion;
        }

        private set
        {
            updateVersion = value;
        }
    }

    public Version PackageVersion
    {
        get
        {
            if(packageVersion == null)
            {
                InitialVersions();
            }
            return packageVersion;
        }

        private set
        {
            packageVersion = value;
        }
    }


    public Version UnzipVersion
    {
        get
        {
            if(unzipVersion == null)
            {
                InitialVersions();
            }
            return unzipVersion;
        }

        private set
        {
            unzipVersion = value;
        }
    }

    public static bool EnableUpdate
    {
        get
        {
            return enableUpdate;
        }
        set
        {
            enableUpdate = value;
        }
    }

    /*****************************************
    * 函数说明: 销毁一个资源包
    * 返 回 值: void
    * 参数说明: pkgName 包名
    * 注意事项: 
    *****************************************/
    public void DestroyPackage(string pkgName,bool unloadAllObjects = false)
    {
        ResInfo resPkg;
        //if (resInfoDict.TryGetValue(pkgName, out resPkg))
        //{
        //    if (resPkg.bundleStatus == BundleStatus.finish)
        //    {
        //        DestroyPackage(resPkg,unloadAllObjects);
        //    } else {
        //        resPkg.bundleStatus = BundleStatus.destroy;
        //    }
        //}
    }


    /*****************************************
    * 函数说明: 销毁一个资源包
    * 返 回 值: void
    * 参数说明: pkgName 包名
    * 注意事项: 
    *****************************************/
    public void DestroyPackageForLoadAllObjects(string pkgName, bool unloadAllObjects)
    {
        ResInfo resPkg;
        //if (resInfoDict.TryGetValue(pkgName, out resPkg))
        //{
        //    if (resPkg.bundleStatus == BundleStatus.finish)
        //    {
        //        DestroyPackage(resPkg, unloadAllObjects);
        //    }
        //    else
        //    {
        //        resPkg.bundleStatus = BundleStatus.destroy;
        //    }
        //}
    }

    /*****************************************
    * 函数说明: 函数说明：销毁所有资源
    * 返 回 值: void
    * 注意事项: 
    *****************************************/
    public void DestroyAll(bool bWithGlobal = false)
    {
        List<string> keys = new List<string>(resInfoDict.Keys);
        //for (int i = 0; i < keys.Count; i++)
        //{
        //    if (!bWithGlobal)
        //    {
        //        if (keys[i] == ResUtils.CONFIG_PKG_NAME)
        //            continue;
        //    }

        //    ResInfo resPkg;
        //    if (!resInfoDict.TryGetValue(keys[i], out resPkg))
        //        continue;

        //    if (resPkg.bundleStatus == BundleStatus.finish)
        //    {
        //        DestroyPackage(resPkg, true);
        //    }
        //    else
        //    {
        //        resPkg.bundleStatus = BundleStatus.destroy;
        //    }
        //}

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    /********************************************************/
    /******************** 以下是内部函数 *********************/
    /********************************************************/

    void OnDestroy()
    {
        DestroyAll(true);

        instance = null;
    }

    private void DestroyPackage(ResInfo resPkg, bool unloadAllObjects = false)
    {
        //if (resPkg != null)
        //{
        //    if (resPkg.assetBundle != null)
        //    {
        //        resPkg.assetBundle.Unload(unloadAllObjects);
        //        resPkg.assetBundle = null;
        //    }
        //    resInfoDict.Remove(resPkg.pkgName);
        //}
    }

    //private void DecodeMemory(ref byte[] bytes)
    //{
    //    //GameEncoder.DecodeBytes(ref bytes, 0, bytes.Length, "WLGame", 2014);
    //}

    void OnLevelWasLoaded(int level)
    {
        CurrentScene = Application.loadedLevelName;
    }

    public bool GetLoadZipPath(BundleType bundleType,out string bundlePath,out string outputPath)
    {
        bool isNeed = false;
        bundlePath = "";
        outputPath = "";
        string lastUnzipVersion = "";
        string lastPackageVersion = "";
        string lastUpdateVersion = "";

        switch (bundleType)
        {
            case BundleType.lua:
                bundlePath = LUA_PATH + ".unity3d";
                outputPath = LOCAL_LUA_PATH;
                lastUpdateVersion = UpdateVersion.luaVersion;
                lastPackageVersion = PackageVersion.luaVersion;
                lastUnzipVersion = UnzipVersion.luaVersion;
                break;
            case BundleType.config:
                bundlePath = CONFIG_PATH + ".unity3d";
                outputPath = LOCAL_CONFIG_PATH;
                lastUpdateVersion = UpdateVersion.configVersion;
                lastPackageVersion = PackageVersion.configVersion;
                lastUnzipVersion = UnzipVersion.configVersion;
                break;
            case BundleType.resource:
                bundlePath = RESOURCE_PATH + ".unity3d";
                outputPath = LOCAL_RESOURCE_PATH;
                lastUpdateVersion = UpdateVersion.resourceVersion;
                lastPackageVersion = PackageVersion.resourceVersion;
                lastUnzipVersion = UnzipVersion.resourceVersion;
                break;
            default:
                bundlePath = LUA_PATH + ".unity3d";
                outputPath = LOCAL_LUA_PATH;
                lastUpdateVersion = UpdateVersion.luaVersion;
                lastPackageVersion = PackageVersion.luaVersion;
                lastUnzipVersion = UnzipVersion.luaVersion;
                break;
        }

        if (lastUpdateVersion.CompareTo(lastUnzipVersion) > 0)
        {
            isNeed = true;
        }


        if (Application.isMobilePlatform || EnableUpdate)
        {
            if (string.IsNullOrEmpty(lastUnzipVersion))
            {
                bundlePath = StreamingAssetsPath + bundlePath;
            }
            else
            {
                bundlePath = PersistentDataPath + "/" + bundlePath;
            }
        }
        else
        {
            bundlePath = StreamingAssetsPath + bundlePath;
            isNeed = false;
        }

        if(bundleType == BundleType.resource)
        {
            if (EnableUpdate)
            {
                bundlePath = PersistentDataPath + "/" + bundlePath;
            }
            else
            {
                bundlePath = RESOURCE_EDITOR_PATH;
            }
        }

        outputPath = PersistentDataPath + outputPath;
        Debug.Log("PersistentDataPath " + PersistentDataPath);
        return isNeed;
    }

    public void UnzipLoadZip(BundleType bundleType)
    {
        string inputPath = null;
        string outputPath = null;
        if (GetLoadZipPath(bundleType,out inputPath,out outputPath) == true)
        {
            AssetBundle assetbundle = AssetBundle.LoadFromFile(inputPath);
            if(assetbundle == null)
                return;
            TextAsset textAsset = assetbundle.LoadAsset<TextAsset>(BUNDLE_PATHS[(int)bundleType]);
            if(textAsset == null || textAsset.bytes == null || textAsset.bytes.Length <= 0)
                return;
            if(Directory.Exists(outputPath) == false)
            {
                Directory.CreateDirectory(outputPath);
            }
            ZipHelper.UnZip(textAsset.bytes, outputPath);
        }
    }

    //获取版本信息，
    private void InitialVersions()
    {
        Version apkPackageVersion = null;
        Version localPackageVersion = null;
        bool isUnzip = false;

        AssetBundle assetbundle = AssetBundle.LoadFromFile(StreamingAssetsPath + VERSIONS_PATH + ".unity3d");
        TextAsset packageAsset = assetbundle.LoadAsset<TextAsset>(PACKAGE);
        TextAsset textAsset = assetbundle.LoadAsset<TextAsset>(VERSIONS_PATH);

        //游戏包找不到指定文件，所以是有错误的，
        if (assetbundle == null || packageAsset == null || textAsset == null)
        {
            Debug.LogError("找不到游戏包指定文件，请确定是否是正常的安装包，" + (assetbundle == null) + " " + (packageAsset == null) + " " + (textAsset == null));
        }

        //查找不到自定文件，所以是第一次安装游戏，
        if (File.Exists(PersistentDataPath + PACKAGE + ".json") == false)
        {
            isUnzip = true;
        }
        else
        {
            //不是第一次安装文件，检查是否apk版本是否和本地版本一样，不一样(覆盖安装apk)，用新的版本文件覆盖老版本文件，
            if (assetbundle != null)
            {
                string packagaStr = packageAsset.text;
                apkPackageVersion = JsonMapper.ToObject<Version>(packagaStr);

                string localPackageVersionStr = File.ReadAllText(PersistentDataPath + PACKAGE + ".json");
                localPackageVersion = JsonMapper.ToObject<Version>(localPackageVersionStr);
                if (apkPackageVersion != null && apkPackageVersion.Equals(localPackageVersion) == false)
                {
                    isUnzip = true;
                }
            }
        }

        if (isUnzip && textAsset.bytes != null && textAsset.bytes.Length > 0)
        {
            ZipHelper.UnZip(textAsset.bytes, PersistentDataPath);
        }

        string versionStr = File.ReadAllText(PersistentDataPath + PACKAGE + ".json");
        if(string.IsNullOrEmpty(versionStr) == false)
        {
            PackageVersion = JsonMapper.ToObject<Version>(versionStr);
        }

        versionStr = File.ReadAllText(PersistentDataPath + UPDATE + ".json");
        if (string.IsNullOrEmpty(versionStr) == false)
        {
            UpdateVersion = JsonMapper.ToObject<Version>(versionStr);
        }

        versionStr = File.ReadAllText(PersistentDataPath + UNZIP + ".json");
        if (string.IsNullOrEmpty(versionStr) == false)
        {
            UnzipVersion = JsonMapper.ToObject<Version>(versionStr);
        }
    }

    public void SetupResources()
    {
        List<ResourceAsset> resourceAssets = new List<ResourceAsset>(ResourceAssetAssetBundleReader.Instance.dicts.Values);
        List<PrefabAsset> prefabAssets = new List<PrefabAsset>(PrefabAssetAssetBundleReader.Instance.dicts.Values);
        for(int i = 0;i < prefabAssets.Count;i++)
        {
            PrefabAsset prefabAsset = prefabAssets[i];
            ResourceAsset resourceAsset = ResourceAssetAssetBundleReader.Instance.FindById(prefabAsset.bundleName);
            if(resourceAsset != null)
            {
                ResInfo resInfo = new ResInfo();
                resInfo.assetName = prefabAsset.id;
                resInfo.assetPath = prefabAsset.name;
                resInfo.bundlePath = resourceAsset.path;
                resInfo.bundleName = resourceAsset.name;
                resInfo.version = resourceAsset.version.ToString();
                resInfoDict[resInfo.assetName] = resInfo;
            }
        }
        for (int i = 0; i < resourceAssets.Count; i++)
        {
            ResourceAsset resourceAsset = resourceAssets[i];
            if (resourceAsset != null)
            {
                ResInfo resInfo = new ResInfo();
                resInfo.assetName = resourceAsset.id;
                resInfo.assetPath = resourceAsset.name;
                resInfo.bundlePath = resourceAsset.path;
                resInfo.bundleName = resourceAsset.name;
                resInfo.version = resourceAsset.version.ToString();
                resInfoDict[resInfo.assetName] = resInfo;
            }
        }
        if (resInfoDict.ContainsKey("Resources"))
        {
            manifestResInfo = resInfoDict["Resources"];
            manifest = LoadResource<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    public void SetVersion(VersionType versionType,Version version)
    {
        string versionStr = "";
        string versionPath = "";
        switch(versionType)
        {
            case VersionType.package:
                return;
            case VersionType.update:
                versionStr = JsonMapper.ToJson(version);
                versionPath = PersistentDataPath + UPDATE + ".json";
                break;
            case VersionType.unzip:
                versionStr = JsonMapper.ToJson(version);
                versionPath = PersistentDataPath + UNZIP + ".json";
                break;
            default:
                return;
        }

        File.WriteAllText(versionPath,versionStr);
    }
}
