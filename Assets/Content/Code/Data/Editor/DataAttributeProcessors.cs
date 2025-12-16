using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using System;
using System.Reflection;
using PhantomBrigade.Data;
using PhantomBrigade.ModTools;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

using System.Linq;
using PhantomBrigade.Functions;
using PhantomBrigade.Functions.Equipment;

public static class AttributeExpressionUtility
{
    private static Dictionary<Type, Dictionary<string, MethodInfo>> methodCaches = new Dictionary<Type, Dictionary<string, MethodInfo>> ();
    private static BindingFlags bindingFlagsLocalPublic = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    private static BindingFlags bindingFlagsLocalPrivate = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    private static List<string> typeSequence = new List<string> ();

    public static IEnumerable<string> GetStringsFromParentProperty (InspectorProperty propertyRoot, string propertyName) =>
        GetStringsFromParentProperty (propertyRoot, propertyName, 1, false);    

    public static IEnumerable<string> GetStringsFromParentProperty (InspectorProperty propertyRoot, string propertyName, int depth) =>
        GetStringsFromParentProperty (propertyRoot, propertyName, depth, false);    
    
    public static IEnumerable<string> GetStringsFromParentProperty (InspectorProperty propertyRoot, string propertyName, int depth, bool isPublic)
    {
        return GetStringsFromParentMethod (propertyRoot, "get_" + propertyName, depth, isPublic);
    }
    
    
    
    public static IEnumerable<string> GetStringsFromParentMethod (InspectorProperty propertyRoot, string methodName) =>
        GetStringsFromParentMethod (propertyRoot, methodName, 1, false);
    
    public static IEnumerable<string> GetStringsFromParentMethod (InspectorProperty propertyRoot, string methodName, int depth) =>
        GetStringsFromParentMethod (propertyRoot, methodName, depth, false);    

    public static IEnumerable<string> GetStringsFromParentMethod (InspectorProperty propertyRoot, string methodName, int depth, bool isPublic)
    {
        if (propertyRoot == null || methodName == null)
            return null;

        var propertyTarget = propertyRoot.Parent;
        for (int i = 1; i < depth; ++i)
        {
            propertyTarget = propertyTarget.Parent;
            if (propertyTarget == null)
            {
                Debug.LogWarning ($"Failed to find parent property {depth} levels above root property {propertyRoot.Name}");
                return null;
            }
        }

        if (propertyTarget.ParentType == null || propertyTarget.ParentValueProperty == null)
        {
            Debug.LogWarning ($"Failed to find final encompassing parent above property {propertyTarget.Name}, {depth} levels up from root property {propertyRoot.Name}");
            return null;
        }

        var valueType = propertyTarget.ParentType;
        var value = Convert.ChangeType (propertyTarget.ParentValueProperty.ValueEntry.WeakSmartValue, valueType);
        
        if (value == null)
        {
            Debug.LogWarning ($"Failed to resolve typed object above property {propertyTarget.Name} using type {valueType}, {depth} levels up from root property {propertyRoot.Name}");
            return null;
        }
        
        Dictionary<string, MethodInfo> methodCache = null;
        bool methodCacheExists = methodCaches.ContainsKey (valueType);
        if (methodCacheExists)
            methodCache = methodCaches[valueType];

        MethodInfo methodInfo = null;
        var bindingFlags = isPublic ? bindingFlagsLocalPublic : bindingFlagsLocalPrivate;
        bool methodInfoExists = methodCacheExists && methodCache.ContainsKey (methodName);
        if (methodInfoExists)
            methodInfo = methodCache[methodName];
        else
            methodInfo = valueType.GetMethod (methodName, bindingFlags);

        if (methodInfo == null)
        {
            var list = new List<string> ();
            var methods = valueType.GetMethods (bindingFlags);
            foreach (var methodInfoOther in methods)
                list.Add (methodInfoOther.Name);

            Debug.LogWarning ($"Failed to find method {methodName} in type {valueType.Name} {depth} levels up from root property {propertyRoot.Name} | Methods: {list.ToStringFormatted ()}");
            return null;
        }

        if (!methodCacheExists)
        {
            methodCache = new Dictionary<string, MethodInfo> ();
            methodCaches.Add (valueType, methodCache);
        }

        if (!methodInfoExists)
            methodCache.Add (methodName, methodInfo);

        var output = methodInfo.Invoke (value, null);
        if (output == null)
        {
            Debug.LogWarning ($"Failed to get any output from method {valueType.Name}/{methodName} to IEnumerable<string>");
            return null;
        }

        var outputTyped = output as IEnumerable<string>;
        if (outputTyped == null)
        {
            Debug.LogWarning ($"Failed to cast output from method {valueType.Name}/{methodName} to IEnumerable<string>");
            return null;
        }

        return outputTyped;
    }
    
    

    
    private static Dictionary<string, Type> typesInDefaultAssembly = new Dictionary<string, Type> ();
    
    public static Type GetFirstTypeByName (string className)
    {
        if (string.IsNullOrEmpty (className))
            return null;

        if (typesInDefaultAssembly.TryGetValue (className, out var v))
            return v;
        
        var defaultAssembly = typeof (DataContainer).Assembly;
        Type typeMatched = defaultAssembly.GetType (className, false, true);
        if (typeMatched != null)
        {
            typesInDefaultAssembly[className] = typeMatched;
            return typeMatched;
        }

        var assemblyTypes = defaultAssembly.GetTypes ();
        for (int j = 0; j < assemblyTypes.Length; j++)
        {
            var typeCandidate = assemblyTypes[j];
            if (string.Equals (typeCandidate.Name, className, StringComparison.InvariantCultureIgnoreCase))
            {
                typeMatched = typeCandidate;
                typesInDefaultAssembly[className] = typeMatched;
                return typeMatched;
            }
        }

        Debug.LogWarning ($"Failed to find type {className} for inspector operation!");
        return null;
    }
    
    
    
