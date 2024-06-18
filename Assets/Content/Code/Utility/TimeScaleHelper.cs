using UnityEngine;

public class TimeScaleHelper : MonoBehaviour
{
	//// Update is called once per frame
 //   private void Awake ()
 //   {
 //       Time.timeScale = 0.0001f;
 //   }

 //   private void Start ()
 //   {
 //       CombineGeometry ();
 //   }

 //   public CombineMeshes meshCombiner;
 //   bool combining = false;

 //   public void CombineGeometry ()
 //   {
 //       if (!combining && meshCombiner != null)
 //       {
 //           combining = true;
 //           GameObject holder = GameObject.FindWithTag ("SceneHolder");
 //           if (holder != null)
 //               meshCombiner.BeginRuntimeBatch (holder.transform, CombineComplete);
 //       }
 //   }

 //   public void CombineComplete ()
 //   {
 //       combining = false;
 //   }

 //   private void Update ()
 //   {
 //       if (Input.GetKeyDown (KeyCode.Alpha0))
 //       {
 //           Time.timeScale = 0.0001f;
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.Alpha1))
 //       {
 //           Time.timeScale = 1f;
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.R))
 //           SceneManager.LoadScene (0);

 //       if (Input.GetKeyDown (KeyCode.E))
 //       {
 //           Time.timeScale = Mathf.Clamp (Time.timeScale + 0.1f, 0f, 1f);
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.Q))
 //       {
 //           Time.timeScale = Mathf.Clamp (Time.timeScale - 0.1f, 0f, 1f);
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.KeypadPlus))
 //       {
 //           Time.timeScale = Mathf.Clamp (Time.timeScale + 0.01f, 0f, 1f);
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.KeypadMinus))
 //       {
 //           Time.timeScale = Mathf.Clamp (Time.timeScale - 0.01f, 0f, 1f);
 //           Debug.Log ("Time scale: " + Time.timeScale);
 //       }

 //       if (Input.GetKeyDown (KeyCode.C))
 //           StartCapture ();

 //       //if (ssaa != null && capture == true)
 //       //    Capture ();
 //   }

 //   private int frameCount = 0;
 //   public int frameCountLimit = 3;
 //   private float timeLast = 0f;
 //   public float timeThreshold = 0.01666666666f;
 //   private bool capture = false;

 //   //public SuperSampling_SSAA ssaa;

 //   private void StartCapture ()
 //   {
 //       //if (ssaa != null)
 //       //{
 //       //    Debug.Log ("Starting capture");
 //       //    capture = true;
 //       //    frameCount = 0;
 //       //    timeLast = Time.time;
 //       //}
 //   }

 //   private void Capture ()
 //   {
 //       if (frameCount < frameCountLimit)
 //       {
 //           if (Time.time - timeLast > timeThreshold)
 //           {
 //               Debug.Log ("Capturing frame " + frameCount);
 //               //ssaa.TakeHighScaledShot (1920, 1080, 2, SSAA.SSAAFilter.BilinearSharper, "D:/Screenshots/img" + frameCount.ToString ("000"), true);
 //               timeLast = Time.time;
 //               frameCount += 1;
 //           }
 //       }
 //       else
 //       {
 //           capture = false;
 //       }
 //   }
}
