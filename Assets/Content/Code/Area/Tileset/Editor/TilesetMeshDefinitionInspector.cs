using UnityEngine;
using UnityEditor;

namespace Area
{
    [CustomEditor (typeof (TilesetMeshDefinition))]
    public class TilesetMeshDefinitionInspector : Editor
    {
        // Looks like plain C# classes can't have custom editors (since they can't be "targeted") and SerializableObject classes can't be inspected at all when in lists, so this is useless
        public override void OnInspectorGUI ()
        {
            TilesetMeshDefinition t = (TilesetMeshDefinition)target;

            EditorGUIUtility.labelWidth = 60f;

            string tileset = EditorGUILayout.TextField ("Tileset", t.tileset);
            string prefix = EditorGUILayout.TextField ("Prefix", t.prefix);

            bool use = EditorGUILayout.Toggle (new GUIContent ("Use", "Whether this rule should be used or ignored by the generator"), t.use);
            bool spawnFlipped = EditorGUILayout.Toggle (new GUIContent ("Flip", "Whether generator should flip this rule when attempting to find a match"), t.spawnFlipped);
            bool helperGeometry = EditorGUILayout.Toggle (new GUIContent ("Helper", "Whether meshes created by this rule should be included into production geometry"), t.helperGeometry);
            bool skipStepAfterPlacement = EditorGUILayout.Toggle (new GUIContent ("Skip step", "Whether meshes created by this rule should be included into production geometry"), t.skipStepAfterPlacement);
            GameObject prefab = (GameObject)EditorGUILayout.ObjectField (new GUIContent ("Mesh", "Object instantiated when requirements are satisfied by a configuration"), t.prefab, typeof (GameObject), false);

            GUILayout.BeginVertical ("Box");
            GUILayout.Label ("", GUILayout.MaxHeight (4f));

            GUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("3 (+X/-Z)", "state3TopXPosZNeg"), EditorStyles.miniLabel);
            TilesetMeshDefinition.SubBlockRequirement state3TopXPosZNeg = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state3TopXPosZNeg);
            Rect rect3 = GUILayoutUtility.GetLastRect ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            TilesetMeshDefinition.SubBlockRequirement state2TopXNegZNeg = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state2TopXNegZNeg);
            Rect rect2 = GUILayoutUtility.GetLastRect ();
            GUILayout.Label (new GUIContent ("2 (-X/-Z)", "state2TopXNegZNeg"), EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("0 (+X/+Z)", "state0TopXPosZPos"), EditorStyles.miniLabel);
            TilesetMeshDefinition.SubBlockRequirement state0TopXPosZPos = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state0TopXPosZPos);
            Rect rect0 = GUILayoutUtility.GetLastRect ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            TilesetMeshDefinition.SubBlockRequirement state1TopXNegZPos = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state1TopXNegZPos);
            Rect rect1 = GUILayoutUtility.GetLastRect ();
            GUILayout.Label (new GUIContent ("1 (-X/+Z)", "state1TopXNegZPos"), EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("7 (+X/-Z)", "state7BottomXPosZNeg"), EditorStyles.miniLabel);
            TilesetMeshDefinition.SubBlockRequirement state7BottomXPosZNeg = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state7BottomXPosZNeg);
            Rect rect7 = GUILayoutUtility.GetLastRect ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            TilesetMeshDefinition.SubBlockRequirement state6BottomXNegZNeg = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state6BottomXNegZNeg);
            Rect rect6 = GUILayoutUtility.GetLastRect ();
            GUILayout.Label (new GUIContent ("6 (-X/-Z)", "state6BottomXNegZNeg"), EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.Label (new GUIContent ("4 (+X/+Z)", "state4BottomXPosZPos"), EditorStyles.miniLabel);
            TilesetMeshDefinition.SubBlockRequirement state4BottomXPosZPos = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state4BottomXPosZPos);
            Rect rect4 = GUILayoutUtility.GetLastRect ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            TilesetMeshDefinition.SubBlockRequirement state5BottomXNegZPos = (TilesetMeshDefinition.SubBlockRequirement)EditorGUILayout.EnumPopup (t.state5BottomXNegZPos);
            Rect rect5 = GUILayoutUtility.GetLastRect ();
            GUILayout.Label (new GUIContent ("5 (-X/+Z)", "state5BottomXNegZPos"), EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.Label ("", GUILayout.MaxHeight (4f));
            GUILayout.EndVertical ();

            if
            (
                t.state0TopXPosZPos == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state1TopXNegZPos == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state2TopXNegZNeg == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state3TopXPosZNeg == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state4BottomXPosZPos == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state5BottomXNegZPos == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state6BottomXNegZNeg == TilesetMeshDefinition.SubBlockRequirement.Irrelevant &&
                t.state7BottomXPosZNeg == TilesetMeshDefinition.SubBlockRequirement.Irrelevant
            )
            {
                EditorGUILayout.HelpBox ("Warning! The definition is invalid: at least one block should be relevant", MessageType.Error);
            }

            if (GUILayout.Button ("Clear"))
            {
                state0TopXPosZPos = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state1TopXNegZPos = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state2TopXNegZNeg = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state3TopXPosZNeg = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state4BottomXPosZPos = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state5BottomXNegZPos = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state6BottomXNegZNeg = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
                state7BottomXPosZNeg = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;
            }

            if (t.prefab != null)
            {
                string rulePath = AssetDatabase.GetAssetPath (t);
                string meshPath = AssetDatabase.GetAssetPath (t.prefab);

                if (meshPath.EndsWith (".prefab"))
                    EditorGUILayout.HelpBox ("Warning! The definition is using a prefab instead of a mesh!", MessageType.Warning);

                else if (meshPath.EndsWith (".fbx"))
                {
                    if (!string.Equals (t.name, t.prefab.name))
                    {
                        GUILayout.BeginHorizontal ();
                        EditorGUILayout.HelpBox ("Warning! The definition is using an incorrect mesh!", MessageType.Error);
                        GUILayout.EndHorizontal ();
                    }
                }

                if (GUILayout.Button ("Set requirements from filename"))
                {
                    SetRequirementsFromName (t.prefab.name);
                }

                if (GUILayout.Button ("Set filenames from requirements"))
                {
                    // First we split the paths to extract the filename and determine what to trim
                    string[] meshPathSplit = meshPath.Split ('/');
                    string[] rulePathSplit = rulePath.Split ('/');

                    // Then we split the extension and the name
                    string[] meshFilenameSplit = meshPathSplit[meshPathSplit.Length - 1].Split ('.');
                    string[] ruleFilenameSplit = rulePathSplit[rulePathSplit.Length - 1].Split ('.');

                    // Then we swap the first parts to a new name
                    string name = t.RequirementsToFilename (t.GetRequirementsTransformed (0, false));
                    meshFilenameSplit[0] = ruleFilenameSplit[0] = name;

                    // Then we build a new path (note how I use .Length and not .Length - 1 to reach a separator char)
                    string meshNameNew = name;// + "." + meshFilenameSplit[1];
                    string ruleNameNew = name;// + "." + ruleFilenameSplit[1];

                    // Then we build a new path (note how I use .Length and not .Length - 1 to reach a separator char)
                    // string meshPathNew = meshPath.Substring (0, meshPath.Length - meshPathSplit[meshPathSplit.Length - 1].Length) + meshNameNew;
                    // string rulePathNew = rulePath.Substring (0, rulePath.Length - rulePathSplit[rulePathSplit.Length - 1].Length) + ruleNameNew;

                    // Then we ask for renaming
                    AssetDatabase.RenameAsset (meshPath, meshNameNew);
                    AssetDatabase.RenameAsset (rulePath, ruleNameNew);
                    AssetDatabase.SaveAssets ();
                    AssetDatabase.Refresh ();
                }

                GUILayout.BeginHorizontal ();
                GUILayout.Label (t.RequirementsToFilename (t.GetRequirementsTransformed (0, false)), EditorStyles.centeredGreyMiniLabel);
                GUILayout.EndHorizontal ();
            }

            if (GUI.changed)
            {
                Undo.RecordObject (t, "TilesetMeshDefition changed");
                t.tileset = tileset;
                t.prefix = prefix;
                t.use = use;
                t.spawnFlipped = spawnFlipped;
                t.helperGeometry = helperGeometry;
                t.skipStepAfterPlacement = skipStepAfterPlacement;
                t.prefab = prefab;
                t.state0TopXPosZPos = state0TopXPosZPos;
                t.state1TopXNegZPos = state1TopXNegZPos;
                t.state2TopXNegZNeg = state2TopXNegZNeg;
                t.state3TopXPosZNeg = state3TopXPosZNeg;
                t.state4BottomXPosZPos = state4BottomXPosZPos;
                t.state5BottomXNegZPos = state5BottomXNegZPos;
                t.state6BottomXNegZNeg = state6BottomXNegZNeg;
                t.state7BottomXPosZNeg = state7BottomXPosZNeg;
                GUI.changed = false;
                EditorUtility.SetDirty (target);
            }

            // Preparing for handles
            Vector3 positionCorner0 = GetPositionFromRect (rect0, true, offsetSmall);
            Vector3 positionCorner1 = GetPositionFromRect (rect1, false, offsetBig);
            Vector3 positionCorner2 = GetPositionFromRect (rect2, false, offsetSmall);
            Vector3 positionCorner3 = GetPositionFromRect (rect3, true, offsetBig);
            Vector3 positionCorner4 = GetPositionFromRect (rect4, true, offsetSmall);
            Vector3 positionCorner5 = GetPositionFromRect (rect5, false, offsetBig);
            Vector3 positionCorner6 = GetPositionFromRect (rect6, false, offsetSmall);
            Vector3 positionCorner7 = GetPositionFromRect (rect7, true, offsetBig);

            // Vertical edges (main)
            DrawLine (positionCorner0, positionCorner4, 1, 3f);
            DrawLine (positionCorner1, positionCorner5, 1, 3f);
            DrawLine (positionCorner2, positionCorner6, 1, 3f);
            DrawLine (positionCorner3, positionCorner7, 1, 3f);

            // Horizontal edges (main, top loop)
            DrawLine (positionCorner0, positionCorner1, 0, 3f);
            DrawLine (positionCorner1, positionCorner2, 2, 3f);
            DrawLine (positionCorner2, positionCorner3, 0, 3f);
            DrawLine (positionCorner3, positionCorner0, 2, 3f);

            // Horizontal edges (main, bottom loop)
            DrawLine (positionCorner4, positionCorner5, 0, 3f);
            DrawLine (positionCorner5, positionCorner6, 2, 3f);
            DrawLine (positionCorner6, positionCorner7, 0, 3f);
            DrawLine (positionCorner7, positionCorner4, 2, 3f);

            // Hint lines
            DrawLine (positionCorner0, positionCorner0 - new Vector3 (offsetSmall + 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner1, positionCorner1 + new Vector3 (offsetBig - 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner2, positionCorner2 + new Vector3 (offsetSmall - 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner3, positionCorner3 - new Vector3 (offsetBig + 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner4, positionCorner4 - new Vector3 (offsetSmall + 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner5, positionCorner5 + new Vector3 (offsetBig - 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner6, positionCorner6 + new Vector3 (offsetSmall - 4f, 0f, 0f), 3, 1f);
            DrawLine (positionCorner7, positionCorner7 - new Vector3 (offsetBig + 4f, 0f, 0f), 3, 1f);

            // Corners
            DrawDot (positionCorner0, t.state0TopXPosZPos);
            DrawDot (positionCorner1, t.state1TopXNegZPos);
            DrawDot (positionCorner2, t.state2TopXNegZNeg);
            DrawDot (positionCorner3, t.state3TopXPosZNeg);
            DrawDot (positionCorner4, t.state4BottomXPosZPos);
            DrawDot (positionCorner5, t.state5BottomXNegZPos);
            DrawDot (positionCorner6, t.state6BottomXNegZNeg);
            DrawDot (positionCorner7, t.state7BottomXPosZNeg);

            // Preparing for subdivisions
            // Vector3 positionMidpoint01 = GetPositionBetween (positionCorner0, positionCorner1);
            // Vector3 positionMidpoint12 = GetPositionBetween (positionCorner1, positionCorner2);
            // Vector3 positionMidpoint23 = GetPositionBetween (positionCorner2, positionCorner3);
            // Vector3 positionMidpoint30 = GetPositionBetween (positionCorner3, positionCorner0);
            // Vector3 positionMidpoint45 = GetPositionBetween (positionCorner4, positionCorner5);
            // Vector3 positionMidpoint56 = GetPositionBetween (positionCorner5, positionCorner6);
            // Vector3 positionMidpoint67 = GetPositionBetween (positionCorner6, positionCorner7);
            // Vector3 positionMidpoint74 = GetPositionBetween (positionCorner7, positionCorner4);
        }


        // For extracting requirements from mesh names
        // Format:
        // *_flip_0x11_1x0x_rq

        private void SetRequirementsFromName (string name)
        {
            string suffix = name.Substring (name.Length - 14, 14);
            string[] split = suffix.Split (new char[] { '_' });

            char[] charTop = split[1].ToCharArray ();
            char[] charBottom = split[2].ToCharArray ();

            if (split.Length != 3 || split[0].Length != 4 || charTop.Length != 4 || charBottom.Length != 4)
            {
                Debug.Log ("TMD | SetRequirementsFromName | Name format is invalid: blocks have incorrect length");
                return;
            }

            bool flip = string.Equals (split[0], "flip") ? true : false;

            TilesetMeshDefinition.SubBlockRequirement state0TopXPosZPos = CharToRequirement (charTop[0]);
            TilesetMeshDefinition.SubBlockRequirement state1TopXNegZPos = CharToRequirement (charTop[1]);
            TilesetMeshDefinition.SubBlockRequirement state2TopXNegZNeg = CharToRequirement (charTop[2]);
            TilesetMeshDefinition.SubBlockRequirement state3TopXPosZNeg = CharToRequirement (charTop[3]);
            TilesetMeshDefinition.SubBlockRequirement state4BottomXPosZPos = CharToRequirement (charBottom[0]);
            TilesetMeshDefinition.SubBlockRequirement state5BottomXNegZPos = CharToRequirement (charBottom[1]);
            TilesetMeshDefinition.SubBlockRequirement state6BottomXNegZNeg = CharToRequirement (charBottom[2]);
            TilesetMeshDefinition.SubBlockRequirement state7BottomXPosZNeg = CharToRequirement (charBottom[3]);

            TilesetMeshDefinition t = (TilesetMeshDefinition)target;
            Undo.RecordObject (t, "TilesetMeshDefition set from mesh filename");

            Debug.Log ("TMD | SetRequirementsFromName | Success!");

            t.spawnFlipped = flip;
            t.state0TopXPosZPos = state0TopXPosZPos;
            t.state1TopXNegZPos = state1TopXNegZPos;
            t.state2TopXNegZNeg = state2TopXNegZNeg;
            t.state3TopXPosZNeg = state3TopXPosZNeg;
            t.state4BottomXPosZPos = state4BottomXPosZPos;
            t.state5BottomXNegZPos = state5BottomXNegZPos;
            t.state6BottomXNegZNeg = state6BottomXNegZNeg;
            t.state7BottomXPosZNeg = state7BottomXPosZNeg;

            GUI.changed = false;
            EditorUtility.SetDirty (target);
        }

        private TilesetMeshDefinition.SubBlockRequirement CharToRequirement (char character)
        {
            TilesetMeshDefinition.SubBlockRequirement result;

            if (character == '0') result = TilesetMeshDefinition.SubBlockRequirement.Empty;
            else if (character == '1') result = TilesetMeshDefinition.SubBlockRequirement.Full;
            else result = TilesetMeshDefinition.SubBlockRequirement.Irrelevant;

            return result;
        }




        private float offsetSmall = 16f;
        private float offsetBig = 64f;

        private Color colorLineXLight = new Color (0.8f, 0.2f, 0.2f);
        private Color colorLineXPro = new Color (1f, 0.2f, 0.2f);
        private Color colorLineYLight = new Color (0.2f, 0.8f, 0.2f);
        private Color colorLineYPro = new Color (0.2f, 1f, 0.2f);
        private Color colorLineZLight = new Color (0.2f, 0.2f, 0.8f);
        private Color colorLineZPro = new Color (0.2f, 0.2f, 1f);
        private Color colorLineSecondaryLight = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        private Color colorLineSecondaryPro = new Color (1f, 1f, 1f, 0.5f);

        private void DrawLine (Vector3 start, Vector3 end, int styleID, float width)
        {
            Color color;

            switch (styleID)
            {
                case 0:
                    color = EditorGUIUtility.isProSkin ? colorLineXPro : colorLineXLight;
                    break;
                case 1:
                    color = EditorGUIUtility.isProSkin ? colorLineYPro : colorLineYLight;
                    break;
                case 2:
                    color = EditorGUIUtility.isProSkin ? colorLineZPro : colorLineZLight;
                    break;
                case 3:
                    color = EditorGUIUtility.isProSkin ? colorLineSecondaryPro : colorLineSecondaryLight;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            Handles.BeginGUI ();
            Handles.color = EditorGUIUtility.isProSkin ? new Color (1f, 1f, 1f, 0.5f) : new Color (0.5f, 0.5f, 0.5f, 0.5f);
            Handles.DrawAAPolyLine (width * 2f, start, end);
            Handles.color = color;
            Handles.DrawAAPolyLine (width, start, end);
            Handles.EndGUI ();
        }

        private Color colorDotIrrelevantLight = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        private Color colorDotIrrelevantPro = new Color (1f, 1f, 1f, 0.2f);
        private Color colorDotEmptyLight = new Color (0.6f, 0.6f, 0.6f, 0.8f);
        private Color colorDotEmptyPro = new Color (0.5f, 0.5f, 0.5f, 0.8f);
        private Color colorDotEmptyInnerLight = new Color (0.867f, 0.867f, 0.867f, 1f);
        private Color colorDotEmptyInnerPro = new Color (0.21875f, 0.21875f, 0.21875f, 1f);
        private Color colorDotFullLight = new Color (0.3f, 0.3f, 0.3f, 0.8f);
        private Color colorDotFullPro = new Color (0.65f, 0.65f, 0.65f, 0.8f);
        private Color colorDotFullInnerLight = new Color (1f, 1f, 1f, 0.8f);
        private Color colorDotFullInnerPro = new Color (1f, 1f, 1f, 0.8f);

        private void DrawDot (Vector3 position, TilesetMeshDefinition.SubBlockRequirement requirement)
        {
            Color colorOuter;
            Color colorInner;
            float radiusOuter;
            float radiusInner;
            switch (requirement)
            {
                case TilesetMeshDefinition.SubBlockRequirement.Irrelevant:
                    colorOuter = colorInner = EditorGUIUtility.isProSkin ? colorDotIrrelevantPro : colorDotIrrelevantLight;
                    radiusOuter = radiusInner = 3f;
                    break;
                case TilesetMeshDefinition.SubBlockRequirement.Empty:
                    colorOuter = EditorGUIUtility.isProSkin ? colorDotEmptyPro : colorDotEmptyLight;
                    colorInner = EditorGUIUtility.isProSkin ? colorDotEmptyInnerPro : colorDotEmptyInnerLight;
                    radiusOuter = 6f;
                    radiusInner = 3f;
                    break;
                case TilesetMeshDefinition.SubBlockRequirement.Full:
                    colorOuter = EditorGUIUtility.isProSkin ? colorDotFullPro : colorDotFullLight;
                    colorInner = EditorGUIUtility.isProSkin ? colorDotFullInnerPro : colorDotFullInnerLight;
                    radiusOuter = 6f;
                    radiusInner = 2f;
                    break;
                default:
                    colorOuter = colorInner = Color.white;
                    radiusOuter = radiusInner = 6f;
                    break;
            }

            Handles.BeginGUI ();
            Handles.color = colorOuter;
            Handles.DrawSolidDisc (position, Vector3.forward, radiusOuter);
            if (requirement != TilesetMeshDefinition.SubBlockRequirement.Irrelevant)
            {
                Handles.color = colorInner;
                Handles.DrawSolidDisc (position, Vector3.forward, radiusInner);
            }
            Handles.EndGUI ();
        }

        private Vector3 GetPositionFromRect (Rect source, bool isRight, float horizontalOffset)
        {
            Vector3 position = new Vector3 (source.x + (isRight ? source.width + horizontalOffset : 0f - horizontalOffset), source.y + source.height / 2f);
            return position;
        }

        private Vector3 GetPositionBetween (Vector3 positionA, Vector3 positionB)
        {
            return 0.5f * (positionA + positionB);
        }
    }
}