using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenu("Rain Rust/Rain Rust Volume")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public class RainRustVolume : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isEnabled = new(true);
    public EnumParameter<RainRustNoiseMode> noiseMode = new(RainRustNoiseMode.Texture);
    public EnumParameter<RainRustAlphaMode> alphaMode = new(RainRustAlphaMode.OneAlpha);
    public TextureParameter noiseTexture = new(null);
    public Vector4Parameter noiseTilingOffset = new(Vector4.one);
    public IntParameter lightSamples = new(16);
    public ClampedFloatParameter lightIntensity = new(0f, 0f, 10f);
    public ClampedFloatParameter lightFalloff = new(0.5f, 0f, 1f);

    public bool IsActive() => isEnabled.value;

    public bool IsTileCompatible() => false;
}

public enum RainRustNoiseMode
{
    None,
    Texture,
    Shader,
}

public enum RainRustAlphaMode
{
    OneAlpha,
    ObjectsMaskAlpha,
    NormalizedAlpha,
}