    public static IEnumerable<string> GetStringsFromParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName) =>
        GetStringsFromParentTypeMethod (propertyRoot, typeName, methodName, false);
    
    public static IEnumerable<string> GetStringsFromParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic)
    {
        var output = GetValueFromParentTypeMethod (propertyRoot, typeName, methodName, isPublic);
        if (output == null)
            return null;
        
        var outputTyped = output as IEnumerable<string>;
        if (outputTyped == null)
        {
            Debug.LogWarning ($"Failed cast output from {typeName}/get_{methodName} to IEnumerable<string>");
            return null;
        }
        
        return outputTyped;
    }
    
    
    
    public static IEnumerable<string> GetStringsFromParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName) =>
        GetStringsFromParentTypeProperty (propertyRoot, typeName, methodName, false);    

    public static IEnumerable<string> GetStringsFromParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic)
    {
        var output = GetValueFromParentTypeMethod (propertyRoot, typeName, "get_" + methodName, isPublic);
        if (output == null)
            return null;
        
        var outputTyped = output as IEnumerable<string>;
        if (outputTyped == null)
        {
            Debug.LogWarning ($"Failed cast output from {typeName}/get_{methodName} to IEnumerable<string>");
            return null;
        }
        
        return outputTyped;
    }
    
    
    
    public static object GetValueFromParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName)
    {
        var output = GetValueFromParentTypeMethod (propertyRoot, typeName, methodName, false);
        return output;
    }
    
    public static object GetValueFromParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic)
    {
        var output = GetValueFromParentTypeMethod (propertyRoot, typeName, "get_" + methodName, isPublic);
        return output;
    }
    
    public static object GetValueFromParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName)
    {
        var output = GetValueFromParentTypeMethod (propertyRoot, typeName, "get_" + methodName, false);
        return output;
    }
    
    
    
    
    public static object GetValueFromParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic)
    {
        if (propertyRoot == null || string.IsNullOrEmpty (typeName) || string.IsNullOrEmpty (methodName))
            return null;

        var propertyTarget = propertyRoot.Parent;
        Type valueType = null;
        var typeConstructed = GetFirstTypeByName (typeName);
        
        typeSequence.Clear ();
        int depth = 0;
        while (true)
        {
            propertyTarget = propertyTarget.Parent;
            depth += 1;
            
            if (propertyTarget == null)
            {
                Debug.LogWarning ($"Failed search for type {typeName} method {methodName} | Couldn't find parent property {depth} levels above root property {propertyRoot.Name} | Type chain:\n{typeSequence.ToStringFormatted (true)}");
                return null;
            }

            if (depth > 50)
            {
                Debug.LogWarning ($"Failed search for type {typeName} method {methodName} | Exceeded maximum iteration depth of 50 | Type chain:\n{typeSequence.ToStringFormatted (true)}");
                return null;
            }
            
            if (propertyTarget.ParentType == null || propertyTarget.ParentValueProperty == null)
            {
                // Debug.LogWarning ($"Failed search for type {typeName} method {methodName} | Property {propertyTarget.Name} has no parent type | Type chain:\n{typeSequence.ToStringFormatted (true)}");
                return null;
            }
            
            // Break if we finally encounter the target type
            valueType = propertyTarget.ParentType;
            typeSequence.Add (valueType.Name);

            bool typeMatch = valueType.Name == typeName;
            if (!typeMatch && typeConstructed != null)
            {
                typeMatch = typeConstructed.IsAssignableFrom (valueType);
                valueType = typeConstructed;
            }
            
            if (typeMatch)
                break;
        }

        // Bail in case value type after the above loop is somehow null
        if (valueType == null)
            return null;

        var value1 = propertyTarget.ParentValueProperty.ValueEntry.WeakSmartValue;
        // var value = Convert.ChangeType (propertyTarget.ParentValueProperty.ValueEntry.WeakSmartValue, valueType);
        
        if (value1 == null)
        {
            Debug.LogWarning ($"Failed search for type {typeName} method {methodName} | Type match depth: {depth} | Couldn't convert property {propertyTarget.Name} using type {valueType}");
            return null;
        }
        
        Dictionary<string, MethodInfo> methodCache = null;
        bool methodCacheExists = methodCaches.ContainsKey (valueType);
        if (methodCacheExists)
            methodCache = methodCaches[valueType];

        MethodInfo methodInfo = null;
        var bindingFlags = isPublic ? bindingFlagsLocalPublic : bindingFlagsLocalPrivate;
        bool methodInfoExists = methodCacheExists && methodCache.ContainsKey (methodName);
        if (methodInfoExists)
            methodInfo = methodCache[methodName];
        else
            methodInfo = valueType.GetMethod (methodName, bindingFlags);

        if (methodInfo == null)
        {
            var list = new List<string> ();
            var methods = valueType.GetMethods (bindingFlags);
            foreach (var methodInfoOther in methods)
                list.Add (methodInfoOther.Name);

            Debug.LogWarning ($"Failed search for type {typeName} method {methodName} ({bindingFlags}) | Type match depth: {depth} | No method with desired name among discovered methods: {list.ToStringFormatted ()}");
            return null;
        }

        if (!methodCacheExists)
        {
            methodCache = new Dictionary<string, MethodInfo> ();
            methodCaches.Add (valueType, methodCache);
        }

        if (!methodInfoExists)
            methodCache.Add (methodName, methodInfo);

        var output = methodInfo.Invoke (value1, null);
        if (output == null)
        {
            // Debug.LogWarning ($"Failed search for type {typeName} method {methodName} ({bindingFlags}) | Type match depth: {depth} | Received null output from the target method");
            return null;
        }

        return output;
    }
}

public static class DropdownUtils
{
    public static IEnumerable<string> ParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName) =>
        AttributeExpressionUtility.GetStringsFromParentTypeProperty (propertyRoot, typeName, methodName, false);    
    
    public static IEnumerable<string> ParentTypeProperty (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic) =>
        AttributeExpressionUtility.GetStringsFromParentTypeMethod (propertyRoot, typeName, "get_" + methodName, isPublic);
    
    public static IEnumerable<string> ParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName) =>
        AttributeExpressionUtility.GetStringsFromParentTypeMethod (propertyRoot, typeName, methodName, false);    
    
    public static IEnumerable<string> ParentTypeMethod (InspectorProperty propertyRoot, string typeName, string methodName, bool isPublic) => 
        AttributeExpressionUtility.GetStringsFromParentTypeMethod (propertyRoot, typeName, methodName, isPublic);

    public static void DrawTexturePreview (string textureKey, string textureGroup, int previewHeight)
    {
        if (string.IsNullOrEmpty (textureKey))
            return;
        
        var texture = TextureManager.GetTexture (textureGroup, textureKey, false);
        if (texture == null)
            return;
        
        previewHeight = Mathf.Clamp (previewHeight, 16, 512);
        var width = Mathf.Min (GUIHelper.ContextWidth - 88f - 15f, previewHeight);
        var shrink = width / (float) texture.width;
        var height = texture.height * shrink;
            
        using (var horizontalScope = new GUILayout.HorizontalScope ())
        {
            GUILayout.Space (15f);
            using ( var verticalScope = new GUILayout.VerticalScope ())
            {
                GUILayout.Label (texture, GUILayout.Width (width), GUILayout.Height (height));
            }
        }
    }
}

