using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

#if !PB_MODSDK
using Rewired;
#endif

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerInputAction : DataMultiLinker<DataContainerInputAction>
    {
        public DataMultiLinkerInputAction ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.uiSettingInputAction); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        public static SortedDictionary<string, List<DataContainerInputAction>> dataPerGroup = new SortedDictionary<string, List<DataContainerInputAction>> ();
        
        [ShowInInspector]
        [FoldoutGroup ("Data Keys Per Group", false)][InlineButton ("UpdateSortedLists", "Update")]
        public static SortedDictionary<string, List<string>> dataKeysPerGroup = new SortedDictionary<string, List<string>> ();

        public static List<string> dataGroupKeysSorted = new List<string>
        {
            InputActionGroup.Global,
            InputActionGroup.Overworld,
            InputActionGroup.Combat
        };
        
        [FoldoutGroup ("Other", false)]
        [ShowInInspector]
        public static bool textSpriteWrap = true;

        [FoldoutGroup ("Other", false)]
        [MultiLineProperty (4), ShowInInspector]
        public static string textInsertionInput = "[ia=global_move_vertical_pos][ia=global_move_horizontal_neg][ia=global_move_vertical_neg][ia=global_move_horizontal_pos]";
        
        [FoldoutGroup ("Other", false)]
        [MultiLineProperty (4), ShowInInspector]
        [InlineButton ("@textInsertionResult = TextLibraryHelper.InsertInputActions (textInsertionInput, textSpriteWrap)", "Fill")]
        public static string textInsertionResult;
        
        [FoldoutGroup ("Other", false)]
        [MultiLineProperty (4), ShowInInspector]
        public static string textListInputFormatted = "Camera movement: {0}{1}{2}{3}";
        
        [FoldoutGroup ("Other", false)]
        [ShowInInspector]
        public static List<string> textListInputKeys = new List<string> { "global_move_vertical_pos", "global_move_horizontal_neg", "global_move_vertical_neg", "global_move_horizontal_pos"  };
        
        [FoldoutGroup ("Other", false)]
        [MultiLineProperty (4), ShowInInspector]
        [InlineButton ("@textListResult = TextLibraryHelper.InsertInputActionList (textListInputFormatted, textListInputKeys, textSpriteWrap)", "Fill")]
        public static string textListResult;
        
        // Example input string
        // test: [ia=global_move_vertical_pos][ia=global_move_horizontal_neg][ia=global_move_vertical_neg][ia=global_move_horizontal_pos]

        private static void OnAfterDeserialization ()
        {
            UpdateSortedLists ();
        }

        public static void UpdateSortedLists ()
        {
            dataKeysPerGroup.Clear ();
            dataPerGroup.Clear ();
            
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (string.IsNullOrEmpty (c.group))
                    continue;
                
                if (!dataPerGroup.ContainsKey (c.group))
                    dataPerGroup.Add (c.group, new List<DataContainerInputAction> { c });
                else
                    dataPerGroup[c.group].Add (c);
            }

            foreach (var kvp in dataPerGroup)
            {
                var list = kvp.Value;
                list.Sort ((x, y) => x.priority.CompareTo (y.priority));

                var keyList = new List<string> ();
                dataKeysPerGroup.Add (kvp.Key, keyList);
                foreach (var c in list)
                    keyList.Add (c.key);
            }
        }
        
        #if !PB_MODSDK
        
        [Button, PropertyOrder (-1), HideInEditorMode]
        private void Populate ()
        {
            var player = InputHelper.player;
            var mapHelper = ReInput.mapping;
            var mapHelperPlayer = player.controllers.maps;
            
            var mapsKeyboard = mapHelperPlayer.GetMaps (ControllerType.Keyboard, 0);
            var mapKeyboard = mapsKeyboard != null && mapsKeyboard.Count > 0 ? mapsKeyboard[0] : null;
            bool mapKeyboardFound = mapKeyboard != null;
            
            var mapsMouse = mapHelperPlayer.GetMaps (ControllerType.Mouse, 0);
            var mapMouse = mapsMouse != null && mapsMouse.Count > 0 ? mapsMouse[0] : null;
            bool mapMouseFound = mapMouse != null;
            
            var mapsGamepad = mapHelperPlayer.GetMaps (ControllerType.Joystick, 0);
            var mapGamepad = mapsGamepad != null && mapsGamepad.Count > 0 ? mapsGamepad[0] : null;
            bool mapGamepadFound = mapGamepad != null;

            data.Clear ();
            
            var actions = mapHelper.Actions;
            foreach (var action in actions)
            {
                Debug.Log ($"Action {action.id} ({action.name}): {action.type}");
                
                var aemsKeyboard = mapKeyboard.GetElementMapsWithAction (action.id);
                var aemsMouse = mapMouse.GetElementMapsWithAction (action.id);
                var aemsGamepad = mapGamepad.GetElementMapsWithAction (action.id);
                
                // Use a regular expression to match any sequence of uppercase letters
                // The string will have an extra leading underscore, so we can remove it using TrimStart
                var keyRoot = action.name;
                keyRoot = Regex.Replace (keyRoot, "[A-Z]", m => "_" + m.Value.ToLower ());
                keyRoot = keyRoot.TrimStart ('_');
                keyRoot = keyRoot.Replace (" ", string.Empty);
                keyRoot = keyRoot.Replace ("u_i_", "ui_");
                
                var config = new DataContainerInputAction ();
                data.Add (keyRoot, config);
                config.key = keyRoot;
                config.actionID = action.id;
                config.textName = action.name;
                config.defaultValueKeyboard = -1;
                config.defaultValueMouse = -1;
                config.defaultValueGamepad = -1;

                bool isAxis = action.type == Rewired.InputActionType.Axis;
                config.type = isAxis ? InputActionType.Axis : InputActionType.ButtonPositive;

                if (isAxis)
                {
                    var configPos = new DataContainerInputAction ();
                    var keyPos = $"{keyRoot}_pos";
                    data.Add (keyPos, configPos);
                    configPos.key = keyPos;
                    configPos.actionID = action.id;
                    configPos.textName = $"{action.name} +";
                    configPos.defaultValueKeyboard = -1;
                    configPos.defaultValueMouse = -1;
                    configPos.defaultValueGamepad = -1;
                    configPos.type = InputActionType.ButtonPositive;
                    
                    var configNeg = new DataContainerInputAction ();
                    var keyNeg = $"{keyRoot}_neg";
                    data.Add (keyNeg, configNeg);
                    configNeg.key = keyNeg;
                    configNeg.actionID = action.id;
                    configNeg.textName = $"{action.name} -";
                    configNeg.defaultValueKeyboard = -1;
                    configNeg.defaultValueMouse = -1;
                    configNeg.defaultValueGamepad = -1;
                    configNeg.type = InputActionType.ButtonNegative;
                    
                    AssignElementsForAxis (aemsKeyboard, "Keyboard", config.key, ref config.defaultValueKeyboard, ref configPos.defaultValueKeyboard, ref configNeg.defaultValueKeyboard);
                    AssignElementsForAxis (aemsMouse, "Mouse", config.key, ref config.defaultValueMouse, ref configPos.defaultValueMouse, ref configNeg.defaultValueMouse);
                    AssignElementsForAxis (aemsGamepad, "Gamepad", config.key, ref config.defaultValueGamepad, ref configPos.defaultValueGamepad, ref configNeg.defaultValueGamepad);
                }
                else
                {
                    AssignElementsForButton (aemsKeyboard, "Keyboard", config.key, ref config.defaultValueKeyboard);
                    AssignElementsForButton (aemsMouse, "Mouse", config.key, ref config.defaultValueMouse);
                    AssignElementsForButton (aemsGamepad, "Gamepad", config.key, ref config.defaultValueGamepad);
                }
            }
        }
        
        private void AssignElementsForButton (ActionElementMap[] aems, string prefix, string key, ref int defaultValue)
        {
            foreach (var aem in aems)
            {
                if (aem.elementType == ControllerElementType.Button && aem.axisContribution == Pole.Positive)
                {
                    Debug.Log ($"{prefix} → {key} | {aem.Print (" | ")}");
                    defaultValue = aem.elementIdentifierId;
                }
            }
        }

        private void AssignElementsForAxis (ActionElementMap[] aems, string prefix, string key, ref int defaultValue, ref int defaultValuePos, ref int defaultValueNeg)
        {
            foreach (var aem in aems)
            {
                if (aem.elementType == ControllerElementType.Axis)
                {
                    Debug.Log ($"{prefix} → {key} | {aem.Print (" | ")}");
                    defaultValue = aem.elementIdentifierId;
                }
                else if (aem.elementType == ControllerElementType.Button)
                {
                    if (aem.axisContribution == Pole.Positive)
                    {
                        Debug.Log ($"{prefix} → {key} | {aem.Print (" | ")}");
                        defaultValuePos = aem.elementIdentifierId;
                    }
                    else
                    {
                        Debug.Log ($"{prefix} → {key} | {aem.Print (" | ")}");
                        defaultValueNeg = aem.elementIdentifierId;
                    }
                }
            }
        }

        [Button ("Auto-Assign Groups")][FoldoutGroup ("Utilities", false)]
        private void AssignGroups ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var c = kvp.Value;

                if (key.Contains ("combat"))
                    c.group = InputActionGroup.Combat;
                else if (key.Contains ("base"))
                    c.group = InputActionGroup.Base;
                else if (key.Contains ("overworld"))
                    c.group = InputActionGroup.Overworld;
                else if (key.Contains ("global"))
                    c.group = InputActionGroup.Global;
                else
                    c.group = InputActionGroup.Internal;
            }
        }
        
        [Button ("Set All Visible")][FoldoutGroup ("Utilities", false)]
        private void SetVisible ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var c = kvp.Value;

                if (!key.Contains ("gamepad"))
                {
                    c.visible = true;
                    c.visibleForMKB = true;
                }
            }
        }

        #endif
    }
}

