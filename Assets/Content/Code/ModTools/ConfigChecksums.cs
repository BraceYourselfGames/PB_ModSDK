using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

namespace PhantomBrigade.SDK.ModTools
{
    using Data;

    public static class ConfigChecksums
    {
        // ***** INCREMENT THIS VALUE EVERY TIME CHANGES TO THE CONFIG DATABASE YAML FILES ARE COMMITTED *****
        public const byte configDatabaseVersion = 2;

        public static bool logPathResolution = false;

        public static bool ChecksumsExist (DirectoryInfo source) => File.Exists (Path.Combine (source.FullName, checksumsFileName));
        public static bool CopyChecksumsFile (DirectoryInfo source, DirectoryInfo dest)
        {
            var checksumsPath = Path.Combine (source.FullName, checksumsFileName);
            var destPath = Path.Combine (dest.FullName, checksumsFileName);
            try
            {
                File.Copy (checksumsPath, destPath, true);
                var fi = new FileInfo (destPath);
                fi.Attributes |= FileAttributes.Hidden;
                return true;
            }
            catch (IOException ioe)
            {
                Debug.LogErrorFormat ("Failed to copy checksums to mod | sdk: {0} | mod: {1}", checksumsPath, destPath);
                Debug.LogException (ioe);
                return false;
            }
        }

        public static void UpdateChecksums (EntrySource source, ConfigDirectory root, ConfigDirectory terminal)
        {
            var dq = new Stack<ConfigDirectory> ();
            dq.Push (root);
            var current = root;
            foreach (var i in terminal.Locator)
            {
                current = (ConfigDirectory)current.Entries[i];
                dq.Push (current);
            }
            dq.Push (terminal);
            while (dq.Count != 0)
            {
                current = dq.Pop ();
                var checksum = current.Checksum;
                current.ComputeChecksum ();
                if (!ChecksumEqual (checksum, current.Checksum))
                {
                    current.Source = source;
                }
            }
        }

        public static bool ChecksumEqual (ConfigEntry sdk, ConfigEntry mod)
        {
            if (sdk == null || mod == null)
            {
                return false;
            }
            if (!ChecksumEqual (sdk.Checksum, mod.Checksum))
            {
                return false;
            }
            return sdk.RelativePath == mod.RelativePath;
        }

        public static bool ChecksumEqual (Checksum lhs, Checksum rhs) => lhs.HalfSum1 == rhs.HalfSum1 && lhs.HalfSum2 == rhs.HalfSum2;

        static readonly byte[] signatureBytes =
        {
            0x63,
            0x63,
            0,  // file format version -- increment when the format is changed
            configDatabaseVersion,  // version of the config databases
        };
        static readonly uint signature = BitConverter.ToUInt32 (signatureBytes, 0);

        static readonly char[] pathSeparators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
        };
        const string checksumsFileName = "checksums.bin";

        public const string DataDecomposedDirectoryName = "DataDecomposed";

        public sealed class Serializer
        {
            public Serializer (DirectoryInfo sourceDirectory, EntrySource source)
            {
                this.sourceDirectory = sourceDirectory;
                pathPrefix = sourceDirectory.FullName;
                configSource = source;
                root = new ConfigDirectory (EntryType.Root)
                {
                    Source = configSource,
                    RelativePath = "",
                    Locator = Array.Empty<int> (),
                };
                current = root;
                useRootChecksumAsOrigin = true;
            }

            public Serializer (DirectoryInfo sourceDirectory, ConfigDirectory root, Checksum originChecksum)
            {
                this.sourceDirectory = sourceDirectory;
                pathPrefix = sourceDirectory.FullName;
                this.root = root;
                this.originChecksum = originChecksum;
                current = root;
            }