public class AlwaysOnlySetValueOnConfirmValueDropdownAttributeProcessor : OdinAttributeProcessor
{
    public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
    {
        return true;
    }

    public override bool CanProcessSelfAttributes(InspectorProperty property)
    {
        return false;
    }

    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        foreach (var attr in attributes)
        {
            if (attr is ValueDropdownAttribute dropdown)
            {
                // dropdown.OnlyChangeValueOnConfirm = true;
            }
        }
    }
}

public class DictionaryDropdownAttributeProcessor : OdinAttributeProcessor<string>
{
    private static Type dictionaryStringKeyTag = typeof (DictionaryStringKeyTag);
    private static Type dictionaryStringValueTag = typeof (DictionaryStringValueTag);


    public override void ProcessSelfAttributes (InspectorProperty property, List<Attribute> attributes)
    {
        bool isKey = false;
        bool isValue = false;

        isKey = attributes.HasAttribute<DictionaryStringKeyTag> ();
        isValue = attributes.HasAttribute<DictionaryStringValueTag> ();

        if (!isKey && !isValue)
            return;

        var p1 = property;
        var p2 = p1.Parent;
        var p3 = p2.Parent;

        if (isKey)
        {
            bool isKeyDropdown = p3.Attributes.HasAttribute<DictionaryKeyDropdown> ();
            if (isKeyDropdown)
            {
                var d = p3.Attributes.GetAttribute<DictionaryKeyDropdown> ();
                // Debug.Log ($"Located a key dropdown tag on: {p3.ParentType.Name}/{p3.Name} (mode: {d.type}, expression: {(string.IsNullOrEmpty (d.expression) ? "none" : d.expression)})");

                if (d.type == DictionaryKeyDropdownType.Expression)
                {
                    var expression = d.expression;
                    if (!string.IsNullOrEmpty (expression))
                        SetupValueDropdown (attributes, expression, d.append);
                }
                else if (d.type == DictionaryKeyDropdownType.Socket)
                    SetupValueDropdown (attributes, "@DataMultiLinkerPartSocket.data.Keys", d.append);
                else if (d.type == DictionaryKeyDropdownType.SocketTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerPartSocket.tags", d.append);
                else if (d.type == DictionaryKeyDropdownType.Hardpoint)
                    SetupValueDropdown (attributes, "@DataMultiLinkerSubsystemHardpoint.data.Keys", d.append);
                else if (d.type == DictionaryKeyDropdownType.HardpointTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerSubsystemHardpoint.tags", d.append);
                else if (d.type == DictionaryKeyDropdownType.Stat)
                    SetupValueDropdown (attributes, "@DataMultiLinkerUnitStats.data.Keys", d.append);
                else if (d.type == DictionaryKeyDropdownType.ActionCustomInt)
                    SetupValueDropdown (attributes, "@ActionCustomIntKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.ActionCustomFloat)
                    SetupValueDropdown (attributes, "@ActionCustomFloatKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.ActionCustomVector)
                    SetupValueDropdown (attributes, "@ActionCustomVectorKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.ActionCustomString)
                    SetupValueDropdown (attributes, "@ActionCustomStringKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.PartCustomInt)
                    SetupValueDropdown (attributes, "@PartCustomIntKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.PartCustomFloat)
                    SetupValueDropdown (attributes, "@PartCustomFloatKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.PartCustomVector)
                    SetupValueDropdown (attributes, "@PartCustomVectorKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.PartCustomString)
                    SetupValueDropdown (attributes, "@PartCustomStringKeys.GetKeys ()", d.append);
                else if (d.type == DictionaryKeyDropdownType.UnitPresetTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerUnitPreset.tags", d.append);
                else if (d.type == DictionaryKeyDropdownType.UnitGroupTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerCombatUnitGroup.tags", d.append);
                else if (d.type == DictionaryKeyDropdownType.ScenarioTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerScenario.tags", d.append);
                else if (d.type == DictionaryKeyDropdownType.AreaTag)
                    SetupValueDropdown (attributes, "@DataMultiLinkerCombatArea.tags", d.append);
            }
        }

        if (isValue)
        {
            bool isValueDropdown = p3.Attributes.HasAttribute<DictionaryValueDropdown> ();
            if (isValueDropdown)
            {
                var d = p3.Attributes.GetAttribute<DictionaryValueDropdown> ();
                // Debug.Log ($"Located a value dropdown tag on: {p3.ParentType.Name}/{p3.Name} (type: {d.type}, expression: {(string.IsNullOrEmpty (d.expression) ? "none" : d.expression)})");

                if (d.type == DictionaryValueDropdownType.Expression)
                {
                    var expression = d.expression;
                    if (!string.IsNullOrEmpty (expression))
                        SetupValueDropdown (attributes, expression, d.append);
                }

                else if (d.type == DictionaryValueDropdownType.Socket)
                    SetupValueDropdown (attributes, "@DataHelperUnitEquipment.GetSockets ()", d.append);
                else if (d.type == DictionaryValueDropdownType.Hardpoint)
                    SetupValueDropdown (attributes, "@DataMultiLinkerSubsystemHardpoint.data.Keys", d.append);
                else if (d.type == DictionaryValueDropdownType.PartPreset)
                    SetupValueDropdown (attributes, $"@DataMultiLinkerPartPreset.data.Keys", d.append);
                else if (d.type == DictionaryValueDropdownType.PartPresetForSocketKey)
                    SetupValueDropdown (attributes, $"@DataHelperUnitEquipment.GetPartPresetsForSocket (Key, {(d.allowEmpty ? "true" : "false")})", d.append);
                else if (d.type == DictionaryValueDropdownType.Subsystem)
                    SetupValueDropdown (attributes, $"@DataMultiLinkerSubsystem.data.Keys", d.append);
                else if (d.type == DictionaryValueDropdownType.SubsystemForHardpointKey)
                    SetupValueDropdown (attributes, $"@DataHelperUnitEquipment.GetSubsystemsForHardpoint (Key, {(d.allowEmpty ? "true" : "false")})", d.append);
            }
        }
    }

    private void SetupValueDropdown (List<Attribute> attributes, string expression, bool append)
    {
        var a = new ValueDropdownAttribute (expression);
        a.AppendNextDrawer = append;
        attributes.Add (a);
    }
}

/*
public class EditableKeyValueStringResolver : OdinAttributeProcessor<EditableKeyValuePair<string, string>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        bool isKey = member.Name == "Key";
        bool isValue = member.Name == "Value";
        if (!isKey && !isValue)
            return;

        if (isKey)
            attributes.Add (new DictionaryStringKeyTag ());
        else if (isValue)
            attributes.Add (new DictionaryStringValueTag ());
    }
}
*/

public class EditableKeyStringResolver<T> : OdinAttributeProcessor<EditableKeyValuePair<string, T>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (member.Name == "Key")
            attributes.Add (new DictionaryStringKeyTag ());
    }
}



// Tor Vestergaard: Right that's an unfortunate hole in the generic matching, mostly related to performance
// and the fact that EditableKeyValuePair is a struct and not a class so you can't use it as a constraint on T.

// Tor Vestergaard: We only have so many matching rules and they're carefully curated to make sure the performance remains acceptable.
// You could, if you wanted, extend the type matching with a match rule of your own, but short of that, there's no other way.

// Try this if the amount of boilerplate grows too much:
// DefaultAttributeProcessorLocator.SearchIndex.IndexingRules.Add
//     (new TypeMatchIndexingRule("My Custom Rule", (ref TypeSearchInfo info, ref string errorMessage) =>
//         { ... // Match logic here }));

public class EditableKeyStringResolverBool : EditableKeyStringResolver<bool> { }
public class EditableKeyStringResolverInt : EditableKeyStringResolver<int> { }
public class EditableKeyStringResolverFloat : EditableKeyStringResolver<float> { }
public class EditableKeyStringResolverVector2 : EditableKeyStringResolver<Vector2> { }
public class EditableKeyStringResolverVector3 : EditableKeyStringResolver<Vector3> { }
public class EditableKeyStringResolverVector4 : EditableKeyStringResolver<Vector4> { }
public class EditableKeyStringResolverString : EditableKeyStringResolver<string> { }
public class EditableKeyStringResolverListString : EditableKeyStringResolver<List<string>> { }
public class EditableKeyStringResolverHashSetString : EditableKeyStringResolver<HashSet<string>> { }

public class EditableKeyStringResolverDB5 : EditableKeyStringResolver<DataBlockStatDistribution> { }
public class EditableKeyStringResolverDB6 : EditableKeyStringResolver<DataBlockStatDistributionSecondary> { }
public class EditableKeyStringResolverDB7 : EditableKeyStringResolver<DataBlockSavedPart> { }
public class EditableKeyStringResolverDB8 : EditableKeyStringResolver<DataBlockSavedSubsystem> { }
public class EditableKeyStringResolverDB9 : EditableKeyStringResolver<DataBlockPresetSubsystem> { }

public class EditableKeyStringResolverDB11 : EditableKeyStringResolver<DataBlockUnitPartOverride> { }
public class EditableKeyStringResolverDB12 : EditableKeyStringResolver<DataBlockUnitBlueprintSocket> { }
public class EditableKeyStringResolverDB13 : EditableKeyStringResolver<List<DataBlockSubsystemTagFilter>> { }
public class EditableKeyStringResolverDB14 : EditableKeyStringResolver<List<DataBlockPartTagFilter>> { }

public class EditableKeyStringResolverDB22 : EditableKeyStringResolver<DataBlockMemoryChangeInt> { }
public class EditableKeyStringResolverDB23 : EditableKeyStringResolver<DataBlockMemoryChangeFloat> { }

public class EditableKeyStringResolverDB24 : EditableKeyStringResolver<DataBlockAnimationStateAudioEvent> { }

public class EditableKeyStringResolverDB26 : EditableKeyStringResolver<DataBlockFactionPartFilters> { }
public class EditableKeyStringResolverDB27 : EditableKeyStringResolver<DataBlockUnitLiverySocket> { }

public class EditableKeyStringResolverDB28 : EditableKeyStringResolver<DataBlockUnitLiveryHardpoint> { }
public class EditableKeyStringResolverDB29 : EditableKeyStringResolver<DataBlockUnitLiveryPresetNode> { }
public class EditableKeyStringResolverDB30 : EditableKeyStringResolver<DataBlockUnitLiveryPresetSocket> { }
public class EditableKeyStringResolverDB31 : EditableKeyStringResolver<DataBlockResourceStatSource> { }
public class EditableKeyStringResolverDB32 : EditableKeyStringResolver<DataBlockVirtualResource> { }
public class EditableKeyStringResolverDB33 : EditableKeyStringResolver<DataBlockOverworldEventSubcheckInt> { }
public class EditableKeyStringResolverDB34 : EditableKeyStringResolver<DataBlockOverworldEventSubcheckFloat> { }

public class EditableKeyStringResolverDB35 : EditableKeyStringResolver<DataBlockSubsystemStat> { }
public class EditableKeyStringResolverDB36 : EditableKeyStringResolver<DataBlockUnitBlueprintTrainingSocket> { }
public class EditableKeyStringResolverDB37 : EditableKeyStringResolver<DataBlockUnitCompositeSpatialEffect> { }

public class EditableKeyStringResolverDB40 : EditableKeyStringResolver<ModConfigEditLinker> { }
public class EditableKeyStringResolverDB41 : EditableKeyStringResolver<ModConfigEditMultiLinker> { }
public class EditableKeyStringResolverDB42 : EditableKeyStringResolver<ModConfigEditSourceFileMultiLinker> { }

// Looks like TempKeyValuePair is severed from parent dictionary property so parentProperty.Parent is null
// This makes current processor setup break, so temp resolver is commented out for now

/*
public class TempKeyStringResolver<T> : OdinAttributeProcessor<TempKeyValuePair<string, T>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (member.Name == "Key")
            attributes.Add (new DictionaryStringKeyTag ());
    }
}

public class TempKeyStringResolverInt : TempKeyStringResolver<int> { }
public class TempKeyStringResolverFloat : TempKeyStringResolver<float> { }
public class TempKeyStringResolverVector2 : TempKeyStringResolver<Vector2> { }
public class TempKeyStringResolverVector3 : TempKeyStringResolver<Vector3> { }
public class TempKeyStringResolverVector4 : TempKeyStringResolver<Vector4> { }
public class TempKeyStringResolverString : TempKeyStringResolver<string> { }

public class TempKeyStringResolverDB1 : TempKeyStringResolver<DataBlockStatBase> { }
public class TempKeyStringResolverDB2 : TempKeyStringResolver<DataBlockStatModifier> { }
public class TempKeyStringResolverDB3 : TempKeyStringResolver<DataBlockStatRoot> { }
public class TempKeyStringResolverDB4 : TempKeyStringResolver<DataBlockStatMultiplier> { }
*/

public class EditableValueStringResolver<T> : OdinAttributeProcessor<EditableKeyValuePair<T, string>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (member.Name == "Value")
            attributes.Add (new DictionaryStringKeyTag ());
    }
}

public class EditableValueStringResolverInt : EditableValueStringResolver<int> { }
public class EditableValueStringResolverFloat : EditableValueStringResolver<float> { }
public class EditableValueStringResolverVector2 : EditableValueStringResolver<Vector2> { }
public class EditableValueStringResolverVector3 : EditableValueStringResolver<Vector3> { }
public class EditableValueStringResolverVector4 : EditableValueStringResolver<Vector4> { }
public class EditableValueStringResolverString : EditableValueStringResolver<string> { }
public class EditableValueStringResolverListString : EditableValueStringResolver<List<string>> { }


public class FoldoutReferenceResolver<T> : OdinAttributeProcessor<T> where T : class, new()
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (!attributes.HasAttribute<FoldoutReference> ())
            return;

        var source = attributes.GetAttribute<FoldoutReference> ();
        var fieldName = member.Name;
        // var fieldProperty = parentProperty.Children.Get (fieldName);
        // var fieldStatus = fieldProperty != null ? fieldProperty.BaseValueEntry.ValueState == PropertyValueState.NullReference ? "null" : "present" : "?";

        var foldoutGroupID = fieldName;
        var horGroupName = $"{foldoutGroupID}/hg";

        var fieldNameNice = ObjectNames.NicifyVariableName (fieldName).ToLowerInvariant ();
        var foldoutGroupLabel = $"@{fieldName} != null ? \"{fieldNameNice.FirstLetterToUpperCase ()}\" : \"No {fieldNameNice}\""; // —

        var foldoutAttr = new FoldoutGroupAttribute (foldoutGroupID, false);
        foldoutAttr.Expanded = false;
        foldoutAttr.GroupName = foldoutGroupLabel;
        foldoutAttr.GroupID = foldoutGroupID;

        // attributes.Add (new InfoBoxAttribute (fieldStatus));
        attributes.Add (foldoutAttr);
        attributes.Add (new HorizontalGroupAttribute (horGroupName));
        attributes.Add (new HideLabelAttribute ());
        attributes.Add (new HideReferenceObjectPickerAttribute ());
        attributes.Add (new ShowIfAttribute (fieldName));
        // attributes.Add (new GUIColorAttribute ($"@new Color (1f, 1f, 1f, {fieldName} != null ? 1f : 0.5f)"));
    }
}

public class InlineButtonClearResolver<T> : OdinAttributeProcessor<T> where T : class, new()
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (!attributes.HasAttribute<InlineButtonClear> ())
            return;

        var source = attributes.GetAttribute<InlineButtonClear> ();
        var fieldName = member.Name;
        var fieldType = member.GetReturnType ();
        var isString = fieldType != null && fieldType == typeof (string);

        var expAction = isString && !source.clearToNull ? $"@{fieldName} = string.Empty" : $"@{fieldName} = null";
        var expShowIf = isString && !source.clearToNull ? $"@!string.IsNullOrEmpty ({fieldName})" : $"@{fieldName} != null";

        var attr = new InlineButtonAttribute (expAction, "-");
        attr.ShowIf = expShowIf;

        attributes.Add (attr);
    }
}

public class FoldoutReferenceButtonResolver<T> : OdinAttributeProcessor<T> where T : class, new()
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (!attributes.HasAttribute<FoldoutReferenceButton> ())
            return;

        var source = attributes.GetAttribute<FoldoutReferenceButton> ();
        var fieldName = source.fieldName;
        // var fieldProperty = parentProperty.Children.Get ("fieldName");
        // var fieldStatus = fieldProperty != null ? fieldProperty.BaseValueEntry.ValueState == PropertyValueState.NullReference ? "null" : "present" : "?";

        var foldoutGroupID = fieldName;
        var horGroupName = $"{foldoutGroupID}/hg";

        var buttonArg = $"@DataEditor.GetToggleLabel ({fieldName})";

        // attributes.Add (new InfoBoxAttribute (fieldStatus));
        attributes.Add (new ButtonAttribute (buttonArg));
        attributes.Add (new HorizontalGroupAttribute (horGroupName, DataEditor.toggleButtonWidth));
    }
}

public class DropdownReferenceResolver<T> : OdinAttributeProcessor<T> where T : class, new()
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (!attributes.HasAttribute<DropdownReference> ())
            return;

        var source = attributes.GetAttribute<DropdownReference> ();
        var fieldName = member.Name;

        string hideIfCondition = null;
        if (attributes.HasAttribute<HideIfAttribute> ())
        {
            var hideIfAttribute = attributes.GetAttribute<HideIfAttribute> ();
            hideIfCondition = hideIfAttribute.Condition;
            attributes.Remove (hideIfAttribute);
        }

        ShowIfAttribute showIfAttribute = null;
        bool showIfAttributePresent = attributes.HasAttribute<ShowIfAttribute> ();

        if (!showIfAttributePresent)
        {
            string showIfCondition = hideIfCondition != null ? $"@{fieldName} != null && !({hideIfCondition})" : $"@{fieldName} != null";
            showIfAttribute = new ShowIfAttribute (showIfCondition);
            attributes.Add (showIfAttribute);
        }
        else
        {
            showIfAttribute = attributes.GetAttribute<ShowIfAttribute> ();
            string showIfCondition = showIfAttribute.Condition;

            if (!showIfCondition.StartsWith ("@"))
                showIfCondition = $"@{showIfCondition}";

            showIfCondition = $"{showIfCondition} && {fieldName} != null";

            if (hideIfCondition != null)
                showIfCondition = $"{showIfCondition} && !({hideIfCondition})";

            showIfAttribute.Condition = showIfCondition;
        }

        if (source.addBoxGroup)
        {
            string groupName = fieldName;
            if (!string.IsNullOrEmpty (source.boxGroupPrefix))
                groupName = $"{source.boxGroupPrefix}/{groupName}";
            if (attributes.HasAttribute<TabGroupAttribute> ())
            {
                var tabGroupAttribute = attributes.GetAttribute<TabGroupAttribute> ();
                groupName = $"{tabGroupAttribute.GroupID}/{groupName}";
            }
            
            attributes.Add (new BoxGroupAttribute (groupName, false));
        }

        attributes.Add (new InlineButtonAttribute ($"@{fieldName} = null", "×"));
    }
}

