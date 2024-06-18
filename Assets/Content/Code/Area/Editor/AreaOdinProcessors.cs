using System;
using System.Collections.Generic;
using System.Reflection;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEngine;

namespace Area
{
    sealed class AreaConfigurationDataAttributeProcessor : OdinAttributeProcessor<AreaConfigurationData>
    {
        public override void ProcessSelfAttributes (InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add (new HideReferenceObjectPickerAttribute ());
        }

        public override void ProcessChildMemberAttributes (
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            attributes.Add (new DisplayAsStringAttribute ());
        }
    }

    sealed class AreaConfigurationDataProcessor : OdinPropertyProcessor<AreaConfigurationData>
    {
        public override void ProcessMemberProperties (List<InspectorPropertyInfo> propertyInfos)
        {
            var compactFormat = Property.Parent.GetAttribute<ConfigurationData.CompactFormatAttribute> () != null;
            if (compactFormat)
            {
                propertyInfos.Clear ();
                propertyInfos.AddValue
                (
                    "value",
                    (ref AreaConfigurationData data) => data.configuration,
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute (),
                    new TableColumnWidthAttribute (50, false)
                );
                propertyInfos.AddValue
                (
                    "configuration",
                    (ref AreaConfigurationData data) => data.configurationAsString,
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute ()
                );
                propertyInfos.AddValue
                (
                    "reference",
                    (ref AreaConfigurationData data) => TilesetUtility.GetStringFromConfiguration (
                        AreaTilesetHelper.configurationCollapseMap != null
                            ? AreaTilesetHelper.configurationCollapseMap[data.configuration]
                            : data.configuration),
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute ()
                );
                propertyInfos.AddValue
                (
                    "RR",
                    (ref AreaConfigurationData data) => data.requiredRotation,
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute (),
                    new TableColumnWidthAttribute (40, false)
                );
                propertyInfos.AddValue
                (
                    "RF",
                    (ref AreaConfigurationData data) => data.requiredFlippingZ ? "Y" : "N",
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute (),
                    new TableColumnWidthAttribute (35, false)
                );
                propertyInfos.AddValue
                (
                    "CR",
                    (ref AreaConfigurationData data) => data.customRotationPossible ? "Y" : "N",
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute (),
                    new TableColumnWidthAttribute (35, false)
                );
                propertyInfos.AddValue
                (
                    "CFM",
                    (ref AreaConfigurationData data) => data.customFlippingMode,
                    null,
                    new DisplayAsStringAttribute (),
                    new EnableGUIAttribute (),
                    new TableColumnWidthAttribute (40, false)
                );
                return;
            }

            for (var i = 0; i < propertyInfos.Count; i += 1)
            {
                var propInfo = propertyInfos[i];
                if (propInfo.PropertyName != nameof(AreaConfigurationData.requiredRotation))
                {
                    continue;
                }
                var getterSetter = new GetterSetter<AreaConfigurationData, string> (
                    (ref AreaConfigurationData data) =>
                    {
                        var reference = AreaTilesetHelper.configurationCollapseMap != null
                            ? AreaTilesetHelper.configurationCollapseMap[data.configuration]
                            : data.configuration;
                        return string.Format("{0} ({1})", reference, TilesetUtility.GetStringFromConfiguration(reference));
                    },
                    null);
                var newProp = InspectorPropertyInfo.CreateValue
                (
                    "Reference",
                    propInfo.Order,
                    SerializationBackend.None,
                    getterSetter,
                    new List<Attribute>()
                    {
                        new DisplayAsStringAttribute (),
                        new EnableGUIAttribute (),
                    });
                propertyInfos.Insert (i, newProp);
                i += 1;
            }
        }
    }

