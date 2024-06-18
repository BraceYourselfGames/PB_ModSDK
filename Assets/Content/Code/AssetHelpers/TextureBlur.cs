using UnityEngine;
using System.IO;

public class TextureBlur : MonoBehaviour
{
    public Texture2D textureSource;
    public Texture2D textureMask;

    [HideInInspector]
    public Texture2D textureProcessed;

    [HideInInspector]
    public string textureProcessedName = string.Empty;

    [Range (0f, 1f)]
    public float slideFactor = 0.8f;

    [Range (1, 120)]
    public int iterations = 3;

    [Range (0.25f, 1f)]
    public float flowMultiplier = 1f;

    public bool writeResult = false;
    
    private Vector2 dirTopLeft = new Vector2 (-1, 1);
    private Vector2 dirTopMid = new Vector2 (0, 1);
    private Vector2 dirTopRight = new Vector2 (1, 1);
    private Vector2 dirLeft = new Vector2 (-1, 0);
    private Vector2 dirRight = new Vector2 (1, 0);
    private Vector2 dirDownLeft = new Vector2 (-1, -1);
    private Vector2 dirDownMid = new Vector2 (0, -1);
    private Vector2 dirDownRight = new Vector2 (1, -1);

    private int reusedIndexPoint, reusedIndexCenter, reusedIndexTopLeft, reusedIndexTopMid, reusedIndexTopRight, reusedIndexLeft, reusedIndexRight, reusedIndexDownLeft, reusedIndexDownMid, reusedIndexDownRight, reusedXFloor, reusedXCeil, reusedYFloor, reusedYCeil = 0;
    private float maskValueCenter, maskValueTopLeft, maskValueTopMid, maskValueTopRight, maskValueLeft, maskValueRight, maskValueDownLeft, maskValueDownMid, maskValueDownRight, maskValueFinalBlur, maskValueFinalVector, reusedFactorX, reusedFactorY;
    private float diffTopLeft, diffTopMid, diffTopRight, diffLeft, diffRight, diffDownLeft, diffDownMid, diffDownRight;
    private Vector2 flowPixelUV, flowDerivedXYIndexes, flow;
    private Color colorCenter, colorFinalBlur, colorFinalVector, colorReusedTopLeft, colorReusedTopRight, colorReusedDownLeft, colorReusedDownRight, colorReusedTopLerped, colorReusedDownLerped, colorReusedSmooth;

    // private Color[,] colorsBlurXY;
    [HideInInspector]
    private Color[] colorsBlur;

    [HideInInspector]
    private Color[] colorsSource;

    [HideInInspector]
    private Color[] colorsMask;

    public void Configure (Texture2D textureSource, Texture2D textureMask, int iterations, float slideFactor, float flowMultiplier, bool writeResult)
    {
        this.textureSource = textureSource;
        this.textureMask = textureMask;
        this.iterations = iterations;
        this.slideFactor = slideFactor;
        this.flowMultiplier = flowMultiplier;
        this.writeResult = writeResult;
    }

    public void PrepareForBlur ()
    {
        if (textureSource == null || textureMask == null || textureSource.width != textureMask.width || textureSource.height != textureMask.height)
            return;

        textureProcessed = new Texture2D (textureSource.width, textureSource.height, TextureFormat.ARGB32, false, false);

        colorsMask = textureMask.GetPixels ();
        colorsSource = textureSource.GetPixels ();
        colorsBlur = new Color[colorsSource.Length];
        // colorsBlurXY = new Color[textureProcessed.width, textureProcessed.height];

        for (int i = 0; i < colorsBlur.Length; i++)
            colorsBlur[i] = colorsSource[i].WithAlpha (colorsMask[i].r);

        textureProcessed.SetPixels (colorsBlur);
        textureProcessed.Apply ();
    }

