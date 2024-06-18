using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker]
    public class DataBlockBasePartDependency
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [HideLabel, HorizontalGroup]
        public string key;

        [HideInInspector]
        public bool required;

        #region Editor
        #if UNITY_EDITOR

        [HorizontalGroup (0.2f)]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void ToggleBoolValue ()
        {
            required = !required;
        }

        private string GetBoolLabel => required ? "Required" : "Prohibited";
        private Color GetBoolColor => Color.HSVToRGB (required ? 0.55f : 0f, 0.5f, 1f);

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;

        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerBasePart.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartParent
    {
        [YamlIgnore, HideInInspector]
        public DataContainerBasePart data;

        [GUIColor ("GetKeyColor")]
        [ValueDropdown ("@DataMultiLinkerBasePart.data.Keys")]
        [HideLabel]
        public string key;

        [HorizontalGroup, LabelText ("Order / Offsets")]
        public int priority;

        [HorizontalGroup (0.2f), HideLabel]
        public int offsetStart;

        [HorizontalGroup (0.2f), HideLabel]
        public int offsetEnd;

        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;

        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            bool present = DataMultiLinkerBasePart.data.ContainsKey (key);
            return present ? colorNormal : colorError;
        }

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartChild
    {
        [YamlIgnore, HideInInspector]
        public DataContainerBasePart data;

        [HideLabel]
        public string key;

        [HorizontalGroup, LabelText ("Order / Offsets")]
        public int priority;

        [HorizontalGroup (0.2f), HideLabel]
        public int offsetStart;

        [HorizontalGroup  (0.2f), HideLabel]
        public int offsetEnd;
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartUnlock
    {
        // No memory based gating is allowed for now, I don't want to complicate merge from develop to overworld branch where memory works differently
        // public List<DataBlockOverworldEventSubcheckMemoryInt> eventMemoryInt;

        public List<DataBlockBasePartDependency> dependencies = new List<DataBlockBasePartDependency> { new DataBlockBasePartDependency () };
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartAudio
    {
        [InlineButtonClear]
        public string onInstall = string.Empty;
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartCost
    {
        [DictionaryKeyDropdown ("@DataMultiLinkerResource.data.Keys")]
        public SortedDictionary<string, int> resources = new SortedDictionary<string, int> { { ResourceKeys.supplies, 10 } };
    }

    public enum BaseStatModifier
    {
        Offset,
        Multiply
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartEffectOnStat
    {
        [HideLabel]
        [ValueDropdown ("@DataMultiLinkerBaseStat.data.Keys")]
        public string key;

        [HideLabel, HorizontalGroup]
        public BaseStatModifier modifier = BaseStatModifier.Multiply;

        [HideLabel, HorizontalGroup]
        public float value = 1f;
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartEffect
    {
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        public List<string> calls;

        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockBasePartEffectOnStat ()")]
        public List<DataBlockBasePartEffectOnStat> stats;

        [DropdownReference]
        public List<IOverworldFunction> functions = new List<IOverworldFunction> ();

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockBasePartEffect () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartVisual
    {
        [PropertyTooltip ("Configs can't embed GameObject references, so a key is used here to refer to specific object on the base enabled if this part is installed. See base hierarchy helper for a list of keys.")]
        public string visualKey;
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartUIGroup
    {
        [YamlIgnore]
        [ShowIf(DataEditor.textAttrArg)]
        public string textName;

        [BoxGroup ("Rect")]
        [HideLabel, HorizontalGroup ("Rect/H1")]
        public int positionX;

        [HideLabel, HorizontalGroup ("Rect/H1")]
        public int positionY;

        [HideLabel, HorizontalGroup ("Rect/H2")]
        public int sizeX;

        [HideLabel, HorizontalGroup ("Rect/H2")]
        public int sizeY;
    }

    [HideReferenceObjectPicker]
    public class DataBlockBasePartUI
    {
        [BoxGroup ("Position")]
        [HideLabel, HorizontalGroup ("Position/H")]
        public int positionX;

        [HideLabel, HorizontalGroup ("Position/H")]
        public int positionY;

        [LabelText ("Name / Desc.")][YamlIgnore]
        public string textName;

        [HideLabel, TextArea (1, 10)][YamlIgnore]
        public string textDesc;

        public bool endpoint;

        [DropdownReference (true)]
        public DataBlockBasePartUIGroup group;

        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        #region Editor
        #if UNITY_EDITOR

        [YamlIgnore, HideInInspector]
        public DataContainerBasePart parent;

        [YamlIgnore, HideInInspector]
        private bool nudgeAsGroup;

        [HorizontalGroup ("Position/H", 32f), Button ("◄")]
        private void NudgeXNeg () => Nudge (-1, 0);

        [HorizontalGroup ("Position/H", 32f), Button ("►")]
        private void NudgeXPos () => Nudge (1, 0);

        [HorizontalGroup ("Position/H", 32f), Button ("▼")]
        private void NudgeYNeg () => Nudge (0, -1);

        [HorizontalGroup ("Position/H", 32f), Button ("▲")]
        private void NudgeYPos () => Nudge (0, 1);

        [HorizontalGroup ("Position/H", 80f), Button ("@GetNudgeAsGroupLabel")]
        private void NudgeAsGroup () => nudgeAsGroup = !nudgeAsGroup;

        private string GetNudgeAsGroupLabel => nudgeAsGroup ? "Group" : "Solo";

        private void Nudge (int x, int y)
        {
            if (nudgeAsGroup && parent != null)
            {
                NudgeRecursive (parent, x, y);
            }
            else
            {
                positionX += x;
                positionY += y;
            }

            DataMultiLinkerBasePart.RedrawUI ();
        }

        private void NudgeRecursive (DataContainerBasePart origin, int x, int y)
        {
            if (origin == null)
                return;

            if (origin.ui != null)
            {
                origin.ui.positionX += x;
                origin.ui.positionY += y;
            }

            if (origin.children != null)
            {
                foreach (var block in origin.children)
                {
                    var child = DataMultiLinkerBasePart.GetEntry (block.key, false);
                    NudgeRecursive (child, x, y);
                }
            }
        }

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockBasePartUI () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    [LabelWidth (160f)]
    public class DataContainerBasePart : DataContainerWithText
    {
        [OnValueChanged ("OnHierarchyRefresh", true)]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockBasePartParent ()")]
        public List<DataBlockBasePartParent> parents = new List<DataBlockBasePartParent> ();

        [YamlIgnore, ReadOnly, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        [HideIf ("@children == null")]
        public List<DataBlockBasePartChild> children;

        [OnValueChanged ("OnUIRefresh")]
        public bool hidden = false;

        [OnValueChanged ("OnUIRefresh")]
        public bool removable = true;

        [OnValueChanged ("OnUIRefresh")]
        public bool preinstalled = false;

        [PropertyRange (1, 5)]
        [OnValueChanged ("OnUIRefresh")]
        public int limit = 1;

        [OnValueChanged ("OnUIRefresh", true)]
        [DropdownReference (true)]
        [LabelText ("UI")]
        public DataBlockBasePartUI ui;

        [OnValueChanged ("OnUIRefresh", true)]
        [DropdownReference (true)]
        public DataBlockBasePartUnlock unlock;

        [DropdownReference (true)]
        public DataBlockBasePartCost cost;

        [DropdownReference (true)]
        public DataBlockBasePartEffect effect;

        [DropdownReference]
        public SortedDictionary<int, DataBlockBasePartEffect> effectPerInstance;

        [DropdownReference (true)]
        public DataBlockBasePartVisual visual;

        [DropdownReference]
        public SortedDictionary<int, DataBlockBasePartVisual> visualPerInstance;

        [DropdownReference (true)]
        public DataBlockBasePartAudio audio;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            limit = Mathf.Clamp (limit, 1, 5);

            #if UNITY_EDITOR
            RefreshParents ();
            #endif
        }

        public override void ResolveText ()
        {
            if (ui != null)
            {
                ui.textName = DataManagerText.GetText (TextLibs.baseUpgrades, $"{key}__name");
                ui.textDesc = DataManagerText.GetText (TextLibs.baseUpgrades, $"{key}__text");

                if(ui.group != null)
				{
                    ui.group.textName = Txt.Get(TextLibs.baseUpgrades, $"group_{key}__name");
                }
            }

        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (ui != null)
            {
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseUpgrades, $"{key}__name", ui.textName);
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseUpgrades, $"{key}__text", ui.textDesc);

                if (ui.group != null)
                {
                    DataManagerText.TryAddingTextToLibrary(TextLibs.baseUpgrades, $"group_{key}__name", ui.group.textName);
                }
            }
        }

        private void RefreshParents ()
        {
            if (ui != null)
                ui.parent = this;
        }

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerBasePart () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private void OnHierarchyRefresh ()
        {
            DataMultiLinkerBasePart.OnHierarchyRefresh ();
            DataMultiLinkerBasePart.RedrawUI ();
        }

        private void OnUIRefresh ()
        {
            RefreshParents ();
            DataMultiLinkerBasePart.RedrawUI ();
        }

        private string GetSelectLabel => DataMultiLinkerBasePart.selection == this ? "Deselect" : "Select";

        [Button ("@GetSelectLabel"), PropertyOrder (-10)]
        public void Select ()
        {
            if (DataMultiLinkerBasePart.selection != this)
                DataMultiLinkerBasePart.selection = this;
            else
                DataMultiLinkerBasePart.selection = null;
        }

        #endif
        #endregion
    }
}
