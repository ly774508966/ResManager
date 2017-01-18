using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Xml;
using System.Collections.Generic;

public class CreatePackage : EditorWindow{

    public GameObject ObjectToCopy = null;
    public System.IO.DirectoryInfo dir = null;
    public int numberOfCopies = 2;
    public string resourceMustPath = "";
    public string resourceOtherPath = "";
    public string iconPath = "";
    public string logoPath = "";
    public string sdkName = "";
    public string jarPath = "";
    public string appConfig = "";

    [MenuItem("Package/Show Create Package")]
    static void CreateWindow()
    {
        // Creates the window for display
        //创建显示向导
        //TestEditor.CreateWindow();
        CreatePackage editor = EditorWindow.GetWindow<CreatePackage>("打包编辑器");
    }

    public void OnGUI()
    {
        //GUILayout.BeginArea(new Rect(0, 0, 800, 600));
        GUILayout.BeginVertical();
        //公用渠道资源
        GUILayout.Label("选择渠道公用资源目录");
        GUILayout.BeginHorizontal();
        GUILayout.Label("公用：" + resourceMustPath);
        if (GUILayout.Button("打开",GUILayout.Width(100)))
        {
            Debug.LogError("选择渠道公用资源目录");
            resourceMustPath = EditorUtility.OpenFolderPanel("选择渠道公用资源目录", "", "");
        }
        GUILayout.EndHorizontal();

        //特定渠道资源
        GUILayout.Label("选择特定渠道资源目录");
        GUILayout.BeginHorizontal();
        GUILayout.Label("渠道：" + resourceOtherPath);
        if (GUILayout.Button("打开", GUILayout.Width(100)))
        {
            Debug.LogError("选择特定渠道资源目录");
            resourceOtherPath = EditorUtility.OpenFolderPanel("选择特定渠道资源目录", "", "");
        }
        GUILayout.EndHorizontal();

        //icon
        GUILayout.Label("选择渠道Icon");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Icon：" + iconPath);
        if (GUILayout.Button("打开", GUILayout.Width(100)))
        {
            Debug.LogError("选择渠道Icon");
            iconPath = EditorUtility.OpenFilePanel("选择Icon", "", "");
        }
        GUILayout.EndHorizontal();

        //Logo
        GUILayout.Label("选择渠道Logo");
        GUILayout.BeginHorizontal();
        GUILayout.Label("渠道：" + logoPath);
        if (GUILayout.Button("打开", GUILayout.Width(100)))
        {
            Debug.LogError("选择渠道Logo");
            logoPath = EditorUtility.OpenFilePanel("选择Logo", "", "");
        }
        GUILayout.EndHorizontal();

        //jar
        GUILayout.Label("选择渠道Jar");
        GUILayout.BeginHorizontal();
        GUILayout.Label("渠道Jar：" + jarPath);
        if (GUILayout.Button("打开", GUILayout.Width(100)))
        {
            Debug.LogError("选择渠道Jar");
            jarPath = EditorUtility.OpenFilePanel("选择Jar", "", "");
        }
        GUILayout.EndHorizontal();

        //渠道名称
        //GUILayout.Label("选择渠道Icon");
        GUILayout.Label("渠道名称：" + sdkName);
        GUILayout.BeginHorizontal();
        sdkName = GUILayout.TextField(sdkName);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        //开始打包
        if (GUILayout.Button("创建资源", GUILayout.Width(100)))
        {
            CreateResources();
        }
        // GUILayout.EndArea();
    }
    //创建打包资源文件，
    public void CreateResources()
    {
        Debug.LogError("CreateResources");
        //
        if (string.IsNullOrEmpty(resourceMustPath) || string.IsNullOrEmpty(resourceOtherPath) || string.IsNullOrEmpty(iconPath) || string.IsNullOrEmpty(logoPath) || string.IsNullOrEmpty(sdkName) || string.IsNullOrEmpty(jarPath))
        {
            Debug.LogError("数据不合法，请检查数据");
            //return;
        }
        string resourcePath = Application.dataPath + "/../OtherSdk/" + sdkName.ToLower() + "/Android";
        //复制公用打包资源，
        if (Directory.Exists(resourcePath) == false)
        {
            Directory.CreateDirectory(resourcePath);
        }
        CopyDirectory(resourceMustPath, resourcePath);
        //////复制特定打包资源，
        CopyDirectory(resourceOtherPath, resourcePath);
        ////////复制icon，
        CopyFile(iconPath, resourcePath + "/icon.png");
        ////////复制logo，
        CopyFile(logoPath, resourcePath + "/logo.png");
        ////////复制jar，
        CopyFile(jarPath, resourcePath + "/projectmoyo.jar");
        //解析xml
        ParseXML();
        //检查appname
        CheckAppName();
    }

