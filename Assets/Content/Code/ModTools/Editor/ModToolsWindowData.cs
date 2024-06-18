using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhantomBrigade.Data;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;

namespace PhantomBrigade.ModTools
{
    public class ModToolsPage
    {
        public string buttonLabel;

        public SdfIconType titleIcon;
        public string titleHeader;
        public string titleDesc;

        public float footerSize = 80;

        [NonSerialized]
        public ModToolsWindow Window;

        public float VerticalSlideT => 1 - Window.verticalSlideT;

        private static GUIStyle padding;

        private Vector2 scrollPos;

        public virtual void DrawFooter (Rect rect)
        {
        }

        public virtual void DrawPage (Rect rect)
        {
        }

        public virtual void GoBack ()
        {
            if (this.Window.pages.Count > 0)
            {
                this.Window.pages.RemoveAt (this.Window.pages.Count - 1);
            }
        }

        public virtual void EnterPage ()
        {
            this.Window.pages.Add (this);
        }

        protected void BeginScrollableLayoutPage (Rect rect, int paddingSize = 20)
        {
            padding ??= new GUIStyle () { padding = new RectOffset () { bottom = 20, left = 20, top = 20, right = 20 } };
            padding.padding.left = paddingSize;
            padding.padding.right = paddingSize;
            padding.padding.top = paddingSize;
            padding.padding.bottom = paddingSize;

            GUILayout.BeginArea (rect);
            var lastScrollPos = scrollPos;
            scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
            if (scrollPos != lastScrollPos)
            {
                GUIHelper.RequestRepaint ();
            }
            GUILayout.BeginVertical (padding);
        }

        protected void EndScrollableLayoutPage ()
        {
            GUILayout.EndVertical ();
            EditorGUILayout.EndScrollView ();
            GUILayout.EndArea ();
        }
    }

    public class ButtonPage : ModToolsPage
    {
        private Action action;

        public ButtonPage (Action action)
        {
            this.action = action;
        }

        public override void EnterPage ()
        {
            this.action ();
        }
    }

    public class ModToolsPageSectioned : ModToolsPage
    {
        public List<ModPageSection> sections = new List<ModPageSection> ();

        private static GUIStyle multiline;
        private float prevWidth = 200;

