using System;
using System.Collections.Generic;
using System.Linq;
using PhantomBrigade.Mods;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Debug = UnityEngine.Debug;
using System.Reflection;
using HarmonyLib;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

#if PB_MODSDK
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PhantomBrigade.ModTools;
using PhantomBrigade.SDK.ModTools;
#else
using PhantomBrigade.TextPipeline;
#endif

namespace PhantomBrigade.Data
{
    public static class DataEditor
    {
        public const float dictionaryKeyWidth = 300f;
        public const float dictionaryKeyWidthCompact = 200f;
        public const float toggleButtonWidth = 30f;

        [HideInInspector]
        public static bool showLibraryText = true;

        public const string textAttrArg = "@DataEditor.showLibraryText";

        #if !PB_MODSDK
        public static INGUIAtlas atlasLast;
        #endif

        public static Color GetColorFromElementIndex (int index, float alpha = 0.2f, float hueStart = 0.6f)
        {
            var b = index % 2 == 0 ? 0.65f : 0.8f;
            var col = Color.HSVToRGB (Mathf.Abs (hueStart - (float)index / 12f) % 1f, 0.25f, b).WithAlpha (alpha);
            return col;
        }

        public static Color GetColorFromElementIndexBright (int index, float hueStart = 0.6f, float saturation = 0.25f)
        {
            var col = Color.HSVToRGB (Mathf.Abs (hueStart - (float)index / 12f) % 1f, saturation, 1f).WithAlpha (1f);
            return col;
        }

        #if !PB_MODSDK
        public static void DrawSpritePreview (string spriteName, bool previewUniform = false, float previewSize = 16f, INGUIAtlas atlas = null)
        {
            if (atlas == null)
                atlas = atlasLast;

            if (atlas == null)
                return;

            atlasLast = atlas;

            var sprite = atlas.GetSprite (spriteName);
            if (sprite == null)
                return;

            var tex = atlas.texture as Texture2D;
            if (tex == null)
                return;

            var width = previewUniform ? previewSize : Mathf.Clamp (sprite.width, 16f, previewSize);
            var height = previewUniform ? previewSize : Mathf.Clamp (sprite.height, 16f, previewSize);

            GUILayout.Space (6f);
            var rectFull = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.Width (width), GUILayout.Height (height));
            var rect = new Rect (rectFull.xMin, rectFull.yMin + 4f, rectFull.width, rectFull.height - 8f);
            GUILayout.Space (4f);

            Rect uv = new Rect (sprite.x, sprite.y, sprite.width, sprite.height);
            uv = NGUIMath.ConvertToTexCoords (uv, tex.width, tex.height);

            // Calculate the texture's scale that's needed to display the sprite in the clipped area
            float scaleX = rect.width / uv.width;
            float scaleY = rect.height / uv.height;

            // Stretch the sprite so that it will appear proper
            float aspect = (scaleY / scaleX) / ((float) tex.height / tex.width);
            Rect clipRect = rect;

            if (aspect != 1f)
            {
                if (aspect < 1f)
                {
                    // The sprite is taller than it is wider
                    float padding = width * (1f - aspect) * 0.5f;
                    clipRect.xMin += padding;
                    clipRect.xMax -= padding;
                }
                else
                {
                    // The sprite is wider than it is taller
                    float padding = height * (1f - 1f / aspect) * 0.5f;
                    clipRect.yMin += padding;
                    clipRect.yMax -= padding;
                }
            }

            GUI.DrawTextureWithTexCoords (clipRect, tex, uv);
        }
        #endif