    sealed class DepthColorLinkAttributeProcessor : OdinAttributeProcessor<AreaManager.DepthColorLink>
    {
        public override void ProcessChildMemberAttributes (
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.Name == nameof(AreaManager.DepthColorLink.color))
            {
                return;
            }
            attributes.Add (new DisplayAsStringAttribute ());
        }
    }

    sealed class AreaManagerAttributeProcessor : OdinAttributeProcessor<AreaManager>
    {
        public override void ProcessChildMemberAttributes (
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.Name != nameof(AreaManager.areaName))
            {
                return;
            }
            attributes.Add (new ReadOnlyAttribute ());
        }
    }

    sealed class AreaManagerProcessor : OdinPropertyProcessor<AreaManager>
    {
        public override void ProcessMemberProperties (List<InspectorPropertyInfo> propertyInfos)
        {
            for (var i = 0; i < propertyInfos.Count; i += 1)
            {
                var propInfo = propertyInfos[i];
                if (propInfo.PropertyName == nameof(AreaManager.areaName))
                {
                    propertyInfos.Insert (0, propInfo);
                    propertyInfos.RemoveAt (i + 1);
                    break;
                }
            }
        }
    }

    sealed class ModeButtonsToolbarAttributeProcessor : OdinAttributeProcessor<ModeButtons>
    {
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member) =>
            parentProperty.GetAttribute<AreaSceneModeToolbarAttribute> () != null;

        public override void ProcessChildMemberAttributes (
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            var label = "";
            var enableIf = "";
            foreach (var attr in attributes)
            {
                if (!(attr is ButtonLabelsAttribute buttonLabels))
                {
                    continue;
                }
                label = buttonLabels.Multiline;
                enableIf = buttonLabels.EnableIf;
                break;
            }
            if (!string.IsNullOrEmpty (label))
            {
                attributes.Add (new AreaSceneModeToolbarButtonAttribute (label, nameof(ModeButtons.mode))
                {
                    EnableIf = enableIf,
                });
                attributes.Add (new EnableGUIAttribute ());
            }
        }
    }

    sealed class ModeButtonsSurrogateGroupAttributeProcessor : OdinAttributeProcessor<ModeButtons>
    {
        public override bool CanProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member) =>
            parentProperty.GetAttribute<InspectorSurrogateGroupAttribute> () != null;

        public override void ProcessChildMemberAttributes (
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            var label = "";
            var enableIf = "";
            foreach (var attr in attributes)
            {
                if (attr is ButtonLabelsAttribute buttonLabels)
                {
                    label = buttonLabels.Oneline;
                    enableIf = buttonLabels.EnableIf;
                    break;
                }
            }
            if (!string.IsNullOrEmpty (label))
            {
                attributes.Add (new PropertySpaceAttribute (-2f));
                attributes.Add (new InspectorSurrogateGroupButtonAttribute(label, nameof(ModeButtons.mode))
                {
                    EnableIf = enableIf,
                });
                attributes.Add (new EnableGUIAttribute ());
            }
        }
    }

    sealed class ModeButtonsSurrogateProcessor : OdinPropertyProcessor<ModeButtons>
    {
        public override bool CanProcessForProperty (InspectorProperty property) => property.GetAttribute<InspectorSurrogateGroupAttribute> () != null;

        public override void ProcessMemberProperties (List<InspectorPropertyInfo> propertyInfos)
        {
            var row = 1;
            var bpr = Property.GetAttribute<InspectorSurrogateGroupAttribute> ().ButtonsPerRow;
            var stop = propertyInfos.Count % bpr == 1 ? propertyInfos.Count - bpr - 1 : propertyInfos.Count;
            for (var i = 0; i < stop; i += 1)
            {
                var propInfo = propertyInfos[i];
                var attrList = propInfo.GetEditableAttributesList ();
                attrList.Add (new HorizontalGroupAttribute (groupPrefix + row) { Gap = 2f });
                row += i % bpr == bpr - 1 ? 1 : 0;
            }

            bpr -= 1;
            for (int i = Mathf.Max (stop, 0), r = 0; i < propertyInfos.Count; i += 1, r += 1)
            {
                var propInfo = propertyInfos[i];
                var attrList = propInfo.GetEditableAttributesList ();
                attrList.Add (new HorizontalGroupAttribute (groupPrefix + row) { Gap = 2f });
                row += r % bpr == bpr - 1 ? 1 : 0;
            }
        }

        const string groupPrefix = "Buttons Row ";
    }
}
