using System;
using System.Collections.Generic;

using UnityEngine;

namespace Area
{
    public static class LevelExtensionManager
    {
        [Flags]
        public enum RegisterResult
        {
            None = 0,
            Error = 1,
            Content = 2,
            Scene = 4,
            Snippet = 5,
        }

        /// <summary>
        /// <para>
        /// Mods may use this method to register their level extensions with the SDK. This should be called from <c>OnSDKLoad</c>.
        /// Registering with the SDK will allow the SDK to show a more advanced UI for the extensions and ensure that any data
        /// container that references the extensions will be serialized correctly.
        /// </para>
        /// <para>
        /// It is not necessary to register level extensions with the game. That is handled automatically when the game loads a mod.
        /// </para>
        /// <para>
        /// Level snippet extensions must have a parameterless constructor. This is true even if the extension is for game play only.
        /// </para>
        /// </summary>
        /// <param name="modID">Mod ID of implementing mod.</param>
        /// <param name="displayText">Short descriptive text of the extension. This will be displayed in the SDK UI.</param>
        /// <param name="factoryFunction">Creates an instance of the extension.</param>
        /// <returns>On success, one or more of <c>Content</c>, <c>Scene</c>, <c>Snippet</c> depending on what the extension implements.
        /// On error, <c>Error</c> and a message will be logged to the console.
        /// If the type of the extension is already registered or the extension doesn't implement any extension interfaces, <c>None</c>.
        /// </returns>
        public static RegisterResult Register (string modID, string displayText, Func<ILevelExtension> factoryFunction)
        {
            try
            {
                var registered = RegisterResult.None;
                var extension = factoryFunction ();
                var t = extension.GetType ();
                if (extension is ILevelSnippetContent cext && !SnippetContentRegistry.ContainsKey (t))
                {
                    SnippetContentRegistry[t] = new LevelSnippetContentExtensionRegistryEntry ()
                    {
                        Priority = cext.GetPriority (),
                        DisplayText = displayText,
                        Create = factoryFunction,
                    };
                    registered |= RegisterResult.Snippet;
                }
                return registered;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat ("{0} -- exception caught when registering extension {1} for mod {2}", nameof(LevelExtensionManager), displayText, modID);
                Debug.LogException (ex);
            }
            return RegisterResult.Error;
        }

        internal static readonly Dictionary<Type, LevelSnippetContentExtensionRegistryEntry> SnippetContentRegistry = new Dictionary<Type, LevelSnippetContentExtensionRegistryEntry> ()
        {
            [typeof(LevelSnippetCore)] = new LevelSnippetContentExtensionRegistryEntry()
            {
                Priority = LevelSnippetCore.Priority,
                DisplayText = LevelSnippetCore.DisplayText,
                Create = LevelSnippetCore.Create,
            },
            [typeof(LevelSnippetProps)] = new LevelSnippetContentExtensionRegistryEntry()
            {
                Priority = LevelSnippetProps.Priority,
                DisplayText = LevelSnippetProps.DisplayText,
                Create = LevelSnippetProps.Create,
            },
        };
    }

    sealed class LevelSnippetContentExtensionRegistryEntry
    {
        public int Priority;
        public string DisplayText;
        public Func<ILevelExtension> Create;
    }
}
