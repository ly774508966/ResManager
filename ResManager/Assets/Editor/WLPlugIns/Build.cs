using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor.Callbacks;

public class Build{

    public static string[] mzwsdkFiles = {"projectmzw.jar","AndroidManifest.xml","res/raw/olsdk_core","mzw_onlinesdk.jar","assets/alipay_plugin_20120428msp.apk"};
    public static string[] misdkFiles = {"projectmi.jar","AndroidManifest.xml","SDK_TY_4.3.4.jar","assets/MiGameCenterSDKService.apk"};
    public static string[] debuglanFiles = {"projectmzw.jar","AndroidManifest.xml","res/raw/olsdk_core","mzw_onlinesdk.jar"};
    public static string[] debugwanFiles = {"projectmzw.jar","AndroidManifest.xml","res/raw/olsdk_core","mzw_onlinesdk.jar"};
    public static string[] ucsdkFiles = {"projectuc.jar","AndroidManifest.xml","alipay-20150513.jar","UCGameSDK-3.5.3.1.jar","assets/"};
    public static string[] qihusdkFiles = {"projectqihu.jar","AndroidManifest.xml","360SDK.jar","android-support-v4.jar","assets/"};
    public static string[] baidusdkFiles = {"projectbaidu.jar","AndroidManifest.xml","res/","libs/"};
    public static string[] allFiles = {"Android/"};
    public static string[] tencentsdkFiles = { "projecttencent.jar", "AndroidManifest.xml", "android-support-v4.jar", "mid-sdk-2.10.jar", "MSDK_Android_2.5.4a_svn55243.jar", "TencentUnipaySDK.jar", "res/", "assets/" };
    public static string[] defaultFiles = {"Android/","Bugly.cs","BuglyInit.cs","Debuger.dll","IBugly.dll"};
    public static string targetname = "";
    #region 内部函数 
    //得到工程中所有场景名称
    static string[] SCENES = FindEnabledEditorScenes();

