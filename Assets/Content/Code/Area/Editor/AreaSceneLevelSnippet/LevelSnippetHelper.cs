namespace Area
{
    static class LevelSnippetHelper
    {
        public static bool LoadToClipboard (LevelSnippet snippet, AreaManager am)
        {
            var (ok, levelData) = snippet.Load ();
            if (!ok)
            {
                return false;
            }
            RestorePositionIndexAndConfiguration (am, levelData);
            RestorePropPositionIndex (levelData);
            var clipboard = am.clipboard;
            clipboard.Reset ();
            clipboard.clipboardBoundsSaved = levelData.Bounds;
            clipboard.clipboardPointsSaved.AddRange (levelData.Points);
            clipboard.clipboardPropsSaved.AddRange (levelData.Props);
            return true;
        }

        public static (bool OK, LevelSnippet Snippet) SaveFromClipboard (AreaClipboard clipboard, LevelSnippetManager.SaveSpec snippetSpec)
        {
            snippetSpec.LevelData = new LevelData ()
            {
                Bounds = clipboard.clipboardBoundsSaved,
                Points = clipboard.clipboardPointsSaved,
                Props = clipboard.clipboardPropsSaved,
            };
            return LevelSnippetManager.Save (snippetSpec);
        }

        static void RestorePositionIndexAndConfiguration (AreaManager am, LevelData data)
        {
            var bounds = data.Bounds;
            var layerSize = bounds.x * bounds.z;
            var volumeSize = layerSize * bounds.y;
            var points = data.Points;
            var neighborOffsets = new []
            {
                0, // self
                1, // east
                bounds.x, // north
                bounds.x + 1, // northeast
                layerSize, // down
                layerSize + 1, // down east
                layerSize + bounds.x, // down north
                layerSize + bounds.x + 1, // down northeast
            };
            var layerEnd = layerSize;
            var rowEnds = new[]
            {
                bounds.x,
                2 * bounds.x,
                layerEnd + bounds.x,
                layerEnd + 2 * bounds.x,
            };
            for (var i = 0; i < points.Count; i += 1)
            {
                if (i == layerEnd)
                {
                    layerEnd += layerSize;
                }
                if (i == rowEnds[0])
                {
                    for (var j = 0; j < rowEnds.Length; j += 1)
                    {
                        rowEnds[j] += bounds.x;
                    }
                }

                var point = points[i];
                point.pointPositionIndex = AreaUtility.GetVolumePositionFromIndex (i, bounds, log: false);
                point.pointsInSpot = new AreaVolumePoint[8];
                point.pointsInSpot[0] = point;
                for (var n = 1; n < neighborOffsets.Length; n += 1)
                {
                    var index = i + neighborOffsets[n];
                    if (n % 2 == 1 && index == rowEnds[n / 2])
                    {
                        point.pointsInSpot[n] = null;
                        continue;
                    }
                    if (index >= layerEnd * (n / 4 + 1))
                    {
                        point.pointsInSpot[n] = null;
                        continue;
                    }
                    if (index >= volumeSize)
                    {
                        point.pointsInSpot[n] = null;
                        continue;
                    }
                    point.pointsInSpot[n] = points[index];
                }
                am.UpdateSpotAtPoint (point, false);
            }
        }

        static void RestorePropPositionIndex (LevelData levelData)
        {
            var points = levelData.Points;
            foreach (var prop in levelData.Props)
            {
                prop.clipboardPosition = points[prop.pivotIndex].pointPositionIndex;
            }
        }
    }
}