            public void PushBranch (string path)
            {
                var relpath = path.Substring (pathPrefix.Length + 1);
                if (relpath == current.RelativePath)
                {
                    return;
                }
                while (!relpath.StartsWith (current.RelativePath))
                {
                    PopBranch ();
                }
                if (current.RelativePath.Length != 0 && Path.GetDirectoryName (relpath) != current.RelativePath)
                {
                    PopBranch ();
                }
                if (current.RelativePath.Length != 0)
                {
                    relpath = relpath.Substring (current.RelativePath.Length + 1);
                }

                var parts = relpath.Split (pathSeparators);
                foreach (var rp in parts)
                {
                    var found = false;
                    foreach (var entry in current.Entries)
                    {
                        if (entry.Type == EntryType.File)
                        {
                            continue;
                        }
                        if (Path.GetFileName (entry.RelativePath) != rp)
                        {
                            continue;
                        }
                        current = (ConfigDirectory)entry;
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        continue;
                    }
                    relpath = Path.Combine (current.RelativePath, rp);
                    var next = new ConfigDirectory (EntryType.Directory)
                    {
                        Source = configSource,
                        RelativePath = relpath,
                        Locator = new int[current.Locator.Length + 1],
                    };
                    Buffer.BlockCopy (current.Locator, 0, next.Locator, 0, current.Locator.Length * 4);
                    next.Locator[current.Locator.Length] = current.Entries.Count;
                    current.Entries.Add (next);
                    current = next;
                }
            }

            public void AddFile (FileInfo file)
            {
                var entry = new ConfigFile ()
                {
                    RelativePath = file.FullName.Substring (pathPrefix.Length + 1),
                };
                entry.Update (configSource, File.ReadAllBytes (file.FullName));
                current.Entries.Add (entry);
            }

            public void Save ()
            {
                current = root;
                ComputeDirectoryChecksums (root);
                var pathChecksums = Path.Combine (sourceDirectory.Parent.FullName, checksumsFileName);
                var fi = new FileInfo (Path.Combine (pathChecksums + ".tmp"));
                using (var outp = new BinaryWriter (fi.OpenWrite ()))
                {
                    directoryQueue.Clear ();
                    fileQueue.Clear ();
                    outp.Write (signatureBytes);
                    var osum = useRootChecksumAsOrigin ? root.Checksum : originChecksum;
                    outp.Write (osum.HalfSum1);
                    outp.Write (osum.HalfSum2);
                    if (root.Entries.Count == 0)
                    {
                        outp.Write ((byte)root.Type);
                        outp.Write ((byte)root.Source);
                        outp.Write ((byte)0);
                        outp.Write (root.Checksum.HalfSum1);
                        outp.Write (root.Checksum.HalfSum2);
                        outp.Write ((ushort)0);
                        outp.Write ((uint)root.Entries.Count);
                        return;
                    }
                    SaveDirectoryChecksum (root, outp);
                    while (directoryQueue.Count != 0)
                    {
                        SaveDirectoryChecksum (directoryQueue.Dequeue (), outp);
                    }
                    while (fileQueue.Count != 0)
                    {
                        var (locator, index, entry) = fileQueue.Dequeue ();
                        SaveFileChecksum (locator, index, entry, outp);
                    }
                }
                if (File.Exists (pathChecksums))
                {
                    fi = fi.Replace (pathChecksums, null);
                }
                else
                {
                    fi.MoveTo (pathChecksums);
                }
            }

            void PopBranch ()
            {
                if (current == root)
                {
                    return;
                }
                var target = root;
                for (var i = 0; i < current.Locator.Length - 1; i += 1)
                {
                    target = (ConfigDirectory)target.Entries[current.Locator[i]];
                }
                current = target;
            }

            void SaveDirectoryChecksum (ConfigDirectory configDirectory, BinaryWriter outp)
            {
                outp.Write ((byte)configDirectory.Type);
                outp.Write ((byte)configDirectory.Source);
                outp.Write ((byte)configDirectory.Locator.Length);
                foreach (var i in configDirectory.Locator)
                {
                    outp.Write ((byte)i);
                }
                outp.Write (configDirectory.Checksum.HalfSum1);
                outp.Write (configDirectory.Checksum.HalfSum2);
                var rpl = Encoding.UTF8.GetByteCount (configDirectory.RelativePath);
                outp.Write ((ushort)rpl);
                if (rpl != 0)
                {
                    outp.Write (Encoding.UTF8.GetBytes (configDirectory.RelativePath), 0, rpl);
                }
                var dirCount = configDirectory.Entries.FindIndex (ent => ent.Type == EntryType.File);
                if (dirCount == -1)
                {
                    dirCount = configDirectory.Entries.Count;
                }
                outp.Write ((ushort)dirCount);
                outp.Write ((ushort)(configDirectory.Entries.Count - dirCount));
                for (var i = 0; i < dirCount; i += 1)
                {
                    var entry = (ConfigDirectory)configDirectory.Entries[i];
                    Buffer.BlockCopy (configDirectory.Locator, 0, entry.Locator, 0, configDirectory.Locator.Length * 4);
                    entry.Locator[entry.Locator.Length - 1] = (byte)i;
                    directoryQueue.Enqueue (entry);
                }
                for (var i = dirCount; i < configDirectory.Entries.Count; i += 1)
                {
                    fileQueue.Enqueue ((configDirectory.Locator, i, configDirectory.Entries[i]));
                }
            }

