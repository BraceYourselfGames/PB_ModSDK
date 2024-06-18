using System;
using System.Collections.Generic;
using PhantomBrigade;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialArrayTester : MonoBehaviour
{
     public string key;
     
     [PropertyRange (16, 64)]
     [OnValueChanged ("ValidateResolution")]
     public int resolution = 16;

     [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, AlwaysAddDefaultValue = true)]
     public List<Material> materials;

     [PreviewField (128f, ObjectFieldAlignment.Left), HideLabel, ShowInInspector, NonSerialized]
     private Texture2D tex;
     
     [HorizontalGroup]
     [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, HideRemoveButton = true, HideAddButton = true)]
     [OnValueChanged ("Apply", true)]
     public byte[] texValues = new byte[16];
     
     [HorizontalGroup]
     [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, HideRemoveButton = true, HideAddButton = true)]
     public Color32[] texColors = new Color32[16];

     public bool raycastUsed;
     public Transform raycastRoot;
     public float raycastRadius = 50f;
     public Vector3 raycastLocalOffset;
     public Vector3 raycastRotationFrom;
     public Vector3 raycastRotationTo;

     private void ValidateResolution ()
     {
          resolution = Mathf.Clamp (resolution, 16, 64);
          resolution = CeilPower2 (resolution);
          
          Apply ();
     }

     [Button ("Apply")]
     public void Apply ()
     {
          bool texPresent = tex != null;
          if (!texPresent || tex.width != resolution)
          {
               if (texPresent)
                    DestroyImmediate (tex);
               tex = new Texture2D (resolution, 1, TextureFormat.RGB24, false, true);
          }

          if (texValues.Length != resolution)
               texValues = new byte[resolution];
          
          if (texColors.Length != resolution)
               texColors = new Color32[resolution];

          byte channelValueZero = 0;
          for (int i = 0; i < resolution; ++i)
          {
               var value = texValues[i];
               texColors[i] = new Color32 (value, channelValueZero, channelValueZero, channelValueZero);
          }
          
          tex.SetPixels32 (texColors);
          tex.Apply (false, false);
          
          if (materials.Count > 0)
          {
               foreach (var material in materials)
               {
                    if (material != null)
                         material.SetTexture (key, tex);
               }
          }
     }

     public void OnDestroy ()
     {
          if (tex != null)
               DestroyImmediate (tex);
     }
     
     public int CeilPower2 (int x)
     {
          if (x < 2)
               return 1;
          return (int) Math.Pow (2, (int) Math.Log (x-1, 2) + 1);
     }

     public void Update ()
     {
          if (raycastUsed && raycastRoot != null && raycastRadius > 0f && resolution >= 2)
          {
               var origin = raycastRoot.TransformPoint (raycastLocalOffset);
               var divider = (float)(resolution - 1);

               for (int i = 0; i < resolution; ++i)
               {
                    var interpolant = Mathf.Clamp01 ((float)i / divider);
                    var rotation = Quaternion.Euler (Vector3.Lerp (raycastRotationFrom, raycastRotationTo, interpolant));
                    var direction = rotation * Vector3.forward;
                    direction = raycastRoot.TransformDirection (direction);
 
                    var ray = new Ray (origin, direction);
                    if (Physics.Raycast (ray, out RaycastHit hit, raycastRadius, LayerMasks.environmentMask))
                    {
                         var distance = Vector3.Distance (origin, hit.point);
                         var distanceNormalized = distance / raycastRadius;
                         texValues[i] = (byte)(distanceNormalized * 255);
                         
                         Debug.DrawLine (origin, origin + direction * distance, Color.yellow);
                         Debug.DrawLine (origin + direction * distance, origin + direction * raycastRadius, Color.red);
                    }
                    else
                    {
                         texValues[i] = 255;
                         Debug.DrawLine (origin, origin + direction * raycastRadius, Color.green);
                    }

                    
               }
               
               Apply ();
          }
     }
}
