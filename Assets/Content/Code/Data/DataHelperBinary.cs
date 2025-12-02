using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using Area;

namespace PhantomBrigade.Data
{
    [AttributeUsage (AttributeTargets.Field)]
    public class BinaryDataAttribute : Attribute
    {
        public string alias = null;
        public bool logIfMissing = true;
        
        public BinaryDataAttribute () {  }
        public BinaryDataAttribute (string alias) { this.alias = alias; }
        public BinaryDataAttribute (string alias, bool logIfMissing) { this.alias = alias; this.logIfMissing = logIfMissing; }
    }

    public static class BinaryDataUtility
    {
        delegate void ReadArrayFromBinary (ref Array target, string pathFull, long fileLength);
        delegate void ReadFromBinary (ref object target, string pathFull, long fileLength);
        delegate void SaveAsBinary (object source, string pathFull);

        private static Type attributeType = typeof (BinaryDataAttribute);
        private static Dictionary<Type, List<FieldInfo>> fieldsCached = new Dictionary<Type, List<FieldInfo>> ();
        
        static readonly Dictionary<Type, (string, ReadArrayFromBinary, SaveAsBinary)> typesArray = new Dictionary<Type, (string, ReadArrayFromBinary, SaveAsBinary)>
        {
            [typeof(byte[])] = (".byte", ReadByteArray, SaveByteArray),
            [typeof(bool[])] = (".bool", ReadBoolArray, SaveBoolArray),
            [typeof(int[])] = (".int", ReadIntArray, SaveIntArray),
            [typeof(int3[])] = (".int3", ReadInt3, SaveInt3Array),
            [typeof(float[])] = (".float", ReadFloat, SaveFloatArray),
            [typeof(float3[])] = (".float3", ReadFloat3, SaveFloat3Array),
            [typeof(float4[])] = (".float4", ReadFloat4, SaveFloat4Array)
        };
        
        static readonly Dictionary<Type, (string, ReadFromBinary, SaveAsBinary)> typesObject = new Dictionary<Type, (string, ReadFromBinary, SaveAsBinary)>
        {
            
        };

        public static bool SaveFieldsToBinary<T> (T data, string pathBase)
        {
            var dataType = data.GetType ();
            var invalidChars = Path.GetInvalidFileNameChars ();
            
            List<FieldInfo> fieldsWithAttribute = null;
            if (!fieldsCached.TryGetValue (dataType, out fieldsWithAttribute))
            {
                fieldsWithAttribute = dataType.GetFields ().Where (fieldInfo => fieldInfo.IsDefined (attributeType, false)).ToList ();
                fieldsCached.Add (dataType, fieldsWithAttribute);
            }

            foreach (var fieldInfo in fieldsWithAttribute)
            {
                var attributeData = fieldInfo.GetCustomAttributes (attributeType, false).FirstOrDefault () as BinaryDataAttribute;
                if (attributeData == null)
                    continue;

                var filename = fieldInfo.Name;
                if (!string.IsNullOrEmpty (attributeData.alias))
                    filename = attributeData.alias;

                foreach (var c in invalidChars)
                    filename = filename.Replace (c, '_');

                var value = fieldInfo.GetValue (data);
                if (value == null)
                    continue;

                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsArray)
                {
                    if (!typesArray.ContainsKey (fieldType))
                    {
                        Debug.LogWarning ($"Can't serialize array to binary at path {pathBase}{filename} - type {fieldType.Name} is not supported");
                        continue;
                    }
                    
                    var (extension, _, save) = typesArray[fieldType];
                    var pathFull = pathBase + filename + extension;
                    // Debug.Log ($"Saving array {pathBase}{filename} (type {fieldType.Name}) to {pathFull}");

                    try
                    {
                        save (value, pathFull);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarningFormat ("Failed to serialize array to binary due to exception | Field: {0} | Type: {1} | Path: {2}", fieldInfo.Name, fieldType.Name, pathFull);
                        Debug.LogException (e);
                        return false;
                    }
                }
                else
                {
                    if (!typesObject.ContainsKey (fieldType))
                    {
                        Debug.LogWarning ($"Can't serialize object to binary at path {pathBase}{filename} - type {fieldType.Name} is not supported");
                        continue;
                    }
                    
                    var (extension, _, save) = typesObject[fieldType];
                    var pathFull = pathBase + filename + extension;
                   //  Debug.Log ($"Saving object {pathBase}{filename} (type {fieldType.Name}) to {pathFull}");
                    
                    try
                    {
                        save (value, pathFull);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarningFormat ("Failed to serialize object to binary due to exception | Field: {0} | Type: {1} | Path: {2}", fieldInfo.Name, fieldType.Name, pathFull);
                        Debug.LogException (e);
                        return false;
                    }
                }
            }

