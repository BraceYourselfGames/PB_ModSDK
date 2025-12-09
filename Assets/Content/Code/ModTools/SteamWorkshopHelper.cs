using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Steamworks;

using PhantomBrigade.Data;
using PhantomBrigade.Mods;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    static class SteamWorkshopHelper
    {
        #if UNITY_EDITOR

        public static readonly string tagCompatibleCurrent = "PB 2.0";
        public static readonly List<string> tags = new List<string> ()
        {
            "Combat",
            "Campaign",
            "Overworld",
            "Events",
            "Base",
            "Pilots",
            "Units",
            "Progression",
            "Equipment",
            "Customization",
            "Upgrades",
            "Balance",
            "Gameplay",
            "Levels",
            "Missions",
            "Models",
            "Textures",
            "UI",
            "Utilities",
            "Other"
        };

        public static bool IsUtilityOperationAvailable => utilityCoroutine == null;

        public static void StopCoroutine ()
        {
            EditorCoroutineUtility.StopCoroutine (utilityCoroutine);
            OnUtilityCoroutineEnd ();
        }

        public static void GetInstallState (DataContainerModData mod)
        {
            if (!IsUtilityOperationAvailable)
            {
                return;
            }
            if (mod == null)
            {
                Debug.LogWarning ("Steam Workshop | No selected mod, can't upload");
                return;
            }
            if (mod.workshop == null)
            {
                Debug.LogWarning ("Steam Workshop | Selected mod has no workshop config, can't upload");
                return;
            }
            if (string.IsNullOrEmpty (mod.workshop.publishedID))
            {
                Debug.LogWarning ($"Steam Workshop | There is no record of published ID for mod {mod.key}");
                return;
            }
            utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (GetInstallStateIE (mod));
        }

        public static void UploadToWorkshop (DataContainerModData mod)
        {
            if (!IsUtilityOperationAvailable)
            {
                return;
            }
            if (mod == null)
            {
                Debug.LogWarning ("Steam Workshop | No selected mod, can't upload");
                return;
            }
            if (string.IsNullOrEmpty (mod.id))
            {
                Debug.LogWarning ("Steam Workshop | Selected mod has no ID, can't upload");
                return;
            }
            if (mod.workshop == null)
            {
                Debug.LogWarning ($"Steam Workshop | Selected mod {mod.id} has no workshop config, can't upload");
                return;
            }
            if (string.IsNullOrEmpty (mod.workshop.changes))
            {
                Debug.LogWarning ($"Steam Workshop | Selected mod {mod.id} has no change text filled, can't upload");
                return;
            }
            if (!Directory.Exists (mod.GetModPathProject ()))
            {
                Debug.LogWarning ($"Steam Workshop | Can't upload: selected mod {mod.id} has no project folder");
                return;
            }

            var modID = mod.id;
            if (!EditorUtility.DisplayDialog ("Start upload", $"Are you sure you'd like to upload the mod {modID}?", "Confirm", "Cancel"))
            {
                return;
            }

            utilityCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless (UploadSelectedWithProgressIE (mod));
        }

        static IEnumerator GetInstallStateIE (DataContainerModData mod)
        {
            var initIE = InitializeSteamIE (mod);
            while (initIE.MoveNext ())
            {
                yield return initIE.Current;
            }

            if (!SteamManagerStatic.initSuccessful)
            {
                yield break;
            }

            var (ok, id) = TryParseSteamID ();
            if (!ok)
            {
                yield break;
            }

            var handle = SteamUGC.StartItemUpdate (SteamUtils.GetAppID (), id);
            var available = SteamUGC.GetItemInstallInfo (id, out var sizeOnDisk, out var folder, 1000, out var timeStamp);

            if (available)
            {
                Debug.LogFormat
                (
                    "Steam Workshop | Mod {0} with ID {1} has install info | Size on disk: {2} | Folder: {3} | Time: {4}",
                    mod.key,
                    mod.workshop.publishedID,
                    sizeOnDisk,
                    folder,
                    timeStamp
                );

                Application.OpenURL (folder);
            }
            else
            {
                Debug.Log ($"Steam Workshop | Mod {mod.key} with ID {mod.workshop.publishedID} has no install info");
            }

            utilityCoroutineCompleted = true;
            OnUtilityCoroutineEnd ();
        }

        static IEnumerator InitializeSteamIE (DataContainerModData mod)
        {
            if (mod == null || string.IsNullOrEmpty (mod.id))
            {
                Debug.LogWarning ("Steam Workshop | Can't initialize, no mod provided");
                yield break;
            }

            modTargetedLast = mod;
            utilityCoroutineCompleted = false;

            if (!SteamManagerStatic.initAttempted)
            {
                Debug.LogWarning ("Steam Workshop | Steam API not initialized yet, starting up...");
                SteamManagerStatic.Initialize ();
                yield return new EditorWaitForSeconds (0.2f);
            }
            if (SteamManagerStatic.initSuccessful)
            {
                yield break;
            }

            Debug.LogWarning ("Steam Workshop | Steam API failed to initialize, can't proceed");
            OnUtilityCoroutineEnd ();
        }

        static (bool, PublishedFileId_t) TryParseSteamID ()
        {
            if (modTargetedLast == null || modTargetedLast.workshop == null)
            {
                Debug.LogWarning ("Steam Workshop | Can't parse published ID, no mod selected or selected mod has no workshop data");
                return (false, default);
            }

            var publishedIDParsed = ulong.TryParse (modTargetedLast.workshop.publishedID, out var publishedID);
            if (!publishedIDParsed)
            {
                Debug.LogWarning ($"Steam Workshop | Can't parse published ID of mod {modTargetedLast} | {modTargetedLast.workshop.publishedID}");
                return (false, default);
            }
            return (true, new PublishedFileId_t (publishedID));
        }

        private static bool texPreviewSharedLoaded = false;
        private static Texture2D texPreviewShared = null;
        private static string texPathFallbackLocal = "ModWindowImages/t0_preview_fallback.png";

        public static Texture2D GetPreviewTexShared ()
        {
            if (!texPreviewSharedLoaded)
            {
                texPreviewSharedLoaded = true;
                var texPathFallback = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), texPathFallbackLocal);

                if (File.Exists (texPathFallback))
                {
                    var pngBytes = File.ReadAllBytes (texPathFallback);
                    texPreviewShared = new Texture2D (4, 4, TextureFormat.RGBA32, true, false)
                    {
                        name = "t0_preview_fallback",
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Bilinear,
                        anisoLevel = 2,
                    };

                    texPreviewShared.LoadImage (pngBytes);
                }
                else
                {
                    Debug.LogWarning ($"Failed to find fallback preview texture at: \n{texPathFallback}");
                }
            }

            return texPreviewShared;
        }

        public static Texture2D GetPreviewTexUnique ()
        {
            var texPathFallback = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), texPathFallbackLocal);
            if (!File.Exists (texPathFallback))
            {
                Debug.LogWarning ($"Failed to find fallback preview texture at: \n{texPathFallback}");
                return null;
            }

            var pngBytes = File.ReadAllBytes (texPathFallback);
            var texPreview = new Texture2D (4, 4, TextureFormat.RGBA32, true, false)
            {
                name = "t0_preview_fallback",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2,
            };

            texPreview.LoadImage (pngBytes);
            return texPreview;
        }

        private static float timeOutLimit = 120f;

        static IEnumerator UploadSelectedWithProgressIE (DataContainerModData mod)
        {
            yield return UploadSelectedIE (mod);
            OnUtilityCoroutineEnd ();
        }
        static IEnumerator UploadSelectedIE (DataContainerModData mod)
        {
            var initIE = InitializeSteamIE (mod);
            while (initIE.MoveNext ())
            {
                yield return initIE.Current;
            }

            if (!SteamManagerStatic.initSuccessful)
            {
                yield break;
            }

            // Export contents of the mod to the temp subfolder
            var dirPathWorkshopTemp = mod.GetModPathWorkshopTemp ();
            bool exportSuccess = mod.TryExportToFolderShared (dirPathWorkshopTemp, "Steam Workshop staging folder", false); 

            yield return new EditorWaitForSeconds (0.5f);
            if (!exportSuccess)
            {
                Debug.LogWarning ("Steam Workshop | Failed to export mod files to local temporary folder, cancelling upload!");
                yield break;
            }

            string progressBarHeader = $"Uploading {mod.id}";
            EditorUtility.DisplayProgressBar (progressBarHeader, "Starting...", 0f);
            yield return new EditorWaitForSeconds (0.05f);

            var modID = mod.id;
            if (string.IsNullOrEmpty (mod.workshop.publishedID))
            {
                Debug.LogWarning ($"Steam Workshop | Uploading {modID} | No saved published ID, requesting a new one...");

                var appId = SteamUtils.GetAppID ();
                var callCreate = SteamUGC.CreateItem (appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                var callResultCreate = CallResult<CreateItemResult_t>.Create (UploadToWorkshopCreateResult);
                callResultCreate.Set (callCreate, UploadToWorkshopCreateResult);

                var timeAtStartOfCreation = Time.realtimeSinceStartupAsDouble;
                while (!utilityCoroutineCompleted)
                {
                    yield return new EditorWaitForSeconds (0.05f);
                    float timePassed = (float)(Time.realtimeSinceStartupAsDouble - timeAtStartOfCreation);
                    EditorUtility.DisplayProgressBar (progressBarHeader, "Waiting for response to creation request...", timePassed % 1f);

                    if (timePassed > timeOutLimit)
                    {
                        Debug.LogWarning ($"Steam Workshop | Mod {modID} creation timed out after {timeOutLimit}s");
                        break;
                    }
                }

                // OnUtilityCoroutineEnd ();
                // yield break;

                if (!utilityCoroutineCompleted)
                {
                    Debug.LogWarning ($"Steam Workshop | Mod {modID} creation is late, bailing without a full upload...");
                    yield break;
                }
                Debug.Log ($"Steam Workshop | Mod {modID} creation complete, proceeding to the full upload...");
                utilityCoroutineCompleted = false;
            }
            else
            {
                Debug.Log ($"Steam Workshop | Uploading {modID} with saved published id {mod.workshop.publishedID}");
            }

            var (ok, id) = TryParseSteamID ();
            if (!ok)
            {
                yield break;
            }

            var handle = SteamUGC.StartItemUpdate (SteamUtils.GetAppID (), id);
            var textTitle = mod.workshop.title;
            var textDesc = mod.workshop.description;

            if (mod.workshop.textFromMetadataMain)
            {
                textTitle = mod.metadata.name;
                textDesc = mod.metadata.desc;
            }

            SteamUGC.SetItemTitle (handle, textTitle);
            SteamUGC.SetItemDescription (handle, textDesc);

            SteamUGC.SetItemVisibility (handle, (ERemoteStoragePublishedFileVisibility)((int)mod.workshop.visibility));
            SteamUGC.SetItemContent (handle, dirPathWorkshopTemp);

            if (mod.workshop.texPreviewUpload)
            {
                var texPath = DataPathHelper.GetCombinedCleanPath (mod.GetModPathProject (), ModWorkshopData.texFilename);
                if (File.Exists (texPath))
                {
                    if (mod.workshop.texPreview != null)
                    {
                        var tex = mod.workshop.texPreview;
                        if (tex.width <= 1024 && tex.height <= 1024)
                            SteamUGC.SetItemPreview (handle, texPath);
                        else
                            Debug.LogWarning ($"Steam Workshop | Preview file resolution doesn't match the requirements (1024x1024): {tex.width}x{tex.height}");
                    }
                }
                else
                {
                    var texPathFallbackLocal = "ModWindowImages/t0_preview_fallback.png";
                    var texPathFallback = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), texPathFallbackLocal);
                    if (File.Exists (texPathFallback))
                    {
                        SteamUGC.SetItemPreview (handle, texPathFallback);
                    }
                }
            }

            if (!string.IsNullOrEmpty (mod.workshop.internalData))
                SteamUGC.SetItemMetadata (handle, mod.workshop.internalData);

            if (mod.workshop.tags != null && mod.workshop.tags.Count != 0)
            {
                var tagsProcessed = new List<string> ();
                foreach (var tagCandidate in mod.workshop.tags)
                {
                    if (!tags.Contains (tagCandidate))
                    {
                        Debug.Log ($"Steam Workshop | Skipping disallowed tag {tagCandidate}");
                        continue;
                    }
                    tagsProcessed.Add (tagCandidate);
                }

                // Allow easy filtering of Workshop for mods compatible with 2.x
                // We could potentially update tagCompatibleCurrent every time SDK is brought up to a new release like 2.1
                if (!string.IsNullOrEmpty (mod.metadata.gameVersionMin) && mod.metadata.gameVersionMin.StartsWith ("2."))
                    tagsProcessed.Add (tagCompatibleCurrent);

                if (tagsProcessed.Count != 0)
                {
                    SteamUGC.SetItemTags (handle, tagsProcessed);
                }
            }

            bool textChangesPresent = !string.IsNullOrEmpty (mod.workshop.changes);
            string textChanges = textChangesPresent ? mod.workshop.changes : "No changelog available";

            if (!string.IsNullOrEmpty (mod.metadata.ver))
            {
                if (textChangesPresent)
                    textChanges = $"{mod.metadata.ver}: \n{textChanges}";
                else
                    textChanges = $"{mod.metadata.ver}: {textChanges}";
            }

            var callUpdate = SteamUGC.SubmitItemUpdate (handle, textChanges);
            var callResultUpdate = CallResult<CreateItemResult_t>.Create (UploadToWorkshopUpdateResult);
            callResultUpdate.Set (callUpdate, UploadToWorkshopUpdateResult);

            var timeAtStartOfUpdate = Time.realtimeSinceStartupAsDouble;
            while (!utilityCoroutineCompleted)
            {
                // Makes the progress display box go away on its own instead of waiting for user interaction in the editor.
                // See https://steamworks.github.io/steammanager/#steamapiruncallbacks
                SteamAPI.RunCallbacks ();
                yield return new EditorWaitForSeconds (0.05f);

                float timePassed = (float)(Time.realtimeSinceStartupAsDouble - timeAtStartOfUpdate);
                // EditorUtility.DisplayProgressBar (progressBarHeader, "Waiting for response to update request...", timePassed % 1f);

                if (timePassed > timeOutLimit)
                {
                    Debug.LogWarning ($"Steam Workshop | Mod {modID} creation timed out after {timeOutLimit}s");
                    yield break;
                }

                var status = SteamUGC.GetItemUpdateProgress (handle, out var bytesDone, out var bytesTotal);
                var progress = (float)bytesDone / bytesTotal;
                string statusText = null;

                Debug.LogFormat ("item update progress | status: {0} | bytes progress: {1} | bytes total: {2}", status, bytesDone, bytesTotal);
                switch (status)
                {
                    case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
                        statusText = "Committing changes...";
                        break;
                    case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
                        statusText = "Uploading preview image...";
                        break;
                    case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
                        statusText = "Uploading content...";
                        break;
                    case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
                        statusText = "Preparing configuration...";
                        break;
                    case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
                        statusText = "Preparing content...";
                        break;
                    default:
                        Debug.Log ("Steam Workshop | Received unexpected status: " + status);
                        statusText = "Unexpected status (" + status.ToString () + ")";
                        break;
                }

                statusText += $" | {bytesDone}/{bytesTotal}B";
                EditorUtility.DisplayProgressBar ("Uploading...", statusText, progress);
            }

            // Ensure published ID is saved
            DataManagerMod.SaveMod (mod);
        }

        static void UploadToWorkshopCreateResult (CreateItemResult_t callback, bool ioFailure) => CompleteWorkshopUpload ("creation", callback, ioFailure);
        static void UploadToWorkshopUpdateResult (CreateItemResult_t callback, bool ioFailure) => CompleteWorkshopUpload ("update", callback, ioFailure);

        static void CompleteWorkshopUpload (string op, CreateItemResult_t callback, bool ioFailure)
        {
            utilityCoroutineCompleted = true;

            if (ioFailure)
            {
                Debug.LogWarning ("Steam Workshop | Upload failed: I/O error");
                return;
            }

            var result = callback.m_eResult;
            if (result == EResult.k_EResultOK)
            {
                Debug.LogFormat ("Steam Workshop | Item {0} successful! Published Item ID: {1}", op, callback.m_nPublishedFileId);

                if (modTargetedLast == null)
                {
                    Debug.LogWarning ("Steam Workshop | Failed to save published item ID to mod config: no selected mod!");
                    return;
                }

                if (modTargetedLast.workshop == null)
                {
                    Debug.LogWarning ("Steam Workshop | Failed to save published item ID to mod config: selected mod has no workshop data. Ensure you don't edit anything in the inspector during utility calls.");
                    return;
                }

                modTargetedLast.workshop.publishedID = callback.m_nPublishedFileId.ToString ();
                return;
            }

            if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
            {
                Debug.Log ("Steam Workshop | User needs to accept user agreement: steam://url/CommunityFilePage/" + callback.m_nPublishedFileId);
            }
            if (!steamWorkshopResultMap.TryGetValue ((int)callback.m_eResult, out var message))
            {
                message = "Upload ended with status: " + callback.m_eResult;
            }
            Debug.LogWarning ("Steam Workshop | " + message);
        }

        static void OnUtilityCoroutineEnd ()
        {
            utilityCoroutine = null;
            EditorUtility.ClearProgressBar ();
        }

        static EditorCoroutine utilityCoroutine;
        static bool utilityCoroutineCompleted;

        static DataContainerModData modTargetedLast;

        static readonly Dictionary<int, string> steamWorkshopResultMap = new Dictionary<int, string> ()
        {
            [(int)EResult.k_EResultInsufficientPrivilege] = "Error: Unfortunately, you're banned by the community from uploading to the workshop!",
            [(int)EResult.k_EResultTimeout] = "Error: Timeout",
            [(int)EResult.k_EResultNotLoggedOn] = "Error: You're not logged into Steam!",
            [(int)EResult.k_EResultBanned] = "You don't have permission to upload content to this hub because there's an active VAC or Game ban.",
            [(int)EResult.k_EResultServiceUnavailable] = "The workshop server hosting the content is having issues - please retry.",
            [(int)EResult.k_EResultInvalidParam] = "One of the submission fields contains something not being accepted by that field.",
            [(int)EResult.k_EResultAccessDenied] = "There was a problem trying to save the title and description. Access was denied.",
            [(int)EResult.k_EResultLimitExceeded] = "You have exceeded your Steam Cloud quota. Remove some items and try again.",
            [(int)EResult.k_EResultFileNotFound] = "The uploaded file could not be found.",
            [(int)EResult.k_EResultDuplicateRequest] = "The file was already successfully uploaded. Please refresh.",
            [(int)EResult.k_EResultDuplicateName] = "You already have a Steam Workshop item with that name.",
            [(int)EResult.k_EResultServiceReadOnly] = "Due to a recent password or email change, you are not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently.",
            [(int)EResult.k_EResultLockingFailed] = "Failed to acquire UGC Lock.",
        };

        #endif
    }
}