        public override void DrawPage (Rect pageRect)
        {
            if (multiline == null)
            {
                multiline = new GUIStyle (SirenixGUIStyles.MultiLineLabel)
                {
                    alignment = TextAnchor.UpperLeft,
                    clipping = TextClipping.Overflow,
                };
            }

            this.BeginScrollableLayoutPage (pageRect);

            if (this.prevWidth != 0)
            {
                var textPadding = 10;
                var iconWidth = 50;

                if (!string.IsNullOrEmpty (this.titleDesc))
                {
                    var predicted = prevWidth - textPadding - textPadding - iconWidth;
                    var descriptionSize = string.IsNullOrEmpty (this.titleDesc) ? 0 : multiline.CalcHeight (GUIHelper.TempContent (this.titleDesc), predicted);
                    var rect = GUILayoutUtility.GetRect (0, (descriptionSize + 20) * this.VerticalSlideT);
                    var descriptionRect = rect;
                    GUI.Label (descriptionRect, this.titleDesc, multiline);
                }

                for (int i = 0; i < sections.Count; i++)
                {
                    var section = sections[i];
                    if (!IsSectionVisible (section))
                    {
                        continue;
                    }

                    var enabled = IsSectionEnabled (section);
                    var completed = IsSectionCompleted (section);
                    var prevEnabled = GUI.enabled;
                    GUI.enabled = enabled;
                    
                    Texture2D image = null;
                    if (!string.IsNullOrEmpty (section.image))
                        image = ModToolsTextureHelper.GetTexture (section.image);
                    
                    var hasActionBtns = section.buttons != null;
                    var actionBtnsHeight = hasActionBtns ? 25 : 0;
                    var difficultyRectWidth = 30;
                    var predicted = prevWidth - textPadding - textPadding - iconWidth - difficultyRectWidth;
                    var descriptionSize = string.IsNullOrEmpty (section.description) ? 0 : multiline.CalcHeight (GUIHelper.TempContent (section.description), predicted);
                    
                    var imageHeightLimit = 512; // Take from field
                    
                    float imageWidth = 0f;
                    float imageHeight = 0f;

                    if (image != null && image.width > 0f && image.height > 0f)
                    {
                        // imageHeight = Mathf.Clamp (image.height, 16, 512);
                        // imageWidth = Mathf.Min (image.width, Mathf.Min (contentWidth, imageHeight));
                        // var shrink = imageWidth / Mathf.Max (2f, image.width);
                        // imageHeight = Mathf.Max (2f,  image.height) * shrink;
                        
                        imageHeightLimit = Mathf.Clamp (imageHeightLimit, 16, 512);
                        imageWidth = Mathf.Min (GUIHelper.ContextWidth - 100f, image.width);
                        var shrink = imageWidth / (float) image.width;
                        imageHeight = image.height * shrink;
                    }
                    
                    var rectHeight = 50 + descriptionSize + actionBtnsHeight + imageHeight;
                    var rect = GUILayoutUtility.GetRect (0, rectHeight * this.VerticalSlideT);
                    
                    var bgRect = rect;
                    var iconRect = rect.TakeFromLeft (iconWidth).AlignCenterY (25 * this.VerticalSlideT);
                    var contentRect = rect.Padding (textPadding);
        
                    var imageRect = contentRect.TakeFromTop (imageHeight);
                    imageRect = imageRect.TakeFromLeft (imageWidth);
                    
                    var titleRect = contentRect.TakeFromTop (20);
                    var difficultyRect = contentRect.TakeFromRight (difficultyRectWidth).AlignBottom (EditorGUIUtility.singleLineHeight);
                    var descriptionRect = contentRect;
                    var isMouseOver = enabled && !hasActionBtns && bgRect.Contains (Event.current.mousePosition);

                    var bgColor = ModToolsColors.BoxBackground;
                    var textColor = ModToolsColors.BoxText;

                    if (completed)
                    {
                        bgColor = ModToolsColors.BoxBackgroundGreen;
                        textColor = ModToolsColors.BoxTextGreen;
                    }

                    if (string.IsNullOrEmpty (section.description))
                    {
                        titleRect.y += 5;
                    }

                    var cc = GUI.contentColor;
                    GUI.contentColor = textColor;

                    SdfIcons.DrawIcon (iconRect.AddX (5), section.icon, textColor);
                    EditorGUI.DrawRect (bgRect, bgColor);

                    if (isMouseOver)
                    {
                        EditorGUI.DrawRect (bgRect, bgColor);
                    }

                    SirenixEditorGUI.DrawBorders (bgRect, 1);

                    if (image != null)
                    {
                        GUI.contentColor = cc;
                        GUI.DrawTexture (imageRect, image);
                        SirenixEditorGUI.DrawBorders (imageRect, 1);
                        // GUITextureDrawingUtil.DrawTexture (imageRect, image, ScaleMode.ScaleAndCrop, Color.white, default, 0f);
                        GUI.contentColor = textColor;
                    }

                    GUI.Label (titleRect, section.header, SirenixGUIStyles.BoldLabel);
                    if (!string.IsNullOrEmpty (section.description))
                    {
                        GUI.Label (descriptionRect, section.description, multiline);
                    }
                    if (!string.IsNullOrEmpty (section.hint))
                    {
                        GUI.Label (difficultyRect, section.hint, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }

                    GUI.contentColor = cc;

                    bool onClickChildPage = section.childPage != null;
                    bool onClickActions = section.actionsOnClick != null && section.actionsOnClick.Count > 0;

                    if (hasActionBtns)
                    {
                        var btnsRect = descriptionRect.TakeFromBottom (actionBtnsHeight);
                        foreach (var button in section.buttons)
                        {
                            var w = GUI.skin.button.CalcSize (GUIHelper.TempContent (button.label)).x + 20;
                            var clicked = GUI.Button (btnsRect.TakeFromLeft (w), button.label);
                            if (clicked && button.actionsOnClick != null)
                            {
                                Debug.Log (button.label);
                                foreach (var action in button.actionsOnClick)
                                {
                                    action?.Run ();
                                }
                            }
                            btnsRect.TakeFromLeft (10);
                        }
                    }
                    else if (onClickChildPage|| onClickActions)
                    {
                        if (GUI.Button (rect, GUIContent.none, GUIStyle.none))
                        {
                            if (onClickChildPage)
                            {
                                section.childPage.Window = this.Window;
                                section.childPage.EnterPage ();
                            }

                            if (onClickActions)
                            {
                                foreach (var action in section.actionsOnClick)
                                {
                                    action?.Run ();
                                }
                            }
                        }
                    }
                    

                    if (i != this.sections.Count - 1)
                    {
                        GUILayoutUtility.GetRect (0, 10);
                    }

                    GUI.enabled = prevEnabled;
                }
            }

            var vw = GUILayoutUtility.GetRect (0, 1).width;
            if (Event.current.type == EventType.Repaint)
            {
                this.prevWidth = vw;
            }
            this.EndScrollableLayoutPage ();
        }

        static bool IsSectionVisible (ModPageSection section) => section.conditionsVisible == null || section.conditionsVisible.All (check => check == null || check.IsTrue ());
        static bool IsSectionEnabled (ModPageSection section) => section.conditionsEnabled == null || section.conditionsEnabled.All (check => check == null || check.IsTrue ());
        static bool IsSectionCompleted (ModPageSection section) => section.conditionsComplete != null && section.conditionsComplete.All (check => check == null || check.IsTrue ());
    }

