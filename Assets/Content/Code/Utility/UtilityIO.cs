using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class UtilityIO
{
    public static string[] GetFilesInProject (string root, string extension, bool searchInSubfolders, bool trimDatapath, bool trimExtensions)
    {
        if (string.IsNullOrEmpty (root))
            return new string[0];

        if (root.EndsWith ("/"))
            root.TrimEnd ('/');

        string pathStart = Application.dataPath + "/../";
        string pathFull = pathStart + root + '/';
        if (!Directory.Exists (pathStart))
            return new string[0];

        string[] pathsToFiles = Directory.GetFiles (pathFull, "*." + extension, searchInSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        for (int i = 0; i < pathsToFiles.Length; ++i)
        {
            string pathToFile = pathsToFiles[i].Replace ('\\', '/');
            if (trimDatapath)
                pathToFile = pathToFile.TrimStart (pathStart);
            if (trimExtensions)
                pathToFile = pathToFile.TrimEnd ('.' + extension);
            pathsToFiles[i] = pathToFile;
        }

        return pathsToFiles;
    }

    public static string[] GetFoldersInProject (string root, bool trimDatapath)
    {
        if (string.IsNullOrEmpty (root))
            return new string[0];

        if (root.EndsWith ("/"))
            root.TrimEnd ('/');

        string pathStart = Application.dataPath + "/../";
        string pathFull = pathStart + root + '/';
        if (!Directory.Exists (pathStart))
            return new string[0];

        string[] pathsToFiles = Directory.GetDirectories (pathFull, "*", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < pathsToFiles.Length; ++i)
        {
            string pathToFile = pathsToFiles[i].Replace ('\\', '/');
            if (trimDatapath)
                pathToFile = pathToFile.TrimStart (pathStart);
            pathsToFiles[i] = pathToFile;
        }

        return pathsToFiles;
    }
    
    public static DateTime GetLinkerTimestampUtc (Assembly assembly)
    {
        var location = assembly.Location;
        return GetLinkerTimestampUtc(location);
    }

    public static DateTime GetLinkerTimestampUtc (string filePath)
    {
        const int peHeaderOffset = 60;
        const int linkerTimestampOffset = 8;
        var bytes = new byte[2048];

        using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            file.Read(bytes, 0, bytes.Length);
        }

        var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
        var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return dt.AddSeconds(secondsSince1970);
    }
}