            static void ComputeDirectoryChecksums (ConfigDirectory configDirectory)
            {
                foreach (var cfgDir in configDirectory.Entries.OfType<ConfigDirectory> ())
                {
                    ComputeDirectoryChecksums (cfgDir);
                }
                configDirectory.ComputeChecksum ();
            }

            void SaveFileChecksum (int[] locator, int index, ConfigEntry entry, BinaryWriter outp)
            {
                outp.Write ((byte)entry.Type);
                outp.Write ((byte)entry.Source);
                outp.Write ((byte)locator.Length);
                foreach (var i in locator)
                {
                    outp.Write ((byte)i);
                }
                outp.Write ((ushort)index);
                outp.Write (entry.Checksum.HalfSum1);
                outp.Write (entry.Checksum.HalfSum2);
                var rpl = Encoding.UTF8.GetByteCount (entry.RelativePath);
                outp.Write ((ushort)rpl);
                outp.Write (Encoding.UTF8.GetBytes (entry.RelativePath), 0, rpl);
            }

            readonly DirectoryInfo sourceDirectory;
            readonly string pathPrefix;
            readonly EntrySource configSource;
            readonly ConfigDirectory root;
            readonly Checksum originChecksum;
            readonly bool useRootChecksumAsOrigin;
            readonly Queue<ConfigDirectory> directoryQueue = new Queue<ConfigDirectory> ();
            readonly Queue<(int[], int, ConfigEntry)> fileQueue = new Queue<(int[], int, ConfigEntry)> ();
            ConfigDirectory current;
        }

        public sealed class Deserializer
        {
            public Deserializer (DirectoryInfo rootDirectory)
            {
                checksumsFile = new FileInfo (Path.Combine (rootDirectory.FullName, checksumsFileName));
            }

            public Result Load ()
            {
                var result = new Result ();
                multiLinkerMap.Clear ();
                linkerMap.Clear ();

                if (!checksumsFile.Exists )
                {
                    result.ErrorMessage = "Checksums file does not exist: " + checksumsFile.FullName;
                    return result;
                }

                using (var inp = new BinaryReader (checksumsFile.OpenRead ()))
                {
                    var sig = inp.ReadUInt32 ();
                    if (sig != signature)
                    {
                        var dataVersion = (byte)(sig >> 24);
                        if (configDatabaseVersion < dataVersion)
                        {
                            var sigBytes = string.Join (" ", signatureBytes.Select (b => b.ToString ("x2")));
                            var readBytes = string.Join (" ", BitConverter.GetBytes (sig).Select (b => b.ToString ("x2")));
                            result.ErrorMessage = string.Format ("Signature error | expected: {0} | actual: {1} | file: {2}", sigBytes, readBytes, checksumsFile.FullName);
                            return result;
                        }
                        result.Code = ResultCode.Upgrade;
                        result.DataVersion = dataVersion;
                        result.ErrorMessage = string.Format
                        (
                            "Upgrade required | current version: {0} | file version: {1} | file: {2}",
                            configDatabaseVersion,
                            dataVersion,
                            checksumsFile.FullName
                        );
                    }

                    var originChecksum = new Checksum ()
                    {
                        HalfSum1 = inp.ReadInt64 (),
                        HalfSum2 = inp.ReadInt64 (),
                    };

                    var (ok, errorMessage, root, dirCount, fileCount) = ReadRootEntry (inp);
                    if (!ok)
                    {
                        result.ErrorMessage = errorMessage + " | file: " + checksumsFile.FullName;
                        return result;
                    }

                    directoryQueue.Enqueue ((root, dirCount));
                    fileQueue.Enqueue ((root, fileCount));

                    while (directoryQueue.Count != 0)
                    {
                        var (ent, n) = directoryQueue.Dequeue ();
                        (ok, errorMessage) = ReadDirectoryEntries (inp, ent.Entries, n);
                        if (!ok)
                        {
                            result.ErrorMessage = errorMessage + " | file: " + checksumsFile.FullName;
                            return result;
                        }
                    }

                    while (fileQueue.Count != 0)
                    {
                        var (ent, n) = fileQueue.Dequeue ();
                        (ok, errorMessage) = ReadFileEntries (inp, ent.Entries, n);
                        if (!ok)
                        {
                            result.ErrorMessage = errorMessage + " | file: " + checksumsFile.FullName;
                            return result;
                        }
                    }

                    result.OriginChecksum = originChecksum;
                    result.Root = root;
                    result.MultiLinkerMap = new Dictionary<Type, ConfigDirectory> (multiLinkerMap);
                    result.LinkerMap = new Dictionary<Type, ConfigFile> (linkerMap);
                }

                if (result.Code != ResultCode.Upgrade)
                {
                    result.Code = ResultCode.OK;
                }
                return result;
            }

