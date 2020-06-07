using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

public class AssetInfo
{
    public const string ASSET_NAME = "Wire Builder";
    public const string ASSET_ID = "142485";
    public const string ASSET_ABRV = "WB";
    public const string INSTALLED_VERSION = "1.0.2";
    public const string MIN_UNITY_VERSION = "2018.2";

    public static bool IS_UPDATED = true;
    public static bool compatibleVersion = true;
    public static bool untestedVersion = false;

    public const string VERSION_FETCH_URL = "http://www.staggart.xyz/backend/versions/wirebuilder.php";
    public const string DOC_URL = "http://staggart.xyz/unity/wire-builder/wb-docs/";
    public const string FORUM_URL = "https://forum.unity.com/threads/743723/";

    public static void OpenStorePage()
    {
        Application.OpenURL("com.unity3d.kharma:content/" + ASSET_ID);
    }

    public static string PACKAGE_ROOT_FOLDER
    {
        get { return SessionState.GetString(ASSET_ABRV + "_BASE_FOLDER", string.Empty); }
        set { SessionState.SetString(ASSET_ABRV + "_BASE_FOLDER", value); }
    }

    public static string GetRootFolder()
    {
        //Get script path
        string[] scriptGUID = AssetDatabase.FindAssets("AssetInfo t:script");
        string scriptFilePath = AssetDatabase.GUIDToAssetPath(scriptGUID[0]);

        //Truncate to get relative path
        PACKAGE_ROOT_FOLDER = scriptFilePath.Replace("Scripts/Editor/AssetInfo.cs", string.Empty);

#if WB_DEV
            Debug.Log("<b>Package root</b> " + PACKAGE_ROOT_FOLDER);
#endif

        return PACKAGE_ROOT_FOLDER;
    }

    public static class VersionChecking
    {
        public static void CheckUnityVersion()
        {
            compatibleVersion = true;
            untestedVersion = false;

#if !UNITY_2018_2_OR_NEWER
            compatibleVersion = false;
#endif
#if UNITY_2019_3_OR_NEWER
            compatibleVersion = false;
            untestedVersion = true;
#endif
        }

        public static string fetchedVersionString;
        public static System.Version fetchedVersion;
        private static bool showPopup;

        public enum VersionStatus
        {
            UpToDate,
            Outdated
        }

        public enum QueryStatus
        {
            Fetching,
            Completed,
            Failed
        }
        public static QueryStatus queryStatus = QueryStatus.Completed;

#if WB_DEV
        [MenuItem("Wire Builder/Check for update")]
#endif
        public static void GetLatestVersionPopup()
        {
            CheckForUpdate(true);
        }

        private static int VersionStringToInt(string input)
        {
            //Remove all non-alphanumeric characters from version 
            input = input.Replace(".", string.Empty);
            input = input.Replace(" BETA", string.Empty);
            return int.Parse(input, System.Globalization.NumberStyles.Any);
        }

        public static void CheckForUpdate(bool showPopup = false)
        {
            VersionChecking.showPopup = showPopup;

            queryStatus = QueryStatus.Fetching;

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(OnRetreivedServerVersion);
                webClient.DownloadStringAsync(new System.Uri(VERSION_FETCH_URL), fetchedVersionString);
            }
        }

        private static void OnRetreivedServerVersion(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                fetchedVersionString = e.Result;
                fetchedVersion = new System.Version(fetchedVersionString);
                System.Version installedVersion = new System.Version(INSTALLED_VERSION);

                //Success
                IS_UPDATED = (installedVersion >= fetchedVersion) ? true : false;

#if WB_DEV
                    Debug.Log("<b>PackageVersionCheck</b> Up-to-date = " + IS_UPDATED + " (Installed:" + INSTALLED_VERSION + ") (Remote:" + fetchedVersionString + ")");
#endif

                queryStatus = QueryStatus.Completed;

                if (VersionChecking.showPopup)
                {
                    if (!IS_UPDATED)
                    {
                        if (EditorUtility.DisplayDialog(ASSET_NAME + ", version " + INSTALLED_VERSION, "A new version is available: " + fetchedVersionString, "Open store page", "Close"))
                        {
                            OpenStorePage();
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog(ASSET_NAME + ", version " + INSTALLED_VERSION, "Your current version is up-to-date!", "Close")) { }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[" + ASSET_NAME + "] Contacting update server failed: " + e.Error.Message);
                queryStatus = QueryStatus.Failed;

                //When failed, assume installation is up-to-date
                IS_UPDATED = true;
            }
        }

    }
}
