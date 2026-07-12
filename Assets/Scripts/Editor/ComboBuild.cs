using UnityEditor;
using System.IO;
using UnityEngine;

namespace TCGE
{
    /// <summary>
    /// <see href="https://docs.unity3d.com/Manual/webgl-texture-compression.html"/>
    /// <example>
    /// index.html
    /// <code>
    /// // choose the data file based on whether there's support for the ASTC texture compression format
    /// var dataFile = "/{{{ DATA_FILENAME }}}";
    /// var c = document.createElement("canvas");
    /// var gl = c.getContext("webgl");
    /// var gl2 = c.getContext("webgl2");
    /// if ((gl && gl.getExtension('WEBGL_compressed_texture_astc')) || (gl2 &&
    ///     gl2.getExtension('WEBGL_compressed_texture_astc'))) {
    ///     dataFile =  "/WebGL_Mobile.data";
    /// }
    ///
    /// var buildUrl = "Build";
    /// var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
    /// var config = {
    ///     dataUrl: buildUrl + dataFile,
    ///     frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
    /// #if USE_WASM
    ///     codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
    /// #endif
    /// #if MEMORY_FILENAME
    ///     memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
    /// #endif
    /// #if SYMBOLS_FILENAME
    ///     symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
    /// #endif
    ///     streamingAssetsUrl: "StreamingAssets",
    ///     companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
    ///     productName: {{{ JSON.stringify(PRODUCT_NAME) }}},
    ///     productVersion: {{{ JSON.stringify(PRODUCT_VERSION) }}},
    ///     showBanner: unityShowBanner,
    /// };
    /// </code>
    /// </example>
    /// </summary>
    public static class ComboBuild
    {
        // Define the output folder structure and names for the builds.
        const string dualBuildPath = "WebGLBuilds";
        const string desktopBuildName = "WebGL_Build";
        const string mobileBuildName = "WebGL_Mobile";
        static readonly string desktopPath = Path.Combine(dualBuildPath, desktopBuildName);
        static readonly string mobilePath = Path.Combine(dualBuildPath, mobileBuildName);
        static readonly string[] scenes = { "Assets/Scenes/Scene0.unity" };
        
        /// <summary>
        /// This creates a menu item to trigger the dual builds.
        /// </summary>
        [MenuItem("Tools/Game Build Menu/Dual Build")]
        public static void BuildGame()
        {
            // Desktop build with DXT texture compression:
            BuildPlayerOptions desktopBuildPlayerOptions = new BuildPlayerOptions();
            desktopBuildPlayerOptions.scenes = scenes;
            desktopBuildPlayerOptions.locationPathName = desktopPath;
            desktopBuildPlayerOptions.target = BuildTarget.WebGL;
            desktopBuildPlayerOptions.options = BuildOptions.Development;
            desktopBuildPlayerOptions.subtarget = (int)WebGLTextureSubtarget.DXT;
            BuildPipeline.BuildPlayer(desktopBuildPlayerOptions);

            // Mobile build with ASTC texture compression:
            BuildPlayerOptions mobileBuildPlayerOptions = new BuildPlayerOptions();
            mobileBuildPlayerOptions.scenes = scenes;
            mobileBuildPlayerOptions.locationPathName = mobilePath;
            mobileBuildPlayerOptions.target = BuildTarget.WebGL;
            mobileBuildPlayerOptions.options = BuildOptions.Development;
            mobileBuildPlayerOptions.subtarget = (int)WebGLTextureSubtarget.ASTC;
            BuildPipeline.BuildPlayer(mobileBuildPlayerOptions);

            // Copy the mobile.data file to the desktop build directory.
            FileUtil.CopyFileOrDirectory(
                Path.Combine(mobilePath, "Build", mobileBuildName + ".data"), 
                Path.Combine(desktopPath, "Build", mobileBuildName + ".data"));
        }
    }
}