public class ColoredBoxGroupDrawer : OdinGroupDrawer<ColoredBoxGroupAttribute>
{
    private ValueResolver labelGetter;


    /// <summary>
    /// initialize values for colors, labels, etc
    /// </summary>
    protected override void Initialize()
    {
        labelGetter = ValueResolver.GetForString(Property, Attribute.LabelText ?? Attribute.GroupName);
    }


    /// <summary>
    /// Draw the stuff
    /// </summary>
    /// <param name="label">Label string</param>
    protected override void DrawPropertyLayout(GUIContent label)
    {
        GUIHelper.PushColor(new Color(Attribute.R, Attribute.G, Attribute.B, Attribute.A));
        labelGetter.DrawError();

        string headerLabel = null;
        if (Attribute.ShowLabel)
        {
            headerLabel = labelGetter.GetWeakValue () as string;
            if (string.IsNullOrEmpty(headerLabel))
            {
                headerLabel = "";
            }
        }

        SirenixEditorGUI.BeginBox();
        SirenixEditorGUI.BeginBoxHeader();
        GUIHelper.PopColor();

        SirenixEditorGUI.Title(headerLabel, null, TextAlignment.Left, false, Attribute.BoldLabel);
        SirenixEditorGUI.EndBoxHeader();

        for (int i = 0; i < Property.Children.Count; i++)
        {
            Property.Children[i].Draw();
        }

        SirenixEditorGUI.EndBox();
    }
}