    public class ModPageSection
    {
        public SdfIconType icon;
        public string image;
        public string hint;
        public string header;
        public string description;

        public List<IModToolsCheck> conditionsVisible;
        public List<IModToolsCheck> conditionsEnabled;
        public List<IModToolsCheck> conditionsComplete;

        public List<IModToolsFunction> actionsOnClick;
        public List<DataBlockModToolsButton> buttons;

        public ModToolsPage childPage;
    }

    public struct ModToolsWindowSection
    {
        public bool enabled;
        public Color hueColor;
        public List<ModToolsSectionStatus> status;
        public List<ModToolsPage> pages;
    }

    public struct ModToolsSectionStatus
    {
        public string textTruePrimary;
        public string textTrueSecondary;

        public string textFalsePrimary;
        public string textFalseSecondary;

        public Func<bool> funcCheck;
        public List<DataBlockModToolsButton> buttons;

    }



    public static class ModToolsWindowHelper
    {
        private static ModToolsWindowSection data;
        private static bool dataLoaded = false;

        private static bool AssetsInstalled () => AssetPackageHelper.AreLevelAssetsInstalled () && AssetPackageHelper.AreUnitAssetsInstalled ();

        private static bool SceneOpened ()
        {
            var sceneActive = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
            bool nameMatch = string.Equals (sceneActive.name, "game_main_sdk") || string.Equals (sceneActive.name, "game_extended_sdk");
            return nameMatch;
        }

        public static ModToolsWindowSection GetData ()
        {
            CheckData ();
            return data;
        }

        private static void CheckData ()
        {
            if (dataLoaded)
                return;

            dataLoaded = true;
            LoadData ();
        }

        private const string patternTagColorStart = "<color=#";
        private const string patternTagEnd = ">";

        private static Color textContentColorLight = new Color (0.25f, 0.1f, 0.1f, 1f);

        private static StringBuilder textProcSb = new StringBuilder ();
        private static List<(string, string)> textProcReplacements = new List<(string, string)>
        {
            ("<c=", "<color="),
            ("</c>", "</color>"),
            ("<hl>", "<color=hl>"),
            ("</hl>", "</color>"),
            ("<color=hl>", "<color=#b5e6ff>")
        };

        private static List<int> indexBuffer = new List<int> ();

        public static List<int> AllIndexesOfPattern (string input, string pattern)
        {
            indexBuffer.Clear ();
            if (string.IsNullOrEmpty (pattern))
            {
                return indexBuffer;
            }
            for (var index = input.IndexOf (pattern, 0, StringComparison.Ordinal);
                index != -1;
                index = input.IndexOf (pattern, index + pattern.Length, StringComparison.Ordinal))
            {
                indexBuffer.Add (index);
            }
            return indexBuffer;
        }

