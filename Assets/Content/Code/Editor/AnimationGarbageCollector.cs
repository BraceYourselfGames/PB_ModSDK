using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if UNITY_EDITOR

public class AnimationClipReferences {
	private Dictionary<string, int> clips = new Dictionary<string, int>();

	public void AddClip(string clip) {
		if (!clips.ContainsKey(clip)) {
			clips.Add(clip, 0);
		}
	}

	public void RemoveClip(string clip) {
		if (clips.ContainsKey(clip)) {
			clips.Remove(clip);
		}
	}

	public void AddClipReference(string clip) {
		int value;
		if (clips.TryGetValue(clip, out value)) {
			clips[clip] = value + 1;
		}
	}

	public void DropClipReference(string clip) {
		int value;
		if (clips.TryGetValue(clip, out value)) {
			if (value > 0) {
				clips[clip] = value - 1;
			} else {
				// ERROR?
			}
		}
	}

	public void ClearClipReferences() {
		var allKeys = clips.Keys.ToArray();
		foreach (var key in allKeys) {
			clips[key] = 0;
		}
	}

	public void Clear() {
		clips.Clear();
	}

	public IEnumerable<string> GetUnreferencedClips() {
		foreach (var entry in clips) {
			if (entry.Value == 0) {
				yield return entry.Key;
			}
		}
	}
}

public class AnimationGarbageCollectorWindow : EditorWindow {

	private List<string> clipFolders = new List<string>();
	private AnimationClipReferences clipReferences = new AnimationClipReferences();
	private List<AnimatorController> animatorControllers = new List<AnimatorController>();

	private HashSet<AnimatorController> selectedAnimators = new HashSet<AnimatorController>();
	private HashSet<string> selectedClips = new HashSet<string>();
	private Vector2 scrollPosition = Vector2.zero;

	private bool animatorControllersFoldoutState = true;
	private Vector2 animationControllersScrollPosition = Vector2.zero;
	private bool clipFolderFoldoutState = true;
	private Vector2 clipFolderScrollPosition = Vector2.zero;
	private bool clipsFoldoutState = true;
	private Vector2 clipsScrollPosition = Vector2.zero;

	private void RefreshAll() {
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

		var alreadyContained = new HashSet<AnimatorController>(animatorControllers);
		var oldSelections = selectedAnimators;
		selectedAnimators = new HashSet<AnimatorController>();

		animatorControllers.Clear();
		foreach (var controllerGuid in AssetDatabase.FindAssets("t:AnimatorController")) {
			var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(controllerGuid));
			if (controller != null) {
				animatorControllers.Add(controller);
				if (!alreadyContained.Contains(controller) || oldSelections.Contains(controller)) {
					selectedAnimators.Add(controller);
				}
			}
		}

