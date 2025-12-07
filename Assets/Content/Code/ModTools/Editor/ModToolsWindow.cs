using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace PhantomBrigade.ModTools
{
    internal class ModToolsMenu
    {
        [MenuItem ("PB Mod SDK/Getting Started", priority = -1000)]
        private static void OpenWindow () => ModToolsWindow.ShowWindow ();

        [MenuItem ("PB Mod SDK/Mod Project Manager", priority = -999)]
        private static void OpenModProjectManager ()
        {
            var obj = GameObject.FindObjectOfType<DataManagerMod> ();
            if (obj != null)
                Selection.activeGameObject = obj.gameObject;
        }
    }

    public class ModToolsWindow : OdinEditorWindow
    {
        private static Texture2D texBackgroundInternal;

        public static Texture2D texBackground
        {
            get
            {
                if (texBackgroundInternal == null)
                {
                    var bytes = Convert.FromBase64String (ModToolsWindowBg.pngBackground512);
                    texBackgroundInternal = TextureUtilities.LoadImage (512, 512, bytes);
                    CleanupUtility.DestroyObjectOnAssemblyReload (texBackgroundInternal);
                }

                return texBackgroundInternal;
            }
        }

        private static Texture2D texIconInternal;

        public static Texture2D texIcon
        {
            get
            {
                if (texIconInternal == null)
                {
                    var bytes = Convert.FromBase64String (ModToolsWindowBg.pngIcon512);
                    texIconInternal = TextureUtilities.LoadImage (512, 512, bytes);
                    CleanupUtility.DestroyObjectOnAssemblyReload (texIconInternal);
                }

                return texIconInternal;
            }
        }


        private static float slideSpeed = 2f;

        [NonSerialized] private ModToolsPage slideToPage;
        [NonSerialized] private ModToolsPage slideFromPage;
        private int horizontalSlideDirection;
        [NonSerialized] private int prevPageCount;

        [SerializeField] private float verticalSlideTRaw;
        [SerializeField] private float horizontalSlideTRaw;

        [SerializeField] public List<ModToolsPage> pages = new List<ModToolsPage> ();
        [SerializeField] public float verticalSlideT;
        [SerializeField] public float horizontalSlideT;

        private static GUIStyle padding;
        private static GUIStyle titleStyle;
        private static GUIStyle descStyle;

        protected override void OnEnable ()
        {
            this.wantsMouseMove = true;
        }

        protected override void OnImGUI ()
        {
            var data = ModToolsWindowHelper.GetData ();

            UpdatePages ();

            var rect = this.position.ResetPosition ();

            DrawToolbar (rect.TakeFromTop (EditorStyles.toolbarButton.fixedHeight + 4));
            DrawFancySelector (data, ref rect);

            if ((this.slideToPage ?? this.slideFromPage) != null)
            {
                if (this.slideToPage == null || this.slideFromPage == null)
                {
                    var page = this.slideToPage ?? this.slideFromPage;
                    this.DrawPage (ref rect, page, page.footerSize);
                }
                else if (this.slideToPage != this.slideFromPage)
                {
                    var left = rect;
                    var right = rect;

                    var t = this.horizontalSlideDirection == 1 ? this.horizontalSlideT : (1 - this.horizontalSlideT);
                    left.x -= t * rect.width;
                    right.x = left.xMax;

                    var from = this.slideFromPage;
                    var to = this.slideToPage;

                    if (this.horizontalSlideDirection < 0)
                    {
                        to = this.slideFromPage;
                        from = this.slideToPage;
                    }

                    var footerSize = from.footerSize * (1 - t) + to.footerSize * t;
                    var prevCol = GUI.color;
                    GUI.color = prevCol * new Color (1, 1, 1, 1 - t);
                    this.DrawPage (ref left, from, footerSize);

                    GUI.color = prevCol * new Color (1, 1, 1, t);
                    this.DrawPage (ref right, to, footerSize);

                    GUI.color = prevCol;

                    rect.TakeFromTop (Mathf.Max (left.height, right.height)); // Left & right height should be the same.
                }
                else
                {
                    this.DrawPage (ref rect, this.slideToPage, this.slideToPage.footerSize);
                }
            }

            this.RepaintIfRequested ();
        }

        private void UpdatePages ()
        {
            if (this.prevPageCount != this.pages.Count)
            {
                this.horizontalSlideDirection = this.prevPageCount > this.pages.Count ? -1 : 1;
                this.prevPageCount = this.pages.Count;
            }

            if (Event.current.type == EventType.Layout)
            {
                // Set current page.
                this.slideToPage = this.pages.Count == 0 ? null : this.pages[this.pages.Count - 1];

                var slideVertically = false;
                var slideHorizontally = false;

                if (this.slideFromPage != this.slideToPage)
                {
                    if (this.slideToPage == null)
                    {
                        // Slide up!
                        slideVertically = true;
                    }
                    else if (this.slideFromPage == null)
                    {
                        // Slide down!
                        slideVertically = true;
                    }
                    else
                    {
                        // Slide side!
                        slideHorizontally = true;
                    }
                }

                if (slideVertically)
                {
                    // Set current page.
                    var targetT = Mathf.MoveTowards (this.verticalSlideTRaw, this.slideToPage == null ? 1 : 0, GUITimeHelper.LayoutDeltaTime * slideSpeed);
                    if (targetT != this.verticalSlideTRaw)
                    {
                        this.verticalSlideTRaw = targetT;
                        GUIHelper.RequestRepaint ();
                    }
                    else
                    {
                        // Arrived at page.
                        this.slideFromPage = this.slideToPage;
                    }

                    this.verticalSlideT = this.verticalSlideTRaw * this.verticalSlideTRaw * (3 - 2 * this.verticalSlideTRaw);
                    this.verticalSlideT = this.verticalSlideT * this.verticalSlideT * (3 - 2 * this.verticalSlideT);
                }

                if (slideHorizontally)
                {
                    // Set current page.
                    var targetT = Mathf.MoveTowards (this.horizontalSlideTRaw, 1, GUITimeHelper.LayoutDeltaTime * slideSpeed);
                    if (targetT != this.horizontalSlideTRaw)
                    {
                        this.horizontalSlideTRaw = targetT;
                        GUIHelper.RequestRepaint ();
                    }
                    else
                    {
                        // Arrived at page.
                        this.slideFromPage = this.slideToPage;
                        this.horizontalSlideTRaw = 1;
                    }

                    this.horizontalSlideT = this.horizontalSlideTRaw * this.horizontalSlideTRaw * (3 - 2 * this.horizontalSlideTRaw);
                    this.horizontalSlideT = this.horizontalSlideT * this.horizontalSlideT * (3 - 2 * this.horizontalSlideT);
                }
                else
                {
                    this.horizontalSlideTRaw = 0;
                    this.horizontalSlideT = 0;
                }

                if (this.slideFromPage != null) this.slideFromPage.Window = this;
                if (this.slideToPage != null) this.slideToPage.Window = this;
            }
        }


        public static void ShowWindow ()
        {
            var wnd = GetWindow<ModToolsWindow> ();
            wnd.position = GUIHelper.GetEditorWindowRect ().AlignCenter (900f, 960f);
            wnd.verticalSlideTRaw = 1;
            wnd.verticalSlideT = 1;
            wnd.slideToPage = null;
            wnd.slideFromPage = null;
            wnd.ShowUtility ();

            wnd.titleContent = new GUIContent("PB Mod SDK");

            wnd.OnClose += CallbackOnClose;
            wnd.OnBeginGUI += CallbackOnBeginGUI;
            wnd.OnEndGUI += CallbackOnEndGUI;
        }

        [InitializeOnLoadMethod]
        private static void AutoOpen ()
        {
            if (EditorPrefs.GetBool ("PB_ModTools_ShowWindowOnStartup", false))
            {
                EditorPrefs.SetBool ("PB_ModTools_ShowWindowOnStartup", false);
                EditorApplication.delayCall += () => ShowWindow ();
            }
            #if PB_MODSDK
            ModToolsHelper.LoadUserDLLTypeHints ();
            #endif
        }

        private static void CallbackOnClose ()
        {
            Debug.Log("Window Closed");
        }

        private static void CallbackOnBeginGUI ()
        {
            // GUILayout.Label("-----------");
        }

        private static void CallbackOnEndGUI ()
        {
            // GUILayout.Label("-----------");
        }



        static bool ToolbarButton (ref Rect rect, SdfIconType icon, string text, bool fromLeft, GUIStyle textLabelStyle)
        {
            var textStyle = textLabelStyle ?? SirenixGUIStyles.Label;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent (text);
            var iconWidth = icon == SdfIconType.None ? 0 : rect.height;
            var iconPadding = icon == SdfIconType.None ? 0 : 5;
            var btnPadding = 5;
            var textWidth = textStyle.CalcSize (content).x;
            var btnWidth = textWidth + iconPadding + btnPadding * 2 + iconWidth;
            var r = fromLeft ? rect.TakeFromLeft (btnWidth) : rect.TakeFromRight (btnWidth);
            var clicked = GUI.Button (r, GUIContent.none, EditorStyles.toolbarButton);

            r.TakeFromLeft (btnPadding);
            var iconRect = r.TakeFromLeft (iconWidth);

            r.TakeFromLeft (iconPadding);
            var textRect = r.TakeFromLeft (textWidth);

            if (icon != SdfIconType.None)
            {
                SdfIcons.DrawIcon (iconRect.AlignCenterY (rect.height - 4), icon, textStyle.normal.textColor);
            }

            GUI.Label (textRect, content, textStyle);

            return clicked;
        }

        static bool ToolbarButtonFromLeft (ref Rect rect, string text, bool isOn, GUIStyle style)
        {
            var content = GUIHelper.TempContent (text);
            var textWidth = style.CalcSize (content).x;
            const float btnPadding = 5f;
            var btnWidth = textWidth + btnPadding * 2f;
            var btnRect = rect.TakeFromLeft (btnWidth);
            var hover = btnRect.Contains (Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                style.Draw (btnRect, content, false, hover, isOn, false);
            }

            return GUI.Button (btnRect, GUIContent.none, GUIStyle.none);
        }

        internal static float CalcButtonWidth (Rect rect, string text)
        {
            var textStyle = SirenixGUIStyles.WhiteLabel;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent (text);
            var iconWidth = rect.height;
            var iconPadding = 0;
            var textWidth = textStyle.CalcSize (content).x;
            var btnWidth = textWidth + iconPadding + 5 + 10 * 2 + iconWidth;
            return btnWidth;
        }

        internal static bool Button (ref Rect rect, string text, SdfIconType icon, Direction takeDirection, Direction iconDirection)
        {
            if (Event.current.type == EventType.Layout)
                return false;

            var btnStyle = GUI.skin.button;
            var textStyle = SirenixGUIStyles.WhiteLabel;
            var content = Sirenix.Utilities.Editor.GUIHelper.TempContent (text);
            var iconWidth = rect.height;
            var iconPadding = 0;
            var textWidth = textStyle.CalcSize (content).x;
            var btnWidth = textWidth + iconPadding + 5 + 10 * 2 + iconWidth;
            var r = rect.TakeFromDir (btnWidth, takeDirection);
            var btnRect = r;

            int id = GUIUtility.GetControlID (21345155, FocusType.Passive, rect);

            Rect iconRect;
            r.TakeFromDir (5, iconDirection);
            iconRect = r.TakeFromDir (iconWidth, iconDirection).AlignCenterY (r.height * 0.4f);
            r.TakeFromDir (iconPadding, iconDirection);
            var textRect = r.TakeFromDir (textWidth, iconDirection);

            Event current = Event.current;
            var hover = btnRect.Contains (Event.current.mousePosition);
            switch (current.type)
            {
                case EventType.Repaint:
                    btnStyle.Draw (btnRect, GUIContent.none, id, GUIUtility.hotControl == id, hover);
                    SdfIcons.DrawIcon (iconRect, icon, textStyle.normal.textColor);
                    GUI.Label (textRect, content, textStyle);
                    break;
                case EventType.MouseDown:
                    if (hover)
                    {
                        GUIUtility.hotControl = id;
                        current.Use ();
                    }

                    break;
                case EventType.KeyDown:
                {
                    bool flag = current.alt || current.shift || current.command || current.control;
                    if ((current.keyCode == KeyCode.Space || current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) && !flag && GUIUtility.keyboardControl == id)
                    {
                        current.Use ();
                        GUI.changed = true;
                        return true;
                    }

                    break;
                }
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIHelper.RemoveFocusControl ();
                        current.Use ();
                        if (hover)
                        {
                            GUI.changed = true;
                            return true;
                        }
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        current.Use ();
                    }

                    break;
            }

            return false;
        }


        private void DrawPage (ref Rect rect, ModToolsPage page, float footerSize)
        {
            var vertical_t = page.VerticalSlideT;

            var prevCol = GUI.color;
            GUI.color *= new Color (1, 1, 1, vertical_t);
            if (rect.height > 1)
            {
                // Top
                {
                    var topRect = rect.TakeFromTop (50 * vertical_t);
                    var headerText = page.titleHeader;
                    var style = SirenixGUIStyles.SectionHeaderCentered;
                    var textWidth = style.CalcSize (GUIHelper.TempContent (headerText)).x;
                    var iconSize = 25;
                    var spacing = 5;
                    var icon = page.titleIcon;
                    var r = topRect.AlignCenterX (iconSize + spacing + textWidth);

                    var c = GUI.color;
                    GUI.color = Color.white;
                    EditorGUI.DrawRect (topRect, SirenixGUIStyles.HeaderBoxBackgroundColor);
                    GUI.color = c;

                    GUI.Label (r.AlignRight (textWidth), headerText, style);
                    SdfIcons.DrawIcon (r.AlignLeft (iconSize).AddX (-6), icon, style.normal.textColor);
                }

                // Bottom
                {
                    var bottomRect = rect.TakeFromBottom (footerSize * vertical_t);

                    // Don't fade slide background sections.
                    var c = GUI.color;
                    GUI.color = Color.white;
                    EditorGUI.DrawRect (bottomRect, SirenixGUIStyles.DarkEditorBackground);
                    EditorGUI.DrawRect (bottomRect.AlignTop (1), SirenixGUIStyles.BorderColor);
                    GUI.color = c;

                    bottomRect = bottomRect.HorizontalPadding (20);
                    bottomRect = bottomRect.AlignCenterY (25);

                    if (Button (ref bottomRect, "Back", SdfIconType.ChevronLeft, Direction.Left, Direction.Left))
                    {
                        page.GoBack ();
                    }

                    page.DrawFooter (bottomRect);
                }

                // Body
                {
                    var bodyRect = rect;
                    EditorGUI.DrawRect (bodyRect.AlignTop (1), SirenixGUIStyles.BorderColor);
                    page.DrawPage (bodyRect);
                }
            }

            GUI.color = prevCol;
        }

        private void DrawToolbar (Rect rect)
        {
            EditorGUI.DrawRect (rect.TakeFromBottom (2), new Color (0, 0, 0, 1));

            rect = rect.AlignCenterY (EditorGUIUtility.singleLineHeight);
            if (ToolbarButton (ref rect, SdfIconType.ArrowClockwise, "Reload", false, null))
            {
                ModToolsTextureHelper.Load ();
                ModToolsWindowHelper.LoadData ();
                if (pages.Count > 0)
                {
                    pages.Clear ();
                }
            }

            if (ToolbarButton (ref rect, SdfIconType.Book, "Modding Wiki", false, null))
            {
                Application.OpenURL ("https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModSystem");
            }
            if (ToolbarButton (ref rect, SdfIconType.Discord, "Discord", false, null))
            {
                Application.OpenURL ("https://discord.gg/WbcZEhYp");
            }
            if (ToolbarButton (ref rect, SdfIconType.Github, "GitHub", false, null))
            {
                Application.OpenURL ("https://github.com/BraceYourselfGames/PB_ModSDK");
            }

            // var versionName = $"{ModToolsVersion.name} {ModToolsVersion.version}";
            // if (ToolbarButton (ref rect, SdfIconType.None, versionName, false, SirenixGUIStyles.CenteredGreyMiniLabel))
            // {
            //     Application.OpenURL ("https://github.com/artyom-zuev/PB_ModSDK");
            // }

            var pagesClosed = pages.Count == 0;
            if (ToolbarButtonFromLeft (ref rect, "Root", pagesClosed, EditorStyles.toolbarButton))
            {
                pages.Clear ();
            }

            for (var i = 0; i < pages.Count; i++)
            {
                var p = pages[i];
                if (ToolbarButtonFromLeft (ref rect, p.titleHeader, i == (pages.Count - 1), EditorStyles.toolbarButton))
                {
                    pages.SetLength (i + 1);
                    break;
                }
            }
        }

        private void DrawFancySelector (ModToolsWindowSection data, ref Rect totalRect)
        {
            var selectorRect = totalRect.TakeFromTop (Mathf.Lerp (totalRect.height, 120, 1 - this.verticalSlideT));

            descStyle = descStyle ?? new GUIStyle (SirenixGUIStyles.MultiLineCenteredLabel);
            descStyle.wordWrap = false;
            descStyle.alignment = TextAnchor.UpperLeft;

            titleStyle = titleStyle ?? new GUIStyle (SirenixGUIStyles.BoldTitle);
            titleStyle.alignment = TextAnchor.UpperLeft;
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontSize = 14;

            EditorGUI.DrawRect (selectorRect.TakeFromBottom (2 * (1 - this.verticalSlideT)), SirenixGUIStyles.BorderColor);
            EditorGUI.DrawRect (selectorRect, SirenixGUIStyles.ListItemColorOdd);

            ref var p = ref data; // ref ModToolsWindowData.Products[i];
            var rect = selectorRect; // selectorRect.Split (i, ModToolsWindowData.Products.Length);
            bool expanded = this.verticalSlideT > 0.001f;

            if (expanded)
            {
                GUI.color = new Color (1, 1, 1, this.verticalSlideT);

                // Draw buttons
                if (p.pages != null && p.pages.Count > 0)
                {
                    rect.TakeFromBottom (20 * this.verticalSlideT);
                    var btnsRect = rect.TakeFromBottom (40 * this.verticalSlideT).Padding (10, 5 * this.verticalSlideT);

                    for (int j = 0; j < p.pages.Count; j++)
                    {
                        var subpage = p.pages[j];
                        if (subpage == null)
                            continue;

                        if (GUI.Button (btnsRect.Split (j, p.pages.Count), subpage.buttonLabel))
                        {
                            subpage.Window = this;
                            subpage.EnterPage ();
                        }
                    }

                    rect.TakeFromBottom (20 * this.verticalSlideT);
                }

                if (p.status != null && p.status.Count > 0)
                {
                    for (int i = p.status.Count - 1; i >= 0; --i)
                    {
                        var status = p.status[i];
                        bool passed = status.funcCheck != null ? status.funcCheck.Invoke () : true;

                        var icon = SdfIconType.CheckSquare;
                        var color = ModToolsColors.HighlightValid;
                        var textPrimary = status.textTruePrimary;
                        var textSecondary = status.textTrueSecondary;

                        if (!passed)
                        {
                            icon = SdfIconType.DashSquare;
                            color = ModToolsColors.HighlightError;
                            textPrimary = status.textFalsePrimary;
                            textSecondary = status.textFalseSecondary;
                        }

                        var r = rect.TakeFromBottom (40 * this.verticalSlideT);
                        EditorGUI.DrawRect (r.AlignBottom (1), SirenixGUIStyles.BorderColor);
                        r = r.Padding (10, 8 * this.verticalSlideT);
                        var iconRect = r.TakeFromLeft (r.height);
                        r.TakeFromLeft (10);

                        SdfIcons.DrawIcon (iconRect, icon, color);

                        if (!string.IsNullOrEmpty (textPrimary))
                            GUI.Label (r, textPrimary, SirenixGUIStyles.BoldTitle);

                        float btnWidth = 0;
                        if (status.buttons != null && status.buttons.Count > 0)
                        {
                            var btnsRect = r;
                            foreach (var button in status.buttons)
                            {
                                bool available = true;
                                if (button is DataBlockModToolsButtonConditional buttonCond && buttonCond.conditions != null)
                                {
                                    foreach (var cond in buttonCond.conditions)
                                    {
                                        if (cond != null && !cond.IsTrue ())
                                        {
                                            available = false;
                                            break;
                                        }
                                    }

                                    if (!available && !buttonCond.visibleUnavailable)
                                        continue;
                                }

                                var w = GUI.skin.button.CalcSize (GUIHelper.TempContent (button.label)).x + 20;
                                btnWidth += w + 10;

                                GUI.enabled = available;
                                var clicked = GUI.Button (btnsRect.TakeFromRight (w), button.label);
                                if (clicked && button.actionsOnClick != null)
                                {
                                    Debug.Log (button.label);
                                    foreach (var action in button.actionsOnClick)
                                    {
                                        action?.Run ();
                                    }
                                }

                                GUI.enabled = true;
                                btnsRect.TakeFromRight (10);
                            }

                            r.TakeFromRight (btnWidth);
                            if (!string.IsNullOrEmpty (textSecondary))
                                GUI.Label (r, textSecondary, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty (textSecondary))
                                GUI.Label (r, textSecondary, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                        }

                    }
                }

                GUI.color = Color.white;
            }

            // Draw image
            {
                var greyScale = p.enabled ? 0f : 1f;
                var color = p.enabled ? Color.white : new Color (1, 1, 1, 0.5f);

                {
                    color = Color.Lerp (color, new Color (1, 1, 1, 1), 1 - this.verticalSlideT);
                    greyScale = Mathf.Lerp (greyScale, 0, 1 - this.verticalSlideT);
                }

                GUITextureDrawingUtil.DrawTexture (rect.Expand (1), texBackground, ScaleMode.ScaleAndCrop, color, p.hueColor, greyScale);
                GUITextureDrawingUtil.DrawTexture (rect.Padding (Mathf.Min (240, rect.width * 0.25f), Mathf.Min (240, rect.height * 0.1f)), texIcon, ScaleMode.ScaleToFit, color, default, greyScale);
            }

            // Draw attribution
            if (expanded)
            {
                var r = rect.TakeFromBottom (40 * this.verticalSlideT);
                r = r.Expand (0f, 0f, 0f, 1f);
                EditorGUI.DrawRect (r, new Color (0f, 0f, 0f, 0.3f));
                EditorGUI.DrawRect (r.AlignBottom (1), SirenixGUIStyles.BorderColor);

                r = r.Padding (10, 8 * this.verticalSlideT);

                var versionText = $"{ModToolsVersion.version} — {ModToolsVersion.attrVersionSuffix}\n{ModToolsVersion.attrByg} — {ModToolsVersion.attrOdin}";
                GUI.Label (r, versionText, SirenixGUIStyles.RightAlignedGreyMiniLabel);
            }
        }
    }
}
