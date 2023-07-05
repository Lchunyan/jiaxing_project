using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using UnityEngine.Networking;
using Ximmerse.XR.Asyncoroutine;
using System.Linq;


namespace Ximmerse
{
    /// <summary>
	/// PEUtilsIO.cs - 和I/O相关的方法在这里。
	/// </summary>
	public static partial class PEUtils
    {
        private const string kFileProtocol = "file://";

        private const string kSearchPatternAllFile = "*.*";

        private const string kSearchPatternAll = "*";

        /// <summary>
        /// 从 Directory 中删除所有的子文件和子目录.
        /// </summary>
        /// <param name="Folder">根目录</param>
        /// <param name="printDebugLogs">是否打印详细的删除记录.</param>
        /// <param name="SearchPatternOnFile"></param>
        /// <param name="searchOptionOnFile"></param>
        /// <param name="SearchPatternOnDirectory"></param>
        /// <param name="searchOptionOnDirectory"></param>
        public static void DeleteAllSubFilesAndFolders(string Folder,
            bool printDebugLogs = false,
            string SearchPatternOnFile = kSearchPatternAllFile,
            SearchOption searchOptionOnFile = SearchOption.AllDirectories,
            string SearchPatternOnDirectory = kSearchPatternAll,
            SearchOption searchOptionOnDirectory = SearchOption.AllDirectories)
        {
            var files = Directory.EnumerateFiles(Folder, SearchPatternOnFile, searchOptionOnFile);
            foreach (var file in files)
            {
                File.Delete(file);
                if (printDebugLogs)
                {
                    Debug.LogFormat("Delete file: {0}", file);
                }
            }

            var subFolders = Directory.EnumerateDirectories(Folder, SearchPatternOnDirectory, searchOptionOnDirectory);
            foreach (var folder in subFolders)
            {
                Directory.Delete(folder);
                if (printDebugLogs)
                {
                    Debug.LogFormat("Delete sub-folder: {0}", folder);
                }
            }
        }

        /// <summary>
        /// 从 目录中删除所有符合格式的文件。
        /// </summary>
        /// <param name="Folder"></param>
        /// <param name="searchPatternOnFile"></param>
        /// <param name="searchOptionOnFile"></param>
        /// <returns>删除的个数。</returns>
        public static int DeleteFilesInFolder(string Folder, string searchPatternOnFile = kSearchPatternAllFile, SearchOption searchOptionOnFile = SearchOption.AllDirectories)
        {
            int deleted = 0;
            var files = Directory.EnumerateFiles(Folder, searchPatternOnFile, searchOptionOnFile);
            foreach (var file in files)
            {
                File.Delete(file);
                deleted++;
            }
            return deleted;
        }

        /// <summary>
        /// 删除文件夹.
        /// 此方法会确保文件夹被删除。
        /// </summary>
        /// <param name="Folder"></param>
        public static void DeleteDirectory(string Folder)
        {
            DeleteFilesInFolder(Folder);//先删除全部的文件
            //再逐一删除子文件夹:
            deleteDirectoryLoop(Folder);
        }

        /// <summary>
        /// 递归删除folder下的全部目录。
        /// folder下必须不包含任何子文件。只有文件夹。
        /// </summary>
        /// <param name="folder"></param>
        private static void deleteDirectoryLoop(string folder)
        {
            var subDirectories = Directory.EnumerateDirectories(folder, kSearchPatternAll, SearchOption.TopDirectoryOnly);
            foreach (var subDir in subDirectories)
            {
                deleteDirectoryLoop(subDir);
            }
            Directory.Delete(folder);
        }

