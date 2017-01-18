using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using WLGame;


/****************************
 ******** HTTP下载者 ********
 ****************************/

public class HttpDownloader : MonoBehaviour
{
    private class HttpTask
    {
        public string fileName;
        public HttpFileRequest request;
        public Action<float, string> onProgress;
        public Action<bool, string> onFinish;
    }
    private Dictionary<string, HttpTask> m_httpTaskDict = new Dictionary<string, HttpTask>();
    private List<HttpTask> m_taskQueue = new List<HttpTask>();

    private static HttpDownloader m_instance = null;
    public static HttpDownloader Instance()
    {
        if (m_instance == null)
        {
            m_instance = (new GameObject("HttpDownloader")).AddComponent<HttpDownloader>();
            GameObject.DontDestroyOnLoad(m_instance.gameObject);
        }
        return m_instance;
    }

    /* 函数说明: 获取已下载完毕的文件的完整路径，若存在则返回true， 否则返回false */
    public static bool GetDownloadedFile(string fileName, out string fullPath)
    {
        return HttpFileRequest.GetDownloadedFile(fileName, out fullPath);
    }

    /* 函数说明: 立即下载文件 */
    public void Download(string fileName, Action<float, string> onProgress, Action<bool, string> onFinish)
    {
        AddNewHttpTask(fileName, onProgress, onFinish);
    }

    /* 函数说明: 以队列形式下载，一次性添加多个文件 */
    public void QueueDownload(ref List<string> fileList, Action<float, string> onProgress, Action<bool, string> onFinish)
    {
        for (int i = 0; i < fileList.Count; i++)
        {
            QueueDownload(fileList[i], onProgress, onFinish);
        }
    }

    /* 函数说明: 以队列形式下载 */
    public void QueueDownload(string fileName, Action<float, string> onProgress, Action<bool, string> onFinish)
    {
        if (m_taskQueue.Count > 0)
        {
            HttpTask task = new HttpTask();
            task.onProgress = onProgress;
            task.onFinish = onFinish;
            task.fileName = fileName;
            task.request = null;
            m_taskQueue.Add(task);
            return;
        }

        HttpTask queueHead = AddNewHttpTask(fileName, onProgress, onFinish);
        m_taskQueue.Add(queueHead);
    }

    /*-------------------- 以下是内部函数 --------------------*/

    /* 函数说明: 下载文件 */
    HttpTask AddNewHttpTask(string fileName, Action<float, string> onProgress, Action<bool, string> onFinish)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            if (onFinish != null)
            {
                onFinish(false, "");
            }
            Debuger.LogError("Download Failed : fileName is null");
            return null;
        }

        // 如果未找到下载请求，则创建新的下载请求
        HttpTask task = null;
        if (!m_httpTaskDict.TryGetValue(fileName, out task))
        {
            task = new HttpTask();
            task.onProgress = onProgress;
            task.onFinish = onFinish;
            task.fileName = fileName;
            task.request = new HttpFileRequest();                
            m_httpTaskDict[fileName] = task;
            StartCoroutine(task.request.StartRequest(fileName));
            return task;
        }

        // 如果之前的下载失败， 则重新创建新的下载请求
        if (task.request.status == HttpFileRequest.Status.Failed)
        {
            task.request = new HttpFileRequest();
            task.onProgress += onProgress;
            task.onFinish += onFinish;
            StartCoroutine(task.request.StartRequest(fileName));
            return task;
        }

        //  如果下载已经完成， 但文件不存在，则重新创下载
        if (task.request.status == HttpFileRequest.Status.Finish)
        {
            if (!File.Exists(task.request.filePath))
            {
                task.onProgress += onProgress;
                task.onFinish += onFinish;
                StartCoroutine(task.request.StartRequest(fileName));
                return task;
            }
        }

        // 正在下载中或已经下载完成，则直接添加回调
        task.onProgress += onProgress;
        task.onFinish += onFinish;
        return task;
    }

    void Update()
    {
        foreach (KeyValuePair<string, HttpTask> pair in m_httpTaskDict)
        {
            HttpTask task = pair.Value;

            if (task.request.status == HttpFileRequest.Status.Downing)
            {
                if (task.onProgress != null)
                {
                    float progress = task.request.GetProgress();
                    task.onProgress(progress, task.request.filePath);
                }
            }
            else if (task.request.status == HttpFileRequest.Status.Failed)
            {
                if (task.onFinish != null)
                {
                    Action<bool, string> callback = task.onFinish;
                    task.onFinish = null;
                    callback(false, "");                        
                }
            }
            else if (task.request.status == HttpFileRequest.Status.Finish)
            {
                if (task.onFinish != null)
                {
                    Action<bool, string> callback = task.onFinish;
                    task.onFinish = null;
                    callback(true, task.request.filePath);
                }
            }
        }

        UpdateQueueTask();
    }

    void UpdateQueueTask()
    {
        if (m_taskQueue.Count == 0)
            return;

        if (m_taskQueue[0].request.status != HttpFileRequest.Status.Finish && 
            m_taskQueue[0].request.status != HttpFileRequest.Status.Failed)
            return;

        m_taskQueue.RemoveAt(0);
        if (m_taskQueue.Count == 0)
            return;

        m_taskQueue[0] = AddNewHttpTask(m_taskQueue[0].fileName, m_taskQueue[0].onProgress, m_taskQueue[0].onFinish);
    }
}

