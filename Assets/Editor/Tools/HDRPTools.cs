using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class HDRPTools
{
    [MenuItem("Tools/HDRP/SynsHDRPScriptsToProject")]
    public static void SynsHDRPScriptsToProject()
    {
        SyncHDRPScriptsFolder(Application.dataPath,"com.unity.render-pipelines.core");
        SyncHDRPScriptsFolder(Application.dataPath, "com.unity.render-pipelines.high-definition");
        SyncHDRPScriptsFolder(Application.dataPath, "com.unity.shadergraph");
    }

    [MenuItem("Tools/HDRP/SynsHDRPScriptsToExternalCache")]
    public static void SynsHDRPScriptsToExternalCache()
    {
        string cacheFolder = Application.dataPath.Replace("Assets","HDRPCacheFolder");

        SyncHDRPScriptsFolder(cacheFolder, "com.unity.render-pipelines.core");
        SyncHDRPScriptsFolder(cacheFolder, "com.unity.render-pipelines.high-definition");
        SyncHDRPScriptsFolder(cacheFolder, "com.unity.shadergraph");
    }

    [MenuItem("Tools/HDRP/ClearHDRPLibraryCacheScripts")]
    public static void ClearHDRPLibraryCacheScripts()
    {
        DeleteHDRPScriptsFolder( "com.unity.render-pipelines.core");
        DeleteHDRPScriptsFolder( "com.unity.render-pipelines.high-definition");
        DeleteHDRPScriptsFolder( "com.unity.shadergraph");
    }

    public static void SyncHDRPScriptsFolder(string outPutFolder,string FolderKey)
    {
        string PackageRootPath = Application.dataPath;

        PackageRootPath = PackageRootPath.Replace("Assets", "Library/PackageCache");

        string hdrpFolder = string.Empty;
        string []dirs = System.IO.Directory.GetDirectories(PackageRootPath);
        
        foreach (string dir in dirs)
        {
            if (dir.Contains(FolderKey))
            {
                hdrpFolder = dir.Replace("\\", "/"); 
                break;
            }
        }

        //拷贝CS文件
        string []allCsFile = System.IO.Directory.GetFiles(hdrpFolder, "*.cs", System.IO.SearchOption.AllDirectories);

        string newCsFileFolder = CommonUtil.CombinePath(outPutFolder, "HDRPScripts/" + FolderKey);
        CommonUtil.DeleteDirectory(newCsFileFolder);

        foreach (string csFile in allCsFile)
        {
            string excFolderPath = csFile.Replace("\\","/");
            excFolderPath = excFolderPath.Replace(hdrpFolder,string.Empty);

            string newPath = CommonUtil.CombinePath(newCsFileFolder,excFolderPath);


            CommonUtil.RemoveFileReadOnlyAttribute(newPath);
            CommonUtil.RemoveFileReadOnlyAttribute(newPath + ".meta");

            CommonUtil.MoveFileHelper(csFile, newPath);
            CommonUtil.MoveFileHelper(csFile + ".meta", newPath + ".meta");
        }

        //拷贝.asmdef文件

        string[] allAsmDefFile = System.IO.Directory.GetFiles(hdrpFolder, "*.asmdef", System.IO.SearchOption.AllDirectories);

        foreach (string asmdefFile in allAsmDefFile)
        {
            string excFolderPath = asmdefFile.Replace("\\", "/");
            excFolderPath = excFolderPath.Replace(hdrpFolder, string.Empty);

            string newPath = CommonUtil.CombinePath(newCsFileFolder, excFolderPath);

            CommonUtil.RemoveFileReadOnlyAttribute(newPath);
            CommonUtil.RemoveFileReadOnlyAttribute(newPath + ".meta");

            CommonUtil.MoveFileHelper(asmdefFile, newPath);
            CommonUtil.MoveFileHelper(asmdefFile + ".meta", newPath + ".meta");
        }

        AssetDatabase.Refresh();

    }

    

    public static void DeleteHDRPScriptsFolder(string FolderKey)
    {
        string PackageRootPath = Application.dataPath;

        PackageRootPath = PackageRootPath.Replace("Assets", "Library/PackageCache");

        string hdrpFolder = string.Empty;
        string[] dirs = System.IO.Directory.GetDirectories(PackageRootPath);
        

        foreach (string dir in dirs)
        {
            if (dir.Contains(FolderKey))
            {
                hdrpFolder = dir.Replace("\\", "/");
                break;
            }
        }

        //拷贝CS文件
        string[] allCsFile = System.IO.Directory.GetFiles(hdrpFolder, "*.cs", System.IO.SearchOption.AllDirectories);

        foreach (string csFile in allCsFile)
        {

            CommonUtil.RemoveFileReadOnlyAttribute(csFile);
            CommonUtil.DeleteFileHelper(csFile);
            CommonUtil.RemoveFileReadOnlyAttribute(csFile + ".meta");
            CommonUtil.DeleteFileHelper(csFile + ".meta");
        }

        //拷贝.asmdef文件

        string[] allAsmDefFile = System.IO.Directory.GetFiles(hdrpFolder, "*.asmdef", System.IO.SearchOption.AllDirectories);

        foreach (string asmdefFile in allAsmDefFile)
        {
            CommonUtil.RemoveFileReadOnlyAttribute(asmdefFile);
            CommonUtil.RemoveFileReadOnlyAttribute(asmdefFile + ".meta");

            CommonUtil.DeleteFileHelper(asmdefFile);
            CommonUtil.DeleteFileHelper(asmdefFile + ".meta");
        }

        AssetDatabase.Refresh();

    }
}