    public void ParseXML()
    {
        string otherXMLPath = Application.dataPath + "/../OtherSdk/" + sdkName.ToLower() + "/Android/AndroidManifest.xml";
        string defaultXMLPath = Application.dataPath + "/../OtherSdk/default/Android/AndroidManifest.xml";
        string destXMLPath = Application.dataPath + "/../OtherSdk/" + sdkName.ToLower() + "/Android/AndroidManifest_other.xml";

        //////复制defaultXMLPath，
        CopyFile(defaultXMLPath,destXMLPath);

        XmlDocument otherXmlDoc = new XmlDocument();
        otherXmlDoc.Load(otherXMLPath);

        XmlDocument destXmlDoc = new XmlDocument();
        destXmlDoc.Load(destXMLPath);

        //修改package name,
        XmlNode otherManifestNode = otherXmlDoc.SelectSingleNode("/manifest");
        XmlNode destManifestNode = destXmlDoc.SelectSingleNode("/manifest");

        XmlAttribute otherPackageNodeAttr = otherManifestNode.Attributes["package"];
        XmlAttribute destPackageNodeAttr = destManifestNode.Attributes["package"];
        destPackageNodeAttr.Value = otherPackageNodeAttr.Value;

        XmlNode otherApp = otherXmlDoc.SelectSingleNode("/manifest/application");
        XmlNode destApp = destXmlDoc.SelectSingleNode("/manifest/application");
        Debug.LogError(otherApp.Attributes.Count);

        //判断是否有属性，android:name，有的话，就修改目标文件，和配置文件，
        if (otherApp.Attributes["android:name"] != null)
        {
            XmlElement xmlelement = (XmlElement)destApp;
            destApp.Attributes["android:name"].Value = otherApp.Attributes["android:name"].Value;
            appConfig += "isInitOnCreate:1\n";
        }
        else
        {
            appConfig += "isInitOnCreate:0\n";
        }
        appConfig += "channelstr:" + sdkName + "\n";

        StreamWriter sw = new StreamWriter(Application.dataPath + "/../OtherSdk/" + sdkName.ToLower() + "/Android/assets/appconfig",false);
        sw.Write(appConfig);
        sw.Close();
        //修改uses-permission,
        XmlNodeList otherPermissionNodeList = otherXmlDoc.SelectNodes("/manifest/uses-permission");
        foreach (XmlNode node in otherPermissionNodeList)
        {
            XmlNode curNode = destXmlDoc.ImportNode(node, true);
            destManifestNode.AppendChild(curNode);
        }

        //修改app,
        XmlNodeList otherAddNodeList = otherApp.ChildNodes;
        foreach (XmlNode node in otherAddNodeList)
        {
            XmlNode curNode = destXmlDoc.ImportNode(node, true);
            destApp.AppendChild(curNode);
        }
        destXmlDoc.Save(otherXMLPath);
        AddPackageConfigData(sdkName,sdkName,"战神王座",destPackageNodeAttr.Value);
    }

    public void CheckAppName()
    {
        string defaultXMLPath = Application.dataPath + "/../OtherSdk/debug_wan/Android/res/values/strings.xml";
        string destXMLPath = Application.dataPath + "/../OtherSdk/" + sdkName.ToLower() + "/Android/res/values/strings.xml";

        if (File.Exists(destXMLPath) == false)
        {
            //////复制defaultXMLPath，
            CopyFile(defaultXMLPath, destXMLPath);
            return;
        }

        XmlDocument destXmlDoc = new XmlDocument();
        destXmlDoc.Load(destXMLPath);

        XmlNode destResourceNode = destXmlDoc.SelectSingleNode("/resources");

        XmlNodeList nodeList = destResourceNode.SelectNodes("string");

        foreach (XmlNode node in nodeList)
        {
            if (node.Attributes["name"].Value == "app_name")
            {
                node.InnerText = "app_name";
                destXmlDoc.Save(destXMLPath);
                return;
            }
        }
        XmlElement elem = destXmlDoc.CreateElement("string");
        elem.SetAttribute("name","app_name");
        elem.InnerText = "app_name";
        destResourceNode.InsertBefore(elem, destResourceNode.FirstChild);
        destXmlDoc.Save(destXMLPath);
    }

    public static void CopyDirectory(string sourcePath, string destinationPath)
    {
        if (Directory.Exists(sourcePath) == false)
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

    //复制制定文件，
    public static void CopyFile(string sourcePath, string destinationPath)
    {
        try
        {
            if (File.Exists(sourcePath) == false)
                return;
            string destdir = destinationPath.Substring(0, destinationPath.LastIndexOf("/"));
            if (Directory.Exists(destdir) == false)
                Directory.CreateDirectory(destdir);
            FileInfo info = new FileInfo(sourcePath);
            File.Copy(info.FullName, destinationPath, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    //add package data,
    public static void AddPackageConfigData(string tag, string desc, string appname, string identifier)
    {
        //add package data,
        string packagePath = Application.dataPath + "/Editor/WLPlugIns/package.xml";

        XmlDocument packageDoc = new XmlDocument();
        packageDoc.Load(packagePath);

        XmlNode packageListNode = packageDoc.SelectSingleNode("/PackageList/PackageAndroidList");
        XmlNodeList androidPacakages = packageDoc.SelectNodes("/PackageList/PackageAndroidList/package");
        int iId = androidPacakages.Count;

        XmlElement packageNode = packageDoc.CreateElement("package");
        packageNode.SetAttribute("id",iId + "");
        packageNode.SetAttribute("type","1");
        packageNode.SetAttribute("tag", tag);
        packageNode.SetAttribute("desc", desc);
        packageNode.SetAttribute("app",appname);
        packageNode.SetAttribute("identifier",identifier);
        packageListNode.InsertAfter(packageNode,packageListNode.LastChild);
        packageDoc.Save(packagePath);
    }
}
