using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

public class CommonUtil
{

    public static System.Text.Encoding encoding = new System.Text.UTF8Encoding(true);
    /// <summary>
    /// 分隔符，分割资源，代码，和配置用的
    /// </summary>
    public static string SplitCSVKey = "#-@&";

    public class CopyDirectoryAsyncOperation
    {
        public string srcDic = string.Empty;
        public string dstDic = string.Empty;

        public bool isDone = false;
    }


    public static void MoveFileHelper(string SrcPath, string DestPath)
    {
        if (!File.Exists(SrcPath))
        {
            return;
        }

        string folderPath = System.IO.Path.GetDirectoryName(DestPath);
        if (!folderPath.Equals(string.Empty) && !System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        DeleteFileHelper(DestPath);
        File.Move(SrcPath, DestPath);
    }

    public static void CopyFileHelper(string SrcPath, string DestPath)
    {
        if (!File.Exists(SrcPath))
        {
            return;
        }

        string folderPath = System.IO.Path.GetDirectoryName(DestPath);
        if (!folderPath.Equals(string.Empty) && !System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        File.Copy(SrcPath, DestPath, true);
    }

    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="NeedToClose"></param>
    /// <returns></returns>
    public static FileStream CreateFileHelper(string path, bool NeedToClose = true)
    {

        string folderPath = System.IO.Path.GetDirectoryName(path);
        if (!folderPath.Equals(string.Empty) && !System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        FileStream str = File.Create(path);
        if (NeedToClose)
        {
            str.Flush();
            str.Close();
            str = null;
        }
        return str;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="dealitFileName">名称</param>
    /// <param name="modifList"></param>
    /// <param name="deleteList"></param>
    public static void DeleteFileHelper(string detailFileName)
    {
        if (File.Exists(detailFileName))
        {
            File.Delete(detailFileName);
        }
    }


    public static bool Is64BitNativeDll(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            return false;
        }
        ushort architecture = 0;
        using (System.IO.FileStream fStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            using (System.IO.BinaryReader bReader = new System.IO.BinaryReader(fStream))
            {
                if (bReader.ReadUInt16() == 23117)
                {
                    fStream.Seek(0x3A, System.IO.SeekOrigin.Current);
                    fStream.Seek(bReader.ReadUInt32(), System.IO.SeekOrigin.Begin);
                    if (bReader.ReadUInt32() == 17744)
                    {
                        fStream.Seek(20, System.IO.SeekOrigin.Current);
                        architecture = bReader.ReadUInt16();
                    }
                }
            }
        }
        return architecture == 0x20b;
    }

    /// <summary>
    /// 将srcdir拷贝的desdir,不会创建源文件夹
    /// </summary>
    /// <param name="srcdir"></param>
    /// <param name="desdir"></param>
    /// <param name="excluKey"></param>
    public static void CopyDirectoryWithOutSrc(string srcdir, string desdir, string excluKey = "")
    {
        if (!CheckDirectory(srcdir))
        {
            return;
        }
        string folderName = srcdir.Substring(srcdir.LastIndexOf("/") + 1);

        string desfolderdir = desdir + "/";

        if (desdir.LastIndexOf("/") == (desdir.Length - 1))
        {
            desfolderdir = desdir + folderName;
        }
        string[] filenames = Directory.GetFileSystemEntries(srcdir);

        foreach (string file in filenames)// 遍历所有的文件和目录
        {
            string strfile = file.Replace("\\", "/");
            if (!string.IsNullOrEmpty(excluKey) && strfile.Contains(excluKey))
            {
                continue;
            }
            if (Directory.Exists(strfile))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
            {

                string currentdir = desfolderdir + "/" + strfile.Substring(strfile.LastIndexOf("/") + 1);
                if (!Directory.Exists(currentdir))
                {
                    Directory.CreateDirectory(currentdir);
                }

                CopyDirectory(strfile, desfolderdir);
            }

            else // 否则直接copy文件
            {
                string srcfileName = strfile.Substring(strfile.LastIndexOf("/") + 1);

                srcfileName = desfolderdir + "/" + srcfileName;


                if (!Directory.Exists(desfolderdir))
                {
                    Directory.CreateDirectory(desfolderdir);
                }


                File.Copy(strfile, srcfileName, true);
            }
        }
    }

    /// <summary>
    /// 将srcdir拷贝的desdir
    /// </summary>
    /// <param name="srcdir"></param>
    /// <param name="desdir"></param>
    /// <param name="excluKey"></param>
    public static void CopyDirectory(string srcdir, string desdir, string excluKey = "")
    {
        if (!CheckDirectory(srcdir))
        {
            return;
        }
        string folderName = srcdir.Substring(srcdir.LastIndexOf("/") + 1);

        string desfolderdir = desdir + "/" + folderName;

        if (desdir.LastIndexOf("/") == (desdir.Length - 1))
        {
            desfolderdir = desdir + folderName;
        }
        string[] filenames = Directory.GetFileSystemEntries(srcdir);

        foreach (string file in filenames)// 遍历所有的文件和目录
        {
            string strfile = file.Replace("\\", "/");
            if (!string.IsNullOrEmpty(excluKey) && strfile.Contains(excluKey))
            {
                continue;
            }
            if (Directory.Exists(strfile))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
            {

                string currentdir = desfolderdir + "/" + strfile.Substring(strfile.LastIndexOf("/") + 1);
                if (!Directory.Exists(currentdir))
                {
                    Directory.CreateDirectory(currentdir);
                }

                CopyDirectory(strfile, desfolderdir);
            }

            else // 否则直接copy文件
            {
                string srcfileName = strfile.Substring(strfile.LastIndexOf("/") + 1);

                srcfileName = desfolderdir + "/" + srcfileName;


                if (!Directory.Exists(desfolderdir))
                {
                    Directory.CreateDirectory(desfolderdir);
                }


                File.Copy(strfile, srcfileName, true);
            }
        }
    }

    /// <summary>
    /// 异步复制文件夹
    /// </summary>
    /// <param name="srcdir"></param>
    /// <param name="desdir"></param>
    /// <returns></returns>
    public static CopyDirectoryAsyncOperation CopyDirectoryAsync(string srcdir, string desdir)
    {
        CopyDirectoryAsyncOperation operation = new CopyDirectoryAsyncOperation();
        operation.srcDic = srcdir;
        operation.dstDic = desdir;

        ThreadPool.QueueUserWorkItem(CopyDirectoryFunc, operation);
        return operation;
    }

    public static void CopyDirectoryFunc(object state)
    {
        CopyDirectoryAsyncOperation operation = state as CopyDirectoryAsyncOperation;
        if (null == operation)
        {
            return;
        }
        string srcdir = operation.srcDic;
        string desdir = operation.dstDic;

        CopyDirectory(srcdir, desdir);
        operation.isDone = true;
    }

    public static bool CheckDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        return System.IO.Directory.Exists(path);
    }

    public static bool DirctoryIsEmpty(string path)
    {
        if (!CheckDirectory(path))
        {
            return true;
        }

        return System.IO.Directory.GetFiles(path).Length <= 0;
    }


    public static bool CheckFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        return System.IO.File.Exists(path);
    }

    public static void CreateDirectory(string dir)
    {
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
    }

    public static void DeleteDirectory(string dir)
    {
        if (System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.Delete(dir, true);
        }
    }

    public static double TicksToSecond(double ticks)
    {
        return ticks * 0.0000001f;
    }

    public static bool isLogHaveError(string Logpath)
    {
        if (!File.Exists(Logpath))
        {
            return false;
        }
        string ErrorKey = "ERROR:";

        string LogText = File.ReadAllText(Logpath, UTF8Encoding.UTF8);
        return LogText.Contains(ErrorKey);
    }

    public static bool isLogHaveFatalError(string Logpath)
    {
        if (!File.Exists(Logpath))
        {
            return false;
        }
        string FatalErrorKey = "Fatal ERROR:";

        string LogText = File.ReadAllText(Logpath, UTF8Encoding.UTF8);
        return LogText.Contains(FatalErrorKey);
    }

    public static bool isLogHaveComplieError(string LogPath)
    {
        if (!File.Exists(LogPath))
        {
            return false;
        }
        string ComplieErrorKey = "compilationhadfailure: True";

        string LogText = File.ReadAllText(LogPath, UTF8Encoding.UTF8);
        return LogText.Contains(ComplieErrorKey);
    }

    public static void KillAllUnity()
    {
        string ret = string.Empty;
        Process process = new Process();
        process.StartInfo.FileName = "taskkill.exe";
        process.StartInfo.Arguments = "/f /im Unity.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        ret = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();
    }

    public static Process CallUnity(string unityPath,string projectPath,string commandLine)
    {
        return CallExternalExe(unityPath, commandLine, projectPath);
    }

    /// <summary>
    /// 调用外部exe
    /// </summary>
    /// <param name="exePath"></param>
    /// <param name="commnadLine"></param>
    /// <param name="WorkingDir"></param>
    /// <returns></returns>
    public static Process CallExternalExe(string exePath, string commnadLine, string WorkingDir = "")
    {
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = exePath;
        if (string.IsNullOrEmpty(WorkingDir))
        {
            info.WorkingDirectory = Path.GetDirectoryName(exePath);
        }
        else
        {
            info.WorkingDirectory = WorkingDir;
        }
            
        info.Arguments = commnadLine;
        Process process = Process.Start(info);

        return process;
    }

    public static bool isProcessFinish(Process proce)
    {
        return proce == null || proce.HasExited;
    }

    public static string CombinePath(string folder, string file)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return file;
        }
        //对HTTP和远程资源服务器做优化
        string extStr = string.Empty;