    //获取到BuildSettings面板里面的场景
    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    //build Android项目
    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
        string res = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);

        if (res.Length > 0)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
    }
    #endregion

    //根据不同平台Build不同包
    public static void BulidTarget(string name, string target, string scriptSymbols = "")
    {
        AssetDatabase.Refresh();
        // 打包LuaZip
        WLGame.PackMenu.PackLuaZip();

        string deleteFile = Application.dataPath + "/Plugins/Android/";
        DeleteFolder(deleteFile);
        targetname = name;
        string now = System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString("D2") + System.DateTime.Now.Day.ToString("D2") + "_" +
            System.DateTime.Now.Hour.ToString("D2") + System.DateTime.Now.Minute.ToString("D2");
        string app_name = "dgame_" + now + "_" + WLGame.BuildPackage.PackageVersion.baseVersion + "_" + name;

        string target_dir = Application.dataPath + "/TargetAndroid";
        string target_name = app_name + ".apk";
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        BuildTarget buildTarget = BuildTarget.Android;
        string applicationPath = Application.dataPath.Replace("/Assets", "");

        if (target == "Android")
        {
            target_dir = applicationPath + "/TargetAndroid";
            target_name = app_name + ".apk";
            targetGroup = BuildTargetGroup.Android;
        }
        if (target == "IOS")
        {
            target_dir = applicationPath + "/TargetIOS";
            target_name = app_name;
            targetGroup = BuildTargetGroup.iOS;
            buildTarget = BuildTarget.iOS;
        }

        //每次build删除之前的残留
        if (Directory.Exists(target_dir))
        {
            if (File.Exists(target_name))
            {
                File.Delete(target_name);
            }
        }
        else
        {
            Directory.CreateDirectory(target_dir);
        }

        //==================这里是比较重要的东西=======================
        string splashsrcPath = Application.dataPath + "/../OtherSdk/debug/splash.jpg";
        string splashdestPath = Application.dataPath + "/Textures/splash.jpg";
        string srcPath = "";
        string destPath = "";
            srcPath = Application.dataPath + "/../OtherSdk/DefaultSdk/";
            destPath = Application.dataPath + "/Plugins/";
            CopyDirectory(srcPath, destPath);
        //PlayerSettings.productName = name + "刀塔";
        string targetpath = name.ToLower();
        Debug.LogError("targetpath " + targetpath);
        srcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/";
        destPath = Application.dataPath + "/Plugins/Android/";
        CopyDirectory(srcPath, destPath);
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/splash.jpg";
        splashdestPath = Application.dataPath + "/Textures/splash.jpg";
        CopyFile(splashsrcPath, splashdestPath);
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/logosplash.png";
        splashdestPath = Application.dataPath + "/Textures/logosplash.png";
        CopyFile(splashsrcPath, splashdestPath);
        CopyIcons(targetpath);

        switch (name)
        {
            case "DEBUG_LAN":       //内网测试包
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                PlayerSettings.productName = "战神王座";
                break;
            case "DEBUG_WAN":       //外网测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.debug";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                PlayerSettings.productName = "战神王座";
                break;
            case "MZWSDK":             //拇指玩测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.mzw";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                break;
            case "MISDK":             //小米测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.mi";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                break;
            case "UCSDK":             //uc测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.uc";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                break;
            case "QIHUSDK":             //qihu测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.qihu";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                break;
            case "BAIDUSDK":             //baidusdk测试包
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.baidu";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                break;
            case "TENCENTSDK":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                AssetDatabase.Refresh();
                PlayerSettings.bundleIdentifier = "com.tencent.tmgp.hdls.dgame";
                break;
            case "HUAWEISDK":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.huawei";
                PlayerSettings.productName = "刀塔大乱斗";
                break;
            case "HEDESDK":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.hede";
                PlayerSettings.productName = "战神王座";
                break;
            case "ROMANLIFESDK":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.romanlife";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_MOYO" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.hdls.dgame.moyo";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_UC":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_UC" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.uc";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_MI":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_MI" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.mi";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_BAIDU":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_BAIDU" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.baidu";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_360":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_360" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.qh";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_TENCENT":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_TENCENT" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.tencent.tmgp.dtwz";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_HUAWEI":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_HUAWEI" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.HUAWEI";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_OPPO":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_OPPO" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.nearme.gamecenter";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_SOGOU":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_SOGOU" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.sogou.com";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_LETV":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_LETV" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.leshi";
                PlayerSettings.productName = "战神王座";
                AssetDatabase.Refresh();
                //return;
                break;
            case "MOYOSDK_WDJ":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_WDJ" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.wdj";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_COOLPAD":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_COOLPAD" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.coolpad";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_LENOVO":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_LENOVO" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.lenovo";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_YOUKU":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_YOUKU" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.youku";
                PlayerSettings.productName = "战神王座";
                break;
            case "MOYOSDK_GIONEE":
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK;MOYOSDK_GIONEE" + scriptSymbols);
                PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.am";
                PlayerSettings.productName = "战神王座";
                break;
        }

        AssetDatabase.Refresh();
        //开始Build场景，等待吧～
        Debug.LogError("target_name " + target_name);
        GenericBuild(SCENES, target_dir + "/" + target_name, buildTarget, BuildOptions.None);
        Debug.LogError("target_name " + target_name);
    }


    //根据不同平台Build不同包
    public static void BulidTarget(string name, string target, string scriptSymbols ,string appname ,string identifier,bool wantBuild = true,bool isExported = false)
    {
        AssetDatabase.Refresh();
        // 打包LuaZip
        //WLGame.PackMenu.PackLuaZip();

        string deleteFile = Application.dataPath + "/Plugins/Android/";
        //DeleteFolder(deleteFile);
        targetname = name;
        string now = System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString("D2") + System.DateTime.Now.Day.ToString("D2") + "_" +
            System.DateTime.Now.Hour.ToString("D2") + System.DateTime.Now.Minute.ToString("D2");
        string app_name = now + "_" + WLGame.BuildPackage.PackageVersion.baseVersion + "_" + name + scriptSymbols;

        string target_dir = Application.dataPath + "/TargetAndroid";
        string target_name = app_name.Replace(":", "_").Replace(";", "_") + ".apk";
        BuildTargetGroup targetGroup = BuildTargetGroup.Android;
        BuildTarget buildTarget = BuildTarget.Android;
        string applicationPath = Application.dataPath.Replace("/Assets", "");

        if (target == "Android")
        {
            target_dir = applicationPath + "/TargetAndroid";
            if (isExported == false)
            {
                target_name = app_name.Replace(":","_").Replace(";","_") + ".apk";
            }
            else
            {
                target_name = app_name.Replace(":","_").Replace(";","_");
            }
            targetGroup = BuildTargetGroup.Android;
        }
        if (target == "IOS")
        {
            target_dir = applicationPath + "/TargetIOS";
            target_name = app_name;
            targetGroup = BuildTargetGroup.iOS;
            buildTarget = BuildTarget.iOS;
        }

        //每次build删除之前的残留
        if (Directory.Exists(target_dir))
        {
            if (File.Exists(target_name))
            {
                File.Delete(target_name);
            }
        }
        else
        {
            Directory.CreateDirectory(target_dir);
        }

        //==================这里是比较重要的东西=======================
        string targetpath = name.ToLower();
        string splashsrcPath = Application.dataPath + "/../OtherSdk/debug/splash.jpg";
        string splashdestPath = Application.dataPath + "/Textures/splash.jpg";
        string srcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/DefaultSdk/";
        string destPath = Application.dataPath + "/Plugins/";
        if(Directory.Exists(srcPath) == false)
        {
            Debug.LogError("srcPath for path " + srcPath);
            srcPath = Application.dataPath + "/../OtherSdk/DefaultSdk/";
        }

        Debug.LogError("copy srcPath " + srcPath + " to destPath " + destPath);

        //CopyDirectory(srcPath, destPath);
        //PlayerSettings.productName = name + "刀塔";
        Debug.LogError("targetpath " + targetpath);
        srcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/";
        destPath = Application.dataPath + "/Plugins/Android/";
        //CopyDirectory(srcPath, destPath);
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/splash.jpg";
        splashdestPath = Application.dataPath + "/Textures/splash.jpg";
        CopyFile(splashsrcPath, splashdestPath);
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/logosplash.png";
        splashdestPath = Application.dataPath + "/Textures/logosplash.png";
		CopyFile(splashsrcPath, splashdestPath);
		splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/Android/loadingbg.png";
		splashdestPath = Application.dataPath + "/Resources/Textrue/UI/UIAtlas/loadingbg.png";
		CopyFile(splashsrcPath, splashdestPath);
        CopyIcons(targetpath);

        PlayerSettings.bundleIdentifier = identifier;
        PlayerSettings.bundleVersion = WLGame.BuildPackage.PackageVersion.baseVersion;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, name + scriptSymbols);
        PlayerSettings.productName = appname;

        AssetDatabase.Refresh();
        //开始Build场景，等待吧～
        Debug.LogError("target_name " + target_name);
        if (wantBuild == true)
        {
            if (isExported == false)
            {
                GenericBuild(SCENES, target_dir + "/" + target_name, buildTarget, BuildOptions.None);
            }
            else
            {
                GenericBuild(SCENES, target_dir + "/" + target_name, buildTarget, BuildOptions.AcceptExternalModificationsToPlayer);
            }
        }
        Debug.LogError("target_name " + target_name);
        AssetDatabase.Refresh();
    }


    [MenuItem("build/export")]
    static void Export()
    {
        AssetDatabase.Refresh();
        // 打包LuaZip
        WLGame.PackMenu.PackLuaZip();

        string deleteFile = Application.dataPath + "/Plugins/Android/";
        DeleteFolder(deleteFile);
        string srcPath = Application.dataPath + "/../OtherSdk/" + "tencentsdk/" + "/DefaultSdk/";
        string destPath = Application.dataPath + "/Plugins/";
        if (Directory.Exists(srcPath) == false)
        {
            Debug.LogError("srcPath for path " + srcPath);
            srcPath = Application.dataPath + "/../OtherSdk/DefaultSdk/";
        }

        Debug.LogError("copy srcPath " + srcPath + " to destPath " + destPath);
        CopyDirectory(srcPath, destPath);
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "ANDROIDSDK");
        string outDirPath = Application.dataPath + "..";
        outDirPath = Path.GetDirectoryName(outDirPath);
        Debug.LogError(outDirPath);
        outDirPath = Path.GetFileName(outDirPath);
        Debug.LogError(outDirPath);
        PlayerSettings.productName = outDirPath;

        PlayerSettings.Android.keystoreName = Application.dataPath + "/../dtwzkey/dntk.keystore";
        PlayerSettings.Android.keystorePass = "hdls8888";
        PlayerSettings.Android.keyaliasName = "dntk";
        PlayerSettings.Android.keyaliasPass = "hdls8888";

        BuildPipeline.BuildPlayer(SCENES, Application.dataPath + "/../build_script/", BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
    }

	//IOS package
	
	//根据不同平台Build不同包
    public static void BuildIOS(string project, string target, string scriptSymbols = "")
    {
        AssetDatabase.Refresh();
		// 打包配置文件
//		WLGame.PackMenu.PackConfigFiles();
		
		// 打包LuaZip
		WLGame.PackMenu.PackLuaZip();

        if (projectName != "")
	    {
            project = projectName;
	    }
	    if (projectTarget != "")
	    {
            target = projectTarget;
	    }
		
		//string deleteFolder = Application.dataPath + "/Plugins/IOS/";
		//DeleteFolder(deleteFolder);
		
		string target_dir = Application.dataPath + "/../build/ios/" + target + "/";
		string target_name = target_dir + project;
		BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
		BuildTarget buildTarget = BuildTarget.iOS;
		
		//每次build删除之前的残留
		if (Directory.Exists(target_name))
		{
			DeleteFolder(target_name);
		}
		else
		{
			Directory.CreateDirectory(target_dir);
		}

		Debug.LogError(target_name);
		//==================这里是比较重要的东西=======================
		string splashsrcPath = Application.dataPath + "/../OtherSdk/debug/splash.jpg";
		string splashdestPath = Application.dataPath + "/Textures/splash.jpg";
		string srcPath = "";
		string destPath = "";
		string targetpath = target.ToLower();
		Debug.LogError("targetpath " + targetpath);

		srcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios";
		destPath = Application.dataPath + "/Plugins/IOS/";
		CopyDirectory(srcPath, destPath);

//		srcPath = Application.dataPath + "/XUPorter/Mods/" + targetpath + "/ios.projmods.txt";
//		destPath = Application.dataPath + "/XUPorter/Mods/ios.projmods";
//		CopyFile(srcPath, destPath);

		splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/splash.jpg";
		splashdestPath = Application.dataPath + "/Textures/splash.jpg";

		CopyFile(splashsrcPath, splashdestPath);
		//copyicon
		splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/icon.png";
		splashdestPath = Application.dataPath + "/Textures/icon.png";
		
		CopyFile(splashsrcPath, splashdestPath);
		
		//copyi logo
		splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/logo.png";
		splashdestPath = Application.dataPath + "/Resources/Textrue/UI/MaterialTexture/logo/logo.png";
		
		CopyFile(splashsrcPath, splashdestPath);

		switch (target)
		{
		case "DEBUG_LAN":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.hdls.dgame.dtwz";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "DEBUG_LAN" + scriptSymbols);
			break;
		case "DEBUG_WAN":       //外网测试包
			PlayerSettings.bundleIdentifier = "com.hdls.dgame.dtwz";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "DEBUG_WAN" + scriptSymbols);
			break;
		case "MOYOSDK_APPSTORE":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_APP" + scriptSymbols);
			break;
		case "MOYOSDK_HM":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.hm";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_HM" + scriptSymbols);
			break;
		case "MOYOSDK_KY":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.7659";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_KY" + scriptSymbols);
            break;
        case "MOYOSDK_TB":       //内网测试包
            PlayerSettings.bundleIdentifier = "com.tongbu.romanlife.dtwz";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_TB" + scriptSymbols);
			break;
		case "MOYOSDK_ITOOLS":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.sky";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_ITOOLS" + scriptSymbols);
			break;
		case "MOYOSDK_PP":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.pp";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_PP" + scriptSymbols);
			break;
		case "MOYOSDK_XY":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.xy";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_XY" + scriptSymbols);
			break;
		case "MOYOSDK_I4":       //内网测试包
			PlayerSettings.bundleIdentifier = "com.romanlife.dtwz.i4";
			PlayerSettings.productName = "战神王座";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "MOYOSDK_APPSTORE;MOYOSDK_I4" + scriptSymbols);
			break;
		}
		
		AssetDatabase.Refresh();
		Debug.LogError("ios target_name " + target_name);
		//开始Build场景，等待吧～
		GenericBuild(SCENES, target_name, buildTarget, BuildOptions.None);
		
	}

    //根据不同平台Build不同包
	public static void BuildIOS(string project, string target, string scriptSymbols, string appname, string identifier,bool wantBuild = true)
    {
        AssetDatabase.Refresh();
        // 打包配置文件
        //		WLGame.PackMenu.PackConfigFiles();

        // 打包LuaZip
        WLGame.PackMenu.PackLuaZip();

        if (projectName != "")
        {
            project = projectName;
        }
        if (projectTarget != "")
        {
            target = projectTarget;
        }

        //string deleteFolder = Application.dataPath + "/Plugins/IOS/";
        //DeleteFolder(deleteFolder);

        string target_dir = Application.dataPath + "/../build/ios/" + target + "/";
        string target_name = target_dir + project;
        BuildTargetGroup targetGroup = BuildTargetGroup.iOS;
        BuildTarget buildTarget = BuildTarget.iOS;

        //每次build删除之前的残留
        if (Directory.Exists(target_name))
        {
            DeleteFolder(target_name);
        }
        else
        {
            Directory.CreateDirectory(target_dir);
        }

        Debug.LogError(target_name);
        //==================这里是比较重要的东西=======================
        string splashsrcPath = Application.dataPath + "/../OtherSdk/debug/splash.jpg";
        string splashdestPath = Application.dataPath + "/Textures/splash.jpg";
        string srcPath = "";
        string destPath = "";
        string targetpath = target.ToLower();
        Debug.LogError("targetpath " + targetpath);

        srcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios";
        destPath = Application.dataPath + "/Plugins/IOS/";
        CopyDirectory(srcPath, destPath);

//        srcPath = Application.dataPath + "/XUPorter/Mods/" + targetpath + "/ios.projmods.txt";
//        destPath = Application.dataPath + "/XUPorter/Mods/ios.projmods";
//        CopyFile(srcPath, destPath);

        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/splash.jpg";
        splashdestPath = Application.dataPath + "/Textures/splash.jpg";

        CopyFile(splashsrcPath, splashdestPath);
        //copyicon
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/icon.png";
        splashdestPath = Application.dataPath + "/Textures/icon.png";

        CopyFile(splashsrcPath, splashdestPath);

        //copyi logo
        splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/logo.png";
        splashdestPath = Application.dataPath + "/Resources/Textrue/UI/MaterialTexture/logo/logo.png";

        CopyFile(splashsrcPath, splashdestPath);

		//copy loadingbg
		splashsrcPath = Application.dataPath + "/../OtherSdk/" + targetpath + "/IOS/ios/loadingbg.png";
		splashdestPath = Application.dataPath + "/Resources/Textrue/UI/UIAtlas/loadingbg.png";
		
		CopyFile(splashsrcPath, splashdestPath);

        PlayerSettings.bundleIdentifier = identifier;
        PlayerSettings.bundleVersion = WLGame.BuildPackage.PackageVersion.baseVersion;
        PlayerSettings.productName = appname;
		PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, target + ";" + scriptSymbols);
        AssetDatabase.Refresh();
        Debug.LogError("ios target_name " + target_name);
		//开始Build场景，等待吧～
		if (wantBuild == true)
		{
			GenericBuild(SCENES, target_name, buildTarget, BuildOptions.None);
		}

    }
	public static string projectName
	{
		get
		{ 
			//在这里分析shell传入的参数， 还记得上面我们说的哪个 project-$1 这个参数吗？
			//这里遍历所有参数，找到 project开头的参数， 然后把-符号 后面的字符串返回，
			//这个字符串就是 uc 了。。
			foreach(string arg in System.Environment.GetCommandLineArgs()) {
				if(arg.StartsWith("project-"))
				{
					return arg.Split("-"[0])[1];
				}
			}
			return "";
		}
	}
	//
	public static string projectTarget
	{
		get
		{ 
			//在这里分析shell传入的参数， 还记得上面我们说的哪个 project-$1 这个参数吗？
			//这里遍历所有参数，找到 project开头的参数， 然后把-符号 后面的字符串返回，
			//这个字符串就是 uc 了。。
			foreach(string arg in System.Environment.GetCommandLineArgs()) {
				if(arg.StartsWith("target-"))
				{
					return arg.Split("-"[0])[1];
				}
			}
			return "";
		}
	}
