using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace WLGame
{
    public class InstallPkgInfo
    {
        public string content;
        public string version;
        public string url;
		public bool isForceUpdateVersion;
    }

    public class ResInfo
    {
        public string name;
        public string md5;
        public string url;
        public string version;
    }

    public class ResUpdater : MonoBehaviour
    {
        private InstallPkgInfo m_installPkgInfo = new InstallPkgInfo();
        private List<ResInfo> m_mustUpdateList = new List<ResInfo>();
        private List<ResInfo> m_chooseUpdateList = new List<ResInfo>();
        private Dictionary<string, string> m_urlDict = new Dictionary<string, string>();
        private Dictionary<string, string> m_serverFilesMD5 = new Dictionary<string, string>();
        private Dictionary<string, string> m_localFilesMD5 = new Dictionary<string, string>();

        private Action<bool> m_onQueryResUpdateCallback = null;
        private static ResUpdater m_instance = null;
        private int m_currQueryResCount = 0;
        private const uint MAX_QUERY_RES_COUNT = 3;

        /* 函数说明: 安装包更新信息 */
        public InstallPkgInfo installPkgInfo
        {
            get { return m_installPkgInfo; }
        }

        /* 函数说明: 强更资源列表 */
        public List<ResInfo> mustUpdateList
        {
            get { return m_mustUpdateList; }
        }

        /* 函数说明: 弱更资源列表 */
        public List<ResInfo> chooseUpdateList
        {
            get { return m_chooseUpdateList; }
        }

        /* 函数说明: 服务器文件md5字典 */
        public Dictionary<string, string> serverFilesMD5
        {
            get { return m_serverFilesMD5; }
        }

        /* 函数说明: 本地文件md5字典 */
        public Dictionary<string, string> localFilesMD5
        {
            get { return m_localFilesMD5; }
        }

        /* 函数说明: 获取唯一实例 */
        public static ResUpdater Instance()
        {
            if (m_instance == null)
            {
                m_instance = (new GameObject("ResUpdater")).AddComponent<ResUpdater>();
                GameObject.DontDestroyOnLoad(m_instance.gameObject);
                m_instance.ReadLocalAssetsMD5();
            }
            return m_instance;
        }

        public string GetDownloadUrl(string fileName)
        {
            string url;
            if (m_urlDict.TryGetValue(fileName, out url))
            {
                return url;
            }
            return "";
        }

        /* 函数说明: 查询服务器的资源列表 */
        public void QueryServerResList(Action<bool> callback)
        {
            Debug.Log("QueryServerResList");
            m_onQueryResUpdateCallback = callback;
            //ResManager.Instance.InitialVersions();

            //Http.Instance().AddField("version", ResUtils.Version);
            //Http.Instance().AddField("platform", ResUtils.PlatformName);
            //Http.Instance().AddField("channel", ResUtils.ChannelName);
            ////lijing
            //Http.Instance().AddField("luaVersion", ResUtils.LuaVersion);
            //Http.Instance().AddField("configVersion", ResUtils.ConfigVersion);

            //Http.Instance().Post(ResUtils.ResQueryUrl, OnQueryServerResList, false);
        }

        /*-------------------- 以下是内部函数 --------------------*/

        void ReadLocalAssetsMD5()
        {
            TextAsset textAsset = Resources.Load("AssetsMD5", typeof(TextAsset)) as TextAsset;
            if (textAsset == null)
            {
                Debuger.LogError("can't load AssetsMD5 xml file !");
                return;
            }
            MemoryStream memStream = new MemoryStream();
            memStream.Write(textAsset.bytes, 0, textAsset.bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(memStream);

            System.Xml.XmlNodeList xmlNodeList = xmlDoc.SelectNodes("/AssetsMD5/File");
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                System.Xml.XmlNode xmlNode = xmlNodeList[i];
                //string path = TextUtils.XmlReadString(xmlNode, "path", "");
                //string md5 = TextUtils.XmlReadString(xmlNode, "md5", "");
                //m_localFilesMD5[path] = md5;
            }

            memStream.Close();
            memStream = null;
        }

        void RepeatQueryWhenFaild()
        {
            QueryServerResList(m_onQueryResUpdateCallback);
        }

        // 响应查询资源列表
        void OnQueryServerResList(bool success, string text)
        {
            if (!success)
            {
                // 查询失败时，尝试多次查询
                if (++m_currQueryResCount < MAX_QUERY_RES_COUNT)
                {
                    Invoke("RepeatQueryWhenFaild", 1f);
                    return;
                }
                if (m_onQueryResUpdateCallback != null)
                {
                    m_onQueryResUpdateCallback(false);
                    m_onQueryResUpdateCallback = null;
                }
                return;
            }

            JsonData jsonRoot = JsonMapper.ToObject(text);
            if (jsonRoot == null)
            {
                if (m_onQueryResUpdateCallback != null)
                {
                    m_onQueryResUpdateCallback(false);
                    m_onQueryResUpdateCallback = null;
                }
                return;
            }

            // --- 解析错误码 ---
            try
            {
                int errcode = (int)jsonRoot["errcode"];
                if (errcode != 0)
                {
                    if (m_onQueryResUpdateCallback != null)
                    {
                        m_onQueryResUpdateCallback(false);
                        m_onQueryResUpdateCallback = null;
                    }
                    return;
                }
            }
            catch (Exception)
            {
                if (m_onQueryResUpdateCallback != null)
                {
                    m_onQueryResUpdateCallback(false);
                    m_onQueryResUpdateCallback = null;
                }
                return;
            }

            // --- 解析安装包信息（只有安卓环境下需要，IOS不允许动态更新整包） ---
#if APPSTORE
            Debug.Log("jsonRoot " + jsonRoot.ToJson());
            try
            {
                string strNewPackage = jsonRoot["newPackage"].ToJson();
                JsonData jsonData = JsonMapper.ToObject(strNewPackage);
                if (jsonData != null)
                {
                    m_installPkgInfo.content = TextUtils.JsonReadString(jsonData, "content");
                    m_installPkgInfo.content = m_installPkgInfo.content.Replace("\\n", "\n");
                    m_installPkgInfo.version = TextUtils.JsonReadString(jsonData, "version");
                    m_installPkgInfo.url = TextUtils.JsonReadString(jsonData, "url");
					m_installPkgInfo.isForceUpdateVersion = TextUtils.JsonReadString(jsonData, "force").Equals("1");
                    if (!string.IsNullOrEmpty(m_installPkgInfo.url))
                    {
                        m_urlDict[m_installPkgInfo.version] = m_installPkgInfo.url;
                    }
                }
            }
            catch (Exception) {}
#endif

            // --- 解析强更信息（即登录前需要下载的资源，如config、lua等） ---
            try
            {
                string strMustDownload = jsonRoot["mustDownload"].ToJson();
                JsonData items1 = JsonMapper.ToObject(strMustDownload);
                for (int i = 0; i < items1.Count; i++)
                {
                    JsonData item = items1[i];
                    if (item.IsObject == false)
                        continue;

                    //ResInfo resUpdate = new ResInfo();
                    //resUpdate.name = TextUtils.JsonReadString(item, "name");
                    //resUpdate.md5 = TextUtils.JsonReadString(item, "md5");
                    //resUpdate.url = TextUtils.JsonReadString(item, "downloadurl");
                    //if (!string.IsNullOrEmpty(resUpdate.name))
                    //{
                    //    m_mustUpdateList.Add(resUpdate);
                    //    m_urlDict[resUpdate.name] = resUpdate.url;
                    //    m_serverFilesMD5[resUpdate.name] = resUpdate.md5;
                    //}
                }
            }
            catch (Exception) {}

            // --- 解析弱更信息（即不需要在登录前就下载好的资源，如QTE视频、场景资源包等） ---
            try
            {
                string strChooseDownload = jsonRoot["chooseDownload"].ToJson();
                JsonData items2 = JsonMapper.ToObject(strChooseDownload);
                for (int i = 0; i < items2.Count; i++)
                {
                    JsonData item = items2[i];
                    if (item.IsObject == false)
                        continue;

                    //ResInfo resUpdate = new ResInfo();
                    //resUpdate.name = TextUtils.JsonReadString(item, "name");
                    //resUpdate.md5 = TextUtils.JsonReadString(item, "md5");
                    //resUpdate.url = TextUtils.JsonReadString(item, "downloadurl");
                    //if (!string.IsNullOrEmpty(resUpdate.name))
                    //{
                    //    m_chooseUpdateList.Add(resUpdate);
                    //    m_urlDict[resUpdate.name] = resUpdate.url;
                    //    m_serverFilesMD5[resUpdate.name] = resUpdate.md5;
                    //}
                }
            }
            catch (Exception) {}

            // 完成回调
            if (m_onQueryResUpdateCallback != null)
            {
                m_onQueryResUpdateCallback(true);
                m_onQueryResUpdateCallback = null;
            }
        }
    }
}
