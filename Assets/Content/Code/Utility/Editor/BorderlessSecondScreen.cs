using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class BorderlessSecondScreen : EditorWindow
{

    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static readonly PropertyInfo ShowToolbarProperty = GameViewType.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);

    static EditorWindow instance;

    [MenuItem("Window/Borderless Second Screen")]
    public static void ShowWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(BorderlessSecondScreen));
    }

    public enum MonitorSide
    {
        right,
        left
    }

    public Vector2Int mainDisplayRes;
    public Vector2Int secondaryDisplayRes;
    public float displayScale = 1;
    public Vector2Int offset;
    public MonitorSide monitorSide = MonitorSide.right;
    public bool refreshing = false;
    public int refreshCounter = 0;

    public bool gameViewOpen = false;

    private GUIStyle headerStyle;
    private GUIStyle buttonHolderStyle;
    private GUIStyle buttonStyle;

    void OnGUI()
    {
        EditorGUIUtility.wideMode = true;

        headerStyle = new GUIStyle("DD HeaderStyle");
        headerStyle.alignment = TextAnchor.MiddleLeft;
        headerStyle.padding = new RectOffset(20, 5, 5, 5);
        headerStyle.margin = new RectOffset(0, 0, 0, 5);

        buttonHolderStyle = new GUIStyle();
        buttonHolderStyle.margin = new RectOffset(10, 10, 10, 10);

        buttonStyle = new GUIStyle("LargeButton");
        buttonStyle.padding = new RectOffset(10, 10, 5, 5);

        GUILayout.Label("Primary Display", headerStyle, GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
            mainDisplayRes = EditorGUILayout.Vector2IntField("Resolution", mainDisplayRes);
            GUILayout.BeginHorizontal();
                displayScale = EditorGUILayout.FloatField("Desktop Scale", displayScale);
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

        GUILayout.Space(15);

        GUILayout.Label("Target Display", headerStyle, GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
            secondaryDisplayRes = EditorGUILayout.Vector2IntField("Resolution", secondaryDisplayRes);
            GUILayout.BeginHorizontal();
                monitorSide = (MonitorSide)EditorGUILayout.EnumPopup("Display Side", monitorSide);
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

        GUILayout.Space(15);

        GUILayout.Label("Game View", headerStyle, GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
            offset = EditorGUILayout.Vector2IntField("Window Offset", offset);
        EditorGUI.indentLevel--;

        GUILayout.Space(15);

        if (!instance && gameViewOpen) {
            if (RecaptureGameView()) {
                refreshing = true;
            }
        }

        if (instance) {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Sometimes the Game View likes to turn its toolbar back on for no reason. We'll try to detect that and fix it immediately, but this only works if this window remains open.", MessageType.Info);
            EditorGUI.indentLevel--;
            GUILayout.BeginHorizontal(buttonHolderStyle);
                ShowToolbarProperty?.SetValue(instance, false);
                if (GUILayout.Button("Refresh Game View", buttonStyle)) {
                    refreshing = true;
                }
                if (GUILayout.Button("Close Game View", buttonStyle)) {
                    instance.Close();
                    instance = null;
                    gameViewOpen = false;
                }
            GUILayout.EndHorizontal();
            if (refreshing) {
                RefreshGameView();
            }
        } else {
            GUILayout.BeginHorizontal(buttonHolderStyle);
                if (GUILayout.Button("Open Game View", buttonStyle)) {
                    OpenGameView();
                    refreshing = true;
                }
            GUILayout.EndHorizontal();
        }

        GUILayout.FlexibleSpace();

    }

    void OpenGameView()
    {

        if (instance != null) {
            // Make sure we only open one window at a time
            instance.Close();
            instance = null;
            gameViewOpen = false;
        }

        instance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);

        instance.ShowPopup();
        instance.Focus();
    }

    void RefreshGameView()
    {
        if (refreshCounter >= 10) {
            Debug.LogWarning("Failed to size a fullscreen Game view after 10 refreshes; aborting");
            refreshCounter = 0;
            refreshing = false;
            return;
        }

        refreshCounter += 1;

        Vector2 targetPosition = mainDisplayRes;
        if (monitorSide == MonitorSide.right) {
            targetPosition.x /= displayScale;
        } else if (monitorSide == MonitorSide.left) {
            targetPosition.x = secondaryDisplayRes.x / displayScale * -1;
        }
        targetPosition.x += offset.x / displayScale;
        targetPosition.y += offset.y / displayScale;
        Rect fullscreenRect = new Rect(targetPosition.x, 0, secondaryDisplayRes.x / displayScale, secondaryDisplayRes.y);

        // The new window can take a few frames to respond, so we'll check to make sure it ends up in the correct position
        if (instance.position == fullscreenRect) {
            refreshCounter = 0;
            refreshing = false;
            gameViewOpen = true;
            return;
        }

        instance.position = fullscreenRect;
    }

    bool RecaptureGameView()
    {
        try {
            instance = EditorWindow.GetWindow(GameViewType);
        } catch (InvalidCastException e) {
            Debug.LogError("Failed to recapture the Game View: " + e);
            return false;
        }
        return true;
    }
}