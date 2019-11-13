using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class HDRPTools
{
    enum ExportMode
    {
        ExprotToProject,
        ExportToExternalFolder,
    }
    public static string ProjectCustomHDRPFloderName = "HDRP";
    public static string cacheCustomHDRPFloderName = "HDRPCache";
    public static string RPCoreFloderName = "com.unity.render-pipelines.core";
    public static string HDRPFloderName = "com.unity.render-pipelines.high-definition";
    public static string ShaderGraphFloderName = "com.unity.shadergraph";

    private static ExportMode s_exportMode = ExportMode.ExprotToProject;

    [MenuItem("Tools/HDRP/SynsHDRPToProject")]
    public static void SynsHDRPToProject()
    {
        s_exportMode = ExportMode.ExprotToProject;
        SyncHDRPFolder(Application.dataPath, RPCoreFloderName);
        SyncHDRPFolder(Application.dataPath, HDRPFloderName);
        SyncHDRPFolder(Application.dataPath, ShaderGraphFloderName);

        ReplaceHDRPFolder(CommonUtil.CombinePath(Application.dataPath, ProjectCustomHDRPFloderName),
                            new string[] { RPCoreFloderName,
                                               HDRPFloderName,
                                                    ShaderGraphFloderName});

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/HDRP/SynsHDRPToExternalCache")]
    public static void SynsHDRPToExternalCache()
    {
        s_exportMode = ExportMode.ExportToExternalFolder;
        string cacheFolder = Application.dataPath.Replace("Assets", cacheCustomHDRPFloderName);

        SyncHDRPFolder(cacheFolder, RPCoreFloderName);
        SyncHDRPFolder(cacheFolder, HDRPFloderName);
        SyncHDRPFolder(cacheFolder, ShaderGraphFloderName);

        ReplaceHDRPFolder(CommonUtil.CombinePath(cacheFolder, ProjectCustomHDRPFloderName),
                            new string[] { RPCoreFloderName,
                                               HDRPFloderName,
                                                    ShaderGraphFloderName});
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/HDRP/ClearHDRPLibraryCache")]
    public static void ClearHDRPLibraryCache()
    {
        DeleteHDRPLibraryCacheFolder(RPCoreFloderName);
        DeleteHDRPLibraryCacheFolder(HDRPFloderName);
        DeleteHDRPLibraryCacheFolder(ShaderGraphFloderName);
        AssetDatabase.Refresh();
    }

    public static void ReplaceHDRPFolder(string folder,string []FolderKeyArry)
    {
        string[] allFile = System.IO.Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories);

        foreach (string csFile in allFile)
        {
            if (IsReplaceFile(csFile))
            {
                string allContext = File.ReadAllText(csFile);

                foreach (string FolderKey in FolderKeyArry)
                {
                    string oldIncludeHeader = "Packages/" + FolderKey;
                    string newIncludeHeader = "Assets/" + ProjectCustomHDRPFloderName + "/" + FolderKey;

                    allContext = allContext.Replace(oldIncludeHeader, newIncludeHeader);
                }
 
                File.WriteAllText(csFile, allContext);
            }
        }
    }

    public static void SyncHDRPFolder(string outPutFolder,string FolderKey)
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
        string []allFile = System.IO.Directory.GetFiles(hdrpFolder, "*.*", System.IO.SearchOption.AllDirectories);

        string newCsFileFolder = CommonUtil.CombinePath(outPutFolder, ProjectCustomHDRPFloderName + "/" + FolderKey);
        CommonUtil.DeleteDirectory(newCsFileFolder);

        foreach (string csFile in allFile)
        {
            //if (csFile.EndsWith(".meta"))
            //{
            //    continue;
            //}
            string excFolderPath = csFile.Replace("\\","/");
            excFolderPath = excFolderPath.Replace(hdrpFolder,string.Empty);

            string newPath = CommonUtil.CombinePath(newCsFileFolder,excFolderPath);

            CommonUtil.RemoveFileReadOnlyAttribute(csFile);
            CommonUtil.CopyFileHelper(csFile, newPath);
            CommonUtil.RemoveFileReadOnlyAttribute(newPath);

            if (s_exportMode == ExportMode.ExprotToProject)
            {
                CommonUtil.DeleteFileHelper(csFile);
            }

            CommonUtil.RemoveFileReadOnlyAttribute(csFile + ".meta");
            CommonUtil.CopyFileHelper(csFile + ".meta", newPath + ".meta");
            if (s_exportMode == ExportMode.ExprotToProject)
            {
                CommonUtil.DeleteFileHelper(csFile + ".meta");
            }
           
        }
    }

    public static bool IsReplaceFile(string file)
    {
        string[] shaderfile = new string[] {".hlsl",".compute",".shader", ".cginc",".cs", ".raytrace", ".template" };
        foreach (string str in shaderfile)
        {
            if (file.EndsWith(str))
            {
                return true;
            }
        }

        return false;
    }
    

    public static void DeleteHDRPLibraryCacheFolder(string FolderKey)
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
        string[] allCsFile = System.IO.Directory.GetFiles(hdrpFolder, "*.*", System.IO.SearchOption.AllDirectories);

        foreach (string csFile in allCsFile)
        {
            if (csFile.EndsWith(".meta"))
            {
                continue;
            }

            CommonUtil.RemoveFileReadOnlyAttribute(csFile);
            CommonUtil.DeleteFileHelper(csFile);

            CommonUtil.RemoveFileReadOnlyAttribute(csFile + ".meta");
            CommonUtil.DeleteFileHelper(csFile + ".meta");
        }


    }
}