        if (folder.StartsWith("http://"))
        {
            extStr = "http://";
            folder = folder.Remove(0, extStr.Length);
        }
        else if (folder.StartsWith("https://"))
        {
            extStr = "https://";
            folder = folder.Remove(0, extStr.Length);
        }
        else if (folder.StartsWith("\\\\"))
        {
            extStr = "\\\\";
            folder = folder.Remove(0, extStr.Length);
        }

        folder = folder.Replace("\\", "/");
        file = file.Replace("\\", "/");
        if (file[0] == '/')
        {
            file = file.Remove(0, 1);
        }
        if (folder[folder.Length - 1] == '/' || folder[folder.Length - 1] == '\\')
        {
            return extStr + folder + file;
        }
        else
        {
            return extStr + folder + "/" + file;
        }
    }

    public static bool isOnEnum(Type enumType, int id)
    {
        bool Reslut = false;
        Array AllEnums = System.Enum.GetValues(enumType);

        int min = (int)AllEnums.GetValue(0);
        int max = (int)AllEnums.GetValue(AllEnums.Length - 1);

        if (id <= min || id >= max)
        {
            return Reslut;
        }

        for (int i = 1; i < AllEnums.Length - 1; i++)
        {
            int v = (int)AllEnums.GetValue(i);
            if (v == id)
            {
                Reslut = true;
                break;
            }
        }

        return Reslut;
    }

    public static bool isOnEnum(Type enumType, string id)
    {
        bool Reslut = false;
        Array AllEnums = System.Enum.GetNames(enumType);

        for (int i = 1; i < AllEnums.Length - 1; i++)
        {
            string v = (string)AllEnums.GetValue(i);
            if (v == id)
            {
                Reslut = true;
                break;
            }
        }

        return Reslut;
    }

    /// <summary>
    /// 获取目录所在的磁盘剩余空间
    /// </summary>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static ulong CheckDiskFreeSpaceInBytes(string dic)
    {

        string str_HardDiskName = dic;
        string[] tempstrs = str_HardDiskName.Split(':');
        str_HardDiskName = tempstrs[0];


        ulong FreeBytesAvailable = 0;
        ulong TotalNumberOfBytes = 0;
        ulong TotalNumberOfFreeBytes = 0;
        str_HardDiskName = str_HardDiskName + ":\\";
        try
        {
            GetDiskFreeSpaceEx(str_HardDiskName, out FreeBytesAvailable, out TotalNumberOfBytes, out TotalNumberOfFreeBytes);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);   
        }

        return TotalNumberOfFreeBytes;
    }


    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFree);


    public static void CreateFolderSoftCut(string targetPath, string sourcePath)
    {
        if (string.IsNullOrEmpty(targetPath) || string.IsNullOrEmpty(sourcePath))
        {
            return;
        }
        string pathRoot = Path.GetPathRoot(sourcePath);
        if (string.IsNullOrEmpty(pathRoot))
        {
            return;
        }
        string mklink = "mklink /D " + "\"" + targetPath + "\"" + " " + "\"" + sourcePath + "\"";

        ProcessStartInfo info = new ProcessStartInfo();

        pathRoot = CombinePath(pathRoot, "mklink.bat");
        File.WriteAllText(pathRoot, mklink);
        info.FileName = pathRoot;
        Process pro = Process.Start(info);
        pro.WaitForExit();
        DeleteFileHelper(pathRoot);

    }


    static Byte[] decode_buf = {
    0x60, 0x20, 0x29, 0xE1, 0x01, 0xCE, 0xAA, 0xFE, 0xA3, 0xAB, 0x8E, 0x30, 0xAF, 0x02, 0xD1, 0x7D,
    0x41, 0x24, 0x06, 0xBD, 0xAE, 0xBE, 0x43, 0xC3, 0xBA, 0xB7, 0x08, 0x13, 0x51, 0xCF, 0xF8, 0xF7,
    0x25, 0x42, 0xA5, 0x4A, 0xDA, 0x0F, 0x52, 0x1C, 0x90, 0x3B, 0x63, 0x49, 0x36, 0xF6, 0xDD, 0x1B,
    0xEA, 0x58, 0xD4, 0x40, 0x70, 0x61, 0x55, 0x09, 0xCD, 0x0B, 0xA2, 0x4B, 0x68, 0x2C, 0x8A, 0xF1,
    0x3C, 0x3A, 0x65, 0xBB, 0xA1, 0xA8, 0x23, 0x97, 0xFD, 0x15, 0x00, 0x94, 0x88, 0x33, 0x59, 0xE9,
    0xFB, 0x69, 0x21, 0xEF, 0x85, 0x5B, 0x57, 0x6C, 0xFA, 0xB5, 0xEE, 0xB8, 0x71, 0xDC, 0xB1, 0x38,
    0x0C, 0x0A, 0x5C, 0x56, 0xC9, 0xB4, 0x84, 0x17, 0x1E, 0xE5, 0xD3, 0x5A, 0xCC, 0xFC, 0x11, 0x86,
    0x7F, 0x45, 0x4F, 0x54, 0xC8, 0x8D, 0x73, 0x89, 0x79, 0x5D, 0xB3, 0xBF, 0xB9, 0xE3, 0x93, 0xE4,
    0x6F, 0x35, 0x2D, 0x46, 0xF2, 0x76, 0xC5, 0x7E, 0xE2, 0xA4, 0xE6, 0xD9, 0x6E, 0x48, 0x34, 0x2B,
    0xC6, 0x5F, 0xBC, 0xA0, 0x6D, 0x0D, 0x47, 0x6B, 0x95, 0x96, 0x92, 0x91, 0xB2, 0x27, 0xEB, 0x9E,
    0xEC, 0x8F, 0xDF, 0x9C, 0x74, 0x99, 0x64, 0xF5, 0xFF, 0x28, 0xB6, 0x37, 0xF3, 0x7C, 0x81, 0x03,
    0x44, 0x62, 0x1F, 0xDB, 0x04, 0x7B, 0xB0, 0x9B, 0x31, 0xA7, 0xDE, 0x78, 0x9F, 0xAD, 0x0E, 0x3F,
    0x3E, 0x4D, 0xC7, 0xD7, 0x39, 0x19, 0x5E, 0xC2, 0xD0, 0xAC, 0xE8, 0x1A, 0x87, 0x8B, 0x07, 0x05,
    0x22, 0xED, 0x72, 0x2E, 0x1D, 0xC1, 0xA9, 0xD6, 0xE0, 0x83, 0xD5, 0xD8, 0xCB, 0x80, 0xF0, 0x66,
    0x7A, 0x9D, 0x50, 0xF9, 0x10, 0x4E, 0x16, 0x14, 0x77, 0x75, 0x6A, 0x67, 0xD2, 0xC0, 0xA6, 0xC4,
    0x53, 0x8C, 0x32, 0xCA, 0x82, 0x2A, 0x18, 0x9A, 0xF4, 0x4C, 0x3D, 0x26, 0x12, 0xE7, 0x98, 0x2F,
    };

    private static string DecodeFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }
        int encryptIndex;
        Byte[] allByte = File.ReadAllBytes(filePath);
        for (encryptIndex = 0; encryptIndex < allByte.Length; encryptIndex++)
        {
            allByte[encryptIndex] = decode_buf[allByte[encryptIndex]];
            allByte[encryptIndex] ^= 3;
        }

        return encoding.GetString(allByte);
    }


    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="reslutPath"></param>
    public static void RARFolder(string folder,string reslutPath)
    {
        if(!System.IO.Directory.Exists(folder))
        {
            return;
        }
        string resPath = reslutPath;

        if(!resPath.EndsWith(".zip") && !resPath.EndsWith(".rar"))
        {
            return;
        }
        DeleteFileHelper(resPath);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "winrar.exe";
        info.Arguments = "a -r " + resPath + " .";
        info.WorkingDirectory = folder;

        Process proc = Process.Start(info);

        while (!isProcessFinish(proc)) ;
    }
}

