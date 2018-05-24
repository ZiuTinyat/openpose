using UnityEngine;
using System.IO;
using System;

#if TRILIB_USE_DEVIL || USE_DEVIL
using DevIL;
#endif

namespace TriLib
{
    /// <summary>
    /// Represents a texture compression parameter.
    /// </summary>
    public enum TextureCompression
    {
        /// <summary>
        /// No texture compression will be applied.
        /// </summary>
        None,

        /// <summary>
        /// Normal-quality texture compression will be applied.
        /// </summary>
        NormalQuality,

        /// <summary>
        /// High-quality texture compression will be applied.
        /// </summary>
        HighQuality
    }

    /// <summary>
    /// Represents a <see cref="UnityEngine.Texture2D"/> post-loading event handle.
    /// </summary>
    public delegate void TextureLoadHandle(string sourcePath, Material material, string propertyName, Texture2D texture);

    /// <summary>
    /// Represents a  <see cref="UnityEngine.Texture2D"/> pre-loading event handle.
    /// </summary>
    public delegate void TexturePreLoadHandle(IntPtr scene, string path, string name, Material material, string propertyName, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, string basePath = null, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, bool isNormalMap = false);

    /// <summary>
    /// Represents a class to load external textures.
    /// </summary>
    public static class Texture2DUtils
    {
        /// <summary>
        /// Loads a <see cref="UnityEngine.Texture2D"/> from a local source.
        /// </summary>
        /// <param name="scene">Scene where the texture belongs.</param>
        /// <param name="path">Path to load the texture data.</param>
        /// <param name="name">Name of the <see cref="UnityEngine.Texture2D"/> to be created.</param>
        /// <param name="material"><see cref="UnityEngine.Material"/> to assign the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="propertyName"><see cref="UnityEngine.Material"/> property name to assign to the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="checkAlphaChannel">If True, checks every image pixel to determine if alpha channel is being used and sets this value.</param>
        /// <param name="textureWrapMode">Wrap mode of the <see cref="UnityEngine.Texture2D"/> to be created.</param>
        /// <param name="basePath">Base path to lookup for the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="onTextureLoaded">Event to trigger when the <see cref="UnityEngine.Texture2D"/> finishes loading.</param>
        /// <param name="textureCompression">Texture loading compression level.</param>
        /// <param name="textureFileNameWithoutExtension">Texture filename without the extension.</param>
        /// <param name="isNormalMap">Is the Texture a Normal Map?</param>
        /// <returns>The loaded <see cref="UnityEngine.Texture2D"/>.</returns> 
        public static Texture2D LoadTextureFromFile(IntPtr scene, string path, string name, Material material, string propertyName, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, string basePath = null, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, string textureFileNameWithoutExtension = null, bool isNormalMap = false)
        {
            if (scene == IntPtr.Zero || string.IsNullOrEmpty(path))
            {
                return null;
            }
            string finalPath;
            byte[] data;
            bool isRawData;
            var width = 0;
            var height = 0;
            if (!LoadEmbeddedTextureData(scene, path, out finalPath, out data, out isRawData, out width, out height))
            {
                string filename = null;
                finalPath = path;
                data = FileUtils.LoadFileData(finalPath);
                if (data.Length == 0 && basePath != null)
                {
                    finalPath = Path.Combine(basePath, path);
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0)
                {
                    filename = FileUtils.GetFilename(path);
                    finalPath = filename;
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0 && basePath != null && filename != null)
                {
                    finalPath = Path.Combine(basePath, filename);
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0)
                {
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Texture '{0}' not found", path);
#endif
                    return null;
                }
            }
            Texture2D tempTexture2D;
            if (ApplyTextureData(data, isRawData, out tempTexture2D, width, height))
            {
                return ProccessTextureData(tempTexture2D, name, material, propertyName, ref checkAlphaChannel, textureWrapMode, finalPath, onTextureLoaded, textureCompression, isNormalMap);
            }
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
            Debug.LogErrorFormat("Unable to load texture '{0}'", path);
#endif
            return null;
        }

