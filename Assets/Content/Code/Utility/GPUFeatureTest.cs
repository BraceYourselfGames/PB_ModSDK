using System.Text;
using UnityEngine;

public class GPUFeatureTest : MonoBehaviour
{
    // Start is called before the first frame update
    private StringBuilder builder;
    void Start()
    {
        if (builder == null)
            builder = new StringBuilder();

        builder.Clear();
        
        builder.AppendLine($"Device Model = {SystemInfo.deviceModel}");
        builder.AppendLine($"Device Name = {SystemInfo.deviceName}");
        builder.AppendLine($"Device Type = {SystemInfo.deviceType.ToString()}");
        builder.AppendLine($"Graphics Vendor = {SystemInfo.graphicsDeviceName}");
        builder.AppendLine($"Graphics Vendor = {SystemInfo.graphicsDeviceVendor}");
        builder.AppendLine($"Graphics Vendor = {SystemInfo.graphicsDeviceType}");
        builder.AppendLine($"Graphics Threading = {SystemInfo.graphicsMultiThreaded}");
        builder.AppendLine($"Graphics Shading Level = {SystemInfo.graphicsShaderLevel}");
        builder.AppendLine($"Graphics Memory Detected = {SystemInfo.graphicsMemorySize}");


        //Compute Support
        builder.AppendLine($"Support Async Compute = {SystemInfo.supportsAsyncCompute}");
        builder.AppendLine($"Support Async Readback = {SystemInfo.supportsAsyncGPUReadback}");
        builder.AppendLine($"Support Compute Shaders = {SystemInfo.supportsComputeShaders}");
        builder.AppendLine($"Support 32Bit Indexes = {SystemInfo.supports32bitsIndexBuffer}");
        builder.AppendLine($"Support Constant Set Buffer = {SystemInfo.supportsSetConstantBuffer}");
        builder.AppendLine($"Support Random Write Target = {SystemInfo.supportedRandomWriteTargetCount}");

        //Rendering Features
        builder.AppendLine($"Support Instancing = {SystemInfo.supportsInstancing}");
        builder.AppendLine($"Support Shadows = {SystemInfo.supportsShadows}");
        builder.AppendLine($"Support Conservative Raster = {SystemInfo.supportsConservativeRaster}");
        builder.AppendLine($"Support Graphics Fencing = {SystemInfo.supportsGraphicsFence}");
        builder.AppendLine($"Support Geometry Shaders = {SystemInfo.supportsGeometryShaders}");
        builder.AppendLine($"Support Mip Streams = {SystemInfo.supportsMipStreaming}");
        builder.AppendLine($"Support Motion Vectors = {SystemInfo.supportsMotionVectors}");
        builder.AppendLine($"Support 2D Array Textures = {SystemInfo.supports2DArrayTextures}");
        builder.AppendLine($"Support 2D Array Multisampling = {SystemInfo.supportsMultisampled2DArrayTextures}");
        builder.AppendLine($"Support 3D Textures = {SystemInfo.supports3DTextures}");
        builder.AppendLine($"Support Render Target blending = {SystemInfo.supportsSeparatedRenderTargetsBlend}");
        builder.AppendLine($"Supports NPOT = {SystemInfo.npotSupport}");
        builder.AppendLine($"Dynamic Uniform Array Indexing in Fragment Shaders = {SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders}");
        
        builder.AppendLine($"Unsupported Identifiers = {SystemInfo.unsupportedIdentifier}");

        Debug.LogWarning(builder.ToString());
    }
}
