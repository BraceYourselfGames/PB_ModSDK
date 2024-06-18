using UnityEngine;
using System;
using System.Collections.Generic;
using PhantomBrigade.Data;

namespace Area
{
    public class AreaChannelRoot : ILevelDataChannel
    {
        public const string alias = "root";
        public AreaDataContainer content;
        private AreaDataContainerSerialized contentSerialized;

        public static void Register ()
        {
            LevelContentHelper.RegisterDataChannel (alias, 1, TryLoadingFromDisk, TryLoadingFromScene);
        }
        
        private static ILevelDataChannel TryLoadingFromDisk (DataBlockAreaContent container, string path)
        {
            var instance = new AreaChannelRoot ();
            instance.contentSerialized = new AreaDataContainerSerialized ();
            
            bool success = BinaryDataUtility.LoadFieldsFromBinary (instance.contentSerialized, path);
            if (!success)
                return null;

            instance.content = new AreaDataContainer (instance.contentSerialized);
            return instance;
        }
        
        public static ILevelDataChannel TryLoadingFromScene (DataBlockAreaContent container)
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
                return null;
            
            var instance = new AreaChannelRoot ();
            instance.contentSerialized = null;
            instance.content = new AreaDataContainer (sceneHelper.areaManager);
            return instance;
        }

        public bool TrySaving (DataBlockAreaContent container, string path)
        {
            if (content == null)
                return false;
            
            // Generate collections from unpacked datas
            contentSerialized = new AreaDataContainerSerialized (content);
            
            // Save content of all fields marked with BinaryData attribute
            BinaryDataUtility.SaveFieldsToBinary (contentSerialized, path);
            
            // Report success
            return true;
        }

        public bool TryApplyingToScene (DataBlockAreaContent container)
        {
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
                return false;
            
            var am = CombatSceneHelper.ins.areaManager;
            am.LoadArea (container.parent.key, container.core, content);
            
            // Report success
            return true;
        }
    }
}