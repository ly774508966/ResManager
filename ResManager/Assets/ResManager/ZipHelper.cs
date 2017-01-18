using UnityEngine;
using System;
using System.IO;
using System.Collections;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;


public class ZipHelper
{
    /// <summary>
    /// 压缩单个文件
    /// </summary>
    /// <param name="FileToZip">被压缩的文件名称(包含文件路径)</param>
    /// <param name="ZipedFile">压缩后的文件名称(包含文件路径)</param>
    /// <param name="CompressionLevel">压缩率0（无压缩）-9（压缩率最高）</param>
    public static void ZipFile(string FileToZip, string ZipedFile, int CompressionLevel)
    {
        //如果文件没有找到，则报错 
        if (!System.IO.File.Exists(FileToZip))
        {
            throw new System.IO.FileNotFoundException("文件：" + FileToZip + "没有找到！");
        }

        if (ZipedFile == string.Empty)
        {
            ZipedFile = Path.GetFileNameWithoutExtension(FileToZip) + ".zip";
        }

        if (Path.GetExtension(ZipedFile) != ".zip")
        {
            ZipedFile = ZipedFile + ".zip";
        }

        //如果指定位置目录不存在，创建该目录
        string zipedDir = ZipedFile.Substring(0, ZipedFile.LastIndexOf("\\"));
        if (!Directory.Exists(zipedDir))
        {
            Directory.CreateDirectory(zipedDir);
        }

        //被压缩文件名称
        string filename = FileToZip.Substring(FileToZip.LastIndexOf('\\') + 1);

        System.IO.FileStream StreamToZip = new System.IO.FileStream(FileToZip, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        System.IO.FileStream ZipFile = System.IO.File.Create(ZipedFile);
        ZipOutputStream ZipStream = new ZipOutputStream(ZipFile);
        ZipEntry ZipEntry = new ZipEntry(filename);
        ZipStream.PutNextEntry(ZipEntry);
        ZipStream.SetLevel(CompressionLevel);
        byte[] buffer = new byte[2048];
        System.Int32 size = StreamToZip.Read(buffer, 0, buffer.Length);
        ZipStream.Write(buffer, 0, size);
        try
        {
            while (size < StreamToZip.Length)
            {
                int sizeRead = StreamToZip.Read(buffer, 0, buffer.Length);
                ZipStream.Write(buffer, 0, sizeRead);
                size += sizeRead;
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
        finally
        {
            ZipStream.Finish();
            ZipStream.Close();
            StreamToZip.Close();
        }
    }

    /// <summary>
    /// 压缩文件夹的方法
    /// </summary>
    public static void ZipDir(string DirToZip, string ZipedFile, int CompressionLevel)
    {
        //压缩文件为空时默认与压缩文件夹同一级目录
        if (ZipedFile == string.Empty)
        {
            ZipedFile = DirToZip.Substring(DirToZip.LastIndexOf("\\") + 1);
            ZipedFile = DirToZip.Substring(0, DirToZip.LastIndexOf("\\")) + "\\" + ZipedFile + ".zip";
        }

        using (ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(ZipedFile)))
        {
            zipOutputStream.SetLevel(CompressionLevel);

            Crc32 crc = new Crc32();
            Hashtable fileList = GetAllFies(DirToZip);
            foreach (DictionaryEntry item in fileList)
            {
                FileStream fs = File.OpenRead(item.Key.ToString());
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                string name = item.Key.ToString();
                name = name.Substring(DirToZip.Length);
                ZipEntry entry = new ZipEntry(name);
                entry.DateTime = (DateTime)item.Value;
                entry.Size = fs.Length;
                fs.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                zipOutputStream.PutNextEntry(entry);
                zipOutputStream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    /// <summary>
    /// 功能：解压zip格式的文件。
    /// </summary>
    /// <param name="zipFilePath">压缩文件路径</param>
    /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>
    public static void UnZip(string zipFilePath, string unZipDir)
    {
        if (zipFilePath == string.Empty)
        {
            throw new Exception("压缩文件不能为空！");
        }
        if (!File.Exists(zipFilePath))
        {
            throw new System.IO.FileNotFoundException("压缩文件不存在！");
        }
        //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
        if (unZipDir == string.Empty)
            unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
        if (!unZipDir.EndsWith("\\"))
            unZipDir += "\\";
        if (!Directory.Exists(unZipDir))
            Directory.CreateDirectory(unZipDir);

        using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
        {
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(unZipDir + directoryName);
                }

                if (!directoryName.EndsWith("\\"))
                {
                    directoryName += "\\";
                }

                if (fileName != String.Empty)
                {
                    using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                    {
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public static void UnZip(byte[] buffer, string unZipDir)
    {
        MemoryStream ms = new MemoryStream(buffer);
        using (ZipInputStream s = new ZipInputStream(ms))
        {
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(unZipDir + directoryName);
                }

                if (!directoryName.EndsWith("\\"))
                {
                    directoryName += "\\";
                }

                if (fileName != String.Empty)
                {
                    using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                    {
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <returns></returns>
    private static Hashtable GetAllFies(string dir)
    {
        Hashtable FilesList = new Hashtable();
        DirectoryInfo fileDire = new DirectoryInfo(dir);
        if (!fileDire.Exists)
        {
            throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
        }

        FileInfo[] fileInfo = fileDire.GetFiles("*.*", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfo.Length; i++)
        {
            FileInfo file = fileInfo[i];
            if (!file.FullName.EndsWith(".meta"))
            {
                FilesList.Add(file.FullName, file.LastWriteTime);
            }
        }
        return FilesList;
    }
}