            (bool, string, ConfigDirectory, int, int) ReadRootEntry (BinaryReader inp)
            {
                if (inp.ReadByte () != (byte)EntryType.Root)
                {
                    return (false, "Root entry should be first entry in file", null, 0, 0);
                }
                var configSource = inp.ReadByte ();
                if (!Enum.IsDefined (typeof(EntrySource), (int)configSource))
                {
                    return (false, "Root entry source should be either SDK or Mod", null, 0, 0);
                }
                if (inp.ReadByte () != 0)
                {
                    return (false, "Root entry should have an empty locator", null, 0, 0);
                }
                var cksum0 = inp.ReadInt64 ();
                var cksum1 = inp.ReadInt64 ();
                if (inp.ReadUInt16 () != 0)
                {
                    return (false, "Root entry should not have a relative path", null, 0, 0);
                }
                var rootEntryCount = inp.ReadUInt32 ();
                if (rootEntryCount == 0)
                {
                    return (false, "Root entry is empty", null, 0, 0);
                }
                var dirCount = (int)(rootEntryCount & 0x0000FFFF);
                var fileCount = (int)((rootEntryCount & 0xFFFF0000) >> 16);

                var root = new ConfigDirectory (EntryType.Root)
                {
                    Source = (EntrySource)configSource,
                    RelativePath = "",
                    Locator = Array.Empty<int> (),
                    Checksum = new Checksum()
                    {
                        HalfSum1 = cksum0,
                        HalfSum2 = cksum1,
                    },
                };
                root.Entries.Capacity = (int)rootEntryCount;
                return (true, "", root, dirCount, fileCount);
            }

            (bool, string) ReadDirectoryEntries (BinaryReader inp, List<ConfigEntry> entries, int entryCount)
            {
                while (entryCount != 0)
                {
                    if (inp.ReadByte () != (byte)EntryType.Directory)
                    {
                        return (false, "Current entry should be a directory");
                    }
                    var configSource = inp.ReadByte ();
                    if (!Enum.IsDefined (typeof(EntrySource), (int)configSource))
                    {
                        return (false, "Entry source should be either SDK or Mod");
                    }
                    var locatorLen = inp.ReadByte ();
                    if (locatorLen == 0)
                    {
                        return (false, "Only the root entry should have an empty locator");
                    }
                    var locator = inp.ReadBytes (locatorLen);
                    if (locatorLen != locator.Length)
                    {
                        return (false, "Locator should have length " + locatorLen);
                    }
                    var cksum0 = inp.ReadInt64 ();
                    var cksum1 = inp.ReadInt64 ();
                    var rpl = inp.ReadUInt16 ();
                    if (rpl == 0)
                    {
                        return (false, "Only the root entry should have an empty relative path");
                    }
                    var rawPath = inp.ReadBytes (rpl);
                    if (rpl != rawPath.Length)
                    {
                        return (false, "Raw path should have length " + rpl);
                    }
                    var entry = new ConfigDirectory (EntryType.Directory)
                    {
                        Source = (EntrySource)configSource,
                        Checksum = new Checksum()
                        {
                            HalfSum1 = cksum0,
                            HalfSum2 = cksum1,
                        },
                        RelativePath = Encoding.UTF8.GetString (rawPath),
                        Locator = locator.Select(v => (int)v).ToArray(),
                    };
                    var dirCount = inp.ReadUInt16 ();
                    directoryQueue.Enqueue ((entry, dirCount));
                    var fileCount = inp.ReadUInt16 ();
                    fileQueue.Enqueue ((entry, fileCount));
                    entries.Add (entry);

                    if (entry.RelativePath.StartsWith (DataDecomposedDirectoryName))
                    {
                        var cleanedPath = DataPathHelper.GetCleanPath (entry.RelativePath) + "/";
                        var typeName = DataPathUtility.GetDataTypeFromPath (cleanedPath, fallbackAllowed: false);
                        if (typeName != null)
                        {
                            var t = FieldReflectionUtility.GetTypeByName (typeName);
                            if (t == null)
                            {
                                if (logPathResolution)
                                {
                                    Debug.Log ("Unable to resolve name to type (directory): " + typeName);
                                }
                            }
                            else
                            {
                                multiLinkerMap[t] = entry;
                            }
                        }
                    }

                    entryCount -= 1;
                }

                return (true, "");
            }