    public void PerformBlur ()
    {
        if (textureProcessed == null || textureSource == null || textureMask == null || colorsBlur == null || colorsBlur.Length == 0)
            return;

        #if UNITY_EDITOR
        float progressBar = 0.0f;
        UnityEditor.EditorUtility.DisplayProgressBar ("Performing blur", "Starting blurring of " + textureProcessedName, progressBar);
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch ();
        timer.Start ();
        #endif

        for (int i = 0; i < iterations; ++i)
        {
            GetBlurByOneStep (textureProcessed);

            #if UNITY_EDITOR
            progressBar = Mathf.Min (1f, (float)(i + 1) / (float)iterations);
            int percentage = (int)(progressBar * 100f);
            string progressStr = percentage.ToString () + "% done | Step " + (i + 1).ToString () + " | " + textureProcessedName;
            UnityEditor.EditorUtility.DisplayProgressBar ("Performing blur", progressStr, progressBar);
            #endif
        }

        #if UNITY_EDITOR
        timer.Stop ();
        Debug.Log ("TextureBlur | Blur performed in " + timer.Elapsed.Milliseconds + " ms");
        UnityEditor.EditorUtility.ClearProgressBar ();
        #endif
    }

    public Texture2D FinishBlur ()
    {
        for (int i = 0; i < colorsBlur.Length; i++)
            colorsBlur[i] = colorsBlur[i].WithAlpha (colorsSource[i].a);

        textureProcessed.SetPixels (colorsBlur);
        textureProcessed.Apply ();

        if (writeResult)
            WriteResult ();

        return textureProcessed;
    }

    private void WriteResult ()
    {
        #if UNITY_EDITOR
        string pathSource = UnityEditor.AssetDatabase.GetAssetPath (textureSource.GetInstanceID ());
        string[] pathSplitFromFormat = pathSource.Split ('.');
        string pathShared = Application.dataPath + pathSplitFromFormat[0].Substring (6, pathSplitFromFormat[0].Length - 6); // Trimming unneeded "Assets" in the path

        byte[] bytesBlur = textureProcessed.EncodeToPNG (); // GetCopyWithoutAlpha (textureProcessed).EncodeToPNG ();
        string pathBlur = pathShared + "_blur_i" + iterations + "_f" + slideFactor + ".png";
        File.WriteAllBytes (pathBlur, bytesBlur);

        UnityEditor.AssetDatabase.Refresh ();
        #endif
    }


    private int GetPixelIndex (int x, int y, int width, int limit)
    {
        reusedIndexPoint = x + y * width;
        if (reusedIndexPoint > 0 && reusedIndexPoint < limit)
            return reusedIndexPoint;
        else
            return 0;
    }

    private Color GetPixelColorFromFloatXY (float x, float y, Color[] colors, int width)
    {
        reusedXFloor = Mathf.FloorToInt (x);
        reusedYFloor = Mathf.FloorToInt (y);

        reusedXCeil = Mathf.CeilToInt (x);
        reusedYCeil = Mathf.CeilToInt (y);

        colorReusedTopLeft = colors[GetPixelIndex (reusedXFloor, reusedYCeil, colors.Length, width)];
        colorReusedTopRight = colors[GetPixelIndex (reusedXCeil, reusedYCeil, colors.Length, width)];

        colorReusedDownLeft = colors[GetPixelIndex (reusedXFloor, reusedYFloor, colors.Length, width)];
        colorReusedDownRight = colors[GetPixelIndex (reusedXCeil, reusedYFloor, colors.Length, width)];

        reusedFactorX = reusedXCeil - x;
        reusedFactorY = reusedYCeil - y;

        colorReusedTopLerped = Color.Lerp (colorReusedTopLeft, colorReusedTopRight, reusedFactorX);
        colorReusedDownLerped = Color.Lerp (colorReusedDownLeft, colorReusedDownRight, reusedFactorX);
        colorReusedSmooth = Color.Lerp (colorReusedDownLerped, colorReusedTopLerped, reusedFactorY);

        return colorReusedSmooth;
    }