#if UNITY_ANDROID
    //内网测试包
    //[MenuItem("BUILD/Build Android 内网测试包")]
    static void PerformAndroidDEBUG_LANBuild()
    {
        BulidTarget("DEBUG_LAN", "Android");
    }

    //外网测试包
    //[MenuItem("BUILD/Build Android 外网测试包")]
    static void PerformAndroidDEBUG_WANBuild()
    {
        BulidTarget("DEBUG_WAN", "Android");
    }

    ////拇指玩包
    ////[MenuItem("BUILD/Build Android 拇指玩测试包")]
    //static void PerformAndroidMZWBuild()
    //{
    //    BulidTarget("MZWSDK", "Android");
    //}
    ////小米测试包
    ////[MenuItem("BUILD/Build Android 小米测试包")]
    //static void PerformAndroidMiBuild()
    //{
    //    BulidTarget("MISDK", "Android");
    //}
    ////uc测试包
    ////[MenuItem("BUILD/Build Android uc测试包")]
    //static void PerformAndroidUcBuild()
    //{
    //    BulidTarget("UCSDK", "Android");
    //}
    ////360测试包
    ////[MenuItem("BUILD/Build Android 360测试包")]
    //static void PerformAndroid360Build()
    //{
    //    BulidTarget("QIHUSDK", "Android");
    //}
    ////baidu测试包
    //[MenuItem("BUILD/Build Android baidu测试包")]
    static void PerformAndroidBaiduBuild()
    {
        BulidTarget("BAIDUSDK", "Android");
    }
    ////[MenuItem("BUILD/Build Android Tecent测试包")]
    //static void PerformAndroidTencentBuild()
    //{ 
    //    BulidTarget("TENCENTSDK","Android");
    //}
    ////[MenuItem("BUILD/Build Android 赫德测试包")]
    //static void PerformAndroidHedeSDKBuild()
    //{
    //    BulidTarget("HEDESDK", "Android");
    //}

    ////[MenuItem("BUILD/Build Android 罗马生活测试包")]
    static void PerformAndroidRomanlifeSDKBuild()
    {
        BulidTarget("ROMANLIFESDK", "Android");
    }

    //[MenuItem("BUILD/Build Android 华为测试包")]
    static void PerformAndroidHuaweiSDKBuild()
    { 
        BulidTarget("HUAWEISDK","Android");
    }

    //[MenuItem("BUILD/Build Android moyo测试包")]
    static void PerformAndroidMOYOSDKBuild()
    {
        BulidTarget("MOYOSDK", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_uc")]
    static void PerformAndroidMOYOSDK_UCBuild()
    {
        BulidTarget("MOYOSDK_UC", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_mi")]
    static void PerformAndroidMOYOSDK_MIBuild()
    {
        BulidTarget("MOYOSDK_MI", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_baidu")]
    static void PerformAndroidMOYOSDK_BAIDUBuild()
    {
        BulidTarget("MOYOSDK_BAIDU", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_360")]
    static void PerformAndroidMOYOSDK_360Build()
    {
        BulidTarget("MOYOSDK_360", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_tencent")]
    static void PerformAndroidMOYOSDK_tencentBuild()
    {
        BulidTarget("MOYOSDK_TENCENT", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_huawei")]
    static void PerformAndroidMOYOSDK_huaweiBuild()
    {
        BulidTarget("MOYOSDK_HUAWEI", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_oppo")]
    static void PerformAndroidMOYOSDK_oppoBuild()
    {
        BulidTarget("MOYOSDK_OPPO", "Android");
    }

    //[MenuItem("BUILD/Build Android moyo_sogou")]
    static void PerformAndroidMOYOSDK_sogouBuild()
    {
        BulidTarget("MOYOSDK_SOGOU", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_letv")]
    static void PerformAndroidMOYOSDK_letvBuild()
    {
        BulidTarget("MOYOSDK_LETV", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_wdj")]
    static void PerformAndroidMOYOSDK_wdjBuild()
    {
        BulidTarget("MOYOSDK_WDJ", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_coolpad")]
    static void PerformAndroidMOYOSDK_coolpadBuild()
    {
        BulidTarget("MOYOSDK_COOLPAD", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_lenovo")]
    static void PerformAndroidMOYOSDK_lenovoBuild()
    {
        BulidTarget("MOYOSDK_LENOVO", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_youku")]
    static void PerformAndroidMOYOSDK_youkuBuild()
    {
        BulidTarget("MOYOSDK_YOUKU", "Android");
    }

    //[MenuItem("BUILD/Build Android moyosdk_gionee")]
    static void PerformAndroidMOYOSDK_gioneeBuild()
    {
        BulidTarget("MOYOSDK_GIONEE", "Android");
    }

    //[MenuItem("BUILD/Build Android All")]
    static void PerformAndroidMOYOSDK_allBuild()
    {
        PerformAndroidMOYOSDK_360Build();
        PerformAndroidMOYOSDK_tencentBuild();
        PerformAndroidMOYOSDK_MIBuild();
        PerformAndroidMOYOSDK_UCBuild();
        PerformAndroidMOYOSDK_BAIDUBuild();
        PerformAndroidMOYOSDK_oppoBuild();
        PerformAndroidMOYOSDK_sogouBuild();
        PerformAndroidMOYOSDK_letvBuild();
        PerformAndroidMOYOSDK_wdjBuild();
        PerformAndroidMOYOSDK_coolpadBuild();
        PerformAndroidMOYOSDK_lenovoBuild();
        PerformAndroidMOYOSDK_youkuBuild();
        PerformAndroidMOYOSDK_gioneeBuild();
        PerformAndroidMOYOSDK_huaweiBuild();
    }
#endif
	#if UNITY_IPHONE
	
	//[MenuItem("BUILD/Build IOS CONF")]
	static void PerformAndroidMOYOSDK_CONFBuild()
	{
//		BuildIOS("DEBUG_WAN", "DEBUG_WAN");
		DeleteFolder (Application.dataPath + "/Scripts/TDRExported/localdata");
		DeleteFolder (Application.dataPath + "/Scripts/TDRExported/Tdr");
		DeleteFile (Application.dataPath + "/Scripts/TDRExported/cs_protobuf/dgproto.cs");
		DeleteFolder (Application.dataPath + "/MobileMovieTexture");

		CopyFile (Application.dataPath + "/Scripts/TDRExported/cs_protobuf/dgproto.dll.bytes", Application.dataPath + "/Plugins/dgproto.dll");
		CopyFile (Application.dataPath + "/Scripts/TDRExported/cs_protobuf/dgprotoserializer.dll.bytes", Application.dataPath + "/Plugins/dgprotoserializer.dll");
		CopyFile (Application.dataPath + "/Scripts/TDRExported/cs_protobuf/protobuf-net.dll.bytes", Application.dataPath + "/Plugins/protobuf-net.dll");
		CopyFile (Application.dataPath + "/Scripts/TDRExported/cs_protobuf/tdrconf.dll.bytes", Application.dataPath + "/Plugins/tdrconf.dll");
	}

    //[MenuItem("BUILD/Build IOS moyo appstore测试包")]
	static void PerformAndroidMOYOSDK_APPSTOREBuild()
    {
		BuildIOS("MOYOSDK_APPSTORE", "MOYOSDK_APPSTORE");
    }
	
	//[MenuItem("BUILD/Build IOS moyo hm测试包")]
	static void PerformAndroidMOYOSDK_HMBuild()
	{
		BuildIOS("MOYOSDK_HM", "MOYOSDK_HM");
	}
	
	//[MenuItem("BUILD/Build IOS moyo ky测试包")]
	static void PerformAndroidMOYOSDK_KYBuild()
	{
		BuildIOS("MOYOSDK_KY", "MOYOSDK_KY");
	}
    
	//[MenuItem("BUILD/Build IOS moyo tb测试包")]
	static void PerformAndroidMOYOSDK_TBBuild()
	{
		BuildIOS("MOYOSDK_TB", "MOYOSDK_TB");
	}
	
	//[MenuItem("BUILD/Build IOS moyo itools测试包")]
	static void PerformAndroidMOYOSDK_ITOOLSBuild()
	{
		BuildIOS("MOYOSDK_ITOOLS", "MOYOSDK_ITOOLS");
	}
	
	//[MenuItem("BUILD/Build IOS moyo pp测试包")]
	static void PerformAndroidMOYOSDK_PPBuild()
	{
		BuildIOS("MOYOSDK_PP", "MOYOSDK_PP");
	}
	
	//[MenuItem("BUILD/Build IOS moyo XY测试包")]
	static void PerformAndroidMOYOSDK_XYBuild()
	{
		BuildIOS("MOYOSDK_XY", "MOYOSDK_XY");
	}
	
	//[MenuItem("BUILD/Build IOS moyo I4测试包")]
	static void PerformAndroidMOYOSDK_I4Build()
	{
		BuildIOS("MOYOSDK_I4", "MOYOSDK_I4");
	}
	
	//[MenuItem("BUILD/Build IOS DEBUG_LAN测试包")]
	static void PerformAndroidMOYOSDK_DEBUG_LANBuild()
	{
		BuildIOS("DEBUG_LAN", "DEBUG_LAN");
	}
	
	//[MenuItem("BUILD/Build IOS DEBUG_WAN测试包")]
	static void PerformAndroidMOYOSDK_DEBUG_WANBuild()
	{
		BuildIOS("DEBUG_WAN", "DEBUG_WAN");
	}

	#endif

    //复制制定文件，
	public static void CopyFile(string sourcePath, string destinationPath)
	{
        try
        {
            if (File.Exists(sourcePath) == false)
                return;
            string destdir = destinationPath.Substring(0,destinationPath.LastIndexOf("/"));
            if(Directory.Exists(destdir) == false)
                Directory.CreateDirectory(destdir);
            FileInfo info = new FileInfo(sourcePath);
            File.Copy(info.FullName, destinationPath,true);
        }catch(System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
	}
    //删除制定文件，
	public static void DeleteFile(string dir)
	{
        try
        {
            if (File.Exists(dir))
            {
                FileInfo fi = new FileInfo(dir);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = FileAttributes.Normal;
                File.Delete(dir);
            }
        }catch(System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
	}
	

    //

	[PostProcessBuild (100)]
	public static void OnPostProcessBuild (BuildTarget target, string pathToBuiltProject)
	{
		return;
		#if UNITY_IPHONE
		if (target == BuildTarget.iPhone)
		{
			//DeleteFolder(Application.dataPath+"/Plugins/IOS");
			//if(projectName== "uc")
			//{
				////当我们在打uc包的时候 这里面做一些 操作。
				
			//}
			//得到xcode工程的路径
			string path = Path.GetFullPath (pathToBuiltProject);
			
			// Create a new project object from build target
			//XCProject project = new XCProject (pathToBuiltProject);
			
			//设置签名的证书， 第二个参数 你可以设置成你的证书
			//project.overwriteBuildSetting ("CODE_SIGN_IDENTITY", "iPhone Distribution: Shenzhen Hudonglingshi Co, ltd. (H5HGYY37Z8)", "Release");
			//project.overwriteBuildSetting ("CODE_SIGN_IDENTITY", "iPhone Distribution: Shenzhen Hudonglingshi Co, ltd. (H5HGYY37Z8)", "Debug");
			// Finally save the xcode project
			//project.Save ();
			return;
		}
		#endif      
		
		#if UNITY_ANDROID
		if (target == BuildTarget.Android)
		{
            //当我们在打特定包的时候 这里面做一些特定操作。
			if(targetname == "DEBUG_WAN")
			{
                for(int i = 0;i < mzwsdkFiles.Length;i++)
                {
                    string deleteFile = Application.dataPath + "Plugins/Android/" + mzwsdkFiles[i];
                    DeleteFile(deleteFile);
                }
				
			}else if(targetname == "DEBUG_LAN")
            {
                for(int i = 0;i < mzwsdkFiles.Length;i++)
                {
                    string deleteFile = Application.dataPath + "Plugins/Android/" + mzwsdkFiles[i];
                    DeleteFile(deleteFile);
                }
            }else if(targetname == "MZWSDK")
            {
                for(int i = 0;i < mzwsdkFiles.Length;i++)
                {
                    string deleteFile = Application.dataPath + "Plugins/Android/" + mzwsdkFiles[i];
                    DeleteFile(deleteFile);
                }
            }else if(targetname == "MISDK")
            {
                for(int i = 0;i < misdkFiles.Length;i++)
                {
                    string deleteFile = Application.dataPath + "Plugins/Android/" + misdkFiles[i];
                    DeleteFile(deleteFile);
                }
            }
			//Debug.LogError("PlayerSettings.GetScriptingDefineSymbolsForGroup" + PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android));
			//string definestr = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
			//int index = definestr.IndexOf("BUGLY;") - 1;
			//definestr = definestr.Substring(0,index) + definestr.Substring(index + "BUGLY;".Length + 1);
			//Debug.LogError(definestr);
			//PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "");
			return;
		}

		#endif
		

	}
	public static void CopyDirectory(string sourcePath, string destinationPath)
	{
		if(Directory.Exists(sourcePath) == false)
			return;
		//Debug.LogError ("cp " + sourcePath);
		DirectoryInfo info = new DirectoryInfo(sourcePath);
		Directory.CreateDirectory(destinationPath);
	    try
	    {
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);
                if (fsi is System.IO.FileInfo)
                    File.Copy(fsi.FullName, destName, true);
                else
                {
                    Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName);
                }
            }
	    }
	    catch (Exception e)
	    {
	        Debug.LogError("Exception " + e.ToString());
	        throw;
	    }
	}

	public static void DeleteFolder(string dir)
	{
		if(Directory.Exists(dir) == false)
			return;
	    try
	    {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                try
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);
                    }
                    else
                    {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                            DeleteFolder(d1.FullName);////递归删除子文件夹
                        }
                        Directory.Delete(d);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("DeleteFolder" + dir + "exception:" + e.Message);
                }

            }
	    }
	    catch (Exception e)
	    {
	        Debug.LogError("Exception " + e.ToString());
	        throw;
	    }
	}

    public static void CopyIcons(string target)
    {
		Debug.LogError("CopyIcons");
		string srcPath = "";
		string destPath = "";
#if UNITY_ANDROID
        srcPath = Application.dataPath + "/../OtherSdk/" + target + "/Android/icon.png";
        destPath = Application.dataPath + "/Plugins/Android/res/drawable/app_icon.png";
        CopyFile(srcPath,destPath);
        destPath = Application.dataPath + "/Plugins/Android/res/drawable-hdpi/app_icon.png";
        CopyFile(srcPath,destPath);
        destPath = Application.dataPath + "/Plugins/Android/res/drawable-ldpi/app_icon.png";
        CopyFile(srcPath,destPath);
        destPath = Application.dataPath + "/Plugins/Android/res/drawable-mdpi/app_icon.png";
        CopyFile(srcPath,destPath);
        destPath = Application.dataPath + "/Plugins/Android/res/drawable-xhdpi/app_icon.png";
        CopyFile(srcPath, destPath);
        destPath = Application.dataPath + "/Textures/icon.png";
        CopyFile(srcPath, destPath);
		
#elif UNITY_IPHONE
        srcPath = Application.dataPath + "/../OtherSdk/" + target + "/IOS/icon.png";
        destPath = Application.dataPath + "/Textures/icon.png";
        CopyFile(srcPath, destPath);
#endif
        //拷贝登录logo
        srcPath = Application.dataPath + "/../OtherSdk/" + target + "/Android/logo.png";
        destPath = Application.dataPath + "/Resources/Textrue/UI/MaterialTexture/logo/logo.png";
        CopyFile(srcPath, destPath);
    }
}