        /// <summary>
        /// 从 Folder 中删除不包含子文件的空目录。
        /// </summary>
        /// <param name="Folder"></param>
        public static void DeleteEmptySubFolders(string Folder)
        {
            var subFolders = Directory.EnumerateDirectories(Folder, kSearchPatternAll, SearchOption.AllDirectories);
            foreach (var folder in subFolders)
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }
                var subfiles = Directory.EnumerateFiles(folder, kSearchPatternAll, SearchOption.AllDirectories);
                bool isEmpty = subfiles.Count() == 0;
                if (isEmpty)
                {
                    Directory.Delete(folder);
                }
            }
        }

        /// <summary>
        /// read bytes from streaming assets, at the relative path to streaming assets.
        /// The method handle platform specifically.
        /// </summary>
        /// <param name="relativePathToStreamingAsset"></param>
        /// <param name="bytes"></param>
        public static async Task<int> ReadFromStreamingAssets(string relativePathToStreamingAsset, List<byte> bytes)
        {
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        var asyncOp = req.SendWebRequest();
                        await asyncOp;
                        int downloadCount = (int)req.downloadedBytes;
                        if (req.downloadHandler != null && downloadCount > 0)
                        {
                            bytes.AddRange(req.downloadHandler.data);
                        }
                        return downloadCount;
                    }
                }
                else
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    bytes.AddRange(buffer);
                    int readCount = buffer.Length;
                    return readCount;
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return 0;
            }

        }


        /// <summary>
        /// Read UTF-8 text from streaming assets path.
        /// </summary>
        /// <param name="relativePathToStreamingAsset"></param>
        /// <returns></returns>
        public static async Task<string> ReadFromStreamingAssets(string relativePathToStreamingAsset)
        {
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        var asyncOp = req.SendWebRequest();
                        await asyncOp;
                        int downloadCount = (int)req.downloadedBytes;
                        string text = string.Empty;
                        if (req.downloadHandler != null && downloadCount > 0)
                        {
                            text = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
                        }
                        return text;
                    }
                }
                else
                {
                    string text = File.ReadAllText(filePath);
                    return text;
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return string.Empty;
            }
        }

        /// <summary>
        /// Read from SA file and copy to dest file path
        /// </summary>
        /// <param name="StreamingAssetRelativePath">Relative file path to SA</param>
        /// <param name="DestFileFullPath">destination file's full path</param>
        /// <returns></returns>
        public static async Task<bool> CopyStreamingAssetFile(string StreamingAssetRelativePath, string DestFileFullPath)
        {
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetRelativePath);
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        var asyncOp = req.SendWebRequest();
                        await asyncOp;
                        int downloadCount = (int)req.downloadedBytes;
                        if (req.downloadHandler != null && downloadCount > 0)
                        {
                            File.WriteAllBytes(DestFileFullPath, req.downloadHandler.data);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    File.WriteAllBytes(DestFileFullPath, bytes);
                    return true;
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return false;
            }
        }


        /// <summary>
        /// Reads a json object from streaming asset folder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relativePathToStreamingAsset"></param>
        /// <returns></returns>
        public static async Task<T> ReadJsonObjectFromStreamingAssets<T>(string relativePathToStreamingAsset)
        {
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        var asyncOp = req.SendWebRequest();
                        await asyncOp;
                        int downloadCount = (int)req.downloadedBytes;
                        string text = string.Empty;
                        if (req.downloadHandler != null && downloadCount > 0)
                        {
                            text = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
                        }
                        return JsonUtility.FromJson<T>(text);
                    }
                }
                else
                {
                    string text = File.ReadAllText(filePath);
                    return JsonUtility.FromJson<T>(text);
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return default(T);
            }
        }
        /// <summary>
        /// Synchronously reads a json object from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullFilePath">File's full path</param>
        /// <returns></returns>
        public static T ReadJsonObjectFromFile<T>(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                return default(T);
            }
            var s = File.ReadAllText(fullFilePath);
            return JsonUtility.FromJson<T>(s);
        }

        /// <summary>
        /// Asynchronously reads a json object from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullFilePath">File's full path</param>
        /// <returns></returns>
        public static async Task<T> ReadJsonObjectFromFileAsync<T>(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                return default(T);
            }
            StreamReader reader = new StreamReader(File.Open(fullFilePath, FileMode.Open));
            var t = reader.ReadToEndAsync();
            while (!t.IsCompleted)
            {
                await new WaitForNextFrame();
            }
            return JsonUtility.FromJson<T>(t.Result);
        }

        /// <summary>
        /// Read bytes from streaming folder file
        /// </summary>
        /// <param name="relativePathToStreamingAsset"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadBytesFromStreamingAssets(string relativePathToStreamingAsset)
        {
            try
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);
                if (Application.platform == RuntimePlatform.Android)
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        var asyncOp = req.SendWebRequest();
                        await asyncOp;
                        int downloadCount = (int)req.downloadedBytes;
                        string text = string.Empty;
                        if (req.downloadHandler != null && downloadCount > 0)
                        {
                            return req.downloadHandler.data;
                        }
                    }
                }
                else
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    return buffer;
                }

                return null;
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
        }



        /// <summary>
        /// Read texture2d from streaming folder file
        /// </summary>
        /// <param name="relativePathToStreamingAsset"></param>
        /// <returns></returns>
        public static async Task<Texture2D> ReadTexture2DFromStreamingAssets(string relativePathToStreamingAsset)
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    string filePath = Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);//jar://
                    var req = UnityWebRequestTexture.GetTexture(filePath);
                    var asyncOp = req.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await new WaitForNextFrame();
                    }
                    return DownloadHandlerTexture.GetContent(req);
                }
                else
                {
                    string filePath = kFileProtocol + Path.Combine(Application.streamingAssetsPath, relativePathToStreamingAsset);
                    //Debug.Log(filePath);
                    var req = UnityWebRequestTexture.GetTexture(filePath);
                    var asyncOp = req.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await new WaitForNextFrame();
                    }
                    return DownloadHandlerTexture.GetContent(req);
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
        }


        /// <summary>
        /// Read texture2d from local file path
        /// </summary>
        /// <param name="localPath">Such as C:\\Image\Logo.png</param>
        /// <returns></returns>
        public static async Task<Texture2D> ReadTexture2DFromLocalPath(string localPath)
        {
            try
            {
                string filePath = kFileProtocol + localPath;
                //Debug.Log(filePath);
                var req = UnityWebRequestTexture.GetTexture(filePath);
                var asyncOp = req.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await new WaitForNextFrame();
                }
                return DownloadHandlerTexture.GetContent(req);
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
        }

        /// <summary>
        /// Read texture2d from URL
        /// </summary>
        /// <param name="url">Such as C:\\Image\Logo.png</param>
        /// <returns></returns>
        public static async Task<Texture2D> ReadTexture2DFromUrl(string url)
        {
            try
            {
                //Debug.Log(filePath);
                var req = UnityWebRequestTexture.GetTexture(url);
                var asyncOp = req.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await new WaitForNextFrame();
                }
                return DownloadHandlerTexture.GetContent(req);
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
        }

        /// <summary>
        /// 将对象以Json的格式输出到文件系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="FileFullPath">Json file full path</param>
        public static void WriteObjectToJsonFile<T>(T obj, string FileFullPath, bool prettyJson = false)
        {
            CreateFileDirectory(FileFullPath);
            string json = JsonUtility.ToJson(obj, prettyJson);
            File.WriteAllText(FileFullPath, json);
        }

        /// <summary>
        /// 异步的形式写出数据。
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="offset"
        /// <param name="append">If true, appends to file if file exists.</param>
        /// <returns></returns>
        public static async Task WriteBufferToFile(string fileFullPath, byte[] buffer, int offset, int length, bool append = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            CreateFileDirectory(fileFullPath);
            //Writes download asset bundle content and bundle.info to local caching directory:
            FileMode fileMode = append ? FileMode.OpenOrCreate | FileMode.Append : FileMode.OpenOrCreate;
            var fileStream = File.Open(fileFullPath, fileMode);
            var task_write = fileStream.WriteAsync(buffer, 0, length, cancellationToken);
            while (!task_write.IsCompleted)
            {
                await new WaitForNextFrame();
            }
            fileStream.Close();
        }

        /// <summary>
        /// 将 Source folder 下的, 满足 [searchOption] 条件的所有文件，移动到 destFolder中， 文件名不变。
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        /// <param name="overwrite">是否覆盖文件?</param>
        /// <param name="searchPattern">默认为*.*， 代表移动全部的文件。 可以为如下多匹配格式: *.mp3|*.txt|*.json</param>
        /// <returns>The moved file count.</returns>
        public static int MoveFilesInFolder(string sourceFolder, string destFolder, bool overwrite = true, string searchPattern = kSearchPatternAll, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            var files = Directory.EnumerateFiles(sourceFolder, searchPattern, searchOption);
            int ret = 0;
            foreach (var filePath in files)
            {
                string newFilePath = filePath.Replace(sourceFolder, destFolder);
                string newFileDir = Directory.GetParent(newFilePath).FullName;
                if (!Directory.Exists(newFileDir))
                {
                    Directory.CreateDirectory(newFileDir);
                }
                if (File.Exists(newFilePath))
                {
                    if (overwrite)
                    {
                        File.Delete(newFilePath);
                    }
                    else
                    {
                        continue;//不覆盖，跳过.
                    }
                }
                File.Move(filePath, newFilePath);
                ret++;
            }
            return ret;
        }



        /// <summary>
        /// 将 Source folder 下的, 满足 [searchOption] 条件的所有文件，Copy到 destFolder中， 文件名不变。
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        /// <param name="overwrite">是否覆盖文件?</param>
        /// <param name="searchPattern">默认为*.*， 代表移动全部的文件。 可以为如下多匹配格式: *.mp3|*.txt|*.json</param>
        /// <returns>The moved file count.</returns>
        public static int CopyFilesInFolder(string sourceFolder, string destFolder, bool overwrite = true, string searchPattern = kSearchPatternAll, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            var files = Directory.EnumerateFiles(sourceFolder, searchPattern, searchOption);
            int ret = 0;
            foreach (var filePath in files)
            {
                string newFilePath = filePath.Replace(sourceFolder, destFolder);
                string newFileDir = Directory.GetParent(newFilePath).FullName;
                if (!Directory.Exists(newFileDir))
                {
                    Directory.CreateDirectory(newFileDir);
                }
                File.Copy(filePath, newFilePath, overwrite);
                ret++;
            }
            return ret;
        }
    }
}