		RefreshClipFolders();
	}

	private void RefreshClipFolders() {
		var oldSelectedClips = selectedClips;
		selectedClips = new HashSet<string>();

		clipReferences.Clear();
		foreach (var addedFolder in clipFolders) {
			foreach (var clipGuid in AssetDatabase.FindAssets("t:AnimationClip", new string[] { addedFolder })) {
				var clip = AssetDatabase.GUIDToAssetPath(clipGuid);
				if (clip != null) {
					clipReferences.AddClip(clip);
					if (oldSelectedClips.Contains(clip)) {
						selectedClips.Add(clip);
					}
				}
			}
		}
	}

	[MenuItem("Window/Animation Garbage Collector")]
	public static void Init() {
		// Get existing open window or if none, make a new one:
		AnimationGarbageCollectorWindow window = GetWindow<AnimationGarbageCollectorWindow>("Animation Garbage Collector");
		window.minSize = new Vector2(100f, 100f);
		window.maxSize = new Vector2(4000f, 4000f);

		window.RefreshAll();

		window.Show();
	}

	public string GetFolderPickerResult() {
		var folder = EditorUtility.OpenFolderPanel("Select folder with AnimationClips", "Assets", "");
		return folder;
	}

	private static GUILayoutOption MakeTextWidth(string content, int padding = 0) {
		return GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(content)).x + padding);
	}

	private static GUILayoutOption MakeTextHeight(string content, int padding = 0) {
		return GUILayout.Height(GUI.skin.label.CalcSize(new GUIContent(content)).y + padding);
	}

	private static string RemoveAssetPrefix(string folder) {
		if (folder.StartsWith(Application.dataPath)) {
			return folder.Replace(Application.dataPath, "Assets");
		}
		return null;
	}

	// Update is called once per frame
	protected void OnGUI() {
		var basisHeight = MakeTextHeight("|");

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		bool animatorSetChanged = false;
		bool clipFoldersChanged = false;

		if (GUILayout.Button("Refresh All")) {
			RefreshAll();
			animatorSetChanged = true;
			clipFoldersChanged = true;
		}

		animatorControllersFoldoutState = EditorGUILayout.Foldout(animatorControllersFoldoutState, "Animator Controllers");

		if (animatorControllersFoldoutState) {
			//animationControllersScrollPosition = EditorGUILayout.BeginScrollView(animationControllersScrollPosition, GUILayout.Height(Mathf.Max(400f, position.height / 3.0f)));

			if (animatorControllers.Count > 0) {
				EditorGUILayout.BeginHorizontal();
				var selectAllText = "Select All";
				if (GUILayout.Button(selectAllText, basisHeight, MakeTextWidth(selectAllText, 10))) {
					foreach (var controller in animatorControllers) {
						selectedAnimators.Add(controller);
					}
					animatorSetChanged = true;
				}
				var selectNoneText = "Select None";
				if (GUILayout.Button(selectNoneText, basisHeight, MakeTextWidth(selectNoneText, 10))) {
					selectedAnimators.Clear();
					animatorSetChanged = true;
				}
				EditorGUILayout.EndHorizontal();

				foreach (var controller in animatorControllers) {
					var controllerPath = AssetDatabase.GetAssetPath(controller);
					bool selected = selectedAnimators.Contains(controller);
					EditorGUILayout.BeginHorizontal();

					bool nowSelected = EditorGUILayout.Toggle(selected, GUILayout.Width(20), basisHeight);
					if (selected != nowSelected) {
						animatorSetChanged = true;
						if (nowSelected) {
							selectedAnimators.Add(controller);
						} else {
							selectedAnimators.Remove(controller);
						}
					}

					EditorGUILayout.SelectableLabel(controllerPath, basisHeight);
					EditorGUILayout.EndHorizontal();
				}
			} else {
				EditorGUILayout.LabelField("No AnimatorControllor assets found in project.");
			}

			//EditorGUILayout.EndScrollView();
		}

		clipFolderFoldoutState = EditorGUILayout.Foldout(clipFolderFoldoutState, "Clip Folders");

		if (clipFolderFoldoutState) {
			//clipFolderScrollPosition = EditorGUILayout.BeginScrollView(clipFolderScrollPosition, GUILayout.Height(Mathf.Max(400f, position.height / 3.0f)));

			var addedFolders = new List<string>();
			var removedFolders = new List<string>();

			foreach (var folderPath in clipFolders) {
				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button(" - ", basisHeight, GUILayout.Width(30))) {
					removedFolders.Add(folderPath);
				}

				EditorGUILayout.SelectableLabel(folderPath, basisHeight);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(" + ", basisHeight, GUILayout.Width(50))) {
				var folder = GetFolderPickerResult();
				if (folder != "") {
					if (clipFolders.Contains(folder)) {
						EditorUtility.DisplayDialog($"Duplicate Folder", $"Folder {folder} is already added", "OK");
					} else {
						addedFolders.Add(folder);
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			foreach (var removedFolder in removedFolders) {
				clipFolders.Remove(removedFolder);
				clipFoldersChanged = true;
			}

			foreach (var addedFolder in addedFolders) {
				var trueFolder = RemoveAssetPrefix(addedFolder);
				if (trueFolder != null) {
					clipFolders.Add(trueFolder);
					clipFoldersChanged = true;
				}
			}

			//EditorGUILayout.EndScrollView();
		}

		if (clipFoldersChanged) {
			RefreshClipFolders();
		}

		if (clipFoldersChanged || animatorSetChanged) {
			clipReferences.ClearClipReferences();
			foreach (var animatorController in selectedAnimators) {
				foreach (var clip in animatorController.animationClips) {
					clipReferences.AddClipReference(AssetDatabase.GetAssetPath(clip));
				}
			}
		}



		clipsFoldoutState = EditorGUILayout.Foldout(clipsFoldoutState, "Orphaned Clips");

		if (clipsFoldoutState) {
			//clipsScrollPosition = EditorGUILayout.BeginScrollView(clipsScrollPosition, GUILayout.Height(Mathf.Max(400f, position.height / 3.0f)));

			// Probably will make sense to use the asset paths instead to make this more efficient to sort
			var sortedClips = clipReferences.GetUnreferencedClips().ToList();

			if (sortedClips.Count > 0) {
				EditorGUILayout.BeginHorizontal();
				var selectAllText = "Select All";
				if (GUILayout.Button(selectAllText, basisHeight, MakeTextWidth(selectAllText, 10))) {
					foreach (var clip in sortedClips) {
						selectedClips.Add(clip);
					}
				}
				var selectNoneText = "Select None";
				if (GUILayout.Button(selectNoneText, basisHeight, MakeTextWidth(selectNoneText, 10))) {
					selectedClips.Clear();
				}
				EditorGUILayout.EndHorizontal();

				var oldSelectedClips = selectedClips;
				selectedClips = new HashSet<string>();

				foreach (var clipPath in sortedClips) {
					bool selected = oldSelectedClips.Contains(clipPath);

					EditorGUILayout.BeginHorizontal();

					selected = EditorGUILayout.Toggle(selected, GUILayout.Width(20), basisHeight);
					if (selected) {
						selectedClips.Add(clipPath);
					}

					EditorGUILayout.SelectableLabel(clipPath, basisHeight);
					EditorGUILayout.EndHorizontal();
				}
			} else {
				EditorGUILayout.LabelField("No orphaned clips to show.");
			}

			//EditorGUILayout.EndScrollView();
		}

		bool anySelected = selectedClips.Count > 0;

		EditorGUI.BeginDisabledGroup(!anySelected);

		var exportSelectedText = "Export Selected...";
		if (GUILayout.Button(exportSelectedText, basisHeight, MakeTextWidth(exportSelectedText, 10))) {
			var target = EditorUtility.SaveFilePanel("Export Orphaned Animations", Application.dataPath, "orphaned_files.txt", "txt");
			if (target != null && target != "") {
				using (var writer = new System.IO.StreamWriter(target)) {
					foreach (var clipPath in selectedClips) {
						writer.WriteLine(Path.GetFullPath(clipPath));
					}
				}
			}
		}

		var deleteSelectedText = "Delete Selected...";
		if (GUILayout.Button(deleteSelectedText, basisHeight, MakeTextWidth(deleteSelectedText, 10))) {
			if (EditorUtility.DisplayDialog("Confirm Delete", $"{selectedClips.Count} clips will be deleted?", "Proceed", "Cancel")) {
				var failedPaths = new List<string>();
				foreach (var path in selectedClips) {
					if (!AssetDatabase.DeleteAsset(path)) {
						failedPaths.Add(path);
					}
				}
				RefreshAll();
			}
		}

		EditorGUI.EndDisabledGroup();

		EditorGUILayout.EndScrollView();
	}
}

#endif  // UNITY_EDITOR