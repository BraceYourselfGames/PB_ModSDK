using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Content.Code.Utility
{
    public class BuildInfoHelper
    {
        private static bool didInit = false;
        private static string buildInfoText = null;
        private static bool experimentalDetected = false;
        
        public const int indexYear = 0;
        public const int indexMonth = 1;
        public const int indexDay = 2;
        public const int indexTime = 3;
        public const int indexNumber = 4;
        public const int indexBuild = 5;
        public const int indexBuildVersion = 0;
        public const int indexBuildStore = 1;
        public const int indexCommit = 6;

        static void Init ()
        {
            didInit = true;
            experimentalDetected = Debug.isDebugBuild;
            buildInfoText = null;
            
            var buildInfoConfig = UtilitiesYAML.LoadDataFromFile<BuildInfo> ("Configs", "buildinfo.yaml");
            if (buildInfoConfig != null)
                buildInfoText = buildInfoConfig.data;
        }
        
        // get the current build info
        public static string GetBuildInfo ()
        {
            if (didInit == false)
                Init ();

            return buildInfoText;
        }

        public static bool IsBuildExperimental ()
        {
            return experimentalDetected;
        }

        public static void GetVersionNumbersFromString (string input, out int main, out int major, out int minor)
        {
            main = 0;
            major = 0;
            minor = 0;
            
            if (string.IsNullOrEmpty (input))
            {
                UnityEngine.Debug.LogWarning ($"Failed to split version string {input}, null or empty");
                return;
            }
        
            var split = input.Split ('-');
            int index = indexNumber;
            if (!index.IsValidIndex (split))
            {
                UnityEngine.Debug.LogWarning ($"Failed to split version string {input}, index {index} invalid for split by -");
                return;
            }

            var split2 = split[index].Split ('.');
            int size = split2.Length;
            
            if (size >= 3)            
                int.TryParse (split2[2], out minor);
            if (size >= 2)
                int.TryParse (split2[1], out major);
            if (size >= 1)
                int.TryParse (split2[0], out main);
        }
        
        public static void PrintInfo ()
        {
            var bt = UtilityIO.GetLinkerTimestampUtc (Assembly.GetExecutingAssembly ());
            var bts = $"{bt.Year}.{bt.Month:D2}.{bt.Day:D2} {bt.Hour}:{bt.Minute}";
            var info = GetBuildInfo ();
            var infoString = "";
            bool experimental = IsBuildExperimental ();

            if (string.IsNullOrEmpty (info))
            {
                Debug.LogWarning ($"Failed to find build info | Reflected build date/time: {bts}");
                infoString = $"Custom build\n{bts}";
            }
            else
            {
                // Example payload
                // data: 2020-08-05-1743-0.0.7.0-b18-67d597dd9e5a4637f6c25e3cba271b8a4a2de08a 
                var s = info.Split ('-');
            
                var year = indexYear.IsValidIndex (s) ? s[indexYear] : "----";
                var month = indexMonth.IsValidIndex (s) ? s[indexMonth] : "--";
                var day = indexDay.IsValidIndex (s) ? s[indexDay] : "--";
            
                var time = indexTime.IsValidIndex (s) ? s[indexTime] : "--:--";
                if (time.Length > 2)
                    time = time.Insert (2, ":");

                var version = indexNumber.IsValidIndex (s) ? s[indexNumber] : "-.-.-";
                var build = indexBuild.IsValidIndex (s) ? s[indexBuild] : "unknown build";
                if (build.StartsWith ("b"))
                    build = build.Substring (1, build.Length - 1);
            
                // var commit = indexCommit.IsValidIndex (s) ? s[indexCommit] : "unknown commit";
                // if (commit.Length > 10)
                //     commit = commit.Substring (0, 10);

                if (experimental)
                    infoString = $"{version} ({build}) [bb]EXP\n{year}.{month}.{day} {time}";
                else
                    infoString = $"{version} ({build})\n[bb]{year}.{month}.{day} {time}";
            }
            
            Debug.Log ("Build info:\n" + infoString);
        }
    }
}