            (bool, string) ReadFileEntries (BinaryReader inp, List<ConfigEntry> entries, int entryCount)
            {
                while (entryCount != 0)
                {
                    if (inp.ReadByte () != (byte)EntryType.File)
                    {
                        return (false, "Current entry should be a file");
                    }
                    var configSource = inp.ReadByte ();
                    if (!Enum.IsDefined (typeof(EntrySource), (int)configSource))
                    {
                        return (false, "Entry source should be either SDK or Mod");
                    }
                    var locatorLen = inp.ReadByte ();
                    var locator = Enumerable.Empty<int> ();
                    if (locatorLen != 0)
                    {
                        var locatorBytes = inp.ReadBytes (locatorLen);
                        if (locatorLen != locatorBytes.Length)
                        {
                            return (false, "Locator should have length " + locatorLen);
                        }
                        locator = locatorBytes.Select (v => (int)v);
                    }
                    var index = inp.ReadUInt16 ();
                    locator = locator.Append (index);
                    var cksum0 = inp.ReadInt64 ();
                    var cksum1 = inp.ReadInt64 ();
                    var rpl = inp.ReadUInt16 ();
                    if (rpl == 0)
                    {
                        return (false, "Only the root entry should have an empty relative path");
                    }
                    var rawPath = inp.ReadBytes (rpl);
                    if (rpl != rawPath.Length)
                    {
                        return (false, "Raw path should have length " + rpl);
                    }
                    var entry = new ConfigFile ()
                    {
                        Source = (EntrySource)configSource,
                        Checksum = new Checksum()
                        {
                            HalfSum1 = cksum0,
                            HalfSum2 = cksum1,
                        },
                        RelativePath = Encoding.UTF8.GetString (rawPath),
                        Locator = locator.ToArray(),
                    };
                    entries.Add (entry);

                    var ext = Path.GetExtension (entry.RelativePath);
                    if (!entry.RelativePath.StartsWith (DataDecomposedDirectoryName) && ext == ".yaml")
                    {
                        var cleanedPath = DataPathHelper.GetCleanPath (entry.RelativePath);
                        cleanedPath = cleanedPath.Substring (0, cleanedPath.Length - ext.Length);
                        var typeName = DataPathUtility.GetDataTypeFromPath (cleanedPath);
                        if (typeName != null)
                        {
                            var t = FieldReflectionUtility.GetTypeByName (typeName);
                            if (t == null)
                            {
                                if (logPathResolution)
                                {
                                    Debug.Log ("Unable to resolve name to type (file): " + typeName);
                                }
                            }
                            else
                            {
                                linkerMap[t] = entry;
                            }
                        }
                    }

                    entryCount -= 1;
                }
                return (true, "");
            }

            readonly FileInfo checksumsFile;
            readonly Queue<(ConfigDirectory, int)> directoryQueue = new Queue<(ConfigDirectory, int)> ();
            readonly Queue<(ConfigDirectory, int)> fileQueue = new Queue<(ConfigDirectory, int)> ();
            readonly Dictionary<Type, ConfigDirectory> multiLinkerMap = new Dictionary<Type, ConfigDirectory> ();
            readonly Dictionary<Type, ConfigFile> linkerMap = new Dictionary<Type, ConfigFile> ();

            public sealed class Result
            {
                public ResultCode Code;
                public byte DataVersion;
                public string ErrorMessage;
                public Checksum OriginChecksum;
                public ConfigDirectory Root;
                public Dictionary<Type, ConfigDirectory> MultiLinkerMap;
                public Dictionary<Type, ConfigFile> LinkerMap;
            }

            public enum ResultCode
            {
                Error = 0,
                Upgrade = 1,
                OK = 2,
            }
        }