        private static string GetTextProcessedForSkin (string input)
        {
            if (string.IsNullOrEmpty (input))
                return input;

            foreach (var kvp in textProcReplacements)
                input = input.Replace (kvp.Item1, kvp.Item2);

            bool isProSkin = EditorGUIUtility.isProSkin;
            if (isProSkin)
                return input;

            var indexesOfColorTag = AllIndexesOfPattern (input, patternTagColorStart);
            if (indexesOfColorTag.Count > 0)
            {
                var tagStartLength = patternTagColorStart.Length;
                var colorHSBBase = new HSBColor (textContentColorLight);

                textProcSb.Clear ();
                textProcSb.Append (input);

                foreach (var index in indexesOfColorTag)
                {
                    var hexIndex = index + tagStartLength;
                    var hex = input.Substring (hexIndex, 6);

                    var colorRGB = UtilityColor.ColorFromHex (hex);
                    var colorHSB = new HSBColor (colorRGB);

                    colorHSB = new HSBColor (colorHSB.h, colorHSBBase.s, colorHSBBase.b);
                    colorRGB = colorHSB.ToColor ();

                    textProcSb.Remove (hexIndex, 6);
                    textProcSb.Insert (hexIndex, UtilityColor.ToHexRGB (colorRGB));
                }

                input = textProcSb.ToString ();
            }

            // Ensure colors embedded into text are readable on light Editor skin
            for (int i = 0, iLimit = input.Length; i < iLimit; ++i)
            {

            }

            return input;
        }

        private static ModToolsPageSectioned LoadPageSectioned (DataContainerModToolsPage pageSource)
        {
            if (pageSource == null)
                return null;

            var page = new ModToolsPageSectioned ();
            page.buttonLabel = pageSource.buttonLabel;
            page.titleIcon = pageSource.titleIcon;
            page.titleHeader = pageSource.titleHeader;
            page.titleDesc = GetTextProcessedForSkin (pageSource.titleDesc);

            if (pageSource.sections != null)
            {
                page.sections = new List<ModPageSection> ();
                foreach (var sectionSource in pageSource.sections)
                {
                    if (sectionSource == null)
                        continue;

                    var section = new ModPageSection ();
                    page.sections.Add (section);

                    section.icon = sectionSource.icon;
                    section.image = sectionSource.image;
                    section.hint = sectionSource.hint;
                    section.header = sectionSource.header;
                    section.description = GetTextProcessedForSkin (sectionSource.description);

                    section.conditionsVisible = sectionSource.conditionsVisible;
                    section.conditionsEnabled = sectionSource.conditionsEnabled;
                    section.conditionsComplete = sectionSource.conditionsComplete;

                    section.actionsOnClick = sectionSource.actionsOnClick;
                    section.buttons = sectionSource.buttons;

                    if (sectionSource.childPage != null)
                        section.childPage = LoadPageSectioned (sectionSource.childPage);
                }
            }

            return page;
        }

