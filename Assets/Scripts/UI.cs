#nullable enable
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TCGE
{
    public sealed class UI : MonoBehaviour
    {
        public static void SetAngle(float angle)
        {
            var sun = RenderSettings.sun;
            if (sun != null)
            {
                var sunTransform = sun.transformHandle;
                sunTransform.rotation = Quaternion.Euler(new Vector3(angle, -60, 0));
            }
        }

        public static void SetIndirect(bool enabled)
        {
            var cam = Camera.main;
            if (cam != null && cam.TryGetComponent<ReflectionProbe>(out var probe))
            {
                probe.intensity = enabled ? 1 : 0;
            }
        }

        static bool FindVolumeProfile([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out VolumeProfile? profile)
        {
            profile = null;
            
            var volume = FindAnyObjectByType<Volume>(FindObjectsInactive.Include);
            if (volume == null)
                return false;
            
            profile = volume.profile;
            return profile != null;
        }

        public static void SetPostProcessing(bool enabled)
        {
            if (!FindVolumeProfile(out var profile))
                return;
            
            if (profile.TryGet<Bloom>(out var bloom))
            {
                bloom.active = enabled;
            }

            if (profile.TryGet<ChromaticAberration>(out var chromaticAberration))
            {
                chromaticAberration.active = enabled;
            }

            if (profile.TryGet<DepthOfField>(out var depthOfField))
            {
                depthOfField.active = enabled;
            }

            if (profile.TryGet<FilmGrain>(out var filmGrain))
            {
                filmGrain.active = enabled;
            }

            if (profile.TryGet<MotionBlur>(out var motionBlur))
            {
                motionBlur.active = enabled;
            }
        }

        [System.Diagnostics.Conditional("USING_VOLUMETRIC_FOG")]
        public static void SetVolumetricFog(bool enabled)
        {
#if USING_VOLUMETRIC_FOG
            if (!FindVolumeProfile(out var profile) || !profile.TryGet<VolumetricFogVolumeComponent>(out var volumetricFog))
                return;
            
            var cam = Camera.main;
            volumetricFog.active = enabled
               && cam != null
               && cam.TryGetComponent<Light>(out var light)
               && light.enabled;
#endif // USING_VOLUMETRIC_FOG
        }
    }
}
