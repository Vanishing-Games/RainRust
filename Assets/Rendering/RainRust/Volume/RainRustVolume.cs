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
    public EnumParameter<RainRustNoiseType> noiseType = new(RainRustNoiseType.Perlin);
    public EnumParameter<RainRustAlphaMode> alphaMode = new(RainRustAlphaMode.OneAlpha);
    public TextureParameter noiseTexture = new(null);
    public Vector4Parameter noiseTilingOffset = new(Vector4.one);
    public Vector2Parameter noiseVelocity = new(Vector2.zero);
    public FloatParameter noiseScale = new(10.0f);
    public ClampedFloatParameter noiseIntensity = new(1.0f, 0f, 10f);
    public IntParameter lightSamples = new(16);
    public ClampedFloatParameter lightIntensity = new(0f, 0f, 10f);
    public ClampedFloatParameter lightFalloffAlpha = new(0.1f, 0.001f, 1f);
    public ClampedFloatParameter lightFalloffGamma = new(2.0f, 0.001f, 10f);

    public bool IsActive() => isEnabled.value;

    public bool IsTileCompatible() => false;
}

public enum RainRustNoiseMode
{
    None,
    Texture,
    Shader,
}

public enum RainRustNoiseType
{
    Value,
    Perlin,
    Simplex,
    Voronoi,
}

public enum RainRustAlphaMode
{
    OneAlpha,
    ObjectsMaskAlpha,
    NormalizedAlpha,
}
