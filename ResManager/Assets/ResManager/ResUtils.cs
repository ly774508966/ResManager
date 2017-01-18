using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ResUtils
{
    //是否允许更新资源
#if UNITY_EDITOR
    public static bool bEnableUpdateRes = false;
#else
#if DISABLE_UPDATE_RES
    public static bool bEnableUpdateRes = false;
#else
    public static bool bEnableUpdateRes = true;
#endif
#endif
    // 客户端版本号
    public static string Version = "0.0.9";

    public static string LuaVersion = ""; 
    public static string ConfigVersion = "";
    // 配置文件的包名
    public const string CONFIG_PKG_NAME = "LocalConfig.unity3d";
    public const string ACTIVE_PKG_NAME = "Active_files.unity3d";
    public const string MINE_PKG_NAME = "mine_files.unity3d";
    /* 函数说明: 是否允许更新资源 */
    public static bool EnableUpdateRes
    {
        get { return bEnableUpdateRes; }
    }

    /* 函数说明: 获取资源地址 */
    public static string ResQueryUrl
    {
        //get { return LoginMgr.ResQueryUrl; }
        get { return ""; }
    }

    /* 函数说明: 获取平台的名称 */
    public static string PlatformName
    {
        get
        {
#if UNITY_ANDROID
        return "android";
#elif UNITY_IPHONE
        return "ios";
#else
        return "pc";
#endif
        }
    }

    /* 函数说明: 获取渠道的名称 */
    public static string ChannelName
    {
        //get { return UISdkSys.Instance.ReadSdkCurCh(); }
        get { return ""; }
    }

    /* 函数说明: 获取本地StreamAsset路径 */
    public static string GetStreamAssetPath(string fileName)
    {
        string path = "";

#if UNITY_ANDROID && (!UNITY_EDITOR)
        path = "jar:file://" + Application.dataPath + "!/assets";
#elif UNITY_IPHONE && (!UNITY_EDITOR)
        path = "file://" + Application.streamingAssetsPath;
#else
        path = "file://" + Application.dataPath + "/StreamingAssets";
#endif

        if (fileName[0] != '/')
        {
            fileName = "/" + fileName;
        }
        path += fileName;

        return path;
    }

    /* 函数说明: 获取本地GetSQLitePath路径 */
    public static string GetSQLitePath(string fileName)
    {
        string path = "";

#if UNITY_ANDROID && (!UNITY_EDITOR)
        path = Application.dataPath + "!/assets";
#elif UNITY_IPHONE && (!UNITY_EDITOR)
        path = Application.streamingAssetsPath;
#else
        path = Application.dataPath + "/StreamingAssets";
#endif

        if (fileName[0] != '/')
        {
            fileName = "/" + fileName;
        }
        path += fileName;

        return path;
    }

    /* 函数说明: 获取下载文件的本地路径 */
    public static string GetDownloadPath(string fileName)
    {
        string path = "";
        if(fileName == null || fileName.Trim() == "")
            fileName = "/";
        if (fileName[0] != '/')
        {
            fileName = "/" + fileName;
        }
        path = Application.persistentDataPath + "/" + fileName;
        Debug.Log("GetDownloadPath " + path);
        return path;
    }

    /* 函数说明: 创建文件目录 */
    /* 注意事项: 手机环境下只能创建下载目录的子目录，Resources/StreamAssets目录无效 */
    public static bool MakeDirectory(string szPath)
    {
        if (string.IsNullOrEmpty(szPath))
            return false;

        try
        {
            szPath = Path.GetDirectoryName(szPath);
            if (!System.IO.Directory.Exists(szPath))
            {
                Directory.CreateDirectory(szPath);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("can not make directory from path: " + szPath);
            return false;
        }

        return true;
    }

    /* 函数说明: 获取文件的MD5码 */
    /* 注意事项: 手机环境下只能读取下载目录的文件，Resources/StreamAssets目录无效 */
    public static string GetFileMD5(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return "";
        }

        var md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
        try
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hashValue = md5Hasher.ComputeHash(fileStream);
            fileStream.Close();

            string strHashData = System.BitConverter.ToString(hashValue);
            strHashData = strHashData.Replace("-", "");
            return strHashData;
        }
        catch (System.Exception)
        {
            Debuger.LogError("get file md5 failed !");
            return "";
        }
    }
        

    /* 函数说明: 安装APK */
    /* 注意事项: 手机环境下只能读取下载目录的文件，Resources/StreamAssets目录无效 */
    public static bool InstallApk(string filePath)
    {
#if UNITY_ANDROID
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (jc == null)
        {
            Debug.LogError("can not get com.unity3d.player.UnityPlayer !");
            return false;
        }
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        if (jo == null)
        {
            Debug.LogError("can not get currentActivity !");
            return false;
        }
        jo.Call("StartInstall", filePath);
#endif
        return true;
    }
}