    public void GetBlurByOneStep (Texture2D textureProcessed)
    {
        int width = textureProcessed.width;
        int height = textureProcessed.height;
        int length = colorsBlur.Length;

        float pixelWidth = 1f / (float)width;
        float pixelHeight = 1f / (float)height;

        colorsBlur = textureProcessed.GetPixels ();
        
        int x, y = 0;
        for (int i = 0; i < length; ++i)
        {
            x = i % width;
            y = (i / width) % height;

            // Fetch colors
            maskValueCenter = colorsBlur[GetPixelIndex (x, y, width, length)].a;
            maskValueTopLeft = colorsBlur[GetPixelIndex (x - 1, y + 1, width, length)].a;
            maskValueTopMid = colorsBlur[GetPixelIndex (x, y + 1, width, length)].a;
            maskValueTopRight = colorsBlur[GetPixelIndex (x + 1, y + 1, width, length)].a;
            maskValueLeft = colorsBlur[GetPixelIndex (x - 1, y, width, length)].a;
            maskValueRight = colorsBlur[GetPixelIndex (x + 1, y, width, length)].a;
            maskValueDownLeft = colorsBlur[GetPixelIndex (x - 1, y - 1, width, length)].a;
            maskValueDownMid = colorsBlur[GetPixelIndex (x, y - 1, width, length)].a;
            maskValueDownRight = colorsBlur[GetPixelIndex (x + 1, y - 1, width, length)].a;

            // Calculate mask differences
            diffTopLeft = Mathf.Abs (maskValueCenter - maskValueTopLeft);
            diffTopMid = Mathf.Abs (maskValueCenter - maskValueTopMid);
            diffTopRight = Mathf.Abs (maskValueCenter - maskValueTopRight);
            diffLeft = Mathf.Abs (maskValueCenter - maskValueLeft);
            diffRight = Mathf.Abs (maskValueCenter - maskValueRight);
            diffDownLeft = Mathf.Abs (maskValueCenter - maskValueDownLeft);
            diffDownMid = Mathf.Abs (maskValueCenter - maskValueDownMid);
            diffDownRight = Mathf.Abs (maskValueCenter - maskValueDownRight);

            // Calculate flow
            Vector2 flow =
            (
                dirTopLeft * diffTopLeft +
                dirTopMid * diffTopMid +
                dirTopRight * diffTopRight +
                dirLeft * diffLeft +
                dirRight * diffRight +
                dirDownLeft * diffDownLeft +
                dirDownMid * diffDownMid +
                dirDownRight * diffDownRight
            );

            // Optional flow multiplication by factors from 0.1 to 1, to slow it down and smooth the result at the cost of iterations
            flow = flow * flowMultiplier;

            // Calculate UV of sampled pixel from current center pixel UV and flow vector
            flowPixelUV = new Vector2 (pixelWidth * x, pixelHeight * y) + new Vector2 (flow.x * pixelWidth, flow.y * pixelHeight);
            // flowDerivedXYIndexes = new Vector2 (x + flow.x, y + flow.y);

            // Grab a neighbour color using flow
            colorCenter = colorsBlur[GetPixelIndex (x, y, width, length)];
            colorsBlur[i] = Color.Lerp (colorCenter, textureProcessed.GetPixelBilinear (flowPixelUV.x, flowPixelUV.y), maskValueCenter * slideFactor);
            // colorsBlur[i] = Color.Lerp (colorCenter, GetPixelColorFromFloatXY (flowDerivedXYIndexes.x, flowDerivedXYIndexes.y, colorsBlur, width), maskValueCenter * slideFactor);
        }

        textureProcessed.SetPixels (colorsBlur);
        textureProcessed.Apply ();   
        
        /*     
        
        for (int y = 0; y < textureProcessed.height; y++)
        {
            for (int x = 0; x < textureProcessed.width; x++)
            {
                // Fetch colors
                maskValueCenter = textureProcessed.GetPixel (x, y).a;
                maskValueTopLeft = textureProcessed.GetPixel (x - 1, y + 1).a;
                maskValueTopMid = textureProcessed.GetPixel (x, y + 1).a;
                maskValueTopRight = textureProcessed.GetPixel (x + 1, y + 1).a;
                maskValueLeft = textureProcessed.GetPixel (x - 1, y).a;
                maskValueRight = textureProcessed.GetPixel (x + 1, y).a;
                maskValueDownLeft = textureProcessed.GetPixel (x - 1, y - 1).a;
                maskValueDownMid = textureProcessed.GetPixel (x, y - 1).a;
                maskValueDownRight = textureProcessed.GetPixel (x + 1, y - 1).a;

                // Calculate mask differences
                diffTopLeft = Mathf.Abs (maskValueCenter - maskValueTopLeft);
                diffTopMid = Mathf.Abs (maskValueCenter - maskValueTopMid);
                diffTopRight = Mathf.Abs (maskValueCenter - maskValueTopRight);
                diffLeft = Mathf.Abs (maskValueCenter - maskValueLeft);
                diffRight = Mathf.Abs (maskValueCenter - maskValueRight);
                diffDownLeft = Mathf.Abs (maskValueCenter - maskValueDownLeft);
                diffDownMid = Mathf.Abs (maskValueCenter - maskValueDownMid);
                diffDownRight = Mathf.Abs (maskValueCenter - maskValueDownRight);

                // Calculate flow
                Vector2 flow =
                (
                    dirTopLeft * diffTopLeft +
                    dirTopMid * diffTopMid +
                    dirTopRight * diffTopRight +
                    dirLeft * diffLeft +
                    dirRight * diffRight +
                    dirDownLeft * diffDownLeft +
                    dirDownMid * diffDownMid +
                    dirDownRight * diffDownRight
                );

                // Normalize flow
                // if (flow.x > 0f || flow.y > 0f)
                //     flow.Normalize ();

                flow = flow * flowMultiplier;

                // Calculate UV of sampled pixel from current center pixel UV and flow vector
                flowPixelUV = new Vector2 (pixelWidth * x, pixelHeight * y) + new Vector2 (flow.x * pixelWidth, flow.y * pixelHeight);

                // Grab a neighbour color using flow
                colorCenter = textureProcessed.GetPixel (x, y);
                colorFinalBlur = Color.Lerp (colorCenter, textureProcessed.GetPixelBilinear (flowPixelUV.x, flowPixelUV.y), maskValueCenter * slideFactor);
                // colorFinalVector = new Color (flow.x, flow.y, 0f, 1f);

                colorsBlurXY[x, y] = colorFinalBlur;
                // colorsVectors[x, y] = colorFinalVector;
            }
        }

        for (int y = 0; y < textureProcessed.height; y++)
        {
            for (int x = 0; x < textureProcessed.width; x++)
                textureProcessed.SetPixel (x, y, colorsBlurXY[x, y]);
        }
        

        // textureProcessed.SetPixels (colorsBlurLinear);
        textureProcessed.Apply ();
        return textureProcessed;
        */

        /*
        if (step % 10 == 0)
        {
            string pathSource = AssetDatabase.GetAssetPath (textureSource.GetInstanceID ());
            string[] pathSplitFromFormat = pathSource.Split ('.');
            string pathShared = Application.dataPath + pathSplitFromFormat[0].Substring (6, pathSplitFromFormat[0].Length - 6); // Trimming unneeded "Assets" in the path

            byte[] bytesBlur = GetCopyWithoutAlpha (textureProcessed).EncodeToPNG ();
            string pathBlur = pathShared + "_blur_i" + iterations + "_f" + slideFactor + "_s" + step + ".png";
            File.WriteAllBytes (pathBlur, bytesBlur);
        }
        */
    }

    public Texture2D GetCopyWithoutAlpha (Texture2D source)
    {
        Texture2D output = Instantiate (source);
        Color[] outputColors = output.GetPixels ();
        for (int i = 0; i < outputColors.Length; ++i)
            outputColors[i] = outputColors[i].WithAlpha (1f);
        output.SetPixels (outputColors);
        return output;
    }










}