        [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
        public class SpriteNameAttribute : Attribute
        {
            public bool uniform = true;
            public float size = 16f;

            public SpriteNameAttribute (bool uniform, float size)
            {
                this.uniform = uniform;
                this.size = size;
            }
        }

        #if UNITY_EDITOR

        public static void ClearValue (EditableKeyValuePair<string, string> pair)
        {
            pair.Value = null;
        }

        public const string toggleLabelPath = "@DataEditor.GetToggleLabel";

        public static string GetToggleLabel (object obj) =>
            obj == null ? "+" : "×";

        public const string foldoutGroupArgument =
            "@$property.ValueEntry.ValueState == PropertyValueState.NullReference ? \"Null\" : \"Used\"";

        public static void ToggleReferenceField<T> (ref T arg) where T : class, new ()
        {
            arg = arg == null ? new T () : null;
        }

        public static void ToggleReferenceField<T> (T obj, FieldInfo fieldInfo) where T : class, new ()
        {
            if (fieldInfo == null || obj == null || fieldInfo.FieldType != typeof (T))
                return;

            var valueCurrent = fieldInfo.GetValue (obj);
            var valueNew = valueCurrent == null ? new T () : null;
            fieldInfo.SetValue (obj, valueNew);
        }

        public static Type typeAttributeDropdownReference = typeof (DropdownReference);
        private static Type typeReferenceDropdownLast = null;
        private static List<FieldInfo> fieldsDropdownReference;
        private static List<string> textDropdownReference = new List<string> ();

        public static List<string> GetReferenceDropdown (object obj)
        {
            if (obj == null)
            {
                Debug.LogWarning ("Received no object, can't compile list of names of valid fields");
                return textDropdownReference;
            }

            var type = obj.GetType ();

            textDropdownReference.Clear ();
            typeReferenceDropdownLast = type;
            fieldsDropdownReference = FieldReflectionUtility.GetPublicFieldsWithAttribute
            (
                type,
                typeAttributeDropdownReference
            );

            foreach (var fieldInfo in fieldsDropdownReference)
            {
                if (fieldInfo == null)
                    continue;

                var valueCurrent = fieldInfo.GetValue (obj);
                if (valueCurrent != null)
                    continue;

                var fieldName = fieldInfo.Name;
                // var fieldNameNice = ObjectNames.NicifyVariableName (fieldName).ToLowerInvariant ().FirstLetterToUpperCase ();
                textDropdownReference.Add (fieldName);
            }

            // Debug.LogWarning ($"Refreshed list of field names using type {type.Name} | Results: {textDropdownReference.Count}");
            return textDropdownReference;
        }

        public static void FillFromReferenceDropdown (object obj, string fieldName, ref string selectionField)
        {
            if (obj == null)
            {
                FillReferenceDropdownString (obj, ref selectionField);
                return;
            }

            var type = obj.GetType ();
            var fieldInfo = type.GetField (fieldName);
            if (fieldInfo == null)
            {
                Debug.LogWarning ($"Failed to get field {fieldName}");
                FillReferenceDropdownString (obj, ref selectionField);
                return;
            }

            var fieldType = fieldInfo.FieldType;
            if (fieldType.IsValueType)
            {
                Debug.LogWarning ($"Field {fieldName} is of value type and can't be used with this utility");
                FillReferenceDropdownString (obj, ref selectionField);
                return;
            }

            var valueCurrent = fieldInfo.GetValue (obj);
            if (valueCurrent != null)
            {
                FillReferenceDropdownString (obj, ref selectionField);
                return;
            }

            if (fieldType.IsInterface)
            {
                var typesPerInterface = FieldReflectionUtility.GetTypesPerInterface (fieldType);
                if (typesPerInterface != null && typesPerInterface.Length != 0)
                {
                    var valueType = typesPerInterface[0];
                    var valueNew = Activator.CreateInstance (valueType);
                    fieldInfo.SetValue (obj, valueNew);
                }
                else
                    Debug.LogWarning ($"Failed to find any implementations of interface {fieldType.Name} used by field {fieldName}");
            }
            else
            {
                bool isString = fieldType == typeof (string);
                var valueNew = isString ? string.Empty : Activator.CreateInstance (fieldType);
                TryFillCollectionInstance (valueNew, fieldType);
                fieldInfo.SetValue (obj, valueNew);
            }

            FillReferenceDropdownString (obj, ref selectionField);
        }

        public static void FillFields (object obj, bool recursive, bool overwrite = false)
        {
            if (obj == null)
            {
                return;
            }

            var objType = obj.GetType ();
            var objFields = objType.GetFields ();
            if (objFields.Length > 0)
            {
                foreach (var f in objFields)
                    FillField (obj, f.Name, recursive, overwrite);
            }
        }

        public static void FillField (object obj, string fieldName, bool recursive, bool overwrite = false)
        {
            if (obj == null)
            {
                return;
            }

            var type = obj.GetType ();
            var fieldInfo = type.GetField (fieldName);
            if (fieldInfo == null)
            {
                Debug.LogWarning ($"FillField | Object type: {type.Name} | Failed to get field {fieldName}");
                return;
            }

            var fieldType = fieldInfo.FieldType;
            if (fieldType.IsValueType)
            {
                Debug.LogWarning ($"FillField | Object type: {type.Name} | Field {fieldName} is of value type and can't be used with this utility");
                return;
            }

            var valueCurrent = fieldInfo.GetValue (obj);
            if (valueCurrent != null && !overwrite)
            {
                Debug.LogWarning ($"FillField | Object type: {type.Name} | Field {fieldName} is already filled");
                return;
            }

            object valueNew = null;
            if (fieldType.IsInterface)
            {
                var typesPerInterface = FieldReflectionUtility.GetTypesPerInterface (fieldType);
                if (typesPerInterface != null && typesPerInterface.Length != 0)
                {
                    var valueType = typesPerInterface[0];
                    valueNew = Activator.CreateInstance (valueType);
                    fieldInfo.SetValue (obj, valueNew);
                }
                else
                    Debug.LogWarning ($"Failed to find any implementations of interface {fieldType.Name} used by field {fieldName}");
            }
            else
            {
                bool isString = fieldType == typeof (string);
                valueNew = isString ? string.Empty : Activator.CreateInstance (fieldType);
                TryFillCollectionInstance (valueNew, fieldType, recursive);
                fieldInfo.SetValue (obj, valueNew);
            }

            if (recursive && valueNew != null)
                FillFields (valueNew, true);
        }

        private static Type typeIEnumerable = typeof (IEnumerable<>);
        private static Type typeList = typeof (List<>);
        private static Type typeHashset = typeof (HashSet<>);
        private static Type typeDictionary = typeof (Dictionary<,>);
        private static Type typeSortedDictionary = typeof (SortedDictionary<,>);

        private static void TryFillCollectionInstance (object obj, Type fieldType, bool recursive = false)
        {
            // TODO: Check for IEnumerable as well?
            if (fieldType.IsGenericType)
            {
                var fieldTypeGeneric = fieldType.GetGenericTypeDefinition ();
                var args = fieldType.GetGenericArguments ();
                var arg1 = args.Length > 0 ? args[0] : null;
                if (arg1 != null)
                {
                    // The whole purpose of everything below is invoking CreateInstance and that can't be done with abstract types
                    if (arg1.IsAbstract)
                        return;

                    if (fieldTypeGeneric == typeList)
                    {
                        Debug.Log ($"{fieldType.Name} is a List<T> with element type {arg1.Name}, adding an element");
                        var entry = arg1 == typeof (string) ? string.Empty : Activator.CreateInstance (arg1);
                        var addMethod = fieldType.GetMethod ("Add");
                        addMethod?.Invoke (obj, new[] { entry });
                        if (recursive)
                            FillFields (entry, true, true);
                    }
                    else if (fieldTypeGeneric == typeHashset)
                    {
                        Debug.Log ($"{fieldType.Name} is a HashSet<T> with element type {arg1.Name}, adding an element");
                        var entry = arg1 == typeof (string) ? string.Empty : Activator.CreateInstance (arg1);
                        var addMethod = fieldType.GetMethod ("Add");
                        addMethod?.Invoke (obj, new[] { entry });
                        if (recursive)
                            FillFields (entry, true, true);
                    }
                    else if (args.Length > 1)
                    {
                        var arg2 = args[1];
                        if (fieldTypeGeneric == typeDictionary)
                        {
                            Debug.Log ($"{fieldType.Name} is a Dictionary<T> with types {arg1.Name}, {arg2.Name}, adding an element");
                            var key = arg1 == typeof (string) ? string.Empty : Activator.CreateInstance (arg1);
                            var entry = arg2 == typeof (string) ? string.Empty : Activator.CreateInstance (arg2);
                            var addMethod = fieldType.GetMethod ("Add");
                            addMethod?.Invoke (obj, new[] { key, entry });
                            if (recursive)
                                FillFields (entry, true, true);
                        }
                        else if (fieldTypeGeneric == typeSortedDictionary)
                        {
                            Debug.Log ($"{fieldType.Name} is a SortedDictionary<T> with types {arg1.Name}, {arg2.Name}, adding an element");
                            var key = arg1 == typeof (string) ? string.Empty : Activator.CreateInstance (arg1);
                            var entry = arg2 == typeof (string) ? string.Empty : Activator.CreateInstance (arg2);
                            var addMethod = fieldType.GetMethod ("Add");
                            addMethod?.Invoke (obj, new[] { key, entry });
                            if (recursive)
                                FillFields (entry, true, true);
                        }
                        else
                        {
                            Debug.LogWarning ($"{fieldType.Name} is an unknown generic type with types {arg1.Name}, {arg2.Name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning ($"{fieldType.Name} is an unknown generic type with argument {arg1.Name}");
                    }
                }
                else
                {
                    Debug.LogWarning ($"{fieldType.Name} is an unknown generic type with no available arguments");
                }
            }
            else
            {
                // Debug.LogWarning ($"{fieldType.Name} is not a generic type");
            }
        }

        public static void FillReferenceDropdownString (object obj, ref string field)
        {
            int count = 0;
            if (obj != null)
            {
                var fieldNamesWithNullValue = GetReferenceDropdown (obj);
                if (fieldNamesWithNullValue != null)
                    count = fieldNamesWithNullValue.Count;
            }

            field = count > 0 ? " ←" : string.Empty; // " - All blocks used";
        }

        [HideReferenceObjectPicker, HideLabel, DisableContextMenu]
        public class DropdownReferenceHelper
        {
            private object obj;

            [ShowIf ("IsHelperVisible")]
            [HorizontalGroup ("Footer")]
            [HideLabel, ShowInInspector, DisplayAsString]
            private string filler = " ";

            [ShowIf ("IsHelperVisible")]
            [HorizontalGroup ("Footer", 240f)]
            [HideLabel, ShowInInspector]
            [ValueDropdown ("GetFieldNames", OnlyChangeValueOnConfirm = true)]
            [OnValueChanged ("OnFieldSelected")]
            [GUIColor ("color")]
            private string selectedField = "←";

            private static Color color = new Color (1f, 1f, 1f, 0.5f);

            private bool IsHelperVisible =>
                !selectedField.IsNullOrWhitespace ();

            private List<string> GetFieldNames =>
                DataEditor.GetReferenceDropdown (obj);

            private void OnFieldSelected () =>
                DataEditor.FillFromReferenceDropdown (obj, selectedField, ref selectedField);

            private object ClearField (string fieldName)
            {
                Debug.LogWarning ($"Clearing field {fieldName}");
                return null;
            }

            public DropdownReferenceHelper (object obj)
            {
                this.obj = obj;
                DataEditor.FillReferenceDropdownString (obj, ref selectedField);
            }
        }

        #endif
    }

    [HideReferenceObjectPicker, DisableContextMenu]
    public class DataKeyReplacement
    {
        [HorizontalGroup(0.5f)][HideLabel]
        public string key;

        [HorizontalGroup(0.5f)][HideLabel][GUIColor ("GetColor")]
        public string value;

        [HideInInspector]
        public bool collision = false;

        private Color GetColor () =>
            Color.HSVToRGB (collision ? 0f : 0.55f, 0.5f, 1f);
    }

    [HideReferenceObjectPicker, DisableContextMenu]
    public class DataFilterKeyValuePair<T> where T : DataContainer, new()
    {
        [HideInInspector]
        public string keyLast;

        [HideInInspector]
        public bool foldoutUsed;

        #if PB_MODSDK && UNITY_EDITOR
        [VerticalGroup ("key", Order = 0)]
        [OnValueChanged (nameof(OnKeyChange))]
        [HideLabel]
        [InlineButton (nameof(ApplyKeyChange), "Rename", ShowIf = nameof(showRename))]
        #endif
        public string key;

        #if PB_MODSDK && UNITY_EDITOR
        [ButtonGroup ("dupdel", VisibleIf = nameof(showDupDel), Order = 1)]
        [Button]
        public void Duplicate ()
        {
            if (parent == null)
            {
                return;
            }
            parent.DuplicateEntry (key, value);
        }

        [ButtonGroup ("dupdel")]
        [Button]
        public void Delete ()
        {
            if (parent == null)
            {
                return;
            }
            parent.DeleteEntry (key);
        }

        #endif

        [VerticalGroup ("value", Order = 2)]
        [HideLabel, DisableContextMenu, HideReferenceObjectPicker, Title (" ")]
        public T value;

        [HideInInspector]
        public DataMultiLinker<T> parent;

        #if UNITY_EDITOR
        #if PB_MODSDK
        bool showRename => DataContainerModData.hasSelectedConfigs && key != keyLast;
        bool showDupDel => DataContainerModData.hasSelectedConfigs;
        #else
        bool showRename => key != keyLast;
        bool showDupDel => true;
        #endif

        void OnKeyChange ()
        {
            if (key == keyLast)
            {
                DataMultiLinker<T>.isEntryKeyChanging = false;
                return;
            }
            DataMultiLinker<T>.isEntryKeyChanging = true;
        }

        void ApplyKeyChange ()
        {
            if (parent == null)
            {
                return;
            }
            if (parent.dataNonStatic.ContainsKey (key))
            {
                return;
            }

            #if PB_MODSDK
            DataMultiLinker<T>.isEntryKeyChanging = false;
            #endif

            parent.ReplaceKey (keyLast, key);
            keyLast = key;
        }

        #endif
    }

    public static class DataMultiLinkerUtility
    {
        public static Dictionary<Type, System.Action> callbacksOnAfterDeserialization = new Dictionary<Type, System.Action> ();
        public static Dictionary<Type, System.Action> callbacksOnBeforeSerialization = new Dictionary<Type, System.Action> ();
        public static Dictionary<Type, System.Action> callbacksOnAfterKeyChange = new Dictionary<Type, System.Action> ();
        public static Dictionary<Type, System.Action> callbacksOnBeforeTextExport = new Dictionary<Type, System.Action> ();
        public static Dictionary<Type, System.Action> callbacksOnAfterTextExport = new Dictionary<Type, System.Action> ();
        public static Dictionary<Type, System.Action> callbacksOnAfterTextLoad = new Dictionary<Type, System.Action> ();

        private static Dictionary<string, IDataMultiLinker> linkersFound = new Dictionary<string, IDataMultiLinker> ();

        public static void RegisterOnAfterDeserialization (Type type, System.Action action)
        {
            if (action != null && !callbacksOnAfterDeserialization.ContainsKey (type))
                callbacksOnAfterDeserialization.Add (type, action);
        }

        public static void RegisterOnBeforeSerialization (Type type, System.Action action)
        {
            if (action != null && !callbacksOnBeforeSerialization.ContainsKey (type))
                callbacksOnBeforeSerialization.Add (type, action);
        }

        public static void RegisterOnAfterKeyChange (Type type, System.Action action)
        {
            if (action != null && !callbacksOnAfterKeyChange.ContainsKey (type))
                callbacksOnAfterKeyChange.Add (type, action);
        }

        public static void RegisterOnAfterTextLoad (Type type, System.Action action)
        {
            if (action != null && !callbacksOnAfterTextLoad.ContainsKey (type))
                callbacksOnAfterTextLoad.Add (type, action);
        }

        public static void RegisterOnTextExport (Type type, System.Action actionBefore, System.Action actionAfter)
        {
            if (actionBefore != null && !callbacksOnBeforeTextExport.ContainsKey (type))
                callbacksOnBeforeTextExport.Add (type, actionBefore);

            if (actionAfter != null && !callbacksOnAfterTextExport.ContainsKey (type))
                callbacksOnAfterTextExport.Add (type, actionAfter);
        }

        public static void RegisterStandardTextHandling (Type type, ref List<string> textSectorKeys, string sector)
        {
            bool supportsText = typeof (DataContainerWithText).IsAssignableFrom (type);
            if (!supportsText)
            {
                Debug.LogWarning ($"{type.Name}: Failed to register for text handling, this type doesn't inherit from DataContainerWithText");
                return;
            }

            // It's a bit of a pain to set this without a ref, since it's a static field on a specific version of DataMultiLinker<T>
            textSectorKeys = new List<string> { sector };

            RegisterOnTextExport
            (
                type,
                () => TextLibraryHelper.OnBeforeTextExport (type, sector),
                () => TextLibraryHelper.OnAfterTextExport (type, sector)
            );
        }

        private static Dictionary<Type, Type> dataTypeToLinkerType = new Dictionary<Type, Type> ();
        private static Dictionary<Type, IDataMultiLinker> dataTypeToLinkerInterface = new Dictionary<Type, IDataMultiLinker> ();
        private static Dictionary<string, IDataMultiLinker> dataTypeNameToLinkerInterface = new Dictionary<string, IDataMultiLinker> ();

        public static IDataMultiLinker FindLinkerAsInterface (Type dataType)
        {
            if (dataType == null)
                return null;

            return FindLinkerAsInterface (dataType.FullName);
        }

        public static IDataMultiLinker FindLinkerAsInterface (string dataTypeName)
        {
            if (string.IsNullOrEmpty (dataTypeName))
            {
                Debug.LogWarning ($"Failed to find linker: no type name provided");
                return null;
            }

            if (dataTypeNameToLinkerInterface.TryGetValue (dataTypeName, out var linkerExisting))
                return linkerExisting;

            var assembly = Assembly.GetExecutingAssembly ();
            var dataType = assembly.GetType (dataTypeName);
            if (dataType == null)
            {
                Debug.LogWarning ($"Failed to find linker for data type {dataTypeName}: given type doesn't exist");
                return null;
            }

            var linkerType = typeof (DataMultiLinker<>).MakeGenericType (new[] { dataType });
            dataTypeToLinkerType[dataType] = linkerType;
            dataTypeToLinkerInterface[dataType] = null;
            dataTypeNameToLinkerInterface[dataTypeName] = null;

            var obj = GameObject.FindObjectOfType (linkerType);
            if (obj == null)
            {
                Debug.Log ($"Failed to find linker for data type {dataTypeName}: can't find any GameObjects of type {linkerType}");
                return null;
            }

            var linker = obj as IDataMultiLinker;
            if (linker == null)
            {
                Debug.LogWarning ($"Failed to find linker for data type {dataTypeName}: type is not a DataMultiLinker");
                return null;
            }

            dataTypeToLinkerInterface[dataType] = linker;
            dataTypeNameToLinkerInterface[dataTypeName] = linker;
            return linker;
        }
    }

    public static class DataMultiLinkerHelper
    {
        public static List<Action> actionListLoad = new List<Action> ();
        public static List<Action> actionListResolveText = new List<Action> ();
    }

    public interface IDataMultiLinker
    {
        public bool IsUsingDirectories ();
        public bool IsDisplayIsolated ();
        public DataContainer GetDisplayIsolatedOverride ();
        public bool IsModdable ();

        public GameObject GetObject ();
        public void SelectObject ();
        public string GetFilter ();
        public void SetFilter (bool filterUsed, string filter, bool filterExact);

        public void SaveDataLocal ();
        public void LoadDataLocal ();
        public void CreateEntry (string key);
        public IEnumerable<string> GetKeysLocal ();

        #if PB_MODSDK && UNITY_EDITOR
        public void ResetLoadedOnce ();
        public List<string> SDKKeys { get; }
        public string FullPath { get; }
        #endif
    }

    [ExecuteInEditMode]
    // [Searchable (FilterOptions = SearchFilterOptions.ISearchFilterableInterface, Recursive = true)]
    public class DataMultiLinker<T> : MonoBehaviour, IDataMultiLinker where T : DataContainer, new()
    {
        public static class OdinGroup
        {
            public static class Name
            {
                public const string KeyList = nameof(KeyList);
                public const string Filter = "filterUsed";
                public const string FilterInput = Filter + "/Input";
                public const string FilterButtons = Filter + "/Buttons";
                public const string Isolated = nameof(Isolated);
                public const string LoadSave = nameof(LoadSave);
                #if PB_MODSDK
                public const string SelectedMod = "Selected Mod";
                #endif
                public const string Settings = nameof(Settings);
                public const string SettingsTextSectors = Settings + "/TextSectors";
                public const string Text = nameof(Text);
                public const string Utilities = nameof(Utilities);
            }

            public static class Order
            {
                #if PB_MODSDK
                public const float SelectedMod = -60f;
                public const float RestoreButtons = -59f;
                #endif
                public const float TextButtons = -56f;
                public const float LoadSaveButtons = -52f;
                public const float Utilities = -10f;
                public const float Settings = -9f;
                public const float KeyList = -4f;
                public const float Isolated = 0f;
                public const float Filter = 50f;
            }
        }

        public static class OdinPropertyOrder
        {
            public const float SettingsTextSectors = -1f;
            public const float DataList = 100f;
        }

        private static DataMultiLinker<T> ins;
        private static SortedDictionary<string, T> dataInternal;
        private static List<T> dataList = new List<T> ();

        public static Type dataTypeStatic = typeof (T);
        private static bool dataTypeWithText = typeof (DataContainerWithText).IsAssignableFrom (typeof (T));

        [NonSerialized]
        public Type dataType = typeof (T);

        private static bool listed = false;

        private static readonly Dictionary<(string, int), T> generatedDataInternal = new Dictionary<(string, int), T> ();

        [NonSerialized]
        private static bool loadedOnce = false;

        #if PB_MODSDK && UNITY_EDITOR
        public void ResetLoadedOnce ()
        {
            if (!loadedOnce)
                return;

            loadedOnce = false;
            path = null;
            dataInternal = null;
        }

        public List<string> SDKKeys
        {
            get
            {
                var pathSDK = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), DataPathUtility.GetPath (typeof(T)));
                if (!Directory.Exists (pathSDK))
                {
                    return new List<string> ();
                }
                return IsUsingDirectories ()
                        ? Directory.EnumerateDirectories (pathSDK).Select (Path.GetFileName).ToList ()
                        : Directory.EnumerateFiles (pathSDK, "*.yaml").Select (Path.GetFileNameWithoutExtension).ToList ();
            }
        }

        public string FullPath
        {
            get
            {
                CheckPathResolved ();
                return pathLocal;
            }
        }

        static bool isSaveAvailable => DataContainerModData.hasSelectedConfigs || typeof(T) == typeof(DataContainerModToolsPage);
        static bool hasSelectedMod => DataContainerModData.hasSelectedConfigs;
        static bool hasChanges => hasSelectedMod && (unsavedChangesPossible || ModToolsHelper.HasChanges (DataContainerModData.selectedMod, typeof(T)));
        static Color colorSepia => ModToolsColors.HighlightNeonSepia;

        [ShowIf (nameof(IsModdable))]
        [ShowInInspector, PropertyOrder (-100), HideReferenceObjectPicker, HideLabel, HideDuplicateReferenceBox]
        private static ModConfigStatus status = new ModConfigStatus ();

        public static string configsPath => DataContainerModData.selectedMod.GetModPathConfigs ();

        [PropertyOrder (OdinGroup.Order.RestoreButtons)]
        [ShowIf(nameof(hasSelectedMod))]
        [EnableIf (nameof(hasChanges))]
        [GUIColor (nameof(colorSepia))]
        [PropertyTooltip ("Reset all entries in config DB to match SDK. This will undo all your changes.")]
        [Button ("Restore All From SDK", ButtonHeight = 32, Icon = SdfIconType.BoxArrowInRight, IconAlignment = IconAlignment.LeftOfText)]
        public void RestoreFromSDK ()
        {
            if (DataContainerModData.selectedMod.multiLinkerChecksumMap == null)
            {
                return;
            }
            if (!DataContainerModData.selectedMod.multiLinkerChecksumMap.TryGetValue (typeof(T), out var pair))
            {
                return;
            }
            if (!EditorUtility.DisplayDialog ("Restore From SDK", "Are you sure you'd like to overwrite all of your changes to this config DB?\n\nConfig path: \n" + path, "Confirm", "Cancel"))
            {
                return;
            }

            DataContainerModData.selectedMod.DeleteConfigOverride (pair.Mod);
            if (pair.SDK == null)
            {
                // This shouldn't happen because multilinker DBs can't be added through the SDK.
                Debug.LogErrorFormat ("MultLinker DB that isn't present in SDK | type: {0} | path: {1}", typeof(T).Name, pair.Mod.RelativePath);
            }
            else
            {
                var sdkPath = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), "Configs", pair.SDK.RelativePath);
                var modPath = DataPathHelper.GetCombinedCleanPath (configsPath, pair.Mod.RelativePath);
                Directory.Delete (modPath, true);
                Directory.CreateDirectory (modPath);
                ModToolsHelper.CopyConfigDB (new DirectoryInfo (sdkPath), modPath);
                LoadData ();
            }

