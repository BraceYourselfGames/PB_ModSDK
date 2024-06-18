using UnityEngine;
using System.IO;
using System;
using Sirenix.OdinInspector;

public class CaptureWithAlpha2 : MonoBehaviour
{
    [Tooltip ("A folder will be created with this base name in your project root")] public string folderBaseName = "Screenshots";
    [Tooltip ("How many frames should be captured per second of game time")] public int frameRate = 24;
    [Tooltip ("How many frames should be captured before quitting")] public int framesToCapture = 24;

    public string filename = "filename";
    public bool appendDateTime = true;
    
    private GameObject whiteCamGameObject;
    private Camera whiteCam;
    private GameObject blackCamGameObject;
    private Camera blackCam;
    private Camera mainCam;
    private int screenWidth;
    private int screenHeight;
    private Texture2D textureBlack;
    private Texture2D textureWhite;
    private Texture2D textureTransparentBackground;

    void Awake ()
    {
        mainCam = gameObject.GetComponent<Camera> ();
        CreateBlackAndWhiteCameras ();
        CacheAndInitialiseFields ();
    }
    
    [Button ("Capture screenshot", ButtonSizes.Large)]
    public void SaveScreenshot ()
    {
        var path = Application.dataPath + "/../" + "Screenshots/";
        var filenameFull = "Screenshot-" + (appendDateTime ? (filename + DateTime.Now.ToString ("MM.dd.HH.mm.ss")) : filename) + ".png";
        var pathFinal = Path.Combine (path, filenameFull);
        Debug.Log ($"Saving screenshot {filenameFull}");
        
        RenderCamToTexture (blackCam, textureBlack);
        RenderCamToTexture (whiteCam, textureWhite);
        CalculateOutputTexture ();
        
        var pngShot = textureTransparentBackground.EncodeToPNG ();
        File.WriteAllBytes (pathFinal, pngShot);
    }

    void RenderCamToTexture (Camera cam, Texture2D tex)
    {
        cam.enabled = true;
        cam.Render ();
        WriteScreenImageToTexture (tex);
        cam.enabled = false;
    }

    void CreateBlackAndWhiteCameras ()
    {
        whiteCamGameObject = (GameObject) new GameObject ();
        whiteCamGameObject.name = "White Background Camera";
        whiteCam = whiteCamGameObject.AddComponent<Camera> ();
        whiteCam.CopyFrom (mainCam);
        whiteCam.backgroundColor = Color.white;
        whiteCamGameObject.transform.SetParent (gameObject.transform, true);

        blackCamGameObject = (GameObject) new GameObject ();
        blackCamGameObject.name = "Black Background Camera";
        blackCam = blackCamGameObject.AddComponent<Camera> ();
        blackCam.CopyFrom (mainCam);
        blackCam.backgroundColor = Color.black;
        blackCamGameObject.transform.SetParent (gameObject.transform, true);
    }

    void WriteScreenImageToTexture (Texture2D tex)
    {
        tex.ReadPixels (new Rect (0, 0, screenWidth, screenHeight), 0, 0);
        tex.Apply ();
    }

    void CalculateOutputTexture ()
    {
        Color color;
        for (int y = 0; y < textureTransparentBackground.height; ++y)
        {
            // each row
            for (int x = 0; x < textureTransparentBackground.width; ++x)
            {
                // each column
                float alpha = textureWhite.GetPixel (x, y).r - textureBlack.GetPixel (x, y).r;
                alpha = 1.0f - alpha;
                if (alpha == 0)
                {
                    color = Color.clear;
                }
                else
                {
                    color = textureBlack.GetPixel (x, y) / alpha;
                }

                color.a = alpha;
                textureTransparentBackground.SetPixel (x, y, color);
            }
        }
    }

    void SavePng ()
    {
        
    }

    void CacheAndInitialiseFields ()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        textureBlack = new Texture2D (screenWidth, screenHeight, TextureFormat.RGB24, false);
        textureWhite = new Texture2D (screenWidth, screenHeight, TextureFormat.RGB24, false);
        textureTransparentBackground = new Texture2D (screenWidth, screenHeight, TextureFormat.ARGB32, false);
    }
}