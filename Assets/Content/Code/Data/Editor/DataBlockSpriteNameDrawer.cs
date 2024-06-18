using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;


using PhantomBrigade.Data.UI;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine.Windows;

public class DataBlockSpriteNameDrawer : OdinValueDrawer<DataBlockSpriteName>
{
    private static bool spritePreviewUsed = true;
    private static bool spritePreviewUniform = false;
    private static int spritePreviewSize = 80;
    
    protected override void DrawPropertyLayout (GUIContent content)
    {
        var target = ValueEntry.SmartValue;
        if (target == null)
            return;

        EditorGUILayout.BeginHorizontal ();
        SpriteHelper.DrawSpritePreview (target.name, spritePreviewUniform, spritePreviewSize);
        EditorGUILayout.EndHorizontal ();
    }

    private void SelectSprite (string spriteName)
    {
        var target = ValueEntry.SmartValue;
        if (target == null)
            return;

        target.name = spriteName;
        UtilityCustomInspector.RepaintAllWindows ();
    }
}

/*
public sealed class SpriteNameAttributeDrawer : OdinAttributeDrawer<DataEditor.SpriteNameAttribute, string>
{
    protected override void DrawPropertyLayout (GUIContent label)
    {
        EditorGUILayout.BeginHorizontal ();
        SpriteHelper.DrawSpritePreview (ValueEntry.SmartValue, Attribute.uniform, Attribute.size);
        EditorGUILayout.EndHorizontal ();
    }

    private void SelectSprite (string spriteName)
    {
        ValueEntry.SmartValue = spriteName;
        UtilityCustomInspector.RepaintAllWindows ();
    }
}
*/

public class SpriteNameAttributeResolver<T> : OdinAttributeProcessor<T> where T : class, new()
{
    public override void ProcessChildMemberAttributes (InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (parentProperty == null || parentProperty.Parent == null)
            return;

        if (!attributes.HasAttribute<DataEditor.SpriteNameAttribute> ())
            return;

        var source = attributes.GetAttribute<DataEditor.SpriteNameAttribute> ();
        var exp = $"@SpriteHelper.DrawSpritePreview ($property, false, {source.size})";
        
        var fieldName = member.Name;
        attributes.Add (new InlineButtonAttribute ($"@{fieldName} = null", "×"));
        attributes.Add (new ValueDropdownAttribute ($"@SpriteHelper.GetSpriteKeys ()"));
        attributes.Add (new OnInspectorGUIAttribute (exp, false));
    }
}

public static class SpriteHelper
{
    public class SpriteInfo
    {
        public string name;
        public int x = 0;
        public int y = 0;
        public int width = 0;
        public int height = 0;
    }
    
    public class SpriteAtlasInfo
    {
        public SortedDictionary<string, SpriteInfo> sprites;
    }
    
    private static bool initialized = false;
    private static SpriteAtlasInfo atlasInfo = null;
    private static Texture2D atlasTex = null;
    private static string[] atlasSpriteKeys = null;

    private const string atlasInfoPath = "Assets/StreamingAssets/UI/Sprites/atlas_info.yaml";
    private const string atlasTexPath = "Assets/StreamingAssets/UI/Sprites/atlas_main.png";
    