            unsavedChangesPossible = false;
        }
        #endif

        [NonSerialized]
        public static bool unsavedChangesPossible = false;

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Settings, false, Order = OdinGroup.Order.Settings)]
        [InfoBox ("No text sector keys declared, text pipeline operations unavailable", InfoMessageType.Warning, "IsTextPipelineWarning")]
        #if PB_MODSDK
        [PropertyTooltip ("$" + nameof(path))]
        #endif
        [FolderPath, ReadOnly]
        public static string path;

        // Gets around the restriction on using static path field in some non-static contexts
        public string pathLocal => path;

        #if UNITY_EDITOR

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.SettingsTextSectors)]
        [ShowIf (nameof(IsTextPipelineAvailable))]
        [PropertyOrder (OdinPropertyOrder.SettingsTextSectors)]
        [LabelText ("Text DB Sectors")]
        private static string textSectorKeySummary => GetTextSectorKeySummary ();

        #endif

        public static List<string> textSectorKeys = null;

        [FoldoutGroup (OdinGroup.Name.Settings)]
        [LabelText ("Auto-load in Edit Mode")]
        public bool autoloadInEditor = false;

        [FoldoutGroup (OdinGroup.Name.Settings)]
        [LabelText ("Auto-load in Play Mode")]
        public bool autoloadInGame = false;

        [FoldoutGroup (OdinGroup.Name.Settings)]
        [OnInspectorGUI ("AppendedInspectorGUI")]
        public bool log = false;

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Settings)]
        [ShowIf (nameof(IsDisplayIsolated))]
        [LabelText ("Force list display")]
        [OnValueChanged ("OnForceListDisplay")]
        public static bool forceListDisplay = false;

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.KeyList, false, Order = OdinGroup.Order.KeyList)]
        [ShowIf (nameof(IsDisplayIsolatedInspector))]
        [InfoBox ("This DB is optimized to display one entry at a time. If you need access to standard list UI, enable \"Force list display\" in Settings.", InfoMessageType.None, VisibleIf = "IsDisplayIsolatedInspector")]
        [GUIColor ("isolatedColor")]
        [HideLabel, ValueDropdown (nameof(GetKeys))]
        [SuffixLabel ("@GetFilterSuffix ()")]
        public string filterIsolatedProperty
        {
            get
            {
                return filter;
            }
            set
            {
                SetFilter (true, value, true);
            }
        }

        [ToggleGroup (OdinGroup.Name.Filter, "Filter", VisibleIf = "@!IsDisplayIsolatedInspector ()", Order = OdinGroup.Order.Filter)]
        public bool filterUsed = false;

        [GUIColor ("filterColor")]
        [ToggleGroup (OdinGroup.Name.Filter)]
        [HorizontalGroup (OdinGroup.Name.FilterInput)]
        [OnValueChanged ("RequestFilter")]
        [HideLabel]
        [SerializeField]
        public string filter = string.Empty;

        [NonSerialized, ShowInInspector]
        [ToggleGroup (OdinGroup.Name.Filter)]
        [HorizontalGroup (OdinGroup.Name.FilterInput, 40f)]
        [HideLabel, ProgressBar (0, 1, DrawValueLabel = false)]
        public double filterTime = 0f;

        [GUIColor ("filterColor")]
        [ToggleGroup (OdinGroup.Name.Filter)]
        [HorizontalGroup (OdinGroup.Name.FilterInput, 60f)]
        [ToggleLeft, LabelText ("Exact")]
        [OnValueChanged ("RequestFilter")]
        public bool filterExact = false;

        private Type entryType;
        private static bool filterRequestedFromLoad = false;
        private bool filterRequested = false;
        private double filterTimeStart = 0;

        [ShowInInspector]
        [BoxGroup(OdinGroup.Name.Isolated, true, LabelText = "Selection", VisibleIf = nameof(IsDisplayIsolatedInspector), Order = OdinGroup.Order.Isolated)]
        [PropertySpace (8f)]
        [OnValueChanged (nameof (ValidateData), true)]
        [HideLabel, HideDuplicateReferenceBox, InlineProperty]
        public DataFilterKeyValuePair<T> dataFilteredIsolated
        {
            get
            {
                if (dataInternal == null && !loadedOnce)
                {
                    LoadData ();
                    if (dataInternal == null)
                    {
                        return null;
                    }
                    var keyFirst = dataInternal.Keys.First ();
                    if (!string.IsNullOrEmpty (keyFirst))
                    {
                        SetFilter (true, keyFirst, true);
                    }
                }

                if (dataFiltered == null || dataFiltered.Count == 0)
                {
                    return null;
                }

                return dataFiltered[0];
            }
            set { }
        }

        [ShowInInspector]
        [ShowIf ("@IsFilterUsed () && !IsDisplayIsolatedInspector () && dataFiltered != null")]
        [PropertySpace (8f), PropertyOrder (OdinPropertyOrder.DataList)]
        [OnValueChanged (nameof(ValidateData), true)]
        [ListDrawerSettings (HideRemoveButton = true, DefaultExpandedState = true, DraggableItems = false, CustomAddFunction = "AddDataFiltered")]
        public static List<DataFilterKeyValuePair<T>> dataFiltered;

        [ShowInInspector]
        #if UNITY_EDITOR
        [ShowIf (nameof(showAllData))]
        #endif
        [OnValueChanged (nameof(ValidateData), true)]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.ExpandedFoldout, KeyLabel = "")]
        [PropertySpace (8f), PropertyOrder (OdinPropertyOrder.DataList)]
        public static SortedDictionary<string, T> data
        {
            get
            {
                if (dataInternal == null && !loadedOnce)
                    LoadData ();
                return dataInternal;
            }
            set
            {
                dataInternal = value;
            }
        }

        #if UNITY_EDITOR
        bool showAllData => !IsFilterUsed () & !IsDisplayIsolatedInspector ();
        #endif

        public SortedDictionary<string, T> dataNonStatic => data;

        private static List<string> keysCached = new List<string> ();

        public static IEnumerable<string> GetKeys ()
        {
            keysCached.Clear ();

            var dataFetched = data;
            if (dataFetched != null)
            {
                bool tagged = typeof (IDataContainerTagged).IsAssignableFrom (dataTypeStatic);
                foreach (var kvp in dataFetched)
                {
                    if (tagged)
                    {
                        var valueTagged = kvp.Value as IDataContainerTagged;
                        if (valueTagged != null && valueTagged.IsHidden ())
                            continue;
                    }

                    keysCached.Add (kvp.Key);
                }
            }

            return keysCached;
        }



        protected virtual void OnEnable ()
        {
            // OnEnable is reliably called first in Edit mode and in Play mode (as opposed to Awake), so we'll use it to set up the instance
            ins = this;

            #if UNITY_EDITOR
            // Since some data linkers are interdependent, we need to wait until every single singleton is set up before we do initial load.
            // Editor usually schedules an Update right after OnEnable, but in case it doesn't, the following call should guarantee it.
            // Since Application.isPlaying can't be reliably used in OnEnable of ExecuteInEditMode, we use a utility function.
            if (UtilityECS.IsApplicationNotPlaying ())
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate ();
            }

            UnityEditor.EditorApplication.update -= UpdateInEditor;
            UnityEditor.EditorApplication.update += UpdateInEditor;

            #endif
        }

        private void Update ()
        {
            // Update is the only function that reliably works for the purpose of initial loading in Edit mode and in Play mode.
            if (loadedOnce)
                return;

            bool inEditor = UtilityECS.IsApplicationNotPlaying ();
            bool autoload = inEditor ? autoloadInEditor : autoloadInGame;
            if (!autoload)
                return;

            // In some rare cases, the data might have been used very early and might have already been loaded - then we don't need to bother
            loadedOnce = true;
            if (data == null)
                LoadData ();

            #if UNITY_EDITOR

            if (Selection.activeGameObject == gameObject)
            {
                if (filterRequestedFromLoad)
                {
                    filterRequestedFromLoad = false;
                    ApplyFilter ();
                }
            }

            #endif
        }

        private void OnDestroy ()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= UpdateInEditor;

            filterRequested = false;
            filterRequestedFromLoad = false;
            #endif
        }

        private static void CheckPathResolved ()
        {
            #if PB_MODSDK && UNITY_EDITOR
            bool moddable = ins == null || ins.IsModdable ();
            var pathPrefix = moddable && hasSelectedMod
                ? DataContainerModData.selectedMod.GetModPathProject ()
                : DataPathHelper.GetApplicationFolder ();
            path = DataPathHelper.GetCombinedCleanPath (pathPrefix, DataPathUtility.GetPath (typeof(T)));
            #else

            if (!string.IsNullOrEmpty (path))
                return;

            var pathInProject = DataPathUtility.GetPath (typeof(T));
            path = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), pathInProject);
            #endif
        }

        #if UNITY_EDITOR
        [ButtonGroup (OdinGroup.Name.LoadSave, Order = OdinGroup.Order.LoadSaveButtons)]
        [Button ("Load data", ButtonSizes.Medium, Icon = SdfIconType.Upload)]
        private void LoadDataButton ()
        {
            LoadData ();
            CheckLoadFlags ();
        }
        #endif

        public virtual bool IsUsingDirectories () => false;
        public virtual bool IsDisplayIsolated () => false;
        public virtual DataContainer GetDisplayIsolatedOverride () => null;
        public virtual bool IsModdable () => true;

        protected static bool IsModdableStatic () => ins != null && ins.IsModdable ();

        private bool IsDisplayIsolatedInspector ()
        {
            #if UNITY_EDITOR
            return IsDisplayIsolated () && !forceListDisplay;
            #else
            return IsDisplayIsolated ();
            #endif
        }

        public static void LoadDataChecked ()
        {
            if (dataInternal == null && !loadedOnce)
                LoadData ();
        }

        // A quick workaround to get around interface restrictions
        public void LoadDataLocal () => LoadData ();

        public static void LoadData ()
        {
            if (!listed)
            {
                listed = true;
                DataMultiLinkerHelper.actionListLoad.Add (LoadData);

                if (dataTypeWithText)
                    DataMultiLinkerHelper.actionListResolveText.Add (ResolveText);
            }

            CheckPathResolved ();
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to load global data container of type {typeof (T).Name} due to missing path");
                return;
            }

            bool directoryMode = ins != null && ins.IsUsingDirectories ();
            dataInternal = UtilitiesYAML.LoadDecomposedDictionary<T> (path, directoryMode: directoryMode, appendApplicationPath: false);

            int i = 0;
            foreach (var kvp in dataInternal)
            {
                var entry = kvp.Value;
                entry.path = path;
                entry.index = i;
                i += 1;
            }

            unsavedChangesPossible = false;
            loadedOnce = true;

            ModManager.ProcessConfigModsForMultiLinker (dataTypeStatic, dataInternal, path);

            foreach (var kvp in dataInternal)
                kvp.Value.OnAfterDeserialization (kvp.Key);

            if (DataMultiLinkerUtility.callbacksOnAfterDeserialization.TryGetValue (dataTypeStatic, out var callback) && callback != null)
                callback.Invoke ();

            RefreshList ();

            #if UNITY_EDITOR

            filterRequestedFromLoad = true;

            if (ins != null && dataInternal != null && ins.IsDisplayIsolatedInspector ())
            {
                if (!dataInternal.ContainsKey (ins.filter))
                    ins.SetFilter (true, dataInternal.Keys.First (), true);
                else
                    ins.SetFilter (true, ins.filter, true);
            }

            #endif

            #if PB_MODSDK && UNITY_EDITOR
            if (hasSelectedMod
                && DataContainerModData.selectedMod.multiLinkerChecksumMap != null
                && DataContainerModData.selectedMod.multiLinkerChecksumMap.TryGetValue (typeof(T), out var pair))
            {
                // Check for disk changes that happened offline.
                var checksum = pair.Mod.Checksum;
                UpdateChecksums ();
                pair = DataContainerModData.selectedMod.multiLinkerChecksumMap[typeof(T)];
                if (!ConfigChecksums.ChecksumEqual (checksum, pair.Mod.Checksum))
                {
                    Debug.Log (typeof(T).Name + ": offline or format changes detected in mod configs -- refreshing checksums on disk");
                    ModToolsHelper.SaveChecksums (DataContainerModData.selectedMod);
                }
            }
            #endif
        }

        public static T LoadDataIsolated (string key)
        {
            // Ensure at least one load has occurred
            var dataTemp = data;

            CheckPathResolved ();
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to load global data container of type {typeof (T).Name} due to missing path");
                return null;
            }

            bool directoryMode = ins != null && ins.IsUsingDirectories ();
            var entry = UtilitiesYAML.LoadDecomposedEntryIsolated<T> (path, key, directoryMode: directoryMode, appendApplicationPath: false);

            if (entry == null)
                return null;

            entry.path = path;
            entry.OnAfterDeserialization (key);
            dataInternal[key] = entry;

            if (DataMultiLinkerUtility.callbacksOnAfterDeserialization.TryGetValue (dataTypeStatic, out var callback) && callback != null)
                callback.Invoke ();

            RefreshList ();

            #if UNITY_EDITOR

            filterRequestedFromLoad = true;

            #endif

            Debug.Log ($"Reloaded individual entry {key} of type {typeof (T).Name}");
            return entry;
        }

        // A quick workaround to get around interface restrictions
        public void SaveDataLocal () => SaveData ();

        [ButtonGroup (OdinGroup.Name.LoadSave)] // [GUIColor ("@GetSaveButtonColor (unsavedChangesPossible)")]
        [Button ("@unsavedChangesPossible ? \"Save data*\" : \"Save data\"", ButtonSizes.Medium, Icon = SdfIconType.Download)]
        #if PB_MODSDK && UNITY_EDITOR
        [EnableIf (nameof(isSaveAvailable))]
        #endif
        public static void SaveData ()
        {
            if (dataInternal == null)
                return;

            #if PB_MODSDK && UNITY_EDITOR
            if (!isSaveAvailable || !unsavedChangesPossible)
            {
                return;
            }
            #endif

            CheckPathResolved ();
            if (string.IsNullOrEmpty (path))
            {
                Debug.LogError ($"Failed to save global data container of type {typeof (T).Name} due to missing path");
                return;
            }

            foreach (var kvp in dataInternal)
                kvp.Value.OnBeforeSerialization ();

            if (DataMultiLinkerUtility.callbacksOnBeforeSerialization.TryGetValue (dataTypeStatic, out var callback) && callback != null)
                callback.Invoke ();

            bool directoryMode = ins != null && ins.IsUsingDirectories ();
            UtilitiesYAML.SaveDecomposedDictionary (path, dataInternal, warnAboutDeletions: false, directoryMode: directoryMode, appendApplicationPath: false);
            unsavedChangesPossible = false;

            foreach (var kvp in dataInternal)
                kvp.Value.OnAfterSerialization ();

            #if PB_MODSDK && UNITY_EDITOR
            SaveChecksums ();
            #endif
        }

        public static void SaveDataIsolated (string key)
        {
            if (dataInternal == null || string.IsNullOrEmpty (key))
                return;

            if (dataInternal.TryGetValue (key, out var entry))
            {
                CheckPathResolved ();
                if (string.IsNullOrEmpty (path))
                {
                    Debug.LogError ($"Failed to save global data container of type {typeof (T).Name} due to missing path");
                    return;
                }

                entry.OnBeforeSerialization ();

                if (DataMultiLinkerUtility.callbacksOnBeforeSerialization.TryGetValue (dataTypeStatic, out var callback) && callback != null)
                    callback.Invoke ();

                bool directoryMode = ins != null && ins.IsUsingDirectories ();
                UtilitiesYAML.SaveDecomposedEntryIsolated (path, key, entry, directoryMode: directoryMode, appendApplicationPath: false);

                entry.OnAfterSerialization ();

                #if PB_MODSDK && UNITY_EDITOR
                SaveChecksums ();
                #endif
            }
            else
            {
                Debug.LogWarning ($"Failed to save isolated config {key} of type {typeof (T).Name}: no such key is present in the dictionary");
            }

        }

        public static void ValidateData ()
        {
            if (dataInternal == null)
                return;

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs && IsModdableStatic ())
                return;
            #endif

            unsavedChangesPossible = true;
            foreach (var kvp in dataInternal)
            {
                var value = kvp.Value;
                if (value == null)
                {
                    Debug.Log ("Null entry: " + kvp.Key);
                    continue;
                }
                value.key = kvp.Key;
            }
        }

        public void ReplaceKey (string keyOld, string keyNew)
        {
            if (dataInternal == null)
                return;

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs && IsModdableStatic ())
                return;
            #endif

            var entry = GetEntry (keyOld);
            if (entry == null)
                return;

            unsavedChangesPossible = true;
            dataInternal.Remove (keyOld);
            dataInternal.Add (keyNew, entry);
            if (IsUsingDirectories ())
            {
                var pathOld = DataPathHelper.GetCombinedCleanPath (pathLocal, keyOld);
                var pathNew = DataPathHelper.GetCombinedCleanPath (pathLocal, keyNew);
                Directory.Move (pathOld, pathNew);
                #if PB_MODSDK
                LoadDataIsolated (keyNew);
                #endif
            }
            RefreshList ();
            entry.OnKeyReplacement (keyOld, keyNew);

            #if PB_MODSDK && UNITY_EDITOR
            if (IsDisplayIsolatedInspector ())
            {
                filterIsolatedProperty = keyNew;
                return;
            }
            #endif

            #if UNITY_EDITOR
            RequestFilter ();
            #endif
        }

        public static List<T> GetDataList ()
        {
            if (dataInternal == null && !loadedOnce)
                LoadData ();
            return dataList;
        }

        private static void RefreshList ()
        {
            dataList.Clear ();
            if (dataInternal == null)
                return;

            foreach (var kvp in dataInternal)
                dataList.Add (kvp.Value);
        }

        public static T GetEntry (string nameInternal, bool printWarning = true)
        {
            if (data != null && !string.IsNullOrEmpty (nameInternal) && data.ContainsKey (nameInternal))
                return data[nameInternal];

            if (printWarning)
                Debug.LogWarning ($"{typeof (T).GetNiceName ()} | Failed to find key {nameInternal} | Path: {path}");
            return null;
        }

        [NonSerialized]
        private string linkerDescription;
        private string GetLinkerDescription ()
        {
            if (string.IsNullOrEmpty (linkerDescription))
            {
                linkerDescription = $"This is a multi-linker component for class of type {typeof (T).GetNiceName ()}. Nothing is serialized on this component: ";
                linkerDescription += "instead, the data is saved to YAML files in a location specified below. <b>Do not forget to save any changes whenever you ";
                linkerDescription += "modify the data!</b>. Expand this box for more details...";
            }

            return linkerDescription;
        }

        [NonSerialized]
        private string linkerDetails;
        private string GetLinkerDetails ()
        {
            if (string.IsNullOrEmpty (linkerDetails))
            {
                linkerDetails = "Linkers are components that allow us not to write boilerplate every time we want to put another piece of data into YAML. ";
                linkerDetails += "They automatically handle saving, loading and singleton-style access to the data, making transition to YAML easier. ";
                linkerDetails += "\n\nThis particular class is a so-called multi-linker, which handles a collection of data (like items, actions, etc). ";
                linkerDetails += "That collection is serialized into separate files, per entry, and is deserialized into a dictionary keyed by filenames. ";
                linkerDetails += "This gives us flexibility to add new entries or edit an entry without just pushing an opaque blob into version control. ";
            }

            return linkerDetails;
        }

        [NonSerialized]
        private string filterDescription;
        private string GetFilterDescription ()
        {
            if (string.IsNullOrEmpty (filterDescription))
            {
                filterDescription = $"<b>This is a filtered subset of the entries in this database.</b> Deleting entries is not supported in this view. ";
                filterDescription += "Use this view to edit pre-existing entries, add new entries or rename entries";
            }

            return filterDescription;
        }

        public static string GetUpgradedKey (string keyInput, out bool replaced)
        {
            return DataLinkerHistory.GetUpgradedKey (dataTypeStatic.Name, keyInput, out replaced);
        }

        public static string GetUpgradedKey (string keyInput)
        {
            return DataLinkerHistory.GetUpgradedKey (dataTypeStatic.Name, keyInput, out bool replaced);
        }

        [ButtonGroup (OdinGroup.Name.Text, Order = OdinGroup.Order.TextButtons)]
        [ShowIf ("IsTextContainer")]
        [Button ("Load text", ButtonSizes.Medium, Icon = SdfIconType.TextIndentRight)]
        public static void ResolveText ()
        {
            if (!dataTypeWithText)
                return;

            ValidateData ();
            // DataManagerText.LoadDataMain ();

            foreach (var kvp in data)
            {
                var value = kvp.Value as DataContainerWithText;
                if (value != null)
                    value.ResolveText ();
            }

            if (DataMultiLinkerUtility.callbacksOnAfterDeserialization.ContainsKey (dataTypeStatic))
                DataMultiLinkerUtility.callbacksOnAfterDeserialization[dataTypeStatic]?.Invoke ();
        }

        public GameObject GetObject ()
        {
            return gameObject;
        }

        public void CreateEntry (string key)
        {
            if (data == null)
                return;

            if (string.IsNullOrEmpty (key))
                return;

            if (data.ContainsKey (key))
                Debug.LogWarning ($"Replaced entry {key} with a new one");

            var value = new T ();
            value.OnAfterDeserialization (key);
            data[key] = value;
        }

        public virtual void SelectObject ()
        {
            #if UNITY_EDITOR
            Selection.activeGameObject = gameObject;
            #endif
        }

        public IEnumerable<string> GetKeysLocal ()
        {
            if (data == null)
                return null;

            return data.Keys;
        }

        public string GetFilter ()
        {
            #if UNITY_EDITOR
            return filterUsed ? null : filter;
            #else
            return null;
            #endif
        }

        public void SetFilter (bool filterUsed, string filter, bool filterExact)
        {
            #if UNITY_EDITOR
            this.filterUsed = filterUsed;
            this.filter = filter;
            this.filterExact = filterExact;
            ApplyFilter ();
            #endif
        }

        #if UNITY_EDITOR

        [HideIf (nameof(IsDisplayIsolatedInspector))]
        [GUIColor ("@" + nameof(filterColor))]
        [ButtonGroup (OdinGroup.Name.FilterButtons)]
        [Button ("@" + nameof(filterButtonName), ButtonSizes.Medium)]
        public void ApplyFilter ()
        {
            if (this == null || gameObject == null)
                return;

            filterRequested = false;
            filterTime = 0;
            if (dataInternal == null || !IsFilterUsed ())
                return;

            if (dataFiltered == null)
                dataFiltered = new List<DataFilterKeyValuePair<T>> (dataInternal.Count);
            else
                dataFiltered.Clear ();

            var filterSplit = filter.Split (' ');
            if (!filterExact && filterSplit.Length > 1)
            {
                var filterCount = filterSplit.Length;
                List<bool> requirementMap = new List<bool> (filterCount);
                for (int i = 0; i < filterSplit.Length; ++i)
                {
                    var filterElement = filterSplit[i];
                    bool negative = filterElement.StartsWith ("*");
                    requirementMap.Add (!negative);

                    if (negative)
                        filterSplit[i] = filterElement.Substring (1, filterElement.Length - 1);
                }

                foreach (var kvp in dataInternal)
                {
                    var key = kvp.Key;
                    bool match = true;

                    for (int i = 0; i < filterCount; ++i)
                    {
                        var filterElement = filterSplit[i];
                        bool required = requirementMap[i];
                        bool contained = key.Contains (filterElement);

                        if (contained != required)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        dataFiltered.Add (new DataFilterKeyValuePair<T>
                        {
                            keyLast = kvp.Key,
                            key = kvp.Key,
                            value = kvp.Value,
                            parent = this,
                            foldoutUsed = true
                        });
                    }
                }
            }
            else
            {
                foreach (var kvp in dataInternal)
                {
                    var key = kvp.Key;
                    bool match = filterExact ? key.Equals (filter) : key.Contains (filter);
                    if (match)
                    {
                        dataFiltered.Add (new DataFilterKeyValuePair<T>
                        {
                            keyLast = kvp.Key,
                            key = kvp.Key,
                            value = kvp.Value,
                            parent = this,
                            foldoutUsed = !filterExact
                        });
                    }
                }
            }

            // PrepareKeyReplacement ();
        }

        private void AddDataFiltered ()
        {
            if (dataInternal == null)
                return;

            int i = 0;
            string key = "new_00";

            while (dataInternal.ContainsKey (key))
            {
                i += 1;
                key = $"new_{i}";

                if (i > 100)
                {
                    Debug.LogWarning ($"Failed to generate new key not present in the collection after {i} iterations, use unfiltered mode to make a new entry");
                    return;
                }
            }

            var entry = new T ();
            entry.key = key;
            dataInternal.Add (key, entry);
            RefreshList ();
            filter = key;
            filterRequested = true;
        }

        private bool IsDataTypeTagged =>
            typeof (IDataContainerTagged).IsAssignableFrom (dataType);

        [ShowIf (nameof(IsDataTypeTagged))]
        [FoldoutGroup (OdinGroup.Name.Utilities, false, Order = OdinGroup.Order.Utilities)]
        [Button ("Modify tags")]
        private void ModifyTags (Dictionary<string, bool> tagChanges)
        {
            if (dataFiltered == null || tagChanges == null || tagChanges.Count == 0)
                return;

            foreach (var kvp in dataFiltered)
            {
                var container = kvp.value;
                if (container == null)
                    continue;

                var containerTagged = container as IDataContainerTagged;
                if (containerTagged == null)
                    continue;

                var containerTags = containerTagged.GetTags (false);
                if (containerTags == null)
                    continue;

                foreach (var kvp2 in tagChanges)
                {
                    var tag = kvp2.Key;
                    var required = kvp2.Value;
                    var present = containerTags.Contains (tag);

                    if (required != present)
                    {
                        if (required)
                        {
                            containerTags.Add (tag);
                            Debug.Log ($"Added tag {tag} to {container.key}");
                        }
                        else
                        {
                            containerTags.Remove (tag);
                            Debug.Log ($"Removed tag {tag} from {container.key}");
                        }
                    }
                }
            }
        }

        public static void PrintChangeWarning ()
        {
            if (ins == null)
                Debug.LogWarning ($"Database of type {typeof (T).Name} was changed!");
            else
                Debug.LogWarning ($"Database of type {typeof (T).Name} ({ins.transform.parent.name}/{ins.transform.name}) was changed!", ins);
        }

        protected void UpdateInEditor ()
        {
            if (dataInternal == null)
                return;

            // Doing so ensures we don't check load flags at runtime when testing the game through data init
            if (!Application.isPlaying)
                CheckLoadFlags ();

            if (filterUsed && filterRequested)
            {
                filterTime = (UnityEditor.EditorApplication.timeSinceStartup - filterTimeStart) * 4f;
                Sirenix.Utilities.Editor.GUIHelper.RequestRepaint ();
                if (filterTime > 1f)
                {
                    filterTime = 1f;
                    filterRequested = false;
                    ApplyFilter ();
                }
            }
        }

        private void CheckLoadFlags ()
        {
            if (filterRequestedFromLoad)
            {
                filterRequestedFromLoad = false;
                ApplyFilter ();
            }
        }

        private string GetFilterSuffix ()
        {
            return dataInternal != null ? dataInternal.Count.ToString () : "?";
        }

        private string filterButtonName => filterExact ? "Find exact match" : "Find matches";
        private Color filterColor => filterExact && !IsDisplayIsolatedInspector () ? colorFilterExact : colorFilterNormal;

        protected virtual Color isolatedColor => colorFilterExact;

        protected static Color colorFilterNormal = new Color (1f, 1f, 1f, 1f);
        protected static Color colorFilterExact = new Color (0.7f, 0.9f, 1f, 1f);

        private void OnForceListDisplay ()
        {
            if (!IsDisplayIsolated () || forceListDisplay)
            {
                SetFilter (true, filter, false);
            }
            else
            {
                var keyFirst = dataInternal.Keys.First ();
                if (!string.IsNullOrEmpty (keyFirst))
                    SetFilter (true, keyFirst, true);
            }
        }

        private bool IsFilterUsed ()
        {
            return filterUsed && !string.IsNullOrEmpty (filter);
        }

        protected void RequestFilter ()
        {
            if (!IsFilterUsed ())
                return;

            filterRequested = true;
            filterTimeStart = UnityEditor.EditorApplication.timeSinceStartup;
        }

        public void SetFilterAndSelect (string key)
        {
            SetFilter (true, key, true);
            SelectObject ();
        }

        private Color GetSaveButtonColor (bool controlValue)
        {
            if (controlValue)
                return Color.HSVToRGB (Mathf.Cos ((float) UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1f, 1f);
            else
                return GUI.color;
        }

        private void AppendedInspectorGUI ()
        {
            DataEditor.showLibraryText = UnityEditor.EditorGUILayout.Toggle ("Show library text", DataEditor.showLibraryText);
        }

        private bool IsTextContainer ()
        {
            return data != null && data.Count > 0 && data.First ().Value is DataContainerWithText;
        }

        [ButtonGroup (OdinGroup.Name.Text)]
        [ShowIf ("IsTextContainer")]
        [Button ("Save text", ButtonSizes.Medium, Icon = SdfIconType.TextIndentLeft)]
        public void SaveText ()
        {
            if (DataMultiLinkerUtility.callbacksOnBeforeTextExport.ContainsKey (dataTypeStatic))
                DataMultiLinkerUtility.callbacksOnBeforeTextExport[dataTypeStatic]?.Invoke ();

            foreach (var kvp in data)
            {
                var value = kvp.Value as DataContainerWithText;
                if (value != null)
                    value.SaveText ();
            }

            if (DataMultiLinkerUtility.callbacksOnAfterTextExport.ContainsKey (dataTypeStatic))
                DataMultiLinkerUtility.callbacksOnAfterTextExport[dataTypeStatic]?.Invoke ();

            DataManagerText.SaveLibrary ();

            #if PB_MODSDK

            ModTextHelper.GenerateTextChangesToSectors (textSectorKeys);

            #endif
        }

        private static bool processInProgress = false;
        private static string textSectorKeySummaryInternal = null;

        private static string GetTextSectorKeySummary ()
        {
            if (textSectorKeySummaryInternal == null)
                textSectorKeySummaryInternal = textSectorKeys.ToStringFormatted (appendBrackets: false);
            return textSectorKeySummaryInternal;
        }

        #if !PB_MODSDK

        [ShowIf ("IsTextPipelineAvailable")]
        [FoldoutGroup("Settings")]
        [Button ("Open"), HorizontalGroup("Settings/TextSectors", 100f)]
        private static void OpenTextSector ()
        {
            if (textSectorKeys == null || textSectorKeys.Count == 0)
                return;

            var helper = GameObject.FindObjectOfType<DataHelperTextPipeline> ();
            if (helper == null)
                return;

            var key = textSectorKeys[0];
            helper.librarySectorKeySelected = key;
            UnityEditor.Selection.activeGameObject = helper.gameObject;
        }

        #endif

        private static bool IsTextPipelineWarning ()
        {
            return dataTypeWithText && !IsTextPipelineAvailable ();
        }

        private static bool IsTextPipelineAvailable ()
        {
            return textSectorKeys != null && textSectorKeys.Count > 0;
        }

        public void DuplicateEntry (string key, T value)
        {
            if (dataInternal == null || value == null)
            {
                return;
            }
            if (!dataInternal.ContainsKey (key))
            {
                return;
            }

            var keyNew = key;
            var i = 0;
            while (dataInternal.ContainsKey (keyNew))
            {
                keyNew = key + "_" + i.ToString ("00");
                i += 1;

                if (i > 99)
                {
                    return;
                }
            }

            if (IsUsingDirectories ())
            {
                var sourcePath = DataPathHelper.GetCombinedCleanPath (pathLocal, key);
                var destPath = DataPathHelper.GetCombinedCleanPath (pathLocal, keyNew);
                CopyDirectoryContents (new DirectoryInfo (sourcePath), destPath);
                LoadDataIsolated (keyNew);
            }
            else
            {
                var copy = UtilitiesYAML.CloneThroughYaml (value);
                dataInternal.Add (keyNew, copy);
            }

            #if PB_MODSDK
            if (IsDisplayIsolatedInspector ())
            {
                filterIsolatedProperty = keyNew;
                return;
            }
            #endif

            ApplyFilter ();
        }

        public void DeleteEntry (string key)
        {
            if (dataInternal == null)
            {
                return;
            }
            if (!dataInternal.ContainsKey (key))
            {
                return;
            }
            if (IsUsingDirectories ())
            {
                if (!EditorUtility.DisplayDialog
                (
                    "Delete Entry",
                    "This entry is contained in a folder. The folder and all data in it will be permanently deleted. Proceed?",
                    "Continue",
                    "Cancel"
                ))
                {
                    return;
                }

                var folderPath = DataPathHelper.GetCombinedCleanPath (pathLocal, key);
                Directory.Delete (folderPath, true);
            }
            dataInternal.Remove (key);

            #if PB_MODSDK
            if (IsDisplayIsolatedInspector ())
            {
                filterIsolatedProperty = dataInternal.Keys.OrderBy (k => k).FirstOrDefault ();
                return;
            }
            #endif

            ApplyFilter ();
        }

        void CopyDirectoryContents (DirectoryInfo source, string dest)
        {
            Directory.CreateDirectory (dest);
            foreach (var f in source.EnumerateFiles ())
            {
                var destPath = DataPathHelper.GetCombinedCleanPath (dest, f.Name);
                f.CopyTo (destPath);
            }
            foreach (var d in source.EnumerateDirectories ())
            {
                var destPath = DataPathHelper.GetCombinedCleanPath (dest, d.Name);
                CopyDirectoryContents (d, destPath);
            }
        }

        #endif

        #if PB_MODSDK && UNITY_EDITOR
        static void UpdateChecksums ()
        {
            if (isEntryKeyChanging)
            {
                return;
            }
            if (!hasSelectedMod)
            {
                return;
            }

            var modData = DataContainerModData.selectedMod;
            if (modData.checksumsRoot == null)
            {
                return;
            }
            if (modData.multiLinkerChecksumMap == null)
            {
                return;
            }
            if (!modData.multiLinkerChecksumMap.TryGetValue (typeof(T), out var pair))
            {
                return;
            }

            var rootDirectory = new DirectoryInfo (configsPath);
            var directoryMode = ins != null && ins.IsUsingDirectories ();
            var modChecksums = new ConfigChecksums.ConfigDirectory (ConfigChecksums.EntryType.Directory)
            {
                Source = ConfigChecksums.EntrySource.Mod,
                Locator = pair.Mod.Locator,
                RelativePath = pair.Mod.RelativePath,
            };

            List<string> errorKeys;
            if (directoryMode)
            {
                var keySet = new HashSet<string> (dataInternal.Keys);
                errorKeys = modChecksums.Upsert (rootDirectory, ConfigChecksums.EntrySource.Mod, keySet);
            }
            else
            {
                errorKeys = new List<string> ();
                var entries = new List<(string, string)> ();
                foreach (var key in dataInternal.Keys)
                {
                    var fileName = key + ".yaml";
                    var filePath = DataPathHelper.GetCombinedCleanPath (path, fileName);
                    try
                    {
                        entries.Add ((fileName, File.ReadAllText (filePath)));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat ("Error during checksum operation | mod: {0} | key: {1} | path: {2}", modData.id, key, path);
                        Debug.LogException (ex);
                        errorKeys.Add (key);
                    }
                }
                modChecksums.AddFiles (ConfigChecksums.EntrySource.Mod, entries);
            }
            UpdateChecksumSource (pair.SDK, modChecksums, directoryMode);

            if (errorKeys.Any ())
            {
                var sdkKeyMap = ModToolsHelper.GetEntriesByKey (pair.SDK, directoryMode);
                var errorIndexes = errorKeys
                    .Where (k => sdkKeyMap.ContainsKey (k))
                    .Select (k => -sdkKeyMap[k].Locator.Last ())
                    .ToList ();

                if (pair.SDK != null)
                {
                    errorIndexes.Sort ();
                    foreach (var idx in errorIndexes)
                    {
                        pair.SDK.Entries.RemoveAt (-idx);
                    }
                    pair.SDK.FixLocators ();
                }

                foreach (var key in errorKeys)
                {
                    dataInternal.Remove (key);
                }
            }

            modData.multiLinkerChecksumMap[typeof(T)] = (pair.SDK, modChecksums);
            modData.checksumsRoot.Patch (modChecksums);
            ConfigChecksums.UpdateChecksums (ConfigChecksums.EntrySource.Mod, modData.checksumsRoot, modChecksums);
        }

        static void UpdateChecksumSource (ConfigChecksums.ConfigDirectory sdk, ConfigChecksums.ConfigDirectory mod, bool directoryMode)
        {
            if (sdk == null)
            {
                return;
            }

            var sdkKeyMap = ModToolsHelper.GetEntriesByKey (sdk, directoryMode);
            var modKeyMap = ModToolsHelper.GetEntriesByKey (mod, directoryMode);
            foreach (var kvp in modKeyMap)
            {
                if (!sdkKeyMap.TryGetValue (kvp.Key, out var sdkEntry))
                {
                    continue;
                }
                if (!ConfigChecksums.ChecksumEqual (sdkEntry.Checksum, kvp.Value.Checksum))
                {
                    continue;
                }
                kvp.Value.Source = ConfigChecksums.EntrySource.SDK;
            }
        }

        static void SaveChecksums ()
        {
            if (!IsModdableStatic ())
                return;

            UpdateChecksums ();
            ModToolsHelper.SaveChecksums (DataContainerModData.selectedMod);
        }

        public static bool isEntryKeyChanging;
        #endif
    }
}