        /// <summary>
        /// Loads a <see cref="UnityEngine.Texture2D"/> from memory.
        /// </summary>
        /// <param name="data">Texture data.</param>
        /// <param name="path">Path to load the texture data.</param>
        /// <param name="material"><see cref="UnityEngine.Material"/> to assign the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="propertyName"><see cref="UnityEngine.Material"/> property name to assign to the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="checkAlphaChannel">If True, checks every image pixel to determine if alpha channel is being used and sets this value.</param>
        /// <param name="textureWrapMode">Wrap mode of the <see cref="UnityEngine.Texture2D"/> to be created.</param>
        /// <param name="onTextureLoaded">Event to trigger when the <see cref="UnityEngine.Texture2D"/> finishes loading.</param>
        /// <param name="textureCompression">Texture loading compression level.</param>
        /// <param name="isNormalMap">Is the Texture a Normal Map?</param>
        /// <param name="isRawData">Is the data in raw format?</param> 
        /// <param name="width">Texture width.</param> 
        /// <param name="height">Texture height.</param> 
        /// <returns>The loaded <see cref="UnityEngine.Texture2D"/>.</returns> 
        public static Texture2D LoadTextureFromMemory(byte[] data, string path, Material material, string propertyName, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, bool isNormalMap = false, bool isRawData = false, int width = 0, int height = 0)
        {
            if (data.Length == 0 || string.IsNullOrEmpty(path))
            {
                return null;
            }
            Texture2D tempTexture2D;
            if (ApplyTextureData(data, isRawData, out tempTexture2D, width, height))
            {
                return ProccessTextureData(tempTexture2D, StringUtils.GenerateUniqueName(path.GetHashCode()), material, propertyName, ref checkAlphaChannel, textureWrapMode, null, onTextureLoaded, textureCompression, isNormalMap);
            }
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
            Debug.LogErrorFormat("Unable to load texture '{0}'", path);
#endif
            return null;
        }

        /// <summary>
        /// Applies the texture data.
        /// </summary>
        /// <returns><c>true</c>, if texture data was loaded, <c>false</c> otherwise.</returns>
        /// <param name="data">Data.</param>
        /// <param name="isRawData">If set to <c>true</c> is raw data.</param>
        /// <param name="tempTexture2D">Temp <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="width">Input texture width, if known.</param>
        /// <param name="height">Input texture height, if known.</param>
        public static bool ApplyTextureData(byte[] data, bool isRawData, out Texture2D tempTexture2D, int width = 0, int height = 0)
        {
            if (data.Length == 0)
            {
                tempTexture2D = null;
                return false;
            }
            if (isRawData)
            {
                try
                {
                    tempTexture2D = new Texture2D(width, height, TextureFormat.ARGB32, true);
                    tempTexture2D.LoadRawTextureData(data);
                    tempTexture2D.Apply();
                    return true;
                }
                catch
                {
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
                       Debug.LogError("Invalid embedded texture data");
#endif
                }
            }
#if (TRILIB_USE_DEVIL || USE_DEVIL) && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            return IlLoader.LoadTexture2DFromByteArray(data, data.Length, out tempTexture2D);
#else
            tempTexture2D = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            return tempTexture2D.LoadImage(data);
#endif
        }

        /// <summary>
        /// Loads the embedded texture data.
        /// </summary>
        /// <returns><c>true</c>, if embedded texture data was loaded, <c>false</c> otherwise.</returns>
        /// <param name="scene">Scene.</param>
        /// <param name="path">Path.</param>
        /// <param name="finalPath">Outputs the final path.</param>
        /// <param name="data">Outputs the data.</param>
        /// <param name="isRawData">Outputs <c>true</c> if data is uncompressed, <c>false</c> otherwise.</param>
        /// <param name="width">Outputs the texture width.</param>
        /// <param name="height">Outputs the texture height.</param>
        public static bool LoadEmbeddedTextureData(IntPtr scene, string path, out string finalPath, out byte[] data, out bool isRawData, out int width, out int height)
        {
            if (scene != IntPtr.Zero && !string.IsNullOrEmpty(path))
            {
                var texture = AssimpInterop.aiScene_GetEmbeddedTexture(scene, path);
                if (texture != IntPtr.Zero)
                {
                    isRawData = !AssimpInterop.aiMaterial_IsEmbeddedTextureCompressed(scene, texture);
                    var dataLength = AssimpInterop.aiMaterial_GetEmbeddedTextureDataSize(scene, texture, !isRawData);
                    data = AssimpInterop.aiMaterial_GetEmbeddedTextureData(scene, texture, dataLength);
                    width = AssimpInterop.aiMaterial_GetEmbeddedTextureWidth(texture);
                    height = AssimpInterop.aiMaterial_GetEmbeddedTextureHeight(texture);
                    finalPath = Path.GetFileNameWithoutExtension(path);
                    return true;
                }
            }
            finalPath = null;
            data = new byte[0];
            isRawData = false;
            width = 0;
            height = 0;
            return false;
        }

