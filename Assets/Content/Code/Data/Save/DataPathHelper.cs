using System;
using System.IO;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public enum PathParentType
    {
        User,
        Project
    }
    
    public enum ModFolderType
    {
        User,
        Application,
        Workshop
    }
    
    public static class DataPathHelper
    {
        private static bool initialized = false;

        private static string applicationFolder;
        private static string documentsFolder;
        private static string userFolder;
        private static string saveFolder;
        private static string saveReportingFolder;
        private static string settingsFolder;
        private static string screenshotsFolder;
        private static string modsFolderUser;
        private static string modsFolderApplication;
        private static string modsFolderWorkshop;
        private static string temporaryFolder;


        private static void Initialize ()
        {
            initialized = true;

            var dataPathSplit = Application.dataPath.Replace ('\\', '/').Split ('/');
            applicationFolder = Application.dataPath.Substring (0, Application.dataPath.Length - dataPathSplit[dataPathSplit.Length - 1].Length - 1);
            if (!applicationFolder.EndsWith ("/"))
                applicationFolder += "/";

            documentsFolder = GetCleanPath (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData));
            userFolder = $"{documentsFolder}/PhantomBrigade/";
            saveFolder = $"{userFolder}Saves/";
            saveReportingFolder = $"{userFolder}Saves (Reporting)/";
            settingsFolder = $"{userFolder}Settings/";
            screenshotsFolder = $"{userFolder}Screenshots/";
            modsFolderUser = $"{userFolder}Mods/";
            modsFolderApplication = $"{applicationFolder}Mods/";
            modsFolderWorkshop = null; // Can't be set on initialization, needs to be refreshed from Steam callback
            temporaryFolder = $"{userFolder}Temp/";

            Debug.Log ($"Application folder: {applicationFolder}\nUser folder: {userFolder}\nSaves: {saveFolder}\nSettings: {settingsFolder}\nScreenshots: {screenshotsFolder}\nMods (user): {modsFolderUser}\nMods (workshop): {modsFolderWorkshop}");
        }

        public static void UpdatePathsFromSteam (string installPath, uint appID)
        {
            string appsPath = Path.GetFullPath (Path.Combine (installPath, @"..\..\"));
            string workshopPath = $"{appsPath}workshop\\content\\{appID}\\";
            bool modsFolderWorkshopExists = Directory.Exists (workshopPath);

            Debug.Log ($"Steam apps folder: {appsPath}\nGame path: {installPath}\\\nWorkshop: {workshopPath}\nWorkshop folder exists: {modsFolderWorkshopExists}");

            if (modsFolderWorkshopExists)
                modsFolderWorkshop = workshopPath.Replace ("\\", "/");
        }

        public static string GetCombinedPath (PathParentType parent, string localPath)
        {
            string parentPath = null;
            if (parent == PathParentType.Project)
                parentPath = GetApplicationFolder ();
            else if (parent == PathParentType.User)
                parentPath = GetUserFolder ();

            if (string.IsNullOrEmpty (parentPath))
            {
                Debug.LogWarning ($"Failed to find parent path of type {parent}");
                return null;
            }

            if (!parentPath.EndsWith ("/") && !localPath.StartsWith ("/"))
                parentPath += "/";

            var pathFull = $"{parentPath}{localPath}";
            return pathFull;
        }

        public static string GetApplicationFolder ()
        {
            if (!initialized) Initialize ();
            return applicationFolder;
        }

        public static string GetUserFolder ()
        {
            if (!initialized) Initialize ();
            return userFolder;
        }

        public static string GetSaveFolder ()
        {
            if (!initialized) Initialize ();
            return saveFolder;
        }

        public static string GetModsFolder (ModFolderType type)
        {
            if (!initialized) Initialize ();

            if (type == ModFolderType.Application)
                return modsFolderApplication;
            if (type == ModFolderType.User)
                return modsFolderUser;
            if (type == ModFolderType.Workshop)
                return modsFolderWorkshop;

            return null;
        }

        public static string GetSaveReportingFolder ()
        {
	        if (!initialized) Initialize ();
	        return saveReportingFolder;
        }

        public static string GetScreenshotFolder ()
        {
            if (!initialized) Initialize ();
            return screenshotsFolder;
        }

        public static string GetSettingsFolder ()
        {
            if (!initialized) Initialize ();
            return settingsFolder;
        }

        public static string GetTemporaryFolder ()
        {
            if (!initialized) Initialize ();
            return temporaryFolder;
        }

        public static string GetCleanPath (string input)
        {
            return input.Replace ("\\", "/").Replace ("//", "/");;
        }

        public static string GetCombinedCleanPath (params string[] args)
        {
            try
            {
                var output = System.IO.Path.Combine (args);
                output = GetCleanPath (output);
                return output;
            }
            catch (Exception e)
            {
                Debug.LogWarning ($"Failed to combine path using arguments: {args.ToStringFormatted ()}");
                Debug.LogException (e);
                return null;
            }
        }

        public static bool IsReservedFilename(string filename)
		{
            // Check against lowercase since Windows filenames are not case sensitive
            filename = filename.ToLower();
            if (filename.StartsWith(AutosaveFilenames.timedPrefix.ToLower()))
                return true;

            foreach(var reservedName in FieldReflectionUtility.GetConstantStringFieldValues(typeof(AutosaveFilenames)))
			{
                if (filename == reservedName.ToLower())
                    return true;
			}

            return false;
		}
    }

    public static class AutosaveFilenames
    {
        public const string timedPrefix = "autosave_timed_";
        public const string quicksave = "autosave_quicksave";
        public const string beforeCombat = "autosave_before_combat";
        public const string afterCombat = "autosave_after_combat";
        public const string campaignExit = "autosave_campaign_exit";
        public const string gameExit = "autosave_game_exit";
    }
}