        public static void LoadData ()
        {
            dataLoaded = true;

            var pages = new List<ModToolsPage> ();
            foreach (var kvp in DataMultiLinkerModToolsPage.data)
            {
                var pageSource = kvp.Value;
                if (pageSource == null)
                    continue;

                var page = LoadPageSectioned (pageSource);
                pages.Add (page);
            }

            data = new ModToolsWindowSection ()
            {
                enabled = true,
                hueColor = SirenixGUIStyles.InspectorOrange,
                status = new List<ModToolsSectionStatus>
                {
                    new ModToolsSectionStatus
                    {
                        textTruePrimary = "Core installed",
                        textTrueSecondary = "Data tools available"
                    },
                    new ModToolsSectionStatus ()
                    {
                        textTruePrimary = "Assets installed",
                        textTrueSecondary = "Level & item previews available",
                        textFalsePrimary = "Assets not installed",
                        textFalseSecondary = "Complete \"Tutorials/Optional Assets\" to unlock more tools",
                        funcCheck = AssetsInstalled,
                        buttons = new List<DataBlockModToolsButton> ()
                        {
                            new DataBlockModToolsButton
                            {
                                label = "Open tutorial",
                                actionsOnClick = new List<IModToolsFunction>
                                {
                                    new ModToolsFunctionOpenTutorial (),
                                },
                            },
                        },
                    },
                    new ModToolsSectionStatus ()
                    {
                        textTruePrimary = "Main scene open",
                        textTrueSecondary = "Modding tools ready",
                        textFalsePrimary = "Incorrect scene open",
                        textFalseSecondary = "Open the main scene to access the modding tools",
                        funcCheck = SceneOpened,
                        buttons = new List<DataBlockModToolsButton>
                        {
                            new DataBlockModToolsButtonConditional
                            {
                                label = "Open main scene",
                                visibleUnavailable = true,
                                conditions = new List<IModToolsCheck>
                                {
                                    new ModToolsCheckSceneName { name = "game_main_sdk", expected = false },
                                    new ModToolsCheckSceneName { name = "game_extended_sdk", expected = false }
                                },
                                actionsOnClick = new List<IModToolsFunction>
                                {
                                    new ModToolsFunctionOpenMainScene ()
                                }
                            }
                            /*
                            new DataBlockModToolsButtonConditional
                            {
                                label = "Open main scene",
                                visibleUnavailable = false,
                                conditions = new List<IModToolsCheck>
                                {
                                    new ModToolsCheckSceneName { name = "game_main_sdk", expected = false },
                                    new ModToolsCheckSceneName { name = "game_extended_sdk", expected = false }
                                },
                                actionsOnClick = new List<IModToolsFunction>
                                {
                                    new ModToolsFunctionOpenScene { path = "Assets/Content/Scenes/game_main_sdk" }
                                }
                            },
                            new DataBlockModToolsButtonConditional
                            {
                                label = "Open extended scene",
                                visibleUnavailable = true,
                                conditions = new List<IModToolsCheck>
                                {
                                    new ModToolsCheckSceneName { name = "game_extended_sdk", expected = false }
                                },
                                actionsOnClick = new List<IModToolsFunction>
                                {
                                    new ModToolsFunctionOpenScene { path = "Assets/ContentOptional/Scenes/game_extended_sdk" }
                                }
                            }
                            */
                        }
                    }
                },
                pages = pages
            };
        }
    }