            return true;
        }

        
        
        
        static void SaveByteArray (object source, string pathFull)
        {
            var ary = (byte[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                outp.Write (ary);
            }
        }
        
        static void SaveBoolArray (object source, string pathFull)
        {
            var ary = (bool[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v);
                }
            }
        }

        static void SaveIntArray (object source, string pathFull)
        {
            var ary = (int[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v);
                }
            }
        }
        
        static void SaveInt3Array (object source, string pathFull)
        {
            var ary = (int3[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v.x);
                    outp.Write (v.y);
                    outp.Write (v.z);
                }
            }
        }

        static void SaveFloatArray (object source, string pathFull)
        {
            var ary = (float[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v);
                }
            }
        }

        static void SaveFloat3Array (object source, string pathFull)
        {
            var ary = (float3[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v.x);
                    outp.Write (v.y);
                    outp.Write (v.z);
                }
            }
        }

        static void SaveFloat4Array (object source, string pathFull)
        {
            var ary = (float4[])source;
            using (var outp = new BinaryWriter (new FileStream (pathFull, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (var v in ary)
                {
                    outp.Write (v.x);
                    outp.Write (v.y);
                    outp.Write (v.z);
                    outp.Write (v.w);
                }
            }
        }
        
        

        public static bool LoadFieldsFromBinary<T> (T data, string pathBase)
        {
            if (data == null)
                return false;
            
            var dataType = data.GetType();

            List<FieldInfo> fieldsWithAttribute = null;
            if (!fieldsCached.TryGetValue (dataType, out fieldsWithAttribute))
            {
                fieldsWithAttribute = dataType.GetFields ().Where (fieldInfo => fieldInfo.IsDefined (attributeType, false)).ToList ();
                fieldsCached.Add (dataType, fieldsWithAttribute);
            }
            
            foreach (var fieldInfo in fieldsWithAttribute)
            {
                var attributeData = fieldInfo.GetCustomAttributes (attributeType, false).FirstOrDefault () as BinaryDataAttribute;
                if (attributeData == null)
                    continue;
                
                var filename = fieldInfo.Name;
                if (!string.IsNullOrEmpty (attributeData.alias))
                    filename = attributeData.alias;
                
                foreach (var c in Path.GetInvalidFileNameChars ())
                    filename = filename.Replace (c, '_');

                var fieldType = fieldInfo.FieldType;
                if (fieldType.IsArray)
                {
                    if (!typesArray.ContainsKey (fieldType))
                    {
                        Debug.LogWarning ($"Can't deserialize array to binary at path {pathBase}{filename} - type {fieldType.Name} is not supported");
                        continue;
                    }
                    
                    var elementType = fieldType.GetElementType ();
                    if (elementType == null)
                        continue;
                    
                    var (extension, read, _) = typesArray[fieldType];
                    var array = Array.CreateInstance (elementType, 0);
                    
                    var (ok, pathFull, fileLength) = ResolvePath (pathBase, filename, extension, attributeData.logIfMissing);
                    if (ok)
                    {
                        try
                        {
                            read (ref array, pathFull, fileLength);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError ("Exception while reading binary file: " + pathFull);
                            Debug.LogException (e);
                            return false;
                        }
                    }
                    
                    fieldInfo.SetValue (data, array);
                }
                else
                {
                    if (!typesObject.ContainsKey (fieldType))
                    {
                        Debug.LogWarning ($"Can't deserialize object to binary at path {pathBase}{filename} - type {fieldType.Name} is not supported");
                        continue;
                    }
                    
                    var elementType = fieldType.GetElementType ();
                    if (elementType == null)
                        continue;
                    
                    var (extension, read, _) = typesObject[fieldType];
                    object value = null;
                    
                    var (ok, pathFull, fileLength) = ResolvePath (pathBase, filename, extension, attributeData.logIfMissing);
                    if (ok)
                    {
                        try
                        {
                            read (ref value, pathFull, fileLength);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError ("Exception while reading binary file: " + pathFull);
                            Debug.LogException (e);
                            return false;
                        }
                    }
                    
                    fieldInfo.SetValue (data, value);
                }
            }
            
            return true;
        }

        static (bool, string, long) ResolvePath (string pathBase, string filename, string extension, bool logIfMissing)
        {
            var pathFull = pathBase + filename + extension;
            var fileInfo = new FileInfo (pathFull);
            if (!fileInfo.Exists)
            {
                if (logIfMissing)
                    Debug.LogWarning ($"Can't deserialize object from binary at path {pathFull} - file doesn't exist");
                return (false, "", 0);
            }
            return (true, pathFull, fileInfo.Length);
        }
        
        static void ReadByteArray (ref Array target, string pathFull, long fileLength)
        {
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                target = inp.ReadBytes ((int)fileLength);
            }
        }
        
        static void ReadBoolArray (ref Array target, string pathFull, long fileLength)
        {
            var ary = new bool[fileLength];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    ary[index] = inp.ReadBoolean ();
                }
            }
            target = ary;
        }

        static void ReadIntArray (ref Array target, string pathFull, long fileLength)
        {
            var ary = new int[fileLength / 4];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    ary[index] = inp.ReadInt32 ();
                }
            }
            target = ary;
        }
        
        static void ReadInt3 (ref Array target, string pathFull, long fileLength)
        {
            var ary = new int3[fileLength / 4 / 3];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    var x = inp.ReadInt32 ();
                    var y = inp.ReadInt32 ();
                    var z = inp.ReadInt32 ();
                    ary[index] = new int3 (x, y, z);
                }
            }
            target = ary;
        }

        static void ReadFloat (ref Array target, string pathFull, long fileLength)
        {
            var ary = new float[fileLength / 4];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    ary[index] = inp.ReadSingle ();
                }
            }
            target = ary;
        }

        static void ReadFloat3 (ref Array target, string pathFull, long fileLength)
        {
            var ary = new float3[fileLength / 4 / 3];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    var x = inp.ReadSingle ();
                    var y = inp.ReadSingle ();
                    var z = inp.ReadSingle ();
                    ary[index] = new float3 (x, y, z);
                }
            }
            target = ary;
        }

        static void ReadFloat4 (ref Array target, string pathFull, long fileLength)
        {
            var ary = new float4[fileLength / 4 / 4];
            using (var inp = new BinaryReader (new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                for (var index = 0; index < ary.Length; index += 1)
                {
                    var x = inp.ReadSingle ();
                    var y = inp.ReadSingle ();
                    var z = inp.ReadSingle ();
                    var w = inp.ReadSingle ();
                    ary[index] = new float4 (x, y, z, w);
                }
            }
            target = ary;
        }
        
        public static void ReadBinaryFormattedObject<T> (ref T target, string pathFull)
        {
            if (!File.Exists (pathFull))
            {
                Debug.LogWarning ($"Can't deserialize object from binary at path {pathFull} - file doesn't exist");
                // fileLoaded = false;
                target = default (T);
                return;
            }

            IFormatter formatter;
            Stream stream;

            try
            {
                formatter = new BinaryFormatter ();
                stream = new FileStream (pathFull, FileMode.Open, FileAccess.Read, FileShare.None);
                T content = (T)formatter.Deserialize (stream);
                stream.Close ();
                // fileLoaded = true;
                target = content;
            }
            catch (Exception e)
            {
                Debug.LogException (e);
                // fileLoaded = false;
                target = default (T);
            }
        }
    }
}