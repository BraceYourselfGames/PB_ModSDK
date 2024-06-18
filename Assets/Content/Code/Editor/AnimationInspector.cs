using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BYG {

	public class AnimationInspectorSettings : ScriptableObject {
		public string LastSaveDir = "";
	}

	public class AnimationInspector : ScriptableObject {
		[SerializeField]
		private Animator Animator;

		public string TargetObjectName = "";

		public List<string> SelectedParameters = new List<string>();
		public float WeightFilterAmount = 0.0f;
		public float TransitionDisplayDuration = 5.0f;
		public bool ShowSubState = true;
		public bool ShowMotionFieldType = true;
		public bool ShowMotionFieldName = true;
		public bool ShowLayerWeight = true;
		public bool ShowBlendTreeInfo = true;
		public List<bool> SelectedLayers = new List<bool>();

		// Start is called before the first frame update
		public AnimationInspector() {
		}

		public AnimatorState FindState(in AnimatorStateInfo info, AnimatorStateMachine machine, out List<AnimatorStateMachine> parentMachines) {
			parentMachines = new List<AnimatorStateMachine>();
			var result = FindStateInternal(info, machine, ref parentMachines);
			parentMachines.Reverse();
			return result;
		}

		private AnimatorState FindStateInternal(in AnimatorStateInfo info, AnimatorStateMachine machine, ref List<AnimatorStateMachine> parentMachines) {
			foreach (var childState in machine.states) {
				var state = childState.state;
				if (state.nameHash == info.shortNameHash && info.IsName(state.name)) {
					parentMachines.Add(machine);
					return state;
				}
			}

			foreach (var childMachine in machine.stateMachines) {
				var subMachine = childMachine.stateMachine;
				var result = FindStateInternal(info, subMachine, ref parentMachines);
				if (result != null) {
					parentMachines.Add(machine);
					return result;
				}
			}

			return null;
		}

		public AnimatorStateTransition FindTransition(in AnimatorTransitionInfo info, AnimatorState currentState) {
			var transitions = currentState.transitions;

			foreach (var transition in transitions) {
				var s = transition.GetDisplayName(transition);
				if (info.IsName(transition.name)) {
					return transition;
				}
			}

			return null;
		}

		public Animator GetAnimator() {
			return Animator;
		}

		public static string GetHierarchialName(GameObject obj) {
			var parentName = "/";
			if (obj.transform.parent != null) {
				parentName = GetHierarchialName(obj.transform.parent.gameObject) + "/";
			}
			return parentName + obj.gameObject.name;
		}

		public void SetAnimator(Animator animator) {
			CacheAnimatorName();
			Animator = animator;
		}

		public void CacheAnimatorName() {
			if (Animator != null) {
				TargetObjectName = GetHierarchialName(Animator.gameObject);
			} else {
				TargetObjectName = "";
			}
		}

		public AnimatorController GetController() {
			if (Animator == null) {
				return null;
			}
			return Animator.runtimeAnimatorController as AnimatorController;
		}

		public void CloneInto(AnimationInspector target) {
			var serializeFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
			foreach (var field in serializeFields) {
				var value = field.GetValue(this);

				if (field.FieldType.IsSerializable) {
					using (var ms = new MemoryStream()) {
						var formatter = new BinaryFormatter();
						formatter.Serialize(ms, value);
						ms.Position = 0;
						value = formatter.Deserialize(ms);
					}
				}

				field.SetValue(target, value);
			}
			CacheAnimatorName();
		}
	}

#if UNITY_EDITOR

	public class PreviousAnimationTransition {
		public AnimatorState PreviousState = null;
		public Dictionary<string, string> RelevantValues = new Dictionary<string, string>();
		// TODO: need to pair the transitions with the relevant states/state machines that we arrived at on each leg of the journey (including if this was a state machine pop)
		public List<AnimatorTransitionBase> Transitions = new List<AnimatorTransitionBase>();
		// The original state stack
		public List<AnimatorStateMachine> StateStack = new List<AnimatorStateMachine>();
		public float TransitionTimestamp = 0f;
		public float StateDuration = 0f;
	}

	public class TransientAnimationState {
		public AnimatorState CurrentState;

		public List<PreviousAnimationTransition> PreviousTransitions = new List<PreviousAnimationTransition>();
	}

	public class AnimationInspectorWindow : EditorWindow {

		private const string InspectorSettingsPath = "Assets/_AnimationInspectorWindowSettings.asset";

		private readonly string[] TabNames = { "Info", "Parameters", };

		public AnimationInspector InspectorData;

		public AnimationInspectorSettings Settings;

		private Vector2 LastScrollPosition;
		private Vector2 LastParameterScrollPosition;

		private int currentTab = 0;

		public bool AutoSelection = false;

		private Animator cachedAnimator = null;
		private Dictionary<string, int> animatorParameterIds = new Dictionary<string, int>();
		private List<AnimatorControllerParameter> animatorParameters = new List<AnimatorControllerParameter>();
		private string parameterFilterText = "";

		private List<TransientAnimationState> LayerTransitionStates = new List<TransientAnimationState>();

		public AnimationInspectorSettings GetDefaultSettings() {
			var settings = AssetDatabase.LoadAssetAtPath<AnimationInspectorSettings>(InspectorSettingsPath);

			if (settings == null) {
				settings = CreateInstance<AnimationInspectorSettings>();

				settings.LastSaveDir = Application.dataPath;

				AssetDatabase.CreateAsset(settings, InspectorSettingsPath);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				settings = AssetDatabase.LoadAssetAtPath<AnimationInspectorSettings>(InspectorSettingsPath);
			}

			return settings;
		}

		public void SaveDefaultSettings() {
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		[MenuItem("Window/Animation Inspector")]
		public static void Init() {
			// Get existing open window or if none, make a new one:
			AnimationInspectorWindow window = GetWindow<AnimationInspectorWindow>("Animation Inspector");
			window.minSize = new Vector2(100f, 100f);
			window.maxSize = new Vector2(4000f, 4000f);
			var currentSelection = Selection.activeGameObject;
			Animator currentAnimator = null;
			if (currentSelection != null) {
				currentAnimator = currentSelection.GetComponentInChildren<Animator>();
			}
			window.InspectorData = CreateInstance<AnimationInspector>();
			window.InspectorData.SetAnimator(currentAnimator);
			window.Show();
		}

		protected void Update() {
			if (InspectorData != null && InspectorData.GetAnimator() != null) {
				Repaint();
			}
		}

		protected void OnSelectionChange() {
			Repaint();
		}

		private void CheckAnimatorChange() {
			var animator = InspectorData.GetAnimator();

			if (animator != cachedAnimator || animator == null || (animator != null && animator.layerCount != LayerTransitionStates.Count)) {
				LayerTransitionStates.Clear();
				if (animator != null) {
					var controller = animator.runtimeAnimatorController as AnimatorController;

					for (int i = 0; i < animator.layerCount; ++i) {
						var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
						var layerInfo = controller.layers[i];
						var foundState = InspectorData.FindState(stateInfo, layerInfo.stateMachine, out var parentMachines);

						LayerTransitionStates.Add(new TransientAnimationState() { CurrentState = foundState, });
					}
				}
			}

			if (animator != cachedAnimator || animator == null || (animator != null && animator.parameterCount != animatorParameterIds.Count)) {
				animatorParameterIds.Clear();
				if (animator != null) {
					var oldParameters = animatorParameters;
					animatorParameters = animator.parameters.ToList();

					for (int i = 0; i < animator.parameterCount; ++i) {
						var param = animator.GetParameter(i);
						animatorParameterIds[param.name] = i;
					}

					InspectorData.SelectedParameters = InspectorData.SelectedParameters.Where(s => animatorParameterIds.ContainsKey(s)).ToList();
				}
			}

			if (animator != cachedAnimator || animator == null || (animator != null && InspectorData.SelectedLayers.Count != animator.layerCount)) {
				InspectorData.SelectedLayers.Clear();
				if (animator != null) {
					for (int i = 0; i < animator.layerCount; ++i) {
						InspectorData.SelectedLayers.Add(true);
					}
				}
			}

			cachedAnimator = animator;
		}

		public AnimatorTransitionBase FindRelevantTransition(PreviousAnimationTransition prev, IEnumerable<AnimatorStateTransition> transitions, float timeElapsed = 0f) {
			return FindRelevantTransition(prev, transitions.Where(t => !t.hasExitTime || timeElapsed >= t.exitTime).Cast<AnimatorTransitionBase>());
		}

		public AnimatorTransitionBase FindRelevantTransition(PreviousAnimationTransition prev, IEnumerable<AnimatorTransitionBase> transitions) {
			foreach (var transition in transitions) {
				if (CheckTransition(transition)) {
					foreach (var condition in transition.conditions) {
						prev.RelevantValues[condition.parameter] = GetParameterString(condition.parameter);
					}
					prev.Transitions.Add(transition);
					return transition;
				}
			}
			return null;
		}

		public bool CheckTransition(AnimatorTransitionBase transition) {
			if (transition.mute) {
				return false;
			}

			bool passed = true;

			foreach (var condition in transition.conditions) {
				var paramType = GetParameterType(condition.parameter);

				if (paramType == AnimatorControllerParameterType.Int) {
					var value = cachedAnimator.GetInteger(condition.parameter);
					var intThreshold = Mathf.RoundToInt(condition.threshold);
					switch (condition.mode) {
						case AnimatorConditionMode.Greater:
							passed &= value > intThreshold;
							break;
						case AnimatorConditionMode.Less:
							passed &= value < intThreshold;
							break;
						case AnimatorConditionMode.Equals:
							passed &= value == intThreshold;
							break;
						case AnimatorConditionMode.NotEqual:
							passed &= value != intThreshold;
							break;
						default:
							passed = false;
							break;
					}
				} else if (paramType == AnimatorControllerParameterType.Float) {
					var value = cachedAnimator.GetFloat(condition.parameter);
					switch (condition.mode) {
						case AnimatorConditionMode.Greater:
							passed &= value > condition.threshold;
							break;
						case AnimatorConditionMode.Less:
							passed &= value < condition.threshold;
							break;
						case AnimatorConditionMode.Equals:
							passed &= value == condition.threshold;
							break;
						case AnimatorConditionMode.NotEqual:
							passed &= value != condition.threshold;
							break;
						default:
							passed = false;
							break;
					}
				} else {
					switch (condition.mode) {
						case AnimatorConditionMode.If:
							passed &= cachedAnimator.GetBool(condition.parameter);
							break;
						case AnimatorConditionMode.IfNot:
							passed &= !cachedAnimator.GetBool(condition.parameter);
							break;
						default:
							passed = false;
							break;
					}
				}

				if (!passed) {
					break;
				}
			}

			return passed;
		}

		public AnimatorControllerParameterType GetParameterType(string parameterName) {
			CheckAnimatorChange();
			int parameterId;
			if (animatorParameterIds.TryGetValue(parameterName, out parameterId)) {
				var param = cachedAnimator.GetParameter(parameterId);
				return param.type;
			}
			return 0;
		}

		public string GetParameterString(string parameterName) {
			var paramType = GetParameterType(parameterName); 
			if (paramType != 0) {
				switch (paramType) {
					case AnimatorControllerParameterType.Bool:
						return cachedAnimator.GetBool(parameterName).ToString();
					case AnimatorControllerParameterType.Int:
						return cachedAnimator.GetInteger(parameterName).ToString();
					case AnimatorControllerParameterType.Float:
						return cachedAnimator.GetFloat(parameterName).ToString();
					case AnimatorControllerParameterType.Trigger:
						return cachedAnimator.GetBool(parameterName).ToString();
				}
			}

			return "None";
		}

		public static string GetComparatorText(AnimatorConditionMode mode) {
			switch (mode) {
				case AnimatorConditionMode.If:
					return "is True";
				case AnimatorConditionMode.IfNot:
					return "is False";
				case AnimatorConditionMode.Equals:
					return "==";
				case AnimatorConditionMode.NotEqual:
					return "!=";
				case AnimatorConditionMode.Less:
					return "<";
				case AnimatorConditionMode.Greater:
					return ">";
				default:
					throw new System.Exception("Unknown Animator Condition Mode");
			}
		}

		private string DumpTransientAnimationState(TransientAnimationState layerTransitionState) {
			string result = "";

			var transitionLines = new List<System.Tuple<string, string>>();
			int maxLineLength = 0;

			for (int transitionGroupIdx = 0; transitionGroupIdx < layerTransitionState.PreviousTransitions.Count; ++transitionGroupIdx) {
				var transitionGroup = layerTransitionState.PreviousTransitions[transitionGroupIdx];
				if (transitionGroupIdx == 0) {
					foreach (var machine in transitionGroup.StateStack.Skip(1)) {
						result += machine.name + " => ";
					}
					result += transitionGroup.PreviousState.name + "\n";
				}

				var currentStateStack = transitionGroup.StateStack.ToList();

				for (int transitionIdx = 0; transitionIdx < transitionGroup.Transitions.Count; ++transitionIdx) {
					var transition = transitionGroup.Transitions[transitionIdx];
					var line = "";

					if (transition.isExit) {
						currentStateStack.RemoveAt(currentStateStack.Count - 1);
						line += "(up) " + currentStateStack.Last().name;
					} else if (transition.destinationStateMachine != null) {
						currentStateStack.Add(transition.destinationStateMachine);
						line += transition.destinationStateMachine.name;
					} else if (transition.destinationState != null) {
						line += transition.destinationState.name;
					} else {
						Debug.Log("Invalid Transition?");
					}

					var values = "";
					var asStateTransition = transition as AnimatorStateTransition;

					if (transitionGroupIdx == 0 && asStateTransition != null && asStateTransition.hasExitTime) {
						values += $"(t={transitionGroup.StateDuration}) >= {asStateTransition.exitTime} ";
					}

					foreach (var condition in transition.conditions) {
						string compText = GetComparatorText(condition.mode);
						if (condition.mode == AnimatorConditionMode.If || condition.mode == AnimatorConditionMode.IfNot) {
							values += $"{condition.parameter} {compText}";
						} else {
							if (transitionGroup.RelevantValues.TryGetValue(condition.parameter, out var parameterValue)) {
								string thresholdText;
								if (GetParameterType(condition.parameter) == AnimatorControllerParameterType.Int) {
									thresholdText = Mathf.RoundToInt(condition.threshold).ToString();
								} else {
									thresholdText = condition.threshold.ToString();
								}

								values += $"({condition.parameter}={parameterValue}) {compText} {thresholdText}";
							}
						}
					}

					transitionLines.Add(new System.Tuple<string, string>(line, values));
					maxLineLength = Mathf.Max(line.Length, maxLineLength);
				}
			}

			foreach (var transitionLine in transitionLines) {
				var transitionStr = transitionLine.Item2.Length == 0 ? "(auto)" : transitionLine.Item2;
				result += "  => " + transitionLine.Item1.PadRight(maxLineLength, ' ') + $" : {transitionStr}\n";
			}

			return result;
		}

		private PreviousAnimationTransition FillOutTransitionState(AnimatorStateInfo stateInfo, AnimatorState currentState, IEnumerable<AnimatorStateMachine> parentMachines) {
			// If we haven't recorded this transition yet...
			var stateStack = parentMachines.ToList();

			// Check for an immediate transition
			PreviousAnimationTransition previousTransitionInformation = new PreviousAnimationTransition();
			previousTransitionInformation.TransitionTimestamp = Time.time;
			previousTransitionInformation.PreviousState = currentState;
			previousTransitionInformation.StateStack = stateStack.ToList();
			var currentStateTime = stateInfo.normalizedTime * stateInfo.length;
			previousTransitionInformation.StateDuration = currentStateTime;

			AnimatorTransitionBase foundTransition = null;
			AnimatorStateMachine poppedStateMachine = null;

			int safety = 1000;

			// We want the state time for the _current_ state, but it will be 0 after any transitions


			while (safety > 0) {
				var currentStateMachine = stateStack.Last();

				if (currentState == null) {
					if (poppedStateMachine != null) {
						foundTransition = FindRelevantTransition(previousTransitionInformation, currentStateMachine.GetStateMachineTransitions(poppedStateMachine));
					} else {
						foundTransition = FindRelevantTransition(previousTransitionInformation, currentStateMachine.entryTransitions);
					}
				} else {
					foundTransition = FindRelevantTransition(previousTransitionInformation, currentState.transitions, currentStateTime);
				}

				if (foundTransition == null) {
					foundTransition = FindRelevantTransition(previousTransitionInformation, currentStateMachine.anyStateTransitions, currentStateTime);
				}

				if (foundTransition != null) {
					if (foundTransition.isExit) {
						poppedStateMachine = currentStateMachine;
						stateStack.RemoveAt(stateStack.Count - 1);
					} else {
						poppedStateMachine = null;
						if (foundTransition.destinationStateMachine != null) {
							stateStack.Add(foundTransition.destinationStateMachine);
						}
					}

					currentState = foundTransition.destinationState;
				}

				if (foundTransition == null) {
					break;
				}

				currentStateTime = 0f;

				--safety;
			}

			return previousTransitionInformation;
		}

		protected void OnGUI() {
			if (InspectorData == null) {
				InspectorData = CreateInstance<AnimationInspector>();
				cachedAnimator = null;
			}

			CheckAnimatorChange();

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Save")) {
				var settings = GetDefaultSettings();
				string file = EditorUtility.SaveFilePanelInProject("Save Settings", "AnimationInspectorData", "asset", "Select a location to save", settings.LastSaveDir);

				if (file != null && file != "") {
					var existingObj = AssetDatabase.LoadAssetAtPath<Object>(file);
					var existingData = existingObj as AnimationInspector;

					if (existingData == null) {
						existingData = CreateInstance<AnimationInspector>();
					}

					if (existingObj != null && existingData != existingObj) {
						AssetDatabase.DeleteAsset(file);
						existingObj = null;
					}

					InspectorData.CloneInto(existingData);

					if (existingObj != existingData) {
						AssetDatabase.CreateAsset(existingData, file);
					}

					settings.LastSaveDir = System.IO.Path.GetDirectoryName(file);

					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}

			if (GUILayout.Button("Load")) {
				var settings = GetDefaultSettings();
				string file = EditorUtility.OpenFilePanel("Load Settings", settings.LastSaveDir, "asset");

				if (file != null && file != "") {
					if (file.StartsWith(Application.dataPath)) {
						file = "Assets/" + file.Substring(Application.dataPath.Length);
					}
					var existingObj = AssetDatabase.LoadAssetAtPath<Object>(file);
					if (existingObj != null) {
						var existingData = existingObj as AnimationInspector;
						if (existingData != null) {
							existingData.CloneInto(InspectorData);
							var obj = GameObject.Find(existingData.TargetObjectName);
							InspectorData.SetAnimator(obj.GetComponent<Animator>());
						}

						settings.LastSaveDir = System.IO.Path.GetDirectoryName(file);
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			SerializedObject serializedObject = new SerializedObject(InspectorData);
			SerializedProperty target = serializedObject.FindProperty("Animator");

			currentTab = GUILayout.Toolbar(currentTab, TabNames);

			var basisHeight = GUILayout.Height(GUI.skin.label.CalcSize(new GUIContent("|")).y);

			if (TabNames[currentTab] == "Info") {
				AutoSelection = EditorGUILayout.Toggle("Use Current Selection", AutoSelection);

				EditorGUI.BeginDisabledGroup(AutoSelection);

				if (AutoSelection) {
					var currentGameObject = Selection.activeGameObject;
					if (currentGameObject != null) {
						var currentAnimator = currentGameObject.GetComponentInChildren<Animator>();
						if (currentAnimator != null) {
							target.objectReferenceValue = currentAnimator;
							serializedObject.ApplyModifiedProperties();
							CheckAnimatorChange();
						}
					}
				}

				EditorGUILayout.PropertyField(target, true);

				EditorGUI.EndDisabledGroup();

				InspectorData.WeightFilterAmount = EditorGUILayout.Slider("Filter Weights Below", InspectorData.WeightFilterAmount, 0.0f, 1.0f);

				InspectorData.TransitionDisplayDuration = Mathf.Max(EditorGUILayout.FloatField("Transition Duration", InspectorData.TransitionDisplayDuration), 0f);

				EditorGUILayout.BeginHorizontal(basisHeight);

				var labelWidth = GUI.skin.label.CalcSize(new GUIContent("Motion Names")).x + 40;

				InspectorData.ShowSubState = EditorGUILayout.ToggleLeft("Sub State", InspectorData.ShowSubState, GUILayout.MaxWidth(labelWidth));
				InspectorData.ShowMotionFieldType = EditorGUILayout.ToggleLeft("Motion Type", InspectorData.ShowMotionFieldType, GUILayout.MaxWidth(labelWidth));
				InspectorData.ShowMotionFieldName = EditorGUILayout.ToggleLeft("Motion Names", InspectorData.ShowMotionFieldName, GUILayout.MaxWidth(labelWidth));
				InspectorData.ShowLayerWeight = EditorGUILayout.ToggleLeft("Weight", InspectorData.ShowLayerWeight, GUILayout.MaxWidth(labelWidth));
				InspectorData.ShowBlendTreeInfo = EditorGUILayout.ToggleLeft("Blend Trees", InspectorData.ShowBlendTreeInfo, GUILayout.MaxWidth(labelWidth));

				EditorGUILayout.EndHorizontal();

				serializedObject.ApplyModifiedProperties();

				InspectorData.CacheAnimatorName();

				var animator = InspectorData.GetAnimator();
				var controller = InspectorData.GetController();

				if (animator != null) {
					LastScrollPosition = EditorGUILayout.BeginScrollView(LastScrollPosition);

					for (int i = 0; i < animator.layerCount; ++i) {
						if (!InspectorData.SelectedLayers[i]) {
							continue;
						}

						var nextStateInfo = animator.GetNextAnimatorStateInfo(i);

						var transitionInfo = animator.GetAnimatorTransitionInfo(i);

						float layerWeight = animator.GetLayerWeight(i);

						// For some reason Unity always reports layer 0's weight as 0.0, when it should be fixed as 1.0. I've reported this as a bug to Unity.
						if (i == 0) {
							layerWeight = 1.0f;
						}

						if (layerWeight < InspectorData.WeightFilterAmount) {
							continue;
						}

						var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
						var layerInfo = controller.layers[i];

						List<AnimatorStateMachine> parentMachines;
						var foundState = InspectorData.FindState(stateInfo, layerInfo.stateMachine, out parentMachines);

						bool isEmptyState = false;

						if (foundState == null) {
							isEmptyState = true;
							for (int j = 0; j < animator.layerCount; ++j) {
								if (j == i) {
									continue;
								}
								foundState = InspectorData.FindState(stateInfo, controller.layers[0].stateMachine, out parentMachines);
								if (foundState != null) {
									break;
								}
							}
						}

						if (foundState != null) {
							if (animator.IsInTransition(i)) {
								var nextState = InspectorData.FindState(nextStateInfo, layerInfo.stateMachine, out var targetParentMachines);
								if (nextState != null && nextState != LayerTransitionStates[i].CurrentState) {
									var previousTransitionInformation = FillOutTransitionState(stateInfo, foundState, parentMachines);

									if (previousTransitionInformation.Transitions.Count == 0) {
										Debug.Log("****ERRROR****: Could not find any transitions, please report this to Stephen!");
									} else if (previousTransitionInformation.Transitions.Last().destinationState != nextState) {
										Debug.Log("****ERRROR****: Endpoint state found does not match the next state given by Unity, please report this to Stephen!");
									}

									LayerTransitionStates[i].PreviousTransitions.Add(previousTransitionInformation);
									LayerTransitionStates[i].CurrentState = nextState;
								}
							}

							// Filter out any older transitions (we might just want only one transition displayed anyways...)
							LayerTransitionStates[i].PreviousTransitions = LayerTransitionStates[i].PreviousTransitions.Where(p => (Time.time - p.TransitionTimestamp) < InspectorData.TransitionDisplayDuration).ToList();

							string result = "";

							result += DumpTransientAnimationState(LayerTransitionStates[i]);

							// TODO: print out the list of transitions that lead to the current state we are in (possibly preceded by the previous state?)

							if (InspectorData.ShowSubState) {
								foreach (var machine in parentMachines.Skip(1)) {
									result += machine.name + " => ";
								}
							}
							result += foundState.name;

							if (foundState.motion is BlendTree) {
								var blendTree = foundState.motion as BlendTree;
								if (InspectorData.ShowMotionFieldType) {
									result += " <blendTree>";
								}
							} else if (foundState.motion is AnimationClip) {
								var clip = foundState.motion as AnimationClip;
								if (InspectorData.ShowMotionFieldType) {
									result += " <animClip>";
								}
							} else if (foundState.motion == null) {
								if (InspectorData.ShowMotionFieldType) {
									result += " <none>";
								}
							}

							if (InspectorData.ShowMotionFieldName) {
								var clips = animator.GetCurrentAnimatorClipInfo(i);
								result += " : {" + string.Join(", ", clips.Where(c => c.weight > 0.0f).Select(c => c.clip.name)) + "}";
							}

							var resultText = $"{i} : {layerInfo.name} : {result}\n";

							if (InspectorData.ShowLayerWeight) {
								resultText += $"Weight : {layerWeight}";
							}

							if (InspectorData.ShowBlendTreeInfo) {
								var blendTree = foundState.motion as BlendTree;
								if (blendTree != null && !isEmptyState) {
									var blendText = $"{blendTree.blendType} : ".PadRight(27, ' ');

									switch (blendTree.blendType) {
										case BlendTreeType.Simple1D:
											blendText += $"{blendTree.blendParameter}: {GetParameterString(blendTree.blendParameter)}";
											break;
										case BlendTreeType.SimpleDirectional2D:
										case BlendTreeType.FreeformDirectional2D:
										case BlendTreeType.FreeformCartesian2D:
											blendText += $" {blendTree.blendParameter}: {GetParameterString(blendTree.blendParameter)}     {blendTree.blendParameterY}: {GetParameterString(blendTree.blendParameterY)}";
											break;
										default:
											blendText += $"I have no idea how to represent {BlendTreeType.Direct} blendTree types, please talk to me if you need this to work.";
											break;
									}

									resultText += "\t\t" + blendText;
								}
							}

							EditorGUILayout.SelectableLabel(resultText, GUILayout.Height(GUI.skin.label.CalcSize(new GUIContent(resultText)).y));
						}
					}

					EditorGUILayout.Space();

					foreach (var paramName in InspectorData.SelectedParameters) {
						int paramId;
						if (animatorParameterIds.TryGetValue(paramName, out paramId)) {
							var param = animatorParameters[paramId];
							EditorGUILayout.SelectableLabel($"{param.name.PadRight(50, ' ')} : {GetParameterString(param.name)}", basisHeight);
						}
					}

					EditorGUILayout.EndScrollView();

					GUILayout.FlexibleSpace();

					EditorGUILayout.BeginHorizontal();

					if (GUILayout.Button("Select All")) {
						for (int i = 0; i < InspectorData.SelectedLayers.Count; ++i) {
							InspectorData.SelectedLayers[i] = true;
						}
					}

					if (GUILayout.Button("Select None")) {
						for (int i = 0; i < InspectorData.SelectedLayers.Count; ++i) {
							InspectorData.SelectedLayers[i] = false;
						}
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();

					float currentXPos = 0f;

					for (int i = 0; i < animator.layerCount; ++i) {
						var layerInfo = controller.layers[i];
						var layerName = "" + i + " : " + layerInfo.name;
						float widgetWidth = GUI.skin.label.CalcSize(new GUIContent(layerName)).x + 30;

						if (currentXPos + widgetWidth > EditorGUIUtility.currentViewWidth) {
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							currentXPos = 0f;
						}

						InspectorData.SelectedLayers[i] = EditorGUILayout.ToggleLeft(layerName, InspectorData.SelectedLayers[i], GUILayout.Width(widgetWidth));
						currentXPos += widgetWidth;
					}

					EditorGUILayout.EndHorizontal();
				}
			} else if (TabNames[currentTab] == "Parameters") {

				parameterFilterText = EditorGUILayout.TextField("Filter", parameterFilterText);

				LastParameterScrollPosition = EditorGUILayout.BeginScrollView(LastParameterScrollPosition);

				var newSelections = new List<string>();

				for (int i = 0; i < animatorParameters.Count; ++i) {
					var param = animatorParameters[i];

					var tokens = parameterFilterText.ToLower().Trim().Split();

					bool found = true;
					foreach (var token in tokens) {
						found &= param.name.ToLower().Contains(token);
					}

					if (found) {
						bool selected = InspectorData.SelectedParameters.Contains(param.name);

						selected = EditorGUILayout.Toggle(param.name, selected, basisHeight);

						if (selected) {
							newSelections.Add(param.name);
						}
					}
				}

				InspectorData.SelectedParameters = newSelections;

				EditorGUILayout.EndScrollView();
			}
		}
	}

#endif  // UNITY_EDITOR

} // namespace BYG