    /*
    public static class ModToolsWindowData
    {
        static ModToolsWindowData ()
        {
            var pageMainExpanded = new ModToolsPageSectioned ()
            {
                titleHeader = "Data Tools",
                titleIcon = SdfIconType.Book,
                titleDesc = "The 1 best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview. " +
                                "The 2 best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview. " +
                                "The 3 best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview. " +
                                "The 4 best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview. " +
                                "The 5 best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview. ",
                sections = new List<ModPageSection> ()
                {
                    new ModPageSection ()
                    {
                        icon = SdfIconType.Window,
                        label = "Beginner",
                        header = "Odin Attributes Overview",
                        description = "The best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin > Inspector > Attribute Overview.",
                        actionsOnClick = () => AttributesExampleWindow.OpenWindow ()
                    },
                    new ModPageSection ()
                    {
                        icon = SdfIconType.Window,
                        label = "Beginner",
                        header = "The Static Inspector",
                        description = "If you're a programmer, then you're likely going find the static inspector helpful during debugging and testing. " +
                                      "Just open up the window, and start using it! You can find the utility under 'Tools > Odin > Inspector > Static Inspector'.",
                        actionsOnClick = () => StaticInspectorWindow.InspectType (typeof (UnityEngine.Time), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete),
                    },
                    new ModPageSection ()
                    {
                        icon = SdfIconType.FolderFill,
                        label = "Advanced",
                        header = "Odin Editor Windows",
                        description = "Learn how you can use Odin to rapidly create custom Editor Windows to help organize your project data. " +
                                      "This is where Odin can really help boost your workflow.",

                        childPage = new ModToolsPageSectioned ()
                        {
                            titleHeader = "Editor Windows",
                            titleIcon = SdfIconType.Window,
                            sections = new List<ModPageSection> ()
                            {
                                new ModPageSection ()
                                {
                                    icon = SdfIconType.FolderFill,
                                    header = AssetsInstalled () ? "Package imported" : "Import package",

                                    actionsOnClick = () =>
                                    {
                                        // AssetDatabase.ImportPackage(SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows.unitypackage", true);
                                        Debug.Log ($"Downloading...");
                                    }
                                },
                                new ModPageSection ()
                                {
                                    icon = SdfIconType.FileEarmarkCodeFill,
                                    label = "Beginner",
                                    header = "Basic Odin Editor Window",
                                    description = "Inherit from OdinEditorWindow instead of EditorWindow. This will enable you to render fields, properties and methods " +
                                                  "and make editor windows using attributes, without writing any custom editor code.",
                                    conditionsEnabled = AssetsInstalled,
                                    buttons = new List<(string btnName, Action action)> ()
                                    {
                                        ("Open window", () =>
                                        {
                                            var t = AssemblyUtilities.GetTypeByCachedFullName ("Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow");
                                            if (t != null)
                                                t.GetMethod ("OpenWindow", Flags.AllMembers)?.Invoke (null, null);
                                        }),
                                        ("Open script", () =>
                                        {
                                            var path = AssetDatabase.LoadAssetAtPath<UnityEngine.Object> (SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/BasicOdinEditorExampleWindow.cs");
                                            AssetDatabase.OpenAsset (path);
                                        }),
                                    },
                                },
                                new ModPageSection ()
                                {
                                    icon = SdfIconType.FileEarmarkCodeFill,
                                    label = "Beginner",
                                    header = "Odin Menu Editor Windows",
                                    description = "Derive from OdinMenuEditorWindow to create windows that inspect a custom tree of target objects. " +
                                                  "These are great for organizing your project, and managing Scriptable Objects etc." +
                                                  " Odin itself uses this to draw its preferences window.",
                                    conditionsEnabled = AssetsInstalled,
                                    buttons = new List<(string btnName, Action action)> ()
                                    {
                                        ("Open window", () => { AssemblyUtilities.GetTypeByCachedFullName ("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample").GetMethod ("OpenWindow", Flags.AllMembers).Invoke (null, null); }),
                                        ("Open script", () => { AssetDatabase.OpenAsset (AssetDatabase.LoadAssetAtPath<UnityEngine.Object> (SirenixAssetPaths.SirenixPluginPath + "Demos/Editor Windows/Scripts/Editor/OdinMenuEditorWindowExample.cs")); }),
                                    },
                                },
                            }
                        }
                    }
                }
            };

            pageMain = new ModToolsWindowSection ()
            {
                enabled = true,
                hueColor = SirenixGUIStyles.InspectorOrange,
                status = new List<ModToolsSectionStatus>
                {
                    new ModToolsSectionStatus
                    {
                        textTruePrimary = "Core installed",
                        textTrueSecondary = "Data tools available"
                    },
                    new ModToolsSectionStatus ()
                    {
                        textTruePrimary = "Assets installed",
                        textTrueSecondary = "Level & item previews available",
                        textFalsePrimary = "Assets not installed",
                        textFalseSecondary = "Level & item previews unavailable",
                        funcCheck = AssetsInstalled
                    }
                },
                pages = new List<ModToolsPage> { pageMainExpanded }
            };
        }
    }
    */

    public sealed class ModToolsFunctionOpenTutorial : IModToolsFunction
    {
        public string pageKey = "main";
        public int tutorialSection = 0;

        public void Run ()
        {
            #if UNITY_EDITOR
            if (!EditorWindow.HasOpenInstances<ModToolsWindow> ())
            {
                return;
            }
            var window = (ModToolsWindow)EditorWindow.GetWindow (typeof(ModToolsWindow));
            if (!DataMultiLinkerModToolsPage.data.TryGetValue (pageKey, out var pageData))
            {
                return;
            }
            if (tutorialSection >= pageData.sections.Count)
            {
                return;
            }
            var page = ModToolsWindowHelper.GetData ().pages.SingleOrDefault (pg => pg.buttonLabel == pageData.buttonLabel);
            if (page == null)
            {
                return;
            }
            var sectionedPage = page as ModToolsPageSectioned;
            if (sectionedPage == null)
            {
                return;
            }

            sectionedPage.Window = window;
            sectionedPage.EnterPage ();
            var childPage = sectionedPage.sections[tutorialSection].childPage;
            childPage.Window = window;
            childPage.EnterPage ();
            #endif
        }
    }
}
