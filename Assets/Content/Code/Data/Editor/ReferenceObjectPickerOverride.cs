#if UNITY_EDITOR


/*
public class ReferenceObjectPickerOverride: OdinAttributeProcessor
{
    public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member) => false;

    public override bool CanProcessSelfAttributes(InspectorProperty property)
    {
        return
            property.BaseValueEntry != null &&
            property.Info.SerializationBackend != SerializationBackend.Unity &&
            !property.BaseValueEntry.BaseValueType.IsEnum &&
            !property.BaseValueEntry.BaseValueType.IsValueType && 
            property.BaseValueEntry.BaseValueType != typeof(string) &&
            !property.Attributes.HasAttribute<HideReferenceObjectPickerAttribute>() && 
            property.BaseValueEntry.BaseValueType.GetCustomAttribute<HideReferenceObjectPickerAttribute> () == null;
    }

    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        if (!attributes.HasAttribute<TypeFilterAttribute>())
        {
            attributes.Add(new TypeFilterAttribute($"@{nameof(ReferenceObjectPickerOverride)}.{nameof(GetAssignableTypes)}($property)"));
            attributes.Add(new InlineButtonClear());
        }
    }

    public static IEnumerable<Type> GetAssignableTypes(InspectorProperty property)
    {
        var t = property.BaseValueEntry.BaseValueType;

        if (!t.IsAbstract && !t.IsInterface)
            yield return t;

        var types = TypeCache.GetTypesDerivedFrom(t);
        foreach (var item in types.Where(x => !x.IsAbstract))
            yield return item;
    }
}
*/
#endif