    private static void Init ()
    {
        if (initialized)
            return;
        
        initialized = true;

        atlasInfo = null;
        atlasTex = null;
        atlasSpriteKeys = null;
        
        atlasInfo = UtilitiesYAML.LoadDataFromFile<SpriteAtlasInfo> (atlasInfoPath, warnIfMissing: false);
        if (atlasInfo == null)
            Debug.LogWarning ($"Can't find sprite atlas info at {atlasInfoPath}");
        
        var atlasTexPathAbsolute = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), atlasTexPath);
        if (!File.Exists (atlasTexPathAbsolute))
            Debug.LogWarning ($"Can't find sprite atlas texture at {atlasTexPath}");
        else
        {
            try
            {
                byte[] pngBytes = File.ReadAllBytes (atlasTexPathAbsolute);
                atlasTex = new Texture2D (4, 4, TextureFormat.BC7, true, false);
                atlasTex.wrapMode = TextureWrapMode.Clamp;
                atlasTex.filterMode = FilterMode.Bilinear;
                atlasTex.anisoLevel = 2;
                atlasTex.LoadImage (pngBytes);
            }
            catch (Exception e)
            {
                Debug.LogWarning (e);
                throw;
            }
        }

        if (atlasInfo != null && atlasInfo.sprites != null)
            atlasSpriteKeys = atlasInfo.sprites.Keys.ToArray ();
    }
    
    public static IEnumerable<string> GetSpriteKeys ()
    {
        if (!initialized)
            Init ();
        
        if (atlasInfo?.sprites == null)
            return null;

        return atlasInfo.sprites.Keys;
    }

    public static void DrawSpritePreview (InspectorProperty spriteProperty, bool previewUniform, float previewSize)
    {
        var spriteName = spriteProperty.ValueEntry.WeakSmartValue as string;
        DrawSpritePreview (spriteName, previewUniform, previewSize);
    }
    
    public static void DrawSpritePreview (string spriteName, bool previewUniform, float previewSize)
    {
        if (!initialized)
            Init ();

        if (atlasInfo?.sprites == null || atlasTex == null || string.IsNullOrEmpty (spriteName))
            return;

        var sprites = atlasInfo.sprites;
        var sprite = sprites != null && sprites.TryGetValue (spriteName, out var sprite1) ? sprite1 : null;
        if (sprite == null)
            return;

        var width = previewUniform ? previewSize : Mathf.Clamp (sprite.width, 16f, 256f);
        var height = Mathf.Clamp (sprite.height, 16f, previewSize);
        
        GUILayout.Space (4);
        var rectFull = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.Width (width), GUILayout.Height (height));
        var rect = new Rect (rectFull.x + 4, rectFull.y, width, height);

        Rect uv = new Rect (sprite.x, sprite.y, sprite.width, sprite.height);
        uv = GetTexCoords (uv, atlasTex.width, atlasTex.height);

        // Calculate the texture's scale that's needed to display the sprite in the clipped area
        float scaleX = rect.width / uv.width;
        float scaleY = rect.height / uv.height;

        // Stretch the sprite so that it will appear proper
        float aspect = (scaleY / scaleX) / ((float) atlasTex.height / atlasTex.width);
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
        
        // var rectTest = GUILayoutUtility.GetRect (GUIContent.none, GUIStyle.none, GUILayout.Height (2));
        // clipRect.x += rectTest.width - clipRect.width;
        // clipRect.x -= 14;
        
        GUI.DrawTextureWithTexCoords (clipRect, atlasTex, uv);
        SirenixEditorGUI.DrawBorders (clipRect.Expand (1), 1);
        
        // GUILayout.Label ($"S: {sprite.width}x{sprite.height} | R1: {rectFull.x}x{rectFull.y} | R2: {clipRect.width}x{clipRect.height}", EditorStyles.miniLabel);

        /*
        if (onSelect != null)
        {
            EditorGUI.BeginChangeCheck ();
            var spriteNameNew = DrawStringDropdown (spriteName, null, atlasSpriteKeys);
            if (EditorGUI.EndChangeCheck () && !string.Equals (spriteNameNew, spriteName))
                onSelect.Invoke (spriteNameNew);
        }
        */
    }
    
    private static string DrawStringDropdown (string keySelected, string label, string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return keySelected;
            
        int index = -1;
        for (int i = 0, count = keys.Length; i < count; ++i)
        {
            var keyCandidate = keys[i];
            if (string.Equals (keyCandidate, keySelected))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            index = 0;
            keySelected = keys[0];
        }

        bool labelUsed = !string.IsNullOrEmpty (label);
        if (labelUsed)
        {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (label, EditorStyles.miniLabel, GUILayout.MaxWidth (80f));
        }

        int indexNew = EditorGUILayout.Popup (index, keys);
        if (indexNew != index)
            keySelected = keys[indexNew];
            
        if (labelUsed)
            GUILayout.EndHorizontal ();

        return keySelected;
    }
    
    private static Rect GetTexCoords (Rect input, int width, int height)
    {
        width = Mathf.Max (1, width);
        height = Mathf.Max (1, height);
        Rect uv = input;
        uv.xMin = input.xMin / width;
        uv.xMax = input.xMax / width;
        uv.yMin = 1f - input.yMax / height;
        uv.yMax = 1f - input.yMin / height;
        return uv;
    }
}
