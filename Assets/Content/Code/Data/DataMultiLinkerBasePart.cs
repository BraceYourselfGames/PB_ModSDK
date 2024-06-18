using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerBasePart : DataMultiLinker<DataContainerBasePart>
    {
        public DataMultiLinkerBasePart ()
        {
            ins = this;
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            
            textSectorKeys = new List<string> { TextLibs.baseUpgrades };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.baseUpgrades),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.baseUpgrades)
            );
        }

        private static DataMultiLinkerBasePart ins;
        public static HashSet<string> dataKeysPreinstalled = new HashSet<string> ();

        public static void OnAfterDeserialization ()
        {
            OnHierarchyRefresh ();
            
            dataKeysPreinstalled.Clear ();
            foreach (var kvp in data)
            {
                if (kvp.Value.preinstalled)
                    dataKeysPreinstalled.Add (kvp.Key);
            }
            
            #if UNITY_EDITOR
            RefreshGrid ();
            #endif
        }

        public static void OnHierarchyRefresh ()
        {
            foreach (var kvp in data)
            {
                var partBlueprint = kvp.Value;
                if (partBlueprint.children != null)
                    partBlueprint.children.Clear ();
            }

            foreach (var kvp in data)
            {
                var partBlueprint = kvp.Value;
                if (partBlueprint == null || partBlueprint.parents == null || partBlueprint.parents.Count == 0)
                    continue;

                foreach (var block in partBlueprint.parents)
                {
                    if (block == null || string.IsNullOrEmpty (block.key))
                        continue;

                    var partBlueprintParent = GetEntry (block.key, false);
                    if (partBlueprintParent == null)
                        continue;

                    if (partBlueprintParent.children == null)
                        partBlueprintParent.children = new List<DataBlockBasePartChild> ();
                    
                    partBlueprintParent.children.Add (new DataBlockBasePartChild
                    {
                        data = partBlueprint,
                        key = partBlueprint.key,
                        priority = block.priority,
                        offsetStart = block.offsetStart,
                        offsetEnd = block.offsetEnd
                    });
                }
            }

            foreach (var kvp in data)
            {
                var partBlueprint = kvp.Value;
                if (partBlueprint == null || partBlueprint.children == null || partBlueprint.children.Count <= 1)
                    continue;
                
                partBlueprint.children.Sort (CompareChildren);
            }
        }
        
        public static int CompareChildren (DataBlockBasePartChild childLink1, DataBlockBasePartChild childLink2)
        {
            return childLink1.priority.CompareTo (childLink2.priority);
        }

        [PropertyOrder (-20), Button ("Redraw UI", ButtonSizes.Large), HideInEditorMode]
        public static void RedrawUI ()
        {
            #if UNITY_EDITOR
            RefreshGrid ();
            #endif
        }
        
        #if UNITY_EDITOR
        
        public static DataContainerBasePart selection;

        private static Vector2Int gridRangeHorizontal;
        private static Vector2Int gridRangeVertical;

        [ShowInInspector, LabelText ("Show hidden on grid")]
        [OnValueChanged ("RefreshGrid")]
        private static bool gridIncludesHidden = false;
        
        [ShowInInspector]
        [TableMatrix (DrawElementMethod = "DrawGridCell", HideColumnIndices = true, HideRowIndices = true, IsReadOnly = true, RowHeight = 32, HorizontalTitle = "@GetGridLabelHorizontal", VerticalTitle = "@GetGridLabelVertical")]
        private static DataContainerBasePart[,] grid;

        private static string GetGridLabelHorizontal => $"{gridRangeHorizontal.x} — {gridRangeHorizontal.y}";
        private static string GetGridLabelVertical => $"{gridRangeVertical.x} — {gridRangeVertical.y}";

        public static void RefreshGrid ()
        {
            gridRangeHorizontal = Vector2Int.zero;
            gridRangeVertical = Vector2Int.zero;

            foreach (var kvp in data)
            {
                var partBlueprint = kvp.Value;
                var uiData = partBlueprint.ui;
                
                if (uiData == null)
                    continue;
                
                if (partBlueprint.hidden && !gridIncludesHidden)
                    continue;

                if (uiData.positionX < gridRangeHorizontal.x)
                    gridRangeHorizontal = new Vector2Int (uiData.positionX, gridRangeHorizontal.y);

                if (uiData.positionX > gridRangeHorizontal.y)
                    gridRangeHorizontal = new Vector2Int (gridRangeHorizontal.x, uiData.positionX);

                if (uiData.positionY < gridRangeVertical.x)
                    gridRangeVertical = new Vector2Int (uiData.positionY, gridRangeVertical.y);

                if (uiData.positionY > gridRangeVertical.y)
                    gridRangeVertical = new Vector2Int (gridRangeVertical.x, uiData.positionY);
            }
            
            var gridSize = new Vector2Int (gridRangeHorizontal.y - gridRangeHorizontal.x + 1, gridRangeVertical.y - gridRangeVertical.x + 1);
            if (grid == null || (grid.GetLength (0) != gridSize.x || grid.GetLength (1) != gridSize.y))
            {
                grid = new DataContainerBasePart[gridSize.x, gridSize.y];
                Debug.LogWarning ($"New grid size: {gridSize.x} x {gridSize.y} | Range: ({gridRangeHorizontal.x}, {gridRangeHorizontal.y}) x ({gridRangeVertical.x}, {gridRangeVertical.y})");
            }

            for (int x = 0; x < gridSize.x; ++x)
            {
                for (int y = 0; y < gridSize.y; ++y)
                    grid[x, y] = null;
            }
            
            foreach (var kvp in data)
            {
                var partBlueprint = kvp.Value;
                var uiData = partBlueprint.ui;

                if (uiData == null)
                    continue;
                
                if (partBlueprint.hidden && !gridIncludesHidden)
                    continue;
                
                int x = uiData.positionX - gridRangeHorizontal.x;
                int y = (gridSize.y - 1) - (uiData.positionY - gridRangeVertical.x);
                
                if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
                {
                    Debug.LogWarning ($"{kvp.Key} | Bad index | X: ({uiData.positionX} - {gridRangeHorizontal.x}) = {x} | Y: ({uiData.positionY} - {gridRangeVertical.x}) = {y}");
                    continue;
                }

                var valueLast = grid[x, y];
                if (valueLast != null)
                {
                    if (!partBlueprint.hidden)
                        Debug.LogWarning ($"{kvp.Key} | Conflict with position of {valueLast.key}: {uiData.positionX}, {uiData.positionY}");
                    continue;
                }

                grid[x, y] = partBlueprint;
            }
        }

        private static Color colorEmpty = new Color (0f, 0f, 0f, 0.5f);
        private static Color colorFull = Color.HSVToRGB (0.6f, 0.3f, 0.7f).WithAlpha (1f);
        private static Color colorFullSelected = Color.HSVToRGB (0.6f, 0.7f, 1f).WithAlpha (1f);

        private static DataContainerBasePart DrawGridCell (Rect rect, DataContainerBasePart value)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains (Event.current.mousePosition))
            {
                // value = -value;
                GUI.changed = true;
                Event.current.Use ();

                selection = value;
                if (ins != null && value != null)
                {
                    ins.filter = value.key;
                    ins.filterUsed = true;
                    ins.ApplyFilter ();
                    UnityEditor.EditorUtility.SetDirty (ins);
                }
            }

            bool present = value != null;
            bool selected = value == selection;
            var color = present ? selected ? colorFullSelected : colorFull : colorEmpty;
            rect = rect.Padding (4);

            var rectSquare = rect;

            rectSquare.height = rect.height * 0.5f;
            rectSquare.y += (rect.height - rectSquare.height) * 0.5f;
            
            rectSquare.width = rectSquare.height;
            rectSquare.x += (rect.width - rectSquare.width) * 0.5f;
            
            UnityEditor.EditorGUI.DrawRect (rectSquare, color);

            if (value != null)
            {
                var content = new GUIContent (selected ? $"{value.ui.positionX}, {value.ui.positionY}" : value.key);
                UnityEditor.EditorGUI.DropShadowLabel (rect, content, UnityEditor.EditorStyles.miniLabel);
            }
            
            return value;
        }
        
        #endif
    }
}


