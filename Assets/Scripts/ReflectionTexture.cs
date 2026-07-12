#nullable enable
using UnityEngine;
using UnityEngine.Rendering;

namespace TCGE
{
    [RequireComponent(typeof(ReflectionProbe))]
    public class ReflectionTexture : MonoBehaviour
    {
        static readonly int Tex = Shader.PropertyToID("_Tex");
        const int cubemapFaces = 6;

        void Start()
        {
            try
            {
                _ = RenderAsync(GetComponent<ReflectionProbe>(), destroyCancellationToken);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        static async Awaitable RenderAsync(ReflectionProbe probe,
            System.Threading.CancellationToken ct)
        {
            int renderID = probe.RenderProbe();
            Material skybox = RenderSettings.skybox;
            AsyncGPUReadbackRequest readback = default;

            while (!ct.IsCancellationRequested)
            {
                if (!probe.isActiveAndEnabled || probe.intensity <= float.Epsilon)
                {
                    SetDefault(skybox);
                    await Awaitable.NextFrameAsync();
                    continue;
                }

                if (!probe.IsFinishedRendering(renderID))
                {
                    await Awaitable.NextFrameAsync();
                    continue;
                }
                
                if (readback.done && !readback.hasError)
                {
                    Texture texture = probe.texture;
                    skybox.SetTexture(Tex, texture);
                    RenderSettings.customReflectionTexture = texture;
                    Apply(readback);
                    renderID = probe.RenderProbe();
                }

                RenderTexture realtimeTexture = probe.realtimeTexture;
                if (realtimeTexture != null)
                {
                    readback = await AsyncGPUReadback.RequestAsync(realtimeTexture, realtimeTexture.mipmapCount - 1,
                        x: 0, width: 1, y: 0, height: 1, z: 0, depth: cubemapFaces, TextureFormat.RGBAHalf);
                }
                else
                {
                    await Awaitable.NextFrameAsync();
                }
            }
        }

        static void Apply(AsyncGPUReadbackRequest readback)
        {
            System.Span<Color> colours = stackalloc Color[cubemapFaces];
            for (int i = 0; i < cubemapFaces; i++)
            {
                var data = readback.GetData<ushort>(i);
                Color colour = default;
                for (int j = 0; j < 4; j++)
                {
                    colour[j] = System.Math.Min(1, Mathf.HalfToFloat(data[j]));
                }
                
                colours[i] = colour;
            }

            RenderSettings.ambientSkyColor = colours[(int)CubemapFace.PositiveY];
            RenderSettings.ambientGroundColor = colours[(int)CubemapFace.NegativeY];
            RenderSettings.ambientEquatorColor = 0.25f
                * (colours[(int)CubemapFace.PositiveX]
                + colours[(int)CubemapFace.NegativeX]
                + colours[(int)CubemapFace.PositiveZ]
                + colours[(int)CubemapFace.NegativeZ]);
        }

        static void SetDefault(Material skybox)
        {
            skybox.SetTexture(Tex, null);
            RenderSettings.customReflectionTexture = null;
            RenderSettings.ambientSkyColor = default;
            RenderSettings.ambientEquatorColor = default;
            RenderSettings.ambientGroundColor = default;
        }
    }
}
