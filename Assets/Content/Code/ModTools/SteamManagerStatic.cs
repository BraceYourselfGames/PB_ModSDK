// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.12

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if !DISABLESTEAMWORKS
using System.Collections;
using Steamworks;
#endif

//
// The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
// It handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class SteamManagerStatic : MonoBehaviour
{
    #if !DISABLESTEAMWORKS

    private static Color GetBoolColor (bool input) => input ? Color.green : Color.white;
    
    [ShowInInspector]
    [LabelText ("Init. on enable")]
    public bool initOnEnable = false;

    [ShowInInspector]
    [LabelText ("Init. attempted"), ReadOnly]
    public static bool initAttempted = false;
    
    [ShowInInspector]
    [GUIColor ("@GetBoolColor (initSuccessful)")]
    [LabelText ("Init. successful"), ReadOnly]
    public static bool initSuccessful = false;
    
    [ShowInInspector]
    [GUIColor ("@GetBoolColor (initAttempted)")]
    [LabelText ("App ID"), ReadOnly]
    public static int appID = -1;

    private static SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    
    private static CallResult<CreateItemResult_t> m_itemCreated;
    private static CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;
    private static CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;
    
    

    [AOT.MonoPInvokeCallback (typeof (SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook (int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning ($"[Steamworks.NET] API message: S{nSeverity} / {pchDebugText.ToString ()}");
    }

    #if UNITY_2019_3_OR_NEWER
    // In case of disabled Domain Reload, reset static members before entering Play Mode.
    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode ()
    {
        initAttempted = false;
    }
    #endif

    [Button (ButtonSizes.Large), ButtonGroup, DisableIf ("initAttempted")]
    public static void Initialize ()
    {
        if (initAttempted)
        {
            // This is almost always an error.
            // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
            // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
            // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
            Debug.LogError ("[Steamworks.NET] Tried to Initialize the SteamAPI twice in one session!");
            return;
        }

        initAttempted = true;

        if (!Packsize.Test ())
        {
            Debug.LogError ("[Steamworks.NET] Packsize test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            return;
        }

        if (!DllCheck.Test ())
        {
            Debug.LogError ("[Steamworks.NET] DllCheck test returned false, one or more of the Steamworks binaries seems to be the wrong version.");
            return;
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (SteamAPI.RestartAppIfNecessary (AppId_t.Invalid))
            {
                Debug.LogError ("[Steamworks.NET] Steam API requests app restart, can't proceed...");
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError ($"[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n{e}");
            return;
        }

        // Initializes the Steamworks API.
        // If this returns false then this indicates one of the following conditions:
        // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
        // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
        // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
        // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
        // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        
        initSuccessful = SteamAPI.Init ();
        if (!initSuccessful)
        {
            Debug.LogError ("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
            return;
        }
        
        // Set up our callback to receive warning messages from Steam.
        // You must launch with "-debug_steamapi" in the launch args to receive warnings.
        m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
        SteamClient.SetWarningMessageHook (m_SteamAPIWarningMessageHook);

        appID = (int)SteamUtils.GetAppID ().m_AppId;
        Debug.Log ($"[Steamworks.NET] Steam API initialized | App ID: {appID}");
        
        // m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create (OnNumberOfCurrentPlayers);
        // m_itemCreated = CallResult<CreateItemResult_t>.Create (OnItemCreated);
        // m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create (OnItemSubmitted);
    }

    // OnApplicationQuit gets called too early to shutdown the SteamAPI.
    // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
    
    [Button (ButtonSizes.Large), ButtonGroup, EnableIf ("initSuccessful")]
    public static void Shutdown ()
    {
        if (!initSuccessful)
        {
            Debug.LogError ("[Steamworks.NET] No need to shut down Steam API, no record of successful initialization");
            return;
        }

        SteamAPI.Shutdown ();
        
        initSuccessful = false;
        initAttempted = false;
        appID = 0;
    }

    
    
    private void OnDestroy ()
    {
        if (initSuccessful)
        {
            Debug.Log ($"[Steamworks.NET] SteamManagerStatic destroyed | Successful initialization on record: {initSuccessful}");
            Shutdown ();
        }
    }
    
    private void OnDisable ()
    {
        
        if (initSuccessful)
        {
            Debug.Log ($"[Steamworks.NET] SteamManagerStatic now disabled | Successful initialization on record: {initSuccessful}");
            Shutdown ();
        }
    }

    private void OnEnable ()
    {
        if (initOnEnable)
        {
            if (!initAttempted)
            {
                Debug.Log ($"[Steamworks.NET] SteamManagerStatic now enabled | Initialization attempted in the past: {initAttempted}");
                Initialize ();
            }
        }
    }
    


    private void Update ()
    {
        if (!initSuccessful)
            return;

        // Run Steam client callbacks
        SteamAPI.RunCallbacks ();
    }
    
    
    
    
    private void OnNumberOfCurrentPlayers (NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess != 1 || bIOFailure)
        {
            Debug.Log ("There was an error retrieving the NumberOfCurrentPlayers.");
        }
        else
        {
            Debug.Log ("The number of players playing your game: " + pCallback.m_cPlayers);
        }
    }
    
    

    #endif // !DISABLESTEAMWORKS
}