sealed class DataFilterKeyValueDrawer<T, U> : OdinValueDrawer<T>
    where T : DataFilterKeyValuePair<U>
    where U : DataContainer, new ()
{
    protected override void DrawPropertyLayout (GUIContent label)
    {
        var v = ValueEntry.SmartValue;
        if (!v.foldoutUsed)
        {
            CallNextDrawer (label);
            return;
        }
        SirenixEditorGUI.BeginBox();
        SirenixEditorGUI.BeginBoxHeader();
        Property.State.Expanded = SirenixEditorGUI.Foldout(Property.State.Expanded, v.keyLast);
        SirenixEditorGUI.EndBoxHeader();
        SirenixEditorGUI.BeginFadeGroup (v.keyLast, Property.State.Expanded);
        if (Property.State.Expanded)
        {
            CallNextDrawer (label);
        }
        SirenixEditorGUI.EndFadeGroup();
        SirenixEditorGUI.EndBox();
    }
}

#if PB_MODSDK
sealed class FunctionSelector : OdinSelector<Type>
{
    public static void Draw<T> (List<T> items)
    {
        if (SirenixEditorGUI.ToolbarButton (SdfIconType.Plus))
        {
            var selector = new FunctionSelector (typeof(T));
            selector.EnableSingleClickToSelect ();
            selector.SelectionConfirmed += col => selector.AddSelectedFunction (items, col.FirstOrDefault ());

            // We have to use a fixed height flyout because of how autosize and single click
            // don't play well together.
            var size = selector.CalcSize ();
            var rect = GUIHelper.GetCurrentLayoutRect ();
            var pos = new Vector2 (rect.x + rect.width - size.x, rect.y);
            var rectSelector = new Rect (pos, size);
            var sr = GUIUtility.GUIToScreenRect (rectSelector);
            var y = SirenixEditorGUI.currentDrawingToolbarHeight;
            if (sr.yMax > Screen.height)
            {
                pos.y = rect.y - y - size.y;
            }
            sr = new Rect (pos, new Vector2 (y, y));

            selector.ShowInPopup (sr, size);
        }
    }