        public struct Checksum
        {
            public long HalfSum1;
            public long HalfSum2;
        }

        public enum EntryType
        {
            File = 0,
            Directory,
            Root,
        }

        public enum EntrySource
        {
            SDK = 0,
            Mod,
        }

        public abstract class ConfigEntry
        {
            protected ConfigEntry (EntryType type)
            {
                Type = type;
            }

            public readonly EntryType Type;
            public EntrySource Source;
            public int[] Locator;
            public string RelativePath;
            public Checksum Checksum;

            public virtual void UpdateLocator (int i)
            {
                if (Locator.Length == 0)
                {
                    return;
                }
                Locator[Locator.Length - 1] = i;
            }
        }

        public sealed class ConfigDirectory : ConfigEntry
        {
            public ConfigDirectory (EntryType type) : base (type) { }

            public override void UpdateLocator (int i)
            {
                base.UpdateLocator (i);
                FixLocators ();
            }

            public void FixLocators ()
            {
                SortEntries ();
                for (var i = 0; i < Entries.Count; i += 1)
                {
                    var entry = Entries[i];
                    Array.Copy (Locator, entry.Locator, Locator.Length);
                    entry.UpdateLocator (i);
                }
            }

            public void Patch (ConfigDirectory entry)
            {
                var cfgDir = this;
                for (var i = 0; i < entry.Locator.Length - 1; i += 1)
                {
                    var loc = entry.Locator[i];
                    cfgDir = (ConfigDirectory)cfgDir.Entries[loc];
                }
                cfgDir.Entries[entry.Locator.Last ()] = entry;
            }

            public List<string> Upsert (DirectoryInfo root, EntrySource source, HashSet<string> keys)
            {
                var errorKeys = new List<string> ();
                var locator = Locator.Append (0);
                var newEntries = keys
                    .OrderBy (key => key)
                    .Where(key =>  Directory.Exists (Path.Combine (root.FullName, RelativePath, key)))
                    .Select (key => new ConfigDirectory (EntryType.Directory)
                    {
                        Source = source,
                        RelativePath = Path.Combine (RelativePath, key),
                        Locator = locator.ToArray (),
                    })
                    .ToList ();
                var errorEntries = new List<int> ();
                for (var i = 0; i < newEntries.Count; i += 1)
                {
                    var entry = newEntries[i];
                    var subdirectory = new DirectoryInfo (Path.Combine (root.FullName, entry.RelativePath));
                    try
                    {
                        AddRecursive (root, source, entry, subdirectory);
                    }
                    catch (Exception ex)
                    {
                        var key = subdirectory.Name;
                        Debug.LogError ("Error during checksum operation | key: " + key + " | path: " + entry.RelativePath);
                        Debug.LogException (ex);
                        errorKeys.Add (key);
                        errorEntries.Add (-i);
                    }
                }
                errorEntries.Sort ();
                foreach (var idx in errorEntries)
                {
                    newEntries.RemoveAt (-idx);
                }
                Entries.Clear ();
                Entries.AddRange (newEntries);
                FixLocators ();
                UpdateChecksum (source);
                return errorKeys;
            }

            public void AddFiles (EntrySource source, IEnumerable<(string FileName, string Content)> contents)
            {
                foreach (var content in contents.OrderBy(x => x.FileName, StringComparer.InvariantCultureIgnoreCase))
                {
                    var ce = new ConfigFile ()
                    {
                        RelativePath = Path.Combine (RelativePath, content.FileName),
                        Locator = Locator.Append (Entries.Count).ToArray (),
                    };
                    Entries.Add (ce);
                    ce.Update (source, content.Content);
                }
                FixLocators ();
                UpdateChecksum (source);
            }

            public void ComputeChecksum ()
            {
                var (offset, bytes) = GetBuffer ();
                bytes[0] = (byte)Type;
                SortEntries ();
                for (var i = 0; i < Entries.Count; i += 1)
                {
                    var entry = Entries[i];
                    var checksumAsBytes = BitConverter.GetBytes (entry.Checksum.HalfSum1);
                    Buffer.BlockCopy (checksumAsBytes, 0, bytes, offset + i * 16, checksumAsBytes.Length);
                    checksumAsBytes = BitConverter.GetBytes (entry.Checksum.HalfSum2);
                    Buffer.BlockCopy (checksumAsBytes, 0, bytes, offset + i * 16 + 8, checksumAsBytes.Length);
                }
                using (var algo = MD5.Create ())
                {
                    var checksum = algo.ComputeHash (bytes);
                    Checksum.HalfSum1 = BitConverter.ToInt64 (checksum, 0);
                    Checksum.HalfSum2 = BitConverter.ToInt64 (checksum, 8);
                }
            }