public class HttpFileRequest
{
    public enum Status { None, Downing, Finish, Failed };

    private string m_fileName = "";
    private string m_filePath = "";
    private WWW m_www = null;
    private FileStream m_fileStream = null;
    private Status m_status = Status.None;

    /* 函数说明: 获取文件名(含相对路径) */
    public string fileName
    {
        get { return m_fileName; }
    }

    /* 函数说明: 获取文件绝对路径 */
    public string filePath
    {
        get { return m_filePath; }
    }

    /* 函数说明: 获取当前状态 */
    public Status status
    {
        get { return m_status; }
    }

    /* 函数说明: 获取当前进度 */
    public float GetProgress()
    {
        if (m_status == Status.Finish)
        {
            return 1.0f;
        }
        if (m_www != null)
        {
            return m_www.progress;
        }
        return 0;
    }

    /* 函数说明: 开始一个下载请求 */
    public IEnumerator StartRequest(string fileName)
    {
        m_fileName = fileName;

        // 判断文件是否已存在
        if (GetDownloadedFile(m_fileName, out m_filePath))
        {
            m_status = Status.Finish;
            Close();
            yield break;
        }

        // 创建本地下载目录
        if (!ResUtils.MakeDirectory(m_filePath))
        {
            m_status = Status.Failed;
            Close();

            Debug.LogError(string.Format("HttpDownloader : MakeDirectory '{0}' Error !", m_filePath));
            yield break;
        }

        string url = ResUpdater.Instance().GetDownloadUrl(m_fileName);
        if (string.IsNullOrEmpty(url))
        {
            m_status = Status.Failed;
            Close();

            Debug.LogError("HttpDownloader : download url is null !");
            yield break;
        }

        m_status = Status.Downing;
        m_www = new WWW(url);
        yield return m_www;

        // 如果错误， 则返回
        if (!string.IsNullOrEmpty(m_www.error))
        {
            m_status = Status.Failed;
            yield break;
        }

        // 写文件
        WriteFile(m_filePath, m_www.bytes);
    }

    /* 函数说明: 关闭下载请求 */
    public void Close()
    {
        if (m_www != null)
        {
            m_www.Dispose();
            m_www = null;
        }
        if (m_fileStream != null)
        {
            m_fileStream.Flush();
            m_fileStream.Close();
            m_fileStream = null;
        }
    }

    public static bool GetDownloadedFile(string fileName, out string fullPath)
    {
        Debug.LogError("Start GetDownloadedFile " + fileName);
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("if (string.IsNullOrEmpty(fileName))");
            fullPath = "";
            return false;
        }

        fullPath = ResUtils.GetDownloadPath(fileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("if (!File.Exists(fullPath)) fullPath :" + fullPath);
            return false;
        }

        // 匹配文件md5
        Dictionary<string, string> serverFilesMD5 = ResUpdater.Instance().serverFilesMD5;
        string fileMD5;
        if (serverFilesMD5.TryGetValue(fileName, out fileMD5))
        {
            Debug.LogError("if (serverFilesMD5.TryGetValue(fileName, out fileMD5)) fileMD5 " + fileMD5);
            if (string.IsNullOrEmpty(fileMD5))
                return false;

            string localFileMD5 = ResUtils.GetFileMD5(fullPath);
            Debug.LogError(" if (string.IsNullOrEmpty(fileMD5)) fileMD5 " + fileMD5 + " localFileMD5 " + localFileMD5);
            return string.Compare(fileMD5, localFileMD5, true) == 0;
        }

        Debug.LogError("Stop GetDownloadedFile");
        return false;
    }

    /*-------------------- 以下是内部函数 --------------------*/

    ~HttpFileRequest()
    {
        Close();
    }

    bool WriteFile(string filePath, byte[] bytes)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("HttpFileRequest : cant not write null bytes !");
            return false;
        }

        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("HttpFileRequest : cant not write null bytes !");
            return false;
        }

        try
        {
            m_fileStream = new FileStream(filePath, FileMode.Create);
            m_fileStream.Write(bytes, 0, bytes.Length);
            Close();

            m_status = Status.Finish;
            return true;
        }
        catch (Exception)
        {
            Debug.LogError("HttpFileRequest : write file failed !");
            m_status = Status.Failed;
            return false;
        }
    }
}
