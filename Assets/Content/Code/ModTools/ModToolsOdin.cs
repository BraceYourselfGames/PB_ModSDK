using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
#endif

namespace PhantomBrigade.SDK.ModTools
{
    using Data;

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class ConditionalSpaceAttribute : Attribute
    {
        public string VisibleIf;
        public readonly float SpaceBefore;
        public readonly float SpaceAfter;

        public ConditionalSpaceAttribute (float spaceBefore)
        {
            SpaceBefore = spaceBefore;
        }

        public ConditionalSpaceAttribute (float spaceBefore, float spaceAfter)
        {
            SpaceBefore = spaceBefore;
            SpaceAfter = spaceAfter;
        }

        public ConditionalSpaceAttribute (float spaceBefore, float spaceAfter, string visibleIf)
        {
            SpaceBefore = spaceBefore;
            SpaceAfter = spaceAfter;
            VisibleIf = visibleIf;
        }
    }

    [Conditional ("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ElidedPathAttribute : Attribute { } 

    #if UNITY_EDITOR
    
    sealed class ConditionalSpaceAttributeDrawer : OdinAttributeDrawer<ConditionalSpaceAttribute>
    {
        protected override void Initialize ()
        {
            visibleIfResolver = ValueResolver.Get (Property, Attribute.VisibleIf, true);
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            ValueResolver.DrawErrors(visibleIfResolver);

            var visible = visibleIfResolver.GetValue ();
            var spaceBefore = visible ? Attribute.SpaceBefore : 0f;
            var spaceAfter = visible ? Attribute.SpaceAfter : 0f;
            if (spaceBefore > 0f)
            {
                GUILayout.Space (Attribute.SpaceBefore);
            }
            CallNextDrawer (label);
            if (spaceAfter > 0f)
            {
                GUILayout.Space (Attribute.SpaceAfter);
            }
        }

        ValueResolver<bool> visibleIfResolver;
    }

    sealed class ElidedPathAttributeDrawer : OdinAttributeDrawer<ElidedPathAttribute>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            var pathName = (string)Property.ValueEntry.WeakSmartValue;
            var rectControl = EditorGUILayout.GetControlRect ();

            bool labelUsed = label != null && !string.IsNullOrEmpty (label.text);
            Rect rectLabel = labelUsed ? rectControl.TakeFromLeft (EditorGUIUtility.labelWidth) : default;
            
            var rectButton = rectControl.TakeFromRight (18f);
            rectControl.TakeFromRight (1f);
            var elidedName = ElidePathName (pathName, rectControl.width);
            using (new EditorGUI.DisabledScope (true))
            {
                if (labelUsed)
                    GUI.Label (rectLabel, label, EditorStyles.label);
                EditorGUI.TextField (rectControl, elidedName);
            }
            if (SirenixEditorGUI.IconButton (rectButton, EditorIcons.Folder) && !string.IsNullOrEmpty(pathName))
            {
                Application.OpenURL ("file://" + pathName);
            }
        }

        static string ElidePathName (string pathName, float width)
        {
            if (string.IsNullOrEmpty (pathName))
            {
                return "";
            }

            var parts = pathName.Split('/').ToList ();
            if (parts[1] == "Users" && parts[5] == "PhantomBrigade")
            {
                var measured = EditorStyles.textField.CalcSize (GUIHelper.TempContent (pathName)).x;
                while (measured > width && parts.Count > 4)
                {
                    parts.RemoveAt (2);
                    pathName = string.Join ("/", parts.Take(2).Concat(parts.Skip(2).Prepend("...")));
                    measured = EditorStyles.textField.CalcSize (GUIHelper.TempContent (pathName)).x;
                }
            }
            return pathName;
        }
    }

