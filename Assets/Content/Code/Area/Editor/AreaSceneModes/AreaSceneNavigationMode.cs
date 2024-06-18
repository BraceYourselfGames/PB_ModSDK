using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaSceneNavigationMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Navigation;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy () { }

        public int LayerMask => AreaSceneCamera.environmentLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!AreaSceneModeHelper.DisplaySpotCursor (bb, hitInfo))
            {
                return false;
            }
            var (eventType, button) = AreaSceneModeHelper.ResolveEvent (e);
            switch (eventType)
            {
                case EventType.MouseDown:
                case EventType.ScrollWheel:
                    Edit (button);
                    return true;
            }
            return false;
        }

        public void OnHoverEnd () => bb.gizmos.cursor.HideCursor ();

        public bool HandleSceneUserInput (Event e) => false;

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            AreaSceneModeHelper.TryRebuildTerrain (bb);

            var am = bb.am;
            var hc = Handles.color;
            Handles.color = Color.white.WithAlpha (1f);
            var colorMain = new HSBColor (0.5f, 1f, 1f, 1f).ToColor ();
            var colorSecondary = new HSBColor (0.5f, 0f, 1f, 1f).ToColor ();
            var colorCulled = new HSBColor (0.55f, 0.5f, 1f, 0.5f).ToColor ();
            var planeRotation = Quaternion.Euler (90f, 0f, 0f);

            if (!AreaSceneCamera.Prepare ())
            {
                Debug.LogWarning ("Unable to display nav graph: no scene camera");
                return;
            }

            foreach (var kvp in am.navOverrides)
            {
                var index = kvp.Key;
                if (!index.IsValidIndex (am.points))
                {
                    continue;
                }

                var navOverride = kvp.Value;
                var pointPos = am.points[index].instancePosition;

                if (!AreaSceneCamera.InView (pointPos, bb.navigationInteractionDistance))
                {
                    continue;
                }
                if (AreaSceneHelper.OverlapsUI (bb, pointPos))
                {
                    continue;
                }

                var center = pointPos + Vector3.up * navOverride.offsetY;
                bb.gizmos.DrawZTestCube (center, Quaternion.identity, 0.5f, colorMain, colorCulled);
                bb.gizmos.DrawZTestRect (center, planeRotation, 1f, colorMain, colorCulled);
                bb.gizmos.DrawZTestRect (pointPos, planeRotation, 0.25f, colorSecondary, colorCulled);
            }

            if (AreaNavUtility.graph == null)
            {
                Handles.color = hc;
                return;
            }

            var graph = AreaNavUtility.graph;
            var graphSize = graph.Count;

            for (var i = 0; i < graphSize; i += 1)
            {
                var node = graph[i];
                reusedLinks = node.GetLinks ();
                if (reusedLinks == null)
                {
                    continue;
                }

                for (var n = 0; n < reusedLinks.Count; n += 1)
                {
                    var link = reusedLinks[n];
                    if (link.destinationIndex < 0 || link.destinationIndex >= graphSize)
                    {
                        continue;
                    }

                    var nodeLinkDestination = graph[link.destinationIndex];
                    var positionStart = node.GetPosition ();
                    var positionEnd = graph[link.destinationIndex].GetPosition ();

                    if (!AreaSceneCamera.InView (positionStart, bb.navigationInteractionDistance))
                    {
                        continue;
                    }
                    if (AreaSceneHelper.OverlapsUI (bb, positionStart) || AreaSceneHelper.OverlapsUI (bb, positionEnd))
                    {
                        continue;
                    }

                    if (link.type == AreaNavLinkType.Horizontal)
                    {
                        var dir = (positionEnd - positionStart).normalized;
                        var right = Vector3.Cross (dir, Vector3.up);
                        pointArrayABC[0] = positionStart;
                        pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + right * bb.navLinkSeparation;
                        pointArrayABC[2] = (positionEnd + pointArrayABC[1]) * 0.5f;

                        Handles.color = Colors.Link.Horizontal;
                        Handles.DrawAAPolyLine (3f, pointArrayABC);
                    }
                    else if (link.type == AreaNavLinkType.Diagonal)
                    {
                        var dir = (positionEnd - positionStart).normalized;
                        var right = Vector3.Cross (dir, Vector3.up);
                        pointArrayABC[0] = positionStart;
                        pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + right * bb.navLinkSeparation;
                        pointArrayABC[2] = (positionEnd + pointArrayABC[1]) * 0.5f;

                        Handles.color = Colors.Link.Diagonal;
                        Handles.DrawAAPolyLine (3f, pointArrayABC);
                    }
                    else if (link.type == AreaNavLinkType.JumpUp)
                    {
                        pointArrayABCDE[0] = positionStart;
                        pointArrayABCDE[1] = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 1.5f, 0f);
                        pointArrayABCDE[2] = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 2f, 0f);
                        pointArrayABCDE[3] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1.25f, 0f);
                        pointArrayABCDE[4] = positionEnd;

                        Handles.color = Colors.Link.JumpUp;
                        Handles.DrawAAPolyLine (3f, pointArrayABCDE);
                    }
                    else if (link.type == AreaNavLinkType.JumpDown)
                    {
                        pointArrayABCD[0] = positionStart;
                        pointArrayABCD[1] = Vector3.Lerp (positionStart, positionEnd, 0.5f) + new Vector3 (0f, 1.5f, 0f);
                        pointArrayABCD[2] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + Vector3.up;
                        pointArrayABCD[3] = positionEnd;

                        Handles.color = Colors.Link.JumpDown;
                        Handles.DrawAAPolyLine (3f, pointArrayABCD);
                    }
                    else if (link.type == AreaNavLinkType.JumpOverDrop)
                    {
                        pointArrayABC[0] = positionStart;
                        pointArrayABC[1] = (positionStart + positionEnd) * 0.5f + Vector3.up;
                        pointArrayABC[2] = positionEnd;

                        Handles.color = Colors.Link.JumpOverDrop;
                        Handles.DrawAAPolyLine (3f, pointArrayABC);
                    }
                    else if (link.type == AreaNavLinkType.JumpOverClimb)
                    {
                        pointArrayABCDE[0] = positionStart;
                        pointArrayABCDE[1] = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 3f, 0f);
                        pointArrayABCDE[2] = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 4f, 0f);
                        pointArrayABCDE[3] = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 3f, 0f);
                        pointArrayABCDE[4] = positionEnd;

                        Handles.color = Colors.Link.JumpOverClimb;
                        Handles.DrawAAPolyLine (3f, pointArrayABCDE);
                    }
                }
            }

            Handles.color = Color.white.WithAlpha (1f);
            for (var i = 0; i < graphSize; i += 1)
            {
                var pos = graph[i].GetPosition ();
                if (!AreaSceneCamera.InView (pos, bb.navigationInteractionDistance))
                {
                    continue;
                }
                if (AreaSceneHelper.OverlapsUI (bb, pos))
                {
                    continue;
                }

                Handles.SphereHandleCap (0, pos, Quaternion.identity, 0.15f, EventType.Repaint);
            }

            Handles.color = hc;
        }

        void Edit (KeyCode mouseButton)
        {
            var am = bb.am;
            var spotIndex = bb.lastSpotHovered.spotIndex;

            switch (mouseButton)
            {
                case KeyCode.Mouse0 when !am.navOverrides.ContainsKey (spotIndex):
                    am.navOverrides.Add (spotIndex, new AreaDataNavOverride
                    {
                        pivotIndex = spotIndex,
                        offsetY = 0f
                    });
                    break;
                case KeyCode.Mouse1 when am.navOverrides.ContainsKey (spotIndex):
                    am.navOverrides.Remove (spotIndex);
                    break;
                case KeyCode.PageDown:
                case KeyCode.PageUp:
                    if (am.navOverrides.ContainsKey (spotIndex))
                    {
                        var forward = mouseButton == KeyCode.PageUp;
                        var offsetYModified = Mathf.Clamp (am.navOverrides[spotIndex].offsetY + (forward ? 1f : -1f) * 0.1f, -1.5f, 1.5f);
                        am.navOverrides[spotIndex] = new AreaDataNavOverride
                        {
                            pivotIndex = spotIndex,
                            offsetY = offsetYModified
                        };
                    }
                    break;
            }
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneNavigationMode (bb);

        AreaSceneNavigationMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            Panel = new AreaSceneNavigationModePanel (bb);
        }

        readonly AreaSceneBlackboard bb;
        List<AreaNavLink> reusedLinks;

        static readonly Vector3[] pointArrayABC = new Vector3[3];
        static readonly Vector3[] pointArrayABCD = new Vector3[4];
        static readonly Vector3[] pointArrayABCDE = new Vector3[5];

            /*
            if (Contexts.sharedInstance.combat.hasPathfindingLink)
            {
                var nodes = Contexts.sharedInstance.combat.pathfindingLink.graph.nodes;
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        AreaNavNode node = nodes[i];
                        if (node.links == null)
                            continue;

                        for (int n = 0; n < node.links.Count; ++n)
                        {
                            AreaNavLink link = node.links[n];
                            if (link.destinationIndex < 0 || link.destinationIndex >= nodes.Length)
                                continue;

                            AreaNavNode nodeLinkDestination = nodes[link.destinationIndex];

                            Vector3 positionStart = (Vector3)node.position;
                            Vector3 positionEnd = (Vector3)nodes[link.destinationIndex].position;
                            Color handleColorCached = Handles.color;

                            if (link.type == AreaNavLinkType.Horizontal)
                            {
                                Handles.color = colorLinkHorizontal;
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpUp)
                            {
                                Handles.color = colorLinkJumpUp;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 1.5f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 2f, 0f);
                                Vector3 positionMidpointC = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1.25f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionMidpointC, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpDown)
                            {
                                Handles.color = colorLinkJumpDown;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.5f) + new Vector3 (0f, 1.5f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 1f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpOverDrop)
                            {
                                Handles.color = colorLinkJumpOverDrop;
                                Vector3 positionMidpoint = (positionStart + positionEnd) / 2f + new Vector3 (0f, 1f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpoint, positionEnd });
                            }
                            else if (link.type == AreaNavLinkType.JumpOverClimb)
                            {
                                Handles.color = colorLinkJumpOverClimb;
                                Vector3 positionMidpointA = Vector3.Lerp (positionStart, positionEnd, 0.25f) + new Vector3 (0f, 3f, 0f);
                                Vector3 positionMidpointB = Vector3.Lerp (positionStart, positionEnd, 0.50f) + new Vector3 (0f, 4f, 0f);
                                Vector3 positionMidpointC = Vector3.Lerp (positionStart, positionEnd, 0.75f) + new Vector3 (0f, 3f, 0f);
                                Handles.DrawAAPolyLine (5f, new Vector3[] { positionStart, positionMidpointA, positionMidpointB, positionMidpointC, positionEnd });
                            }

                            Handles.color = handleColorCached;
                        }
                    }
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        Handles.CubeHandleCap (0, am.transform.position + (Vector3)nodes[i].position, Quaternion.identity, 0.5f);
                    }
                }
            }
            */
    }
}
