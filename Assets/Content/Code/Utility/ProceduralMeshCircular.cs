using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralMeshCircular : MonoBehaviour
{
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class MeshCustomTexcoordHorizontal
    {
        public bool enabled = false;

        [LabelText (" Tiles")]
        public int tiles = 1;
        
        [LabelText (" Offset")]
        public float offset = 0f;
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class MeshLoopCustomVertexColor
    {
        public bool enabled = false;
        
        [LabelText ("@(parent != null && parent.AreInParametersUsed ()) ? \"Out\" : \"\"")]
        [HorizontalGroup]
        public Color colorOut = Color.white.WithAlpha (1f);
        
        [ShowIf ("@parent != null ? parent.AreInParametersUsed () : true")]
        [LabelText (" In")]
        [HorizontalGroup]
        public Color colorIn = Color.white.WithAlpha (1f);
        
        [NonSerialized]
        public MeshLoop parent;
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class MeshLoopCustomTexcoordVertical
    {
        public bool enabled = false;

        [LabelText ("@(parent != null && parent.AreInParametersUsed ()) ? \"Out\" : \"\"")]
        [HorizontalGroup]
        public float texcoordOut = 1f;
        
        [ShowIf ("@parent != null ? parent.AreInParametersUsed () : true")]
        [LabelText (" In")]
        [HorizontalGroup]
        public float texcoordIn = 0f;
        
        [NonSerialized]
        public MeshLoop parent;
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class MeshLoopCustomTexcoordHorizontalTile
    {
        public bool enabled = false;

        [LabelText (" Tiles")]
        public int tiles = 1;
    }
    
    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)]
    public class MeshLoopCustomTexcoordHorizontalOffset
    {
        public bool enabled = false;

        [LabelText (" Offset")]
        public float offset = 0f;
    }

    [Serializable][Toggle ("enabled", CollapseOthersOnExpand = false)][LabelWidth (90f)]
    public class MeshLoop
    {
        public bool enabled = true;
        
        // [NonSerialized][ShowInInspector][ReadOnly][LabelText ("Rad. length")]
        // public float radialLength = 0f;
        
        [HorizontalGroup("Split", 0.5f, LabelWidth = 50)]
        [ShowIf ("AreInParametersUsed")]
        [LabelText (" In")]
        [BoxGroup ("Split/Radius")]
        public float radiusIn = 1;
        
        [LabelText (" Out")]
        [BoxGroup ("Split/Radius")]
        public float radiusOut = 2;

        
        [Space (4f)]
        [ShowIf ("AreInParametersUsed")]
        [LabelText (" In")]
        [BoxGroup ("Split/Height")]
        public float heightIn = 0;
        
        [LabelText (" Out")]
        [BoxGroup ("Split/Height")]
        public float heightOut = 0;

        [LabelText ("Custom vertex color")]
        public MeshLoopCustomVertexColor customVertexColor = new MeshLoopCustomVertexColor ();
        
        // [HideIf ("IsUVGlobal")]
        [LabelText ("Custom UV (vertical axis)")]
        public MeshLoopCustomTexcoordVertical customTexcoordVertical = new MeshLoopCustomTexcoordVertical ();
        
        [HideIf ("IsUVGlobal")]
        [LabelText ("Custom UV (horizontal tile count)")]
        public MeshLoopCustomTexcoordHorizontalTile customTexcoordHorizontalTile = new MeshLoopCustomTexcoordHorizontalTile ();
        
        [HideIf ("IsUVGlobal")]
        [LabelText ("Custom UV (horizontal tile offset)")]
        public MeshLoopCustomTexcoordHorizontalOffset customTexcoordHorizontalOffset = new MeshLoopCustomTexcoordHorizontalOffset ();
        
        
        
        
        [NonSerialized]
        public float radiusInFinal, radiusOutFinal = 0f;
        
        [NonSerialized]
        public float heightInFinal, heightOutFinal = 0f;
        
        [NonSerialized]
        public Color vertexColorInFinal, vertexColorOutFinal = Color.white.WithAlpha (1f);
        
        [NonSerialized]
        public float texcoordVerticalInFinal, texcoordVerticalOutFinal = 0f;
        
        [NonSerialized]
        public ProceduralMeshCircular parent;
        
        [NonSerialized]
        public int index = -1;
        
        #if UNITY_EDITOR

        private bool IsUVGlobal () =>
            parent != null && parent.customTexcoordHorizontal.enabled;

        public bool AreInParametersUsed ()
        {
            if (index <= 0 || parent == null)
                return true;
            else
                return !parent.joinLoops;
        }

        #endif
    }

    [BoxGroup ("Components")][LabelText ("Filter")]
    public MeshFilter mf;
    
    [BoxGroup ("Components")][LabelText ("Renderer")]
    public MeshRenderer mr;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    public int sideCount = 24;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    public bool joinLoops = false;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    public bool localContinuity = false;
    
    [BoxGroup ("Core")]
    [LabelText ("Bake Quadrilateral UVs")][OnValueChanged ("Rebuild")]
    public bool bakeQuadrilateralUVs = true;
    
    [BoxGroup ("Core")]
    [LabelText ("Debug Quadrilateral UVs")]
    public bool debugQuadrilateralUVs = false;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    public bool log = false;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    [Range (1f, 360f)]
    public float arc = 360f;
    
    [BoxGroup ("Core")]
    [OnValueChanged ("Rebuild")]
    [Range (-1f, 1f)]
    public float offset = 0f;
    
    [LabelText ("Custom UV (global)")]
    [OnValueChanged ("Rebuild", true)]
    public MeshCustomTexcoordHorizontal customTexcoordHorizontal = new MeshCustomTexcoordHorizontal ();

    [Space (4f)]
    [OnValueChanged ("Rebuild", true)]
    public List<MeshLoop> loops;



    [NonSerialized] private Quaternion rotationCurrent;
    [NonSerialized] private Vector3 posInCurrent;
    [NonSerialized] private Vector3 posOutCurrent;
    [NonSerialized] private Vector3 uvInCurrent;
    [NonSerialized] private Vector3 uvOutCurrent;

    [NonSerialized] private int indexForFirstVertex;
    [NonSerialized] private int indexForFirstCorner;
    [NonSerialized] private float textureCoordU;
    [NonSerialized] private bool lastSegment;

    [NonSerialized] private Vector3 posInBase;
    [NonSerialized] private Vector3 posOutBase;

    [NonSerialized] private Quaternion rotationBase;
    [NonSerialized] private Vector3 posInSecond;
    [NonSerialized] private Vector3 posOutSecond;

    [NonSerialized] private Vector3 midpointIn;
    [NonSerialized] private Vector3 midpointOut;

    [NonSerialized] private Vector3 quadPlaneY;
    [NonSerialized] private Vector3 quadPlaneX;
    [NonSerialized] private Vector3 quadPlaneNormal;

    [NonSerialized] private Vector3 diagonalA;
    [NonSerialized] private Vector3 diagonalB;
    [NonSerialized] private Vector3 diagonalBPlaneNormal;

    [NonSerialized] private Plane diagonalBPlane;
    [NonSerialized] private Vector3 diagonalIntersection;
    [NonSerialized] private bool diagonalIntersectionPossible;
    [NonSerialized] private float diagonalIntersectionDistanceIn;
    [NonSerialized] private float diagonalIntersectionDistanceOut;

    [NonSerialized] private float correctionIn;
    [NonSerialized] private float correctionOut;




    private void OnEnable ()
    {
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying)
            return;
        #endif
        
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mf != null)
            mf.sharedMesh = null;

        Rebuild ();
    }

    private void Update ()
    {
        if (!debugQuadrilateralUVs)
            return;

        Debug.DrawLine (transform.TransformPoint (posInBase), transform.TransformPoint (posOutSecond), Color.red);
        Debug.DrawLine (transform.TransformPoint (posInSecond), transform.TransformPoint (posOutBase), Color.green);
        Debug.DrawLine (transform.TransformPoint (diagonalIntersection), transform.TransformPoint (diagonalIntersection + quadPlaneNormal), Color.cyan);
    }

    [Button (ButtonSizes.Large), PropertyOrder (-1)]
    public void Rebuild ()
    {
        #if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset (gameObject))
            return;
        #endif
            
        if (mf == null)
            mf = gameObject.GetComponent<MeshFilter> ();

        if (mr == null)
            mr = gameObject.GetComponent<MeshRenderer> ();

        if (mf == null || mr == null)
            return;

        if (loops.Count == 0)
            return;

        if (sideCount < 3)
            sideCount = 3;

        Mesh mesh = mf.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh ();
            mesh.name = "procedural_mesh_circular_l" + loops.Count + "_s" + sideCount;
            mf.sharedMesh = mesh;
        }

        Vector3[] vertices = new Vector3[2 * loops.Count * (sideCount + 1)];
        int[] triangles = new int[2 * 3 * loops.Count * sideCount];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector2[] uv2 = new Vector2[vertices.Length];
        Quaternion normalRotation = Quaternion.Euler (90f, 0f, 0f);
        Color[] colors = new Color[vertices.Length];
        Color vertexColorDefault = Color.white.WithAlpha (1f);
        
        var shiftPerSide = arc / (float) sideCount;

        MeshLoop loopPrevious = null;
        for (int i = 0; i < loops.Count; ++i)
        {
            MeshLoop loop = loops[i];

            loop.index = i;
            loop.parent = this;
            loop.customVertexColor.parent = loop;
            loop.customTexcoordVertical.parent = loop;
            
            if (!loop.enabled)
                continue;

            loop.radiusInFinal = loop.radiusIn;
            loop.radiusOutFinal = loop.radiusOut;
            loop.heightInFinal = loop.heightIn;
            loop.heightOutFinal = loop.heightOut;
            loop.vertexColorInFinal = loop.customVertexColor.enabled ? loop.customVertexColor.colorIn : vertexColorDefault;
            loop.vertexColorOutFinal = loop.customVertexColor.enabled ? loop.customVertexColor.colorOut : vertexColorDefault;
            loop.texcoordVerticalInFinal = loop.customTexcoordVertical.enabled ? loop.customTexcoordVertical.texcoordIn : 0f;
            loop.texcoordVerticalOutFinal = loop.customTexcoordVertical.enabled ? loop.customTexcoordVertical.texcoordOut : 1f;

            if (loopPrevious != null)
            {
                if (joinLoops)
                {
                    loop.radiusInFinal = loopPrevious.radiusOutFinal;
                    loop.heightInFinal = loopPrevious.heightOutFinal;
                    loop.vertexColorInFinal = loopPrevious.vertexColorOutFinal;
                    loop.texcoordVerticalInFinal = loopPrevious.texcoordVerticalOutFinal;
                }
                else if (localContinuity)
                {
                    loop.radiusInFinal = loopPrevious.radiusOutFinal + loop.radiusIn;
                    loop.heightInFinal = loopPrevious.heightOutFinal + loop.heightIn;
                    loop.texcoordVerticalInFinal = loopPrevious.texcoordVerticalOutFinal + (loop.customTexcoordVertical.enabled ? loop.customTexcoordVertical.texcoordIn : 0f);
                }

                if (localContinuity)
                {
                    loop.radiusOutFinal = loop.radiusInFinal + loop.radiusOut;
                    loop.heightOutFinal = loop.heightInFinal + loop.heightOut;
                    loop.texcoordVerticalOutFinal = loop.texcoordVerticalInFinal + (loop.customTexcoordVertical.enabled ? loop.customTexcoordVertical.texcoordOut : 1f);
                }
            }
            
            // loop.radialLength = new Vector2 (loop.radiusOutFinal - loop.radiusInFinal, loop.heightOutFinal - loop.heightInFinal).magnitude;
            loopPrevious = loop;
            
            int indexOffsetForVertices = i * 2 * (sideCount + 1);
            int indexOffsetForCorners = i * 2 * 3 * sideCount;

            float circumference = 2f * Mathf.PI * ((loop.radiusInFinal + loop.radiusOutFinal) / 2f);
            float height = Mathf.Abs (loop.heightOutFinal - loop.heightInFinal);
            float width = Mathf.Abs (loop.radiusOutFinal - loop.radiusInFinal);
            float edge = Mathf.Sqrt (height * height + width * width);
            
            int uvMultiplierHorizontal = customTexcoordHorizontal.enabled ? 
                customTexcoordHorizontal.tiles : 
                loop.customTexcoordHorizontalTile.enabled ? loop.customTexcoordHorizontalTile.tiles : Mathf.RoundToInt (circumference / edge);

            if (log)
            {
                Debug.Log
                (
                    "L" + i +
                    " | Index offset for vertices: " + indexOffsetForVertices +
                    " | Index offset for triangles: " + indexOffsetForCorners +
                    " | Circumference: " + circumference + " | Edge: " + edge + " | Horizontal multiplier: " + uvMultiplierHorizontal
                );
            }

            // Figuring out affine projection compensation for quadrilaterals

            correctionIn = 1f;
            correctionOut = 1f;

            if (bakeQuadrilateralUVs)
            {
                posInBase = new Vector3 (0f, loop.heightInFinal, loop.radiusInFinal);
                posOutBase = new Vector3 (0f, loop.heightOutFinal, loop.radiusOutFinal);

                rotationBase = Quaternion.Euler (0f, arc / (float)sideCount, 0f);
                posInSecond = rotationBase * posInBase;
                posOutSecond = rotationBase * posOutBase;

                midpointIn = (posInBase + posInSecond) / 2f;
                midpointOut = (posOutBase + posOutSecond) / 2f;

                quadPlaneY = (midpointOut - midpointIn).normalized;
                quadPlaneX = (posInSecond - posInBase).normalized;
                quadPlaneNormal = Vector3.Cross (quadPlaneY, quadPlaneX);

                diagonalA = posOutSecond - posInBase;
                diagonalB = posOutBase - posInSecond;
                diagonalBPlaneNormal = Vector3.Cross (diagonalB.normalized, quadPlaneNormal);

                diagonalBPlane = new Plane (diagonalBPlaneNormal, posInSecond);
                diagonalIntersectionDistanceIn = 0f;
                diagonalIntersectionDistanceOut = 0f;
                diagonalIntersectionPossible = diagonalBPlane.Raycast (new Ray (posInBase, diagonalA.normalized), out diagonalIntersectionDistanceIn);
                if (diagonalIntersectionPossible)
                {
                    diagonalIntersection = posInBase + diagonalA.normalized * diagonalIntersectionDistanceIn;
                    diagonalIntersectionDistanceOut = diagonalA.magnitude - diagonalIntersectionDistanceIn;

                    correctionIn = (diagonalIntersectionDistanceIn + diagonalIntersectionDistanceOut) / diagonalIntersectionDistanceOut;
                    correctionOut = (diagonalIntersectionDistanceIn + diagonalIntersectionDistanceOut) / diagonalIntersectionDistanceIn;
                }
            }

            // Iterate for side count + 1 to allow for non-continuous UVs

            for (int s = 0; s < sideCount + 1; ++s)
            {
                rotationCurrent = Quaternion.Euler (0f, arc * (float)s / (float)sideCount + arc * offset, 0f);
                posInCurrent = rotationCurrent * new Vector3 (0f, loop.heightInFinal, loop.radiusInFinal);
                posOutCurrent = rotationCurrent * new Vector3 (0f, loop.heightOutFinal, loop.radiusOutFinal);

                indexForFirstVertex = indexOffsetForVertices + (s * 2);
                indexForFirstCorner = indexOffsetForCorners + (s * 2 * 3);

                textureCoordU = (float) s / (float) sideCount * uvMultiplierHorizontal;
                if (!customTexcoordHorizontal.enabled && loop.customTexcoordHorizontalOffset.enabled)
                    textureCoordU += loop.customTexcoordHorizontalOffset.offset;
                else if (customTexcoordHorizontal.enabled)
                    textureCoordU += customTexcoordHorizontal.offset;
                
                lastSegment = s == sideCount;

                vertices[indexForFirstVertex] = posInCurrent;
                vertices[indexForFirstVertex + 1] = posOutCurrent;

                normals[indexForFirstVertex] = Vector3.up;
                normals[indexForFirstVertex + 1] = Vector3.up;

                uvInCurrent = new Vector3 (textureCoordU, loop.texcoordVerticalInFinal, 1f) * correctionIn;
                uvOutCurrent = new Vector3 (textureCoordU, loop.texcoordVerticalOutFinal, 1f) * correctionOut;

                uv[indexForFirstVertex] = new Vector2 (uvInCurrent.x, uvInCurrent.y);
                uv[indexForFirstVertex + 1] = new Vector2 (uvOutCurrent.x, uvOutCurrent.y);

                uv2[indexForFirstVertex] = new Vector2 (uvInCurrent.z, 1f);
                uv2[indexForFirstVertex + 1] = new Vector2 (uvOutCurrent.z, 1f);

                colors[indexForFirstVertex] = loop.vertexColorInFinal;
                colors[indexForFirstVertex + 1] = loop.vertexColorOutFinal;

                if (!lastSegment)
                {
                    triangles[indexForFirstCorner] = indexForFirstVertex;
                    triangles[indexForFirstCorner + 1] = indexForFirstVertex + 1;
                    triangles[indexForFirstCorner + 2] = triangles[indexForFirstCorner + 3] = indexForFirstVertex + 3;
                    triangles[indexForFirstCorner + 4] = indexForFirstVertex + 2;
                    triangles[indexForFirstCorner + 5] = indexForFirstVertex;

                    if (log)
                    {
                        Debug.Log
                        (
                            "L" + i + " | S" + s +
                            " | Vertex indexes: " + indexForFirstVertex + " / " + (indexForFirstVertex + 1) +
                            " | Triangle indexes: " + triangles[indexForFirstCorner] + "-" + triangles[indexForFirstCorner + 1] + "-" + triangles[indexForFirstCorner + 2] + " / " + triangles[indexForFirstCorner + 3] + "-" + triangles[indexForFirstCorner + 4] + "-" + triangles[indexForFirstCorner + 5]
                        );
                    }
                }
                else
                {
                    if (log)
                    {
                        Debug.Log
                        (
                            "L" + i + " | S" + s +
                            " | Vertex indexes: " + indexForFirstVertex + " / " + (indexForFirstVertex + 1)
                        );
                    }
                }
            }
        }

        if (mesh.vertices != null && mesh.vertices.Length != vertices.Length)
            mesh.Clear ();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.colors = colors;

        mesh.RecalculateNormals ();
        normals = mesh.normals;

        for (int i = 0; i < loops.Count; ++i)
        {
            MeshLoop loop = loops[i];
            int indexOffsetForVertices = i * 2 * (sideCount + 1);

            if (joinLoops && i > 0)
            {
                int indexOffsetForVerticesPrevious = (i - 1) * 2 * (sideCount + 1);
                for (int s = 0; s < sideCount + 1; ++s)
                {
                    int indexForFirstVertex = indexOffsetForVertices + (s * 2);
                    int indexForFirstVertexPrevious = indexOffsetForVerticesPrevious + (s * 2);

                    normals[indexForFirstVertex] = normals[indexForFirstVertexPrevious];
                    normals[indexForFirstVertex + 1] = normals[indexForFirstVertexPrevious + 1];
                }
            }

            normals[indexOffsetForVertices + sideCount * 2] = normals[indexOffsetForVertices];
            normals[indexOffsetForVertices + sideCount * 2 + 1] = normals[indexOffsetForVertices + 1];
        }

        mesh.normals = normals;
        mesh.RecalculateTangents ();
        mesh.RecalculateBounds ();
    }

    public void OnDestroy ()
    {
        if (mf != null && mf.sharedMesh != null)
            DestroyImmediate (mf.sharedMesh);
    }
}