    [DrawerPriority (0f, 0f, 1000f)]
    sealed class DataMultiLinkerKeyDrawer : OdinValueDrawer<string>
    {
        protected override bool CanDrawValueProperty (InspectorProperty property)
        {
            if (property.Parent == null)
            {
                return false;
            }
            var parent = property.Parent;
            if (parent.Parent == null)
            {
                return false;
            }
            var grandparent = parent.Parent;
            if (grandparent == null)
            {
                return false;
            }
            if (grandparent.Name != "data")
            {
                return false;
            }
            var parentType = property.ParentType;
            if (!parentType.IsConstructedGenericType)
            {
                return false;
            }
            var genericType = parentType.GetGenericTypeDefinition ();
            if (!genericType.Name.StartsWith ("EditableKeyValuePair"))
            {
                return false;
            }
            if (parentType.GetGenericArguments ()[0] != typeof(string))
            {
                return false;
            }
            var ancestorType = grandparent.ParentType;
            if (!ancestorType.Name.StartsWith ("DataMultiLinker"))
            {
                return false;
            }
            return true;
        }

        protected override void DrawPropertyLayout (GUIContent label)
        {
            var disabled = !DataContainerModData.hasSelectedConfigs;
            GUIHelper.PushIndentLevel (1);
            SirenixEditorGUI.BeginIndentedHorizontal ();
            using (new EditorGUI.DisabledScope (disabled))
            {
                ValueEntry.SmartValue = GUILayout.TextField (ValueEntry.SmartValue);
            }
            SirenixEditorGUI.EndIndentedHorizontal ();
            GUIHelper.PopIndentLevel ();
        }
    }

    sealed class DataMultiLinkerAttributeProcessor<T, U> : OdinAttributeProcessor<T>
        where T : DataMultiLinker<U>
        where U : DataContainer, new()
    {
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member) =>
            (member.Name == nameof(DataMultiLinker<U>.dataFiltered) || 
            member.Name == nameof(DataMultiLinker<U>.data)) && 
            typeof(U) != typeof(DataContainerModToolsPage);