        /// <summary>
        /// Proccesses the texture.
        /// </summary>
        /// <returns>The texture.</returns>
        /// <param name="tempTexture2D">Temp <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="name">Name.</param>
        /// <param name="material">Material.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="checkAlphaChannel">Check alpha channel.</param>
        /// <param name="textureWrapMode">Texture wrap mode.</param>
        /// <param name="finalPath">Final path.</param>
        /// <param name="onTextureLoaded">On texture loaded.</param>
        /// <param name="textureCompression">Texture compression.</param>
        /// <param name="isNormalMap">If set to <c>true</c> is normal map.</param>
        public static Texture2D ProccessTextureData(Texture2D tempTexture2D, string name, Material material, string propertyName, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, string finalPath = null, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, bool isNormalMap = false)
        {
            if (tempTexture2D == null)
            {
                return null;
            }
            tempTexture2D.name = name;
            tempTexture2D.wrapMode = textureWrapMode;
            var colors = tempTexture2D.GetPixels32();
            Texture2D finalTexture2D;
            if (isNormalMap)
            {
#if UNITY_5
                finalTexture2D = new Texture2D(tempTexture2D.width, tempTexture2D.height, TextureFormat.ARGB32, true);
                for (var i = 0; i < colors.Length; i++)
                {
                    var color = colors[i];
                    color.a = color.r;
                    color.r = 0;
                    color.b = 0;
                    colors[i] = color;
                }
#else
                finalTexture2D = Texture2D.Instantiate(AssetLoader.NormalBaseTexture);
                finalTexture2D.filterMode = tempTexture2D.filterMode;
                finalTexture2D.wrapMode = tempTexture2D.wrapMode;
                finalTexture2D.Resize(tempTexture2D.width, tempTexture2D.height);
#endif
                finalTexture2D.SetPixels32(colors);
                finalTexture2D.Apply();
            }
            else

                {
                    finalTexture2D = new Texture2D(tempTexture2D.width, tempTexture2D.height, TextureFormat.ARGB32, true);
                finalTexture2D.SetPixels32(colors);
                finalTexture2D.Apply();
                if (textureCompression != TextureCompression.None)
                {
                    tempTexture2D.Compress(textureCompression == TextureCompression.HighQuality);
                }
            }
            if (checkAlphaChannel)
            {
                checkAlphaChannel = false;
                foreach (var color in colors)
                {
                    if (color.a != 255)
                    {
                        checkAlphaChannel = true;
                        break;
                    }
                }
            }
            if (material != null)
            {
                material.SetTexture(propertyName, finalTexture2D);
            }
            if (onTextureLoaded != null)
            {
                onTextureLoaded(finalPath, material, propertyName, finalTexture2D);
            }
            return finalTexture2D;
        }


        /// <summary>
        /// Scales the texture.
        /// </summary>
        /// <returns>The scaled texture.</returns>
        /// <param name="source">Source.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, true);
            var rpixels = result.GetPixels(0);
            var incX = (1.0f / (float)targetWidth);
            var incY = (1.0f / (float)targetHeight);
            for (var px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Determines if the specified number is power of two.
        /// </summary>
        /// <returns><c>true</c> if the specified number is power of two; otherwise, <c>false</c>.</returns>
        /// <param name="number">Number.</param>
        public static bool IsPowerOfTwo(float number)
        {
            var log = Mathf.Log(number, 2f);
            var pow = Mathf.Pow(2f, Mathf.Round(log));
            return pow == number;
        }
    }
}