    protected override void BuildSelectionTree (OdinMenuTree tree)
    {
        tree.Config.UseCachedExpandedStates = false;
        tree.Config.SelectMenuItemsOnMouseDown = true;
        tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
        tree.Selection.SupportsMultiSelect = false;
        if (functions.Count == 0)
        {
            return;
        }

        Func<Type, string> pf = NameOnly;
        if (isNamespaced)
        {
            pf = UseNamespace;
        }
        tree.AddRange (functions, pf);
    }

    static string NameOnly (Type t) => t.Name;
    static string UseNamespace (Type t) => t.Namespace + "/" + t.Name;

    void AddSelectedFunction<T> (List<T> items, Type selected)
    {
        if (selected == null)
        {
            return;
        }

        var ctor = selected.GetConstructor (new Type[] { });
        if (ctor == null)
        {
            Debug.LogWarning ("No default constructor for function | name: " + selected.FullName);
            return;
        }

        var f = (T)ctor.Invoke (new object[] { });
        items.Add (f);
    }

    Vector2 CalcSize ()
    {
        var s = isNamespaced && longestNamespace.Length > longestName.Length ? longestNamespace : longestName;
        var size = SirenixGUIStyles.ListItem.CalcSize (GUIHelper.TempContent (s));
        var x = size.x + (isNamespaced ? paddingListItemNamespace : paddingListItemName);
        var y = Mathf.Max (size.y, EditorGUIUtility.singleLineHeight) * Mathf.Min (functions.Count + 1.5f, lineCount);
        return new Vector2 (x, y);
    }