            static void AddRecursive (DirectoryInfo root, EntrySource source, ConfigDirectory configDir, DirectoryInfo directory)
            {
                var newEntries = directory.EnumerateDirectories()
                    .OrderBy(di => di.Name, StringComparer.InvariantCultureIgnoreCase)
                    .Select (d => new ConfigDirectory (EntryType.Directory)
                    {
                        Source = source,
                        RelativePath = Path.Combine (configDir.RelativePath, d.Name),
                        Locator = configDir.Locator.Append (configDir.Entries.Count).ToArray (),
                    })
                    .ToList ();
                configDir.Entries.AddRange (newEntries);
                foreach (var file in directory.EnumerateFiles ().OrderBy(fi => fi.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    var entry = new ConfigFile ()
                    {
                        RelativePath = Path.Combine (configDir.RelativePath, file.Name),
                        Locator = configDir.Locator.Append (configDir.Entries.Count).ToArray (),
                    };
                    entry.Update (source, File.ReadAllBytes (file.FullName));
                    configDir.Entries.Add (entry);
                }
                foreach (var entry in newEntries)
                {
                    var subdirectory = new DirectoryInfo (Path.Combine (root.FullName, entry.RelativePath));
                    if (!subdirectory.Exists)
                    {
                        continue;
                    }
                    AddRecursive (root, source, entry, subdirectory);
                }
                configDir.ComputeChecksum ();
            }

            void UpdateChecksum (EntrySource source)
            {
                var csum = Checksum;
                ComputeChecksum ();
                if (!ChecksumEqual (csum, Checksum))
                {
                    Source = source;
                }
            }

            void SortEntries () => Entries.Sort (OrderByEntryTypeAndName);

            (int, byte[]) GetBuffer ()
            {
                var offset = 1;
                var bufferLength = offset + Entries.Count * 16;
                var keyAsBytes = Encoding.UTF8.GetBytes (Path.GetFileNameWithoutExtension (RelativePath));
                bufferLength += keyAsBytes.Length;
                var bytes = new byte[bufferLength];
                Buffer.BlockCopy (keyAsBytes, 0, bytes, offset, keyAsBytes.Length);
                offset += keyAsBytes.Length;
                return (offset, bytes);
            }

            static int OrderByEntryTypeAndName (ConfigEntry lhs, ConfigEntry rhs)
            {
                var order = -((int)lhs.Type).CompareTo ((int)rhs.Type);
                return order != 0 ? order : StringComparer.InvariantCultureIgnoreCase.Compare (lhs.RelativePath, rhs.RelativePath);
            }

            public readonly List<ConfigEntry> Entries = new List<ConfigEntry> ();
        }

        public sealed class ConfigFile : ConfigEntry
        {
            public ConfigFile() : base(EntryType.File) { }

            public void Update (EntrySource source, string content)
            {
                var raw = Encoding.UTF8.GetBytes (content);
                Update (source, raw);
            }

            public void Update (EntrySource source, byte[] content)
            {
                var key = Path.GetFileNameWithoutExtension (RelativePath);
                var keyAsBytes = Encoding.UTF8.GetBytes (key);
                var bytes = new byte[1 + keyAsBytes.Length + content.Length];
                bytes[0] = (byte)Type;
                Buffer.BlockCopy (keyAsBytes, 0, bytes, 1, keyAsBytes.Length);
                Buffer.BlockCopy (content, 0, bytes, 1 + keyAsBytes.Length, content.Length);
                using (var algo = MD5.Create ())
                {
                    var checksum = algo.ComputeHash (bytes);
                    var csum = new Checksum ()
                    {
                        HalfSum1 = BitConverter.ToInt64 (checksum, 0),
                        HalfSum2 = BitConverter.ToInt64 (checksum, 8),
                    };
                    if (!ChecksumEqual (csum, Checksum))
                    {
                        Checksum.HalfSum1 = csum.HalfSum1;
                        Checksum.HalfSum2 = csum.HalfSum2;
                        Source = source;
                    }
                }
            }
        }
    }
}
