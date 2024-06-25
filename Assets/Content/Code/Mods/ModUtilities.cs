/*
Code based on this repository
https://github.com/echkode/PhantomBrigadeMod_ProcessConfigEdit

BSD 3-Clause License
Copyright (c) 2023, EchKode

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PhantomBrigade.Data;
using UnityEngine;

namespace PhantomBrigade.Mods
{
    public static class ModUtilities
    {
	    public enum EditOperation
        {
            Override = 0,
            Add,
            Remove,
            DefaultValue,
            NullValue,
        }

        public static class Constants
        {
            public static class Operator
            {
                public const string Insert = "!+";
                public const string Remove = "!-";
                public const string DefaultValue = "!d";
                public const string NullValue = "!n";
            }
        }

        public class EditSpec
        {
            public string dataTypeName;
            public object root;
            public string filename;
            public string fieldPath;
            public string valueRaw;
            public int modIndex;
            public string modID;
            public EditState state = new EditState();

            public EditSpec () { }

            public EditSpec (EditSpec source)
            {
	            CopyFrom (source);
            }

            public void CopyFrom (EditSpec source)
            {
	            if (source == null)
		            return;

	            dataTypeName = source.dataTypeName;
	            root = source.root;
	            filename = source.filename;
	            fieldPath = source.fieldPath;
	            valueRaw = source.valueRaw;
	            modIndex = source.modIndex;
	            modID = source.modID;

	            if (source.state != null)
	            {
		            if (state == null)
			            state = new EditState ();
		            state.CopyFrom (source.state);
	            }
	            else
		            state = null;
            }

            public override string ToString ()
            {
	            return string.Format
		        (
			        "- Data type: {0}\n- Root: {1}\n- Filename: {2}\n- Field path: {3}\n- Raw value: {4}\n- Mod index & ID: {5} / {6}",
			        dataTypeName,
			        root != null ? root.ToString () : "null",
			        filename,
			        fieldPath,
			        valueRaw,
			        modIndex,
			        modID
		        );
            }
        }

        public class EditState
        {
            public object target;
            public EditOperation op;
            public int pathSegmentCount;
            public int pathSegmentIndex;
            public string pathSegment;
            public bool atEndOfPath;
            public int targetIndex;
            public object parent;
            public object targetKey;
            public FieldInfo fieldInfo;
            public Type targetType;

            public EditState () { }

            public EditState (EditState source)
            {
	            CopyFrom (source);
            }

            public void CopyFrom (EditState source)
            {
	            if (source == null)
	            {
		            Clear ();
		            return;
	            }

	            target = source.target;
	            op = source.op;
	            pathSegmentCount = source.pathSegmentCount;
	            pathSegmentIndex = source.pathSegmentIndex;
	            pathSegment = source.pathSegment;
	            atEndOfPath = source.atEndOfPath;
	            targetIndex = source.targetIndex;
	            parent = source.parent;
	            targetKey = source.targetKey;
	            fieldInfo = source.fieldInfo;
	            targetType = source.targetType;
            }

            public void Clear ()
            {
	            target = null;
	            op = EditOperation.DefaultValue;
	            pathSegmentCount = 0;
	            pathSegmentIndex = 0;
	            pathSegment = null;
	            atEndOfPath = false;
	            targetIndex = 0;
	            parent = null;
	            targetKey = null;
	            fieldInfo = null;
	            targetType = null;
            }

            public EditState GetCopy ()
            {
	            var copy = this.MemberwiseClone ();
	            return copy as EditState;
            }

            public override string ToString ()
            {
	            return string.Format
	            (
		            "- Target (index, key, type): {0} ({1}, {2}, {3})\n- Operation: {4}\n- Path segment (index/count): {5} ({6}/{7})\n- Field (type): {8} ({9})",
		            target != null ? target.GetType ().ToString () : "null",
		            targetIndex,
		            targetKey,
		            targetType != null ? targetType.GetNiceTypeName () : "null",
		            op,
		            pathSegment,
		            pathSegmentIndex,
		            pathSegmentCount,
		            fieldInfo != null ? fieldInfo.Name : "null",
		            fieldInfo != null ? fieldInfo.FieldType.GetNiceTypeName () : "?"
	            );
            }
        }

        private static Type typeString;
		private static Type typeBool;
		private static Type typeInt;
		private static Type typeFloat;
		private static Type typeVector2;
		private static Type typeVector3;
		private static Type typeVector4;
		private static Type typeColor;
		private static Type typeIList;
		private static Type typeHashSet;
		private static Type typeIDictionary;
		private static Type typeEnum;

		private static HashSet<Type> allowedKeyTypes;

		private static Dictionary<string, EditOperation> operationMap;
		private static HashSet<EditOperation> allowedHashSetOperations;
		private static Dictionary<Type, Action<EditSpec, Action<object>>> updaterMap;
		private static Dictionary<Type, object> defaultValueMap;

		private static bool logReferenceSetup = false;

		public static void Initialize (bool full = true)
		{
			typeString = typeof(string);
			typeBool = typeof(bool);
			typeInt = typeof(int);
			typeFloat = typeof(float);
			typeVector2 = typeof(Vector2);
			typeVector3 = typeof(Vector3);
			typeVector4 = typeof(Vector4);
			typeColor = typeof(Color);
			typeIList = typeof(IList);
			typeHashSet = typeof(HashSet<string>);
			typeIDictionary = typeof(IDictionary);
			typeEnum = typeof(Enum);

			operationMap = new Dictionary<string, EditOperation>()
			{
				[Constants.Operator.Insert] = EditOperation.Add,
				[Constants.Operator.Remove] = EditOperation.Remove,
				[Constants.Operator.DefaultValue] = EditOperation.DefaultValue,
				[Constants.Operator.NullValue] = EditOperation.NullValue,
			};

			allowedHashSetOperations = new HashSet<EditOperation>()
			{
				EditOperation.Add,
				EditOperation.Remove,
				EditOperation.DefaultValue,
			};

			allowedKeyTypes = new HashSet<Type>()
			{
				typeof(string),
				typeof(int),
			};

			updaterMap = new Dictionary<Type, Action<EditSpec, Action<object>>>()
			{
				[typeString] = UpdateStringField,
				[typeBool] = UpdateBoolField,
				[typeInt] = UpdateIntField,
				[typeFloat] = UpdateFloatField,
				[typeVector2] = UpdateVector2Field,
				[typeVector3] = UpdateVector3Field,
				[typeVector4] = UpdateVector4Field,
				[typeColor] = UpdateColorField,
				[typeHashSet] = UpdateHashSet,
				[typeEnum] = UpdateEnum,
			};

			if (full)
			{
				UtilitiesYAML.RebuildTagMappings ();
				var tagMappings = UtilitiesYAML.GetTagMappings ();

				Debug.LogFormat
				(
					"Mod {0} ({1}) YAML tags ({2}):\n{3}",
					ModLink.modIndex,
					ModLink.modID,
					tagMappings.Count,
					tagMappings.ToStringFormattedKeyValuePairs (true, multilinePrefix: "- ")
				);
			}

			defaultValueMap = new Dictionary<Type, object>()
			{
				[typeString] = "",
				[typeInt] = 0,
				[typeFloat] = 0f,
				[typeVector3] = Vector3.zero,
			};
		}

		private static List<Type> typesInheritingMultilinker = null;

		/*
		internal static string FindConfigKeyIfEmpty
		(
			object target,
			string dataTypeName,
			string key
		)
		{
			if (!string.IsNullOrEmpty(key))
			{
				return key;
			}

			if (typesInheritingMultilinker == null)
			{
				var assembly = System.Reflection.Assembly.GetExecutingAssembly ();
				var types = assembly.GetTypes ();

				var typeGenericMultilinker = typeof (DataMultiLinker<>);
				typesInheritingMultilinker = types.Where
				(
					t => t.BaseType != null &&
					     t.BaseType.IsGenericType &&
					     t.BaseType.GetGenericTypeDefinition () == typeGenericMultilinker
				).ToList ();
			}

			Type typeMatched = null;
			foreach (var typeCandidate in typesInheritingMultilinker)
			{
				var genericArgument = typeCandidate.GenericTypeArguments.FirstOrDefault();
				if (genericArgument != null && string.Equals (genericArgument, dataTypeName))
				{
					typeMatched = typeCandidate;
					break;
				}
			}

			var fi = AccessTools.DeclaredField (typeMatched, "dataInternal");
			var d = (IDictionary)fi.GetValue(null);
			foreach (var k in d.Keys)
			{
				if (ReferenceEquals(d[k], target))
				{
					return (string)k;
				}
			}

			return key;
		}
		*/

		internal static string FindConfigKeyIfEmpty
		(
			object target,
			string dataTypeName,
			string key)
		{
			if (!string.IsNullOrEmpty(key))
			{
				return key;
			}

			var multilinker = typeof(DataContainerSubsystem).Assembly.GetTypes()
				.Where(t => t.Name.StartsWith("DataMultiLinker"))
				.Select(t => t.BaseType)
				.Where(t => t.IsGenericType)
				.Where(t => t.GenericTypeArguments.Any(gt => gt.Name == dataTypeName))
				.SingleOrDefault();
			if (multilinker != null)
			{
				var fi = AccessTools.DeclaredField(multilinker, "dataInternal");
				var d = (IDictionary)fi.GetValue(null);
				foreach (var k in d.Keys)
				{
					if (ReferenceEquals(d[k], target))
					{
						return (string)k;
					}
				}
			}

			return key;
		}

		public static void ProcessFieldEdit (object target, string filename, string fieldPath, string valueRaw, int i, string modID, string dataTypeName)
		{
			var spec = new EditSpec
			{
				modIndex = i,
				modID = modID,
				filename = FindConfigKeyIfEmpty (target, dataTypeName, filename),
				dataTypeName = dataTypeName,
				root = target,
				fieldPath = fieldPath,
				valueRaw = valueRaw,
			};

			Debug.LogFormat
			(
				"Mod {0} ({1}) applying edit to config {2} path {3}",
				spec.modIndex,
				spec.modID,
				spec.filename,
				spec.fieldPath
			);

			try
			{
				ProcessFieldEdit (spec);
			}
			catch (Exception e)
			{
				ReportWarning(
					spec,
					"failed to edit",
					$"Encountered an exception: {e}");
			}
		}

		internal static void ProcessFieldEdit(EditSpec spec)
		{
			if (string.IsNullOrEmpty(spec.fieldPath) || string.IsNullOrEmpty(spec.valueRaw))
			{
				ReportWarning(
					spec,
					"failed to edit",
					$"Missing field path ({spec.fieldPath}) or raw value ({spec.valueRaw})");
				return;
			}

			var (eop, valueRaw) = ParseOperation(spec.valueRaw);
			spec.state.op = eop;
			spec.valueRaw = valueRaw;

			spec.state.target = spec.root;
			spec.state.parent = spec.root;
			spec.state.targetIndex = -1;
			spec.state.targetKey = null;
			spec.state.fieldInfo = null;
			spec.state.targetType = null;

			if (!WalkFieldPath(spec))
			{
				return;
			}

			var (ok, update) = ValidateEditState(spec);
			if (!ok)
			{
				return;
			}

			if (spec.state.op == EditOperation.NullValue)
			{
				if (spec.state.targetType.IsValueType)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Value type {spec.state.targetType.Name} cannot be set to null");
					return;
				}

				spec.state.fieldInfo.SetValue(spec.state.parent, null);
				Report(
					spec,
					"edits",
					$"Assigning null to target field");
				return;
			}

			if (updaterMap.TryGetValue(spec.state.targetType, out var updater))
			{
				updater(spec, update);
				return;
			}

			if (spec.state.op != EditOperation.DefaultValue)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Value type {spec.state.targetType.Name} has no string parsing implementation - try using {Constants.Operator.DefaultValue} keyword if you're after filling it with default instance");
				return;
			}

			var instanceType = spec.state.targetType;
			var isTag = valueRaw.StartsWith ("!");
			if (isTag)
			{
				var tagMappings = UtilitiesYAML.GetTagMappings ();
				if (tagMappings == null || !tagMappings.TryGetValue(valueRaw, out instanceType))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"There is no type associated with tag {valueRaw}");
					return;
				}

				if (!spec.state.targetType.IsAssignableFrom (instanceType))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Tag type {instanceType.GetNiceTypeName ()} is not compatible with field type {spec.state.targetType.GetNiceTypeName ()}");
					return;
				}
			}

			if (spec.state.targetIndex != -1)
			{
				var list = (IList)spec.state.parent;
				list[spec.state.targetIndex] = Activator.CreateInstance(instanceType);
				Report(
					spec,
					"edits",
					$"Assigning new default object of type {instanceType.Name} to target index {spec.state.targetIndex}");
				return;
			}

			if (spec.state.targetKey != null)
			{
				var map = (IDictionary)spec.state.parent;
				map[spec.state.targetKey] = Activator.CreateInstance(instanceType);
				Report(
					spec,
					"edits",
					$"Assigning new default object of type {instanceType.Name} to target key {spec.state.targetKey}");
				return;
			}

			if (spec.state.fieldInfo == null)
			{
				var parentType = spec.state.parent?.GetType().Name ?? "null";
				var targetType = spec.state.target?.GetType().Name ?? "null";
				ReportWarning(
					spec,
					"attempts to edit",
					"no target field info -- WalkFieldPath() failed to terminate properly"
						+ $" | segment: {spec.state.pathSegment}"
						+ $" | segmentIndex: {spec.state.pathSegmentIndex}"
						+ $" | segmentCount: {spec.state.pathSegmentCount}"
						+ $" | atEnd: {spec.state.atEndOfPath}"
						+ $" | op: {spec.state.op}"
						+ $" | parent: {parentType}"
						+ $" | target: {targetType}"
						+ $" | targetType: {spec.state.targetType}"
						+ $" | targetIndex: {spec.state.targetIndex}"
						+ $" | targetKey: {spec.state.targetKey}");
				return;
			}

			var instance = Activator.CreateInstance(instanceType);
			spec.state.fieldInfo.SetValue(spec.state.parent, instance);
			Report(
				spec,
				"edits",
				$"Assigning new default object of type {instanceType.Name} to target field");
		}

		public static (EditOperation, string) ParseOperation(string valueRaw)
		{
			foreach (var kvp in operationMap)
			{
				var opr = kvp.Key;
				if (valueRaw.EndsWith(opr))
				{
					return (kvp.Value, valueRaw.Replace(opr, "").TrimEnd(' '));
				}
			}
			return (EditOperation.Override, valueRaw);
		}


		private const string editShorthandLastTarget = "^";

		public static EditSpec shortcutEditSpec = new EditSpec ();
		public static EditState shortcutEditState = new EditState ();

		public class EditStateCached
		{
			[TextArea]
			public string context;
			public EditState state;
		}

		private static bool WalkFieldPath(EditSpec spec)
		{
			if (shortcutEditSpec == null)
				shortcutEditSpec = new EditSpec ();

			if (shortcutEditState == null)
				shortcutEditState = new EditState ();

			var pathSegments = spec.fieldPath.Split('.');
			int pathSegmentCount = pathSegments.Length;
			spec.state.pathSegmentCount = pathSegmentCount;

			for (int i = 0; i < pathSegmentCount; ++i)
			{
				spec.state.pathSegmentIndex = i;
				spec.state.pathSegment = pathSegments[i];

				bool endOfPath = i >= (pathSegmentCount - 1);
				spec.state.atEndOfPath = endOfPath;

				/*
				if (spec.state.pathSegmentCount != pathSegmentCount)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"I{i} | Segment count in spec doesn't match loop count: {spec.state.pathSegmentCount} / {pathSegmentCount}");
				}
				*/

				if (spec.state.target == null)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"I{i} | Can't proceed past {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}), current target reference is null");
					return false;
				}

				// Grab last target
				if (spec.state.pathSegment == editShorthandLastTarget)
				{
					if (logReferenceSetup)
					{
						var s = shortcutEditState;
						Report (
							spec,
							"attempts to edit",
							$"I{i} | Using last edit state {s.pathSegment} (S{s.pathSegmentIndex}/{s.pathSegmentCount}, {(s.atEndOfPath ? "end" : "mid")}), {s.op})\n- Target: ({s.target}) (I{s.targetIndex}, {s.targetKey}, {s.targetType?.Name})\n- Field: {s.fieldInfo?.Name}\n- Parent: {s.parent} ({s.parent?.GetType ()?.Name})");
					}

					spec.state.CopyFrom (shortcutEditState);
					continue;
				}

				spec.state.targetType = spec.state.target.GetType();
				var child = i > 0;
				bool recordLastState = !endOfPath;

				if (child && typeIList.IsAssignableFrom(spec.state.targetType))
				{
					if (!ProduceListElement(spec))
						return false;
				}
				else if (child && typeIDictionary.IsAssignableFrom(spec.state.targetType))
				{
					if (!ProduceMapEntry(spec))
						return false;
				}
				else if (!ProduceField(spec))
				{
					return false;
				}

				// Save the edit state targeting second-to-last object for later use in shortcuts
				if (!endOfPath)
				{
					if (logReferenceSetup)
					{
						var s = spec.state;
						Report (
							spec,
							"attempts to edit",
							$"Setting last edit target to field {s.pathSegment} (S{s.pathSegmentIndex}/{s.pathSegmentCount}, {(s.atEndOfPath ? "end" : "mid")}), {s.op})\n- Target: ({s.target}) (I{s.targetIndex}, {s.targetKey}, {s.targetType?.Name})\n- Field: {s.fieldInfo?.Name}\n- Parent: {s.parent} ({s.parent?.GetType ()?.Name})");
					}

					shortcutEditState.CopyFrom (spec.state);
				}
			}

			return true;
		}

		private static bool ProduceListElement(EditSpec spec, bool editPermitted = true)
		{
			var list = spec.state.target as IList;
			if (!int.TryParse(spec.state.pathSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result < 0)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Index {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}) can't be parsed or is negative");
				return false;
			}

			var elementType = list.GetType().GetGenericArguments()[0];
			if (spec.state.atEndOfPath && editPermitted)
			{
				if (!EditList(spec, list, result, elementType))
				{
					return false;
				}
			}
			else if (result >= list.Count)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Can't proceed past {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}), current target reference is beyond end of list (size={list.Count})");
				return false;
			}

			spec.state.parent = spec.state.target;
			spec.state.fieldInfo = null;
			spec.state.targetIndex = result;
			spec.state.targetKey = null;
			spec.state.target = list[result];
			spec.state.targetType = elementType;

			return true;
		}

		private static bool EditList(
			EditSpec spec,
			IList list,
			int index,
			Type elementType)
		{
			var outOfBounds = index >= list.Count;

			if (spec.state.op == EditOperation.Add)
			{
				var instance = DefaultValue(elementType);
				if (outOfBounds)
				{
					list.Add(instance);
					Report(
						spec,
						"edits",
						$"Adding new entry of type {elementType.Name} to end of the list (step {spec.state.pathSegmentIndex}) | New list size: {list.Count}");
				}
				else
				{
					list.Insert(index, instance);
					Report(
						spec,
						"edits",
						$"Inserting new entry of type {elementType.Name} to index {index} of the list (step {spec.state.pathSegmentIndex}) | New list size: {list.Count}");
				}

				var nonEmptyValue = !string.IsNullOrWhiteSpace(spec.valueRaw);
				var isTag = nonEmptyValue && elementType != typeString && spec.valueRaw.StartsWith("!");
				if (isTag)
				{
					spec.state.op = EditOperation.DefaultValue;
				}

				// Just in case, prepare another spec to avoid modifying main spec in a way not expected by the rest of the system
				shortcutEditSpec.CopyFrom (spec);

				// The spec is at collection level right now - to be useful for shortcuts, we need to reattempt this call
				// We block repeated edits just in case, to avoid an infinite loop
				ProduceListElement (shortcutEditSpec, false);

				// Fill the reference spec used with a ^ shortcut
				shortcutEditState.CopyFrom (shortcutEditSpec.state);

				if (logReferenceSetup)
				{
					var s = shortcutEditState;
					Report (
						spec,
						"attempts to edit",
						$"Setting last edit target to list entry {s.pathSegment} (S{s.pathSegmentIndex}/{s.pathSegmentCount}, {(s.atEndOfPath ? "end" : "mid")}), {s.op})\n- Target: ({s.target}) (I{s.targetIndex}, {s.targetKey}, {s.targetType?.Name})\n- Field: {s.fieldInfo?.Name}\n- Parent: {s.parent} ({s.parent?.GetType ()?.Name})");
				}

				return nonEmptyValue;
			}

			if (spec.state.op == EditOperation.Remove)
			{
				if (outOfBounds)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Index {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}) can't be removed as it's out of bounds for list size {list.Count}");
					return false;
				}

				list.RemoveAt(index);
				Report(
					spec,
					"edits",
					$"Removing entry at index {index} of the list (step {spec.state.pathSegmentIndex})");
				return false;
			}

			return true;
		}

		private static bool ProduceMapEntry(EditSpec spec, bool editPermitted = true)
		{
			var map = spec.state.target as IDictionary;
			if (map == null)
			{
				Report(
					spec,
					"attempts to edit",
					$"Unable to edit map - casting to IDictionary returns null");
				return false;
			}

			// var key = spec.state.pathSegment;
			// var entryExists = map.Contains(key);

			var entryTypes = map.GetType().GetGenericArguments();
			var keyType = entryTypes[0];
			var valueType = entryTypes[1];

			if (!allowedKeyTypes.Contains(keyType))
			{
				var permittedTypes = string.Join(", ", allowedKeyTypes.Select(t => t.Name));
				Report(
					spec,
					"attempts to edit",
					$"Unable to produce map entry (step {spec.state.pathSegmentIndex}) - only keys of types [{permittedTypes}] are supported");
				return false;
			}

			var key = spec.state.pathSegment;
			var (keyOK, resolvedKey) = ResolveTargetKey(map.GetType(), key);
			if (!keyOK)
			{
				Report(
					spec,
					"attempts to edit",
					$"Checking map for key {key} (step {spec.state.pathSegmentIndex}) - unable to cast key to the correct type");
				return false;
			}

			var entryExists = map.Contains(resolvedKey);





			if (spec.state.atEndOfPath && editPermitted)
			{
				if (!EditMap(
					spec,
					map,
					valueType,
					resolvedKey,
					entryExists))
				{
					return false;
				}
			}
			else if (!entryExists)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Can't proceed past {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}), current target reference doesn't exist in dictionary. Keys: {map.ToStringFormattedKeysLegacy ()}");
				return false;
			}

			spec.state.parent = spec.state.target;
			spec.state.fieldInfo = null;
			spec.state.targetIndex = -1;
			spec.state.targetKey = resolvedKey;
			spec.state.target = map[key];
			spec.state.targetType = valueType;

			return true;
		}

		private static bool EditMap(
			EditSpec spec,
			IDictionary map,
			Type valueType,
			object key,
			bool entryExists)
		{
			if (spec.state.op == EditOperation.Add)
			{
				if (!entryExists)
				{
					object instance = DefaultValue(valueType);
					map.Add(key, instance);
					Report(
						spec,
						"edits",
						$"Adding key {key} (step {spec.state.pathSegmentIndex}) to target dictionary | Value: {spec.valueRaw} | Non empty: {!string.IsNullOrWhiteSpace(spec.valueRaw)}");

					// Just in case, prepare another spec to avoid modifying main spec in a way not expected by the rest of the system
					shortcutEditSpec.CopyFrom (spec);

					// The spec is at collection level right now - to be useful for shortcuts, we need to reattempt this call
					// We block repeated edits just in case, to avoid an infinite loop
					ProduceMapEntry (shortcutEditSpec, false);

					// Fill the reference spec used with a ^ shortcut
					shortcutEditState.CopyFrom (shortcutEditSpec.state);

					if (logReferenceSetup)
					{
						var s = shortcutEditState;
						Report (
							spec,
							"attempts to edit",
							$"Setting last edit target to map entry {s.pathSegment} (S{s.pathSegmentIndex}/{s.pathSegmentCount}, {(s.atEndOfPath ? "end" : "mid")}), {s.op})\n- Target: ({s.target}) (I{s.targetIndex}, {s.targetKey}, {s.targetType?.Name})\n- Field: {s.fieldInfo?.Name}\n- Parent: {s.parent} ({s.parent?.GetType ()?.Name})");
					}
				}
				else
				{
					Report(
						spec,
						"attempts to edit",
						$"Key {key} already exists, ignoring the command to add it");
				}

				var nonEmptyValue = !string.IsNullOrWhiteSpace(spec.valueRaw);
				var isTag = nonEmptyValue && valueType != typeString && spec.valueRaw.StartsWith("!");
				if (isTag)
				{
					spec.state.op = EditOperation.DefaultValue;
				}

				return nonEmptyValue;
			}

			if (spec.state.op == EditOperation.Remove)
			{
				if (!entryExists)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Key {key} (step {spec.state.pathSegmentIndex}) can't be removed from target dictionary - it can't be found");
					return false;
				}

				Report(
					spec,
					"edits",
					$"Removing key {key} (step {spec.state.pathSegmentIndex}) from target dictionary");
				map.Remove(key);
				return false;
			}

			return true;
		}

		private static bool ProduceField(EditSpec spec)
		{
			var field = spec.state.targetType.GetField(spec.state.pathSegment);
			if (field == null)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Field {spec.state.pathSegment} (step {spec.state.pathSegmentIndex}) could not be found on type {spec.state.targetType}");
				return false;
			}

			spec.state.parent = spec.state.target;
			spec.state.fieldInfo = field;
			spec.state.targetIndex = -1;
			spec.state.targetKey = null;
			spec.state.target = field.GetValue(spec.state.target);
			spec.state.targetType = field.FieldType;

			return true;
		}

		private static (bool, Action<object>) ValidateEditState(EditSpec spec)
		{
			if (spec == null || spec.state == null || spec.state.parent == null)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Current spec doesn't allow validation of edit state due missing spec data | Spec: {spec.ToStringNullCheck ()} | State: {spec?.state.ToStringNullCheck ()} | Parent: {spec?.state?.parent.ToStringNullCheck ()}");
				return (false, null);
			}

			var parentType = spec.state.parent.GetType();
			var parentIsList = typeIList.IsAssignableFrom(parentType);
			if (parentIsList)
			{
				if (spec.state.targetIndex == -1)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Value is contained in a list but list index {spec.state.pathSegment} is not valid");
					return (false, null);
				}

				var parentList = (IList)spec.state.parent;
				var targetIndex = spec.state.targetIndex;
				return (true, v => parentList[targetIndex] = v);
			}

			var parentIsMap = typeIDictionary.IsAssignableFrom(parentType);
			if (parentIsMap)
			{
				if (spec.state.targetKey == null)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Value is contained in a dictionary but the key {spec.state.pathSegment} is not valid");
					return (false, null);
				}

				var parentMap = (IDictionary)spec.state.parent;
				var targetKey = spec.state.targetKey;
				return (true, v => parentMap[targetKey] = v);
			}

			if (spec.state.fieldInfo == null)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Value can't be modified due to missing field info");
				return (false, null);
			}

			var fieldIsEnum = typeEnum.IsAssignableFrom(spec.state.targetType);
			if (fieldIsEnum)
			{
				spec.state.targetType = typeEnum;
			}

			var parent = spec.state.parent;
			var fieldInfo = spec.state.fieldInfo;
			return (true, v => fieldInfo.SetValue(parent, v));
		}

		private static (bool, object) ResolveTargetKey(Type parentType, string targetKey)
		{
			if (!parentType.IsGenericType)
			{
				return (false, targetKey);
			}
			if (!parentType.IsConstructedGenericType)
			{
				return (false, targetKey);
			}
			var typeArgs = parentType.GetGenericArguments();
			if (typeArgs.Length != 2)
			{
				return (false, targetKey);
			}
			var keyType = typeArgs[0];
			if (keyType == typeof(string))
			{
				return (true, targetKey);
			}
			if (keyType == typeof(int))
			{
				if (!int.TryParse(targetKey, out var intKey))
				{
					return (false, targetKey);
				}
				return (true, intKey);
			}
			return (false, targetKey);
		}

		private static void UpdateStringField(EditSpec spec, Action<object> update)
		{
			var v = spec.state.op != EditOperation.DefaultValue ? spec.valueRaw : null;
			update(v);
			Report(
				spec,
				"edits",
				$"String field modified with value {v}");
		}

		private static void UpdateBoolField(EditSpec spec, Action<object> update)
		{
			var v = spec.state.op != EditOperation.DefaultValue
				&& string.Equals(spec.valueRaw, "true", StringComparison.OrdinalIgnoreCase);
			update(v);
			Report(
				spec,
				"edits",
				$"Bool field modified with value {v}");
		}

		private static void UpdateIntField(EditSpec spec, Action<object> update)
		{
			var v = 0;
			if (spec.state.op != EditOperation.DefaultValue)
			{
				if (!int.TryParse(spec.valueRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Integer field can't be overwritten - can't parse raw value {spec.valueRaw}");
					return;
				}
			}

			update(v);
			Report(
				spec,
				"edits",
				$"Integer field modified with value {v}");
		}

		private static void UpdateFloatField(EditSpec spec, Action<object> update)
		{
			var v = 0.0f;
			if (spec.state.op != EditOperation.DefaultValue)
			{
				if (!float.TryParse(spec.valueRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Float field can't be overwritten - can't parse raw value {spec.valueRaw}");
					return;
				}
			}

			update(v);
			Report(
				spec,
				"edits",
				$"Float field modified with value {v}");
		}

		private static void UpdateVector2Field(EditSpec spec, Action<object> update)
		{
			UpdateVectorField(
				spec,
				update,
				2,
				ary => new Vector2(ary[0], ary[1]),
				Vector2.zero);
		}

		private static void UpdateVector3Field(EditSpec spec, Action<object> update)
		{
			UpdateVectorField(
				spec,
				update,
				3,
				ary => new Vector3(ary[0], ary[1], ary[2]),
				Vector3.zero);
		}

		private static void UpdateVector4Field(EditSpec spec, Action<object> update)
		{
			UpdateVectorField(
				spec,
				update,
				4,
				ary => new Vector4(ary[0], ary[1], ary[2], ary[3]),
				Vector4.zero);
		}

		private static void UpdateVectorField(
			EditSpec spec,
			Action<object> update,
			int vectorLength,
			Func<float[], object> ctor,
			object zero)
		{
			var v = zero;
			if (spec.state.op != EditOperation.DefaultValue)
			{
				var (ok, parsed) = ParseVectorValue(spec, vectorLength, ctor);
				if (!ok)
				{
					return;
				}
				v = parsed;
			}

			update(v);
			Report(
				spec,
				"edits",
				$"Vector{vectorLength} field modified with value {v}");
		}

		private static (bool, object) ParseVectorValue(
			EditSpec spec,
			int vectorLength,
			Func<float[], object> ctor)
		{
			if (!spec.valueRaw.StartsWith("(") || !spec.valueRaw.EndsWith(")"))
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Vector{vectorLength} field can't be overwritten - can't parse raw value {spec.valueRaw} - missing parentheses");
				return (false, null);
			}

			var valueRaw = spec.valueRaw.Substring(1, spec.valueRaw.Length - 2);
			var velems = valueRaw.Split(',');
			if (velems.Length != vectorLength)
			{
				ReportWarning(
					spec,
					"attempts to edit",
					$"Vector{vectorLength} field can't be overwritten - can't parse raw value {spec.valueRaw} - invalid number of elements");
				return (false, null);
			}

			var parsed = new float[velems.Length];
			for (var i = 0; i < velems.Length; i += 1)
			{
				if (!float.TryParse(velems[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Vector{vectorLength} field can't be overwritten - can't parse raw value {spec.valueRaw}");
					return (false, null);
				}
				parsed[i] = result;
			}

			return (true, ctor(parsed));
		}

		private static void UpdateColorField(EditSpec spec, Action<object> update)
		{
			object v = Color.black;
			if (spec.state.op != EditOperation.DefaultValue)
			{
				if (!spec.valueRaw.StartsWith("(") || !spec.valueRaw.EndsWith(")"))
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Color field can't be overwritten - can't parse raw value {spec.valueRaw} - missing parentheses");
					return;
				}

				try
				{
					Report(
						spec,
						"attempts to change color",
						$"Color field parsing attempt with input {spec.valueRaw}");

					var valueRaw = spec.valueRaw.Substring(1, spec.valueRaw.Length - 2);
					v = ColorParser.Parse (valueRaw);
				}
				catch (Exception e)
				{
					ReportWarning(
						spec,
						"attempts to change color",
						e.Message);
					return;
				}
			}

			update(v);
			Report(
				spec,
				"edits",
				$"Color field modified with value {v}");
		}

		private static void UpdateHashSet(EditSpec spec, Action<object> _)
		{
			if (!allowedHashSetOperations.Contains(spec.state.op))
			{
				ReportWarning(
					spec,
					"attempts to edit",
					"No addition or removal keywords detected - no other operations are supported on hashsets");
				return;
			}

			if (spec.state.op == EditOperation.DefaultValue)
			{
				if (spec.state.target != null)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						"Hashset exists -- cannot replace with default value");
					return;
				}

				spec.state.fieldInfo.SetValue(spec.state.parent, new HashSet<string>());
				Report(
					spec,
					"edits",
					$"Assigning new hashset to target field");
				return;
			}

			var stringSet = spec.state.target as HashSet<string>;
			var found = stringSet.Contains(spec.valueRaw);

			switch (spec.state.op)
			{
				case EditOperation.Add:
					if (found)
					{
						Report(
							spec,
							"attempts to edit",
							$"Value {spec.valueRaw} already exists in target set, ignoring addition command prompted by {Constants.Operator.Insert} keyword");
						return;
					}
					stringSet.Add(spec.valueRaw);
					Report(
						spec,
						"edits",
						$"Value {spec.valueRaw} is added to target set due to {Constants.Operator.Insert} keyword");
					break;
				case EditOperation.Remove:
					if (!found)
					{
						Report(
							spec,
							"attempts to edit",
							$"Value {spec.valueRaw} doesn't exist in target set, ignoring removal command prompted by {Constants.Operator.Remove} keyword");
						return;
					}
					stringSet.Remove(spec.valueRaw);
					Report(
						spec,
						"edits",
						$"Value {spec.valueRaw} is removed from target set due to {Constants.Operator.Remove} keyword");
					break;
			}
		}

		private static void UpdateEnum(EditSpec spec, Action<object> update)
		{
			var targetType = spec.state.fieldInfo.FieldType;
			var values = Enum.GetValues(targetType);
			// This makes the assumption that the bottom value of the enum also has the lowest
			// unsigned integer value.
			var v = values.GetValue(0);

			if (spec.state.op != EditOperation.DefaultValue)
			{
				var names = Enum.GetNames(targetType);
				var idx = Array.FindIndex(names, name => string.CompareOrdinal(name, spec.valueRaw) == 0);
				if (idx == -1)
				{
					ReportWarning(
						spec,
						"attempts to edit",
						$"Enum field can't be overwritten - can't parse raw value | type: {targetType.Name} | value: {spec.valueRaw}");
					return;
				}
				v = values.GetValue(idx);
			}

			update(v);
			Report(
				spec,
				"edits",
				$"Enum field modified with value {v}");
		}

		private static object DefaultValue(Type elementType)
		{
			if (defaultValueMap.TryGetValue(elementType, out var value))
			{
				return value;
			}
			if (elementType.IsInterface)
			{
				return null;
			}
			if (elementType.IsAbstract)
			{
				return null;
			}
			return Activator.CreateInstance(elementType);
		}

		private static void Report(EditSpec spec, string verb, string msg)
		{
			Debug.LogFormat
			(
				"Mod {0} ({1}) {2} config {3} of type {4}, field {5} | {6}\nState:\n{7}",
				spec.modIndex,
				spec.modID,
				verb,
				spec.filename,
				spec.dataTypeName,
				spec.fieldPath,
				msg,
				spec.state.ToString ()
			);
		}

		private static void ReportWarning(EditSpec spec, string verb, string msg)
		{
			Debug.LogWarningFormat
			(
				"Mod {0} ({1}) {2} config {3} of type {4}, field {5} | {6}\nState:\n{7}",
				spec.modIndex,
				spec.modID,
				verb,
				spec.filename,
				spec.dataTypeName,
				spec.fieldPath,
				msg,
				spec.state
			);
		}
    }
}
