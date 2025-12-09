using UnityEngine;
using System;
using System.Collections.Generic;
using PhantomBrigade.Data;

namespace Area
{
    public class AreaChannelUntrackedPoints : ILevelDataChannel
    {
        private class Content
        {
            [BinaryData ("untrackedIndexes", false)]
            public int[] indexes;
        }
        
        private Content content;

        public static void Register ()
        {
            LevelContentHelper.RegisterDataChannel ("points_untracked", 1, TryLoadingFromDisk, TryLoadingFromScene);
        }
        
        private static ILevelDataChannel TryLoadingFromDisk (DataBlockAreaContent container, string path)
        {
            var instance = new AreaChannelUntrackedPoints ();
            instance.content = new Content ();
            
            bool success = BinaryDataUtility.LoadFieldsFromBinary (instance.content, path);
            if (!success)
                return null;

            return instance;
        }

        private static ILevelDataChannel TryLoadingFromScene (DataBlockAreaContent container)
        {
            var instance = new AreaChannelUntrackedPoints ();
            var am = CombatSceneHelper.ins.areaManager;
            var points = am.points;

            var indexes = new List<int> ();
            for (int i = 0, iLimit = points.Count; i < iLimit; ++i)
            {
                var pointSource = points[i];
                if (pointSource == null)
                    continue;
                
                if (pointSource.destructionUntracked)
                    indexes.Add (i);
            }

            if (indexes.Count == 0)
                return null;
            
            instance.content = new Content ();
            instance.content.indexes = indexes.ToArray ();

            return instance;
        }

        public bool TrySaving (DataBlockAreaContent container, string path)
        {
            if (content == null || content.indexes.Length == 0)
                return false;
            
            BinaryDataUtility.SaveFieldsToBinary (content, path);
            
            return true;
        }

        public bool TryApplyingToScene (DataBlockAreaContent container)
        {
            if (content == null)
                return false;
            
            var sceneHelper = CombatSceneHelper.ins;
            if (sceneHelper == null || sceneHelper.areaManager == null)
                return false;
            
            var am = CombatSceneHelper.ins.areaManager;
            var points = am.points;
            
            int pointsCount = points.Count;
            for (int i = 0; i < content.indexes.Length; ++i)
            {
                var index = content.indexes[i];
                if (index >= 0 && index < pointsCount)
                {
                    var point = points[index];
                    if (point != null)
                        point.destructionUntracked = true;
                }
            }
            
            return true;
        }
    }
}