    static bool FilterInterface (Type t, object o) => t == (Type)o;

    FunctionSelector (Type functionInterfaceType)
    {
        functions = AppDomain.CurrentDomain
            .GetAssemblies ()
            .SelectMany (assy => assy.GetTypes ())
            .Where (t => t.IsClass)
            .Where (t => t.FindInterfaces (FilterInterface, functionInterfaceType).Length != 0)
            .ToList ();

        longestNamespace = "";
        longestName = functions.Count == 0 ? "" : functions[0].Name;
        if (functions.Count > 1)
        {
            var ns = functions[0].Namespace;
            isNamespaced = functions.Any (t => t.Namespace != ns);
            foreach (var t in functions)
            {
                if ((t.Namespace?.Length ?? 0) > longestNamespace.Length)
                {
                    longestNamespace = t.Namespace;
                }
                if (t.Name.Length > longestName.Length)
                {
                    longestName = t.Name;
                }
            }
        }
    }

    readonly List<Type> functions;
    readonly bool isNamespaced;
    readonly string longestNamespace;
    readonly string longestName;

    const float paddingListItemNamespace = 60f;
    const float paddingListItemName = 45;
    const float lineCount = 15f;
}

abstract class FunctionListAttributeProcessor<T> : OdinAttributeProcessor<List<T>>
{
    public override void ProcessSelfAttributes (InspectorProperty property, List<Attribute> attributes)
    {
        var hasAttr = attributes.HasAttribute<ListDrawerSettingsAttribute> ();
        var attr = hasAttr
            ? attributes.GetAttribute<ListDrawerSettingsAttribute> ()
            : new ListDrawerSettingsAttribute ();
        if (!hasAttr)
        {
            attributes.Add (attr);
        }
        attr.HideAddButton = true;
        var call = string.Format ("@{0}.{1}<{2}> ($value)", nameof(FunctionSelector), nameof(FunctionSelector.Draw), property.Info.TypeOfValue.GetGenericArguments()[0].Name);
        attr.OnTitleBarGUI = call;
    }
}

// TypeHinted interfaces
sealed class FunctionListAttributeProcessor02 : FunctionListAttributeProcessor<IOverworldActionFunction> { }
sealed class FunctionListAttributeProcessor03 : FunctionListAttributeProcessor<ICombatFunctionTargeted> { }
sealed class FunctionListAttributeProcessor04 : FunctionListAttributeProcessor<ICombatFunctionSpatial> { }
sealed class FunctionListAttributeProcessor05 : FunctionListAttributeProcessor<ICombatFunction> { }
sealed class FunctionListAttributeProcessor06 : FunctionListAttributeProcessor<IOverworldFunction> { }
sealed class FunctionListAttributeProcessor07 : FunctionListAttributeProcessor<ISubsystemFunctionGeneral> { }
sealed class FunctionListAttributeProcessor08 : FunctionListAttributeProcessor<ISubsystemFunctionTargeted> { }
sealed class FunctionListAttributeProcessor09 : FunctionListAttributeProcessor<ISubsystemFunctionAction> { }
sealed class FunctionListAttributeProcessor10 : FunctionListAttributeProcessor<ITargetModifierFunction> { }
sealed class FunctionListAttributeProcessor11 : FunctionListAttributeProcessor<ICombatActionExecutionFunction> { }
sealed class FunctionListAttributeProcessor12 : FunctionListAttributeProcessor<ICombatActionValidationFunction> { }
sealed class FunctionListAttributeProcessor13 : FunctionListAttributeProcessor<ICombatStateValidationFunction> { }
sealed class FunctionListAttributeProcessor15 : FunctionListAttributeProcessor<ICombatPositionValidationFunction> { }
sealed class FunctionListAttributeProcessor16 : FunctionListAttributeProcessor<ICombatUnitValidationFunction> { }
sealed class FunctionListAttributeProcessor17 : FunctionListAttributeProcessor<ICombatUnitValueResolver> { }
sealed class FunctionListAttributeProcessor18 : FunctionListAttributeProcessor<IPartGenStep> { }
sealed class FunctionListAttributeProcessor19 : FunctionListAttributeProcessor<IPartGenCheck> { }
#endif

// Looks like TempKeyValuePair is severed from parent dictionary property so parentProperty.Parent is null
// This makes current processor setup break, so temp resolver is commented out for now
/*
public class TempValueStringResolver<T> : OdinAttributeProcessor<TempKeyValuePair<T, string>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (member.Name == "Value")
            attributes.Add (new DictionaryStringValueTag ());
    }
}

public class TempValueStringResolverInt : TempValueStringResolver<int> { }
public class TempValueStringResolverFloat : TempValueStringResolver<float> { }
public class TempValueStringResolverVector2 : TempValueStringResolver<Vector2> { }
public class TempValueStringResolverVector3 : TempValueStringResolver<Vector3> { }
public class TempValueStringResolverVector4 : TempValueStringResolver<Vector4> { }
public class TempValueStringResolverString : TempValueStringResolver<string> { }
*/


// This almost works, but fails at final LINQ cast where null turns up
// Not sure what's up there
/*
public static class DictionaryKeyDropdownProvider
{
    public static Dictionary<string, Func<IEnumerable<string>>> providers = new Dictionary<string, Func<IEnumerable<string>>>
    {
        { (typeof (DataBlockStatBase)).Name, GetUnitStats },
        { (typeof (DataBlockStatMultiplier)).Name, GetUnitStats },
        { (typeof (DataBlockStatRoot)).Name, GetUnitStats },
        { (typeof (DataBlockStatModifier)).Name, GetUnitStats },
    };

    public static Func<IEnumerable<string>> GetDropdown (string key) =>
        !string.IsNullOrEmpty (key) && providers.ContainsKey (key) ? providers[key] : GetFallback;

    public static IEnumerable<string> GetUnitStats () => DataMultiLinkerUnitStats.data.Keys;

    private static List<string> fallback = new List<string> ();
    public static IEnumerable<string> GetFallback () => fallback;
}

public class TempKeyDropdownResolver<T> : OdinAttributeProcessor<TempKeyValuePair<string, T>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            var typeName = (typeof (T)).Name;
            var expression = $"@DictionaryKeyDropdownProvider.GetDropdown (\"{typeName}\")";
            attributes.Add (new ValueDropdownAttribute (expression));
            Debug.Log ($"Created value dropdown attribute with expression {expression}");
        }
    }
}

public class TempKeyDropdownResolverInt : TempKeyDropdownResolver<int> { }
public class TempKeyDropdownResolverFloat : TempKeyDropdownResolver<float> { }
public class TempKeyDropdownResolverVector2 : TempKeyDropdownResolver<Vector2> { }
public class TempKeyDropdownResolverVector3 : TempKeyDropdownResolver<Vector3> { }
public class TempKeyDropdownResolverVector4 : TempKeyDropdownResolver<Vector4> { }
public class TempKeyDropdownResolverString : TempKeyDropdownResolver<string> { }

public class TempKeyDropdownResolverDB1 : TempKeyDropdownResolver<DataBlockStatBase> { }
public class TempKeyDropdownResolverDB2 : TempKeyDropdownResolver<DataBlockStatModifier> { }
public class TempKeyDropdownResolverDB3 : TempKeyDropdownResolver<DataBlockStatRoot> { }
public class TempKeyDropdownResolverDB4 : TempKeyDropdownResolver<DataBlockStatMultiplier> { }
public class TempKeyDropdownResolverDB5 : TempKeyDropdownResolver<DataBlockUnitStatDistribution> { }
public class TempKeyDropdownResolverDB6 : TempKeyDropdownResolver<DataBlockUnitStatDistributionSecondary> { }
public class TempKeyDropdownResolverDB7 : TempKeyDropdownResolver<DataBlockSavedPart> { }
public class TempKeyDropdownResolverDB8 : TempKeyDropdownResolver<DataBlockSavedSubsystem> { }
*/

/*
public class TempValueReferenceResolver<T> : OdinAttributeProcessor<TempKeyValuePair<string, T>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Value")
            attributes.Add (new HideReferenceObjectPickerAttribute ());
    }
}

public class TempValueReferenceResolverInt : TempValueReferenceResolver<int> { }
public class TempValueReferenceResolverFloat : TempValueReferenceResolver<float> { }
public class TempValueReferenceResolverVector2 : TempValueReferenceResolver<Vector2> { }
public class TempValueReferenceResolverVector3 : TempValueReferenceResolver<Vector3> { }
public class TempValueReferenceResolverVector4 : TempValueReferenceResolver<Vector4> { }
public class TempValueReferenceResolverString : TempValueReferenceResolver<string> { }

public class TempValueReferenceResolverDB3 : TempValueReferenceResolver<DataBlockStatRoot> { }
public class TempValueReferenceResolverDB4 : TempValueReferenceResolver<DataBlockStatMultiplier> { }
public class TempValueReferenceResolverDB5 : TempValueReferenceResolver<DataBlockUnitStatDistribution> { }
public class TempValueReferenceResolverDB6 : TempValueReferenceResolver<DataBlockUnitStatDistributionSecondary> { }
public class TempValueReferenceResolverDB7 : TempValueReferenceResolver<DataBlockSavedPart> { }
public class TempValueReferenceResolverDB8 : TempValueReferenceResolver<DataBlockSavedSubsystem> { }
*/

/*
public class OnInspectorGUIStartDrawer : OdinValueDrawer<OnInspectorGUIStart>
{
    private InspectorPropertyValueGetter<int> valueGetter;

    protected override void Initialize ()
    {
        this.valueGetter = new InspectorPropertyValueGetter<int> (this.Property, "OnInspectorGUIStart.expression");
        var value = this.valueGetter.GetValue ();
        SkipWhenDrawing = true;
    }

    protected override void DrawPropertyLayout (GUIContent label)
    {
        var value = this.valueGetter.GetValue ();
    }
}
*/

/*
public class StringToFloatResolver : OdinAttributeProcessor<EditableKeyValuePair<string, float>>
{
    private static Type typeofDataContainerUnitStat = typeof (DataContainerUnitStat);

    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty != null && parentProperty.Parent != null)
        {
            var parentType = parentProperty.Parent.ParentType;
            if (parentType == typeofDataContainerUnitStat)
            {
                if (member.Name == "Key")
                    attributes.Add (new ValueDropdownAttribute ("@DataHelperUnitEquipment.GetSockets ()"));
            }
        }
    }
}
*/

/*
public class DataBlockStatModifierTempResolver : OdinAttributeProcessor<TempKeyValuePair<string, DataBlockStatModifier>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            attributes.Add (new ValueDropdownAttribute ("@DataMultiLinkerUnitStats.data.Keys"));
        }
        else if (member.Name == "Value")
        {
            attributes.Add (new HideReferenceObjectPickerAttribute ());
        }
    }
}

public class DataBlockStatModifierEditResolver : OdinAttributeProcessor<EditableKeyValuePair<string, DataBlockStatModifier>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            attributes.Add (new ValueDropdownAttribute ("@DataMultiLinkerUnitStats.data.Keys"));
        }
        else if (member.Name == "Value")
        {
            attributes.Add (new HideReferenceObjectPickerAttribute ());
        }
    }
}

public class DataBlockStatBaseTempResolver : OdinAttributeProcessor<TempKeyValuePair<string, DataBlockStatBase>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            attributes.Add (new ValueDropdownAttribute ("@DataMultiLinkerUnitStats.data.Keys"));
        }
        else if (member.Name == "Value")
        {
            attributes.Add (new HideReferenceObjectPickerAttribute ());
        }
    }
}

public class DataBlockStatBaseEditResolver : OdinAttributeProcessor<EditableKeyValuePair<string, DataBlockStatBase>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            attributes.Add (new ValueDropdownAttribute ("@DataMultiLinkerUnitStats.data.Keys"));
        }
        else if (member.Name == "Value")
        {
            attributes.Add (new HideReferenceObjectPickerAttribute ());
        }
    }
}

public class DataContainerEventOutcomeResolver : OdinAttributeProcessor<EditableKeyValuePair<string, DataContainerEventOutcome>>
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (member.Name == "Key")
        {
            attributes.Add (new GUIColorAttribute("@Value.GetColor()"));
        }
    }
}
*/