        public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            var isSDK = !DataContainerModData.hasSelectedConfigs;
            switch (member.Name)
            {
                case nameof(DataMultiLinker<U>.data):
                    {
                        var dds = attributes.GetAttribute<DictionaryDrawerSettings> ();
                        dds.IsReadOnly = isSDK;
                        dds.DisplayMode = isSDK ? DictionaryDisplayOptions.CollapsedFoldout : DictionaryDisplayOptions.ExpandedFoldout;
                    }
                    break;
                case nameof(DataMultiLinker<U>.dataFiltered):
                    attributes.GetAttribute<ListDrawerSettingsAttribute> ().HideAddButton = isSDK;
                    break;
            }
        }
    }

    sealed class DataLinkerAttributeProcessor<T, U> : OdinAttributeProcessor<T>
        where T : DataLinker<U>
        where U : DataContainerUnique, new ()
    {
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member) =>
            member.Name == nameof(DataLinker<U>.data);

        public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes) =>
            ReadOnlyAttributeApplicator.Apply (attributes);
    }

    sealed class DataFilterKeyValuePairAttributeProcessor<T, U> : OdinAttributeProcessor<T>
        where T : DataFilterKeyValuePair<U>
        where U : DataContainer, new()
    {
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member) =>
            (member.Name == nameof (DataFilterKeyValuePair<U>.key) || 
             member.Name == nameof (DataFilterKeyValuePair<U>.value)) && 
            typeof(U) != typeof(DataContainerModToolsPage);

        public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member.Name == nameof(DataFilterKeyValuePair<U>.key))
            {
                ReadOnlyAttributeApplicator.Apply (attributes);
                return;
            }
            ReadOnlyAttributeApplicator.Apply (typeof(U), attributes);
        }
    }

    sealed class DataContainerAttributeProcessor<T> : OdinAttributeProcessor<T>
        where T : DataContainer, new ()
    {
        public override bool CanProcessSelfAttributes (InspectorProperty propery) => typeof(T) != typeof(DataContainerModData) && typeof(T) != typeof(DataContainerModToolsPage);
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentPropery, MemberInfo member) => typeof(T) != typeof(DataContainerModData) && typeof(T) != typeof(DataContainerModToolsPage);

        public override void ProcessSelfAttributes (InspectorProperty property, List<Attribute> attributes)
        {
            if (!referencePickers.TryGetValue (typeof(T), out var hidesPicker))
            {
                hidesPicker = attributes.HasAttribute<HideReferenceObjectPickerAttribute> ();
                referencePickers[typeof(T)] = hidesPicker;
            }
            if (hidesPicker)
            {
                return;
            }
            
            /*
            attributes.RemoveAttributeOfType<HideReferenceObjectPickerAttribute> ();
            if (!DataContainerModData.hasSelectedConfigs)
            {
                attributes.Add (new HideReferenceObjectPickerAttribute ());
            }
            */
        }

        public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            ReadOnlyAttributeApplicator.ApplyToChildMember (parentProperty.Info.TypeOfValue, member, attributes);
        }

        readonly Dictionary<Type, bool> referencePickers = new Dictionary<Type, bool> ();
    }

    abstract class SelectForEditingAttributeProcessor<T> : OdinAttributeProcessor<T>
    {
        public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes) =>
            ReadOnlyAttributeApplicator.ApplyToChildMember (typeof(T), member, attributes);
    }

    sealed class DataBlockAreaSpawnGroupAttributeProcessor : SelectForEditingAttributeProcessor<DataBlockAreaSpawnGroup> { }
    sealed class DataBlockAreaLocationTaggedAttributeProcessor : SelectForEditingAttributeProcessor<DataBlockAreaLocationTagged> { }
    sealed class DataBlockAreaVolumeTaggedAttributeProcessor : SelectForEditingAttributeProcessor<DataBlockAreaVolumeTagged> { }
    sealed class DataBlockAreaFieldAttributeProcessor : SelectForEditingAttributeProcessor<DataBlockAreaField> { }
    sealed class DataBlockAreaWaypointGroupAttributeProcessor : SelectForEditingAttributeProcessor<DataBlockAreaWaypointGroup> { }

    static class ReadOnlyAttributeApplicator
    {
        public static void Apply (List<Attribute> attributes)
        {
            attributes.RemoveAttributeOfType<ReadOnlyAttribute> ();
            if (!DataContainerModData.hasSelectedConfigs)
            {
                attributes.Add(new ReadOnlyAttribute ());
            }
        }

        public static void Apply (Type t, List<Attribute> attributes)
        {
            attributes.RemoveAttributeOfType<ReadOnlyAttribute> ();
            if (DataContainerModData.hasSelectedConfigs || containerPropMap.ContainsKey(t))
            {
                return;
            }
            attributes.Add (new ReadOnlyAttribute ());
        }

        public static void ApplyToChildMember (Type t, MemberInfo member, List<Attribute> attributes)
        {
            attributes.RemoveAttributeOfType<ReadOnlyAttribute> ();
            if (DataContainerModData.hasSelectedConfigs)
            {
                return;
            }
            if (containerPropMap.TryGetValue (t, out var propSet) && propSet.Contains (member.Name))
            {
                return;
            }
            if (attributes.HasAttribute<ButtonAttribute> () && disabledButtonMap.TryGetValue(t, out propSet) && propSet.Contains(member.Name))
            {
                attributes.Add (new DisableIfAttribute ("@DataContainerModData.selectedMod == null"));
                return;
            }
            attributes.Add (new ReadOnlyAttribute ());
        }

        // Exclude these properties from being marked readonly.
        static readonly Dictionary<Type, HashSet<string>> containerPropMap = new Dictionary<Type, HashSet<string>> ()
        {
            [typeof(DataContainerBasePart)] = new HashSet<string> ()
            {
                nameof(DataContainerBasePart.Select),
            },
            [typeof(DataContainerCombatArea)] = new HashSet<string> ()
            {
                nameof(DataContainerCombatArea.SelectAndApplyToScene),
            },
            [typeof(DataContainerOverworldProvinceBlueprint)] = new HashSet<string> ()
            {
                nameof(DataContainerOverworldProvinceBlueprint.SelectToInspector),
                nameof(DataContainerOverworldProvinceBlueprint.DeselectInInspector),
            },
            [typeof(DataContainerPartPreset)] = new HashSet<string> ()
            {
                nameof(DataContainerPartPreset.VisualizeAndFocusIsolated),
                nameof(DataContainerPartPreset.VisualizeAndFocusProcessed),
            },
            [typeof(DataContainerScenario)] = new HashSet<string> ()
            {
                nameof(DataContainerScenario.SetTabCore),
                nameof(DataContainerScenario.SetTabSteps),
                nameof(DataContainerScenario.SetTabStates),
                nameof(DataContainerScenario.SetTabOther),
            },
            [typeof(DataContainerSubsystem)] = new HashSet<string> ()
            {
                nameof(DataContainerSubsystem.Visualize),
                nameof(DataContainerSubsystem.VisualizeIsolaved),
            },
            [typeof(DataContainerUnitComposite)] = new HashSet<string> ()
            {
                nameof(DataContainerUnitComposite.DestroyVisualHolder),
                nameof(DataContainerUnitComposite.Visualize),
            },
            [typeof(DataBlockAreaSpawnGroup)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaSpawnGroup.SelectForEditing),
            },
            [typeof(DataBlockAreaLocationTagged)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaLocationTagged.SelectForEditing),
            },
            [typeof(DataBlockAreaVolumeTagged)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaVolumeTagged.SelectForEditing),
            },
            [typeof(DataBlockAreaField)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaField.SelectForEditing),
            },
            [typeof(DataBlockAreaWaypointGroup)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaWaypointGroup.SelectForEditing),
            },
        };

        // All buttons in a button group must have the ReadOnlyAttribute to be disabled.
        // If even one is missing that attribute, it doesn't get applied to any.
        static readonly Dictionary<Type, HashSet<string>> disabledButtonMap = new Dictionary<Type, HashSet<string>> ()
        {
            [typeof(DataContainerOverworldProvinceBlueprint)] = new HashSet<string> ()
            {
                nameof(DataContainerOverworldProvinceBlueprint.ReverseBorderPoints),
                nameof(DataContainerOverworldProvinceBlueprint.AddObjectiveSpawn),
            },
            [typeof(DataBlockAreaSpawnGroup)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaSpawnGroup.SnapToGrid),
                nameof(DataBlockAreaSpawnGroup.Ground),
                nameof(DataBlockAreaSpawnGroup.Linearize),
                nameof(DataBlockAreaSpawnGroup.Remove),
                nameof(DataBlockAreaSpawnGroup.RenameDuplicate),
                nameof(DataBlockAreaSpawnGroup.TagAsRoad),
            },
            [typeof(DataBlockAreaLocationTagged)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaLocationTagged.SnapToGrid),
                nameof(DataBlockAreaLocationTagged.Ground),
                nameof(DataBlockAreaLocationTagged.Remove),
                nameof(DataBlockAreaLocationTagged.Duplicate),
                nameof(DataBlockAreaLocationTagged.TagAsEscape),
                nameof(DataBlockAreaLocationTagged.TagAsDefenseTrigger),
                nameof(DataBlockAreaLocationTagged.TagAsDefenseOrigin),
            },
            [typeof(DataBlockAreaVolumeTagged)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaVolumeTagged.Remove),
                nameof(DataBlockAreaVolumeTagged.Duplicate),
            },
            [typeof(DataBlockAreaField)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaField.Remove),
                nameof(DataBlockAreaField.Duplicate),
            },
            [typeof(DataBlockAreaWaypointGroup)] = new HashSet<string> ()
            {
                nameof(DataBlockAreaWaypointGroup.Remove),
                nameof(DataBlockAreaWaypointGroup.RenameDuplicate),
            },
        };
    }
    
    #endif
}
