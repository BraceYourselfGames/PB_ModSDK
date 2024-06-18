using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace PhantomBrigade
{
    public static class FieldReflectionUtility
    {
        private static Type intType = typeof (int);
        private static Type stringType = typeof (string);
        
        private static Dictionary<Type, Type[]> typesPerInterfaces = new Dictionary<Type, Type[]> ();
        private static Dictionary<Type, List<string>> fieldValues = new Dictionary<Type, List<string>> ();
        private static List<string> fieldValuesFallback = new List<string> ();
        
        public static Type[] GetTypesPerInterface (Type type)
        {
            
            if (type == null)
                return null;

            if (typesPerInterfaces.ContainsKey (type))
                return typesPerInterfaces[type];

            var asm = Assembly.GetExecutingAssembly ();
            var types = asm.GetExportedTypes ();
            var typesPerInterface = (from t in types
                where !t.IsInterface && !t.IsAbstract
                where type.IsAssignableFrom (t)
                select t).ToArray ();

            typesPerInterfaces.Add (type, typesPerInterface);
            return typesPerInterface;
        }

        private static List<Type> typeResults = new List<Type> ();

        public static List<Type> GetTypesByName (string typeName)
        {
            typeResults.Clear ();
            
            if (!string.IsNullOrEmpty (typeName))
            {
                var asm = Assembly.GetExecutingAssembly ();
                var types = asm.GetExportedTypes ();

                foreach (var typeCandidate in types)
                {
                    if (string.Equals (typeCandidate.Name, typeName, StringComparison.InvariantCultureIgnoreCase))
                        typeResults.Add (typeCandidate);
                }
            }

            return typeResults;
        }
        
        public static Type GetTypeByName (string typeName)
        {
            if (string.IsNullOrEmpty (typeName))
                return null;
            
            var asm = Assembly.GetExecutingAssembly ();
            var types = asm.GetExportedTypes ();

            foreach (var typeCandidate in types)
            {
                if (string.Equals (typeCandidate.Name, typeName, StringComparison.InvariantCultureIgnoreCase))
                    return typeCandidate;
            }

            return null;
        }
        
        public static List<string> GetConstantStringFieldValues (Type type, bool addEmptyEntry = false)
        {
            if (type == null)
                return fieldValuesFallback;

            if (fieldValues.ContainsKey (type))
                return fieldValues[type];
            
            var values = new List<string> ();
            fieldValues.Add (type, values);
            
            var fields = type.GetFields (BindingFlags.Static | BindingFlags.Public);
            if (addEmptyEntry)
                values.Add (string.Empty);

            foreach (var fieldInfo in fields)
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType != stringType) continue;
                if (fieldInfo.GetValue (null) is string fieldValue) 
                    values.Add (fieldValue);
            }

            return values;
        }
        
        private static Dictionary<Type, List<string>> fieldNamesInt = new Dictionary<Type, List<string>> ();

        public static List<string> GetConstantIntFieldNames (Type type, bool addEmptyEntry = false)
        {
            if (type == null)
                return fieldNamesFallback;

            if (fieldNamesInt.ContainsKey (type))
                return fieldNamesInt[type];
            
            var names = new List<string> ();
            fieldNamesInt.Add (type, names);
            
            var fields = type.GetFields (BindingFlags.Static | BindingFlags.Public);
            if (addEmptyEntry)
                names.Add (string.Empty);

            foreach (var fieldInfo in fields)
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType != intType) continue;
                names.Add (fieldInfo.Name);
            }

            return names;
        }
        
        private static Dictionary<Type, Dictionary<string, int>> fieldNamesValuesInt = new Dictionary<Type, Dictionary<string, int>> ();
        private static Dictionary<string, int> fieldNamesValuesFallbackInt = new Dictionary<string, int> ();
        
        public static Dictionary<string, int> GetConstantIntFieldNamesValues (Type type)
        {
            if (type == null)
                return fieldNamesValuesFallbackInt;

            if (fieldNamesValuesInt.ContainsKey (type))
                return fieldNamesValuesInt[type];
            
            var pairs = new Dictionary<string, int> ();
            fieldNamesValuesInt.Add (type, pairs);
            
            var fields = type.GetFields (BindingFlags.Static | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType != intType) continue;
                if (fieldInfo.GetValue (null) is int fieldValue)
                    pairs.Add (fieldInfo.Name, fieldValue);
            }

            return pairs;
        }

        
        private static Dictionary<Type, List<string>> fieldNamesString = new Dictionary<Type, List<string>> ();
        private static List<string> fieldNamesFallback = new List<string> ();
        
        public static List<string> GetConstantStringFieldNames (Type type, bool addEmptyEntry = false)
        {
            if (type == null)
                return fieldNamesFallback;

            if (fieldNamesString.ContainsKey (type))
                return fieldNamesString[type];
            
            var names = new List<string> ();
            fieldNamesString.Add (type, names);
            
            var fields = type.GetFields (BindingFlags.Static | BindingFlags.Public);
            if (addEmptyEntry)
                names.Add (string.Empty);

            foreach (var fieldInfo in fields)
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType != stringType) continue;
                names.Add (fieldInfo.Name);
            }

            return names;
        }

        private static Dictionary<Type, Dictionary<string, string>> fieldNamesValues = new Dictionary<Type, Dictionary<string, string>> ();
        private static Dictionary<string, string> fieldNamesValuesFallback = new Dictionary<string, string> ();
        
        public static Dictionary<string, string> GetConstantStringFieldNamesValues (Type type, bool addEmptyEntry = false)
        {
            if (type == null)
                return fieldNamesValuesFallback;

            if (fieldNamesValues.ContainsKey (type))
                return fieldNamesValues[type];
            
            var pairs = new Dictionary<string, string> ();
            fieldNamesValues.Add (type, pairs);
            
            var fields = type.GetFields (BindingFlags.Static | BindingFlags.Public);
            if (addEmptyEntry)
                pairs.Add (string.Empty, string.Empty);

            foreach (var fieldInfo in fields)
            {
                var fieldType = fieldInfo.FieldType;
                if (fieldType != stringType) continue;
                if (fieldInfo.GetValue (null) is string fieldValue)
                    pairs.Add (fieldInfo.Name, fieldValue);
            }

            return pairs;
        }

        private static Type typeLast;
        private static Type attributeTypeLast;
        private static List<FieldInfo> fieldsLast;
        
        public static List<FieldInfo> GetPublicFieldsWithAttribute (Type type, Type attributeType)
        {
            if (type == null)
                return null;

            if (type == typeLast && attributeType == attributeTypeLast)
                return fieldsLast;

            typeLast = type;
            attributeTypeLast = attributeType;
            
            fieldsLast = new List<FieldInfo> ();
            var fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            foreach (var fieldInfo in fields)
            {
                if (!Attribute.IsDefined (fieldInfo, attributeType))
                    continue;
                    
                var attr = Attribute.GetCustomAttribute (fieldInfo, attributeType);
                if (attr == null)
                    continue;
            
                fieldsLast.Add (fieldInfo);
            }

            return fieldsLast;
        }
    }
}
