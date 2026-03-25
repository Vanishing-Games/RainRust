<!--
 * --------------------------------------------------------------------------------
 * Copyright (c) 2025 Vanishing Games. All Rights Reserved.
 * @Author: VanishXiao
 * @Date: 2026-03-25 21:21:56
 * @LastEditTime: 2026-03-25 21:22:16
 * --------------------------------------------------------------------------------
-->
# RainRust Lighting System

RainRust 是一个基于屏幕空间跳跃洪水算法（Jump Flood Algorithm, JFA）和光线追踪（Ray Tracing）的 2D 自定义光照系统，专为 URP（Universal Render Pipeline）设计。

## 核心原理

1.  **光源提取 (Draw Objects Pass)**：根据配置的 LayerMask 提取光源物体和接收光照的物体。
2.  **JFA 生成 (Jump Flood Pass)**：通过 JFA 算法生成光源的距离场（Distance Field）。
3.  **光线追踪 (Ray Tracing Pass)**：在屏幕空间利用距离场进行采样，模拟光线遮蔽和衰减，生成光照贴图。
4.  **最终合成 (Composition Pass)**：将 URP 渲染的内容（背景）、生成的接收者图层与光照贴图进行混合。

## 快速上手

### 1. 配置渲染特性 (Render Feature)

在 URP 的 `Universal Renderer Data` 资源中添加 `RainRust Lighting` 特性，并进行如下设置：

*   **Injection Point**: 建议设置为 `Before Rendering Opaques`。
*   **Light Sources Layer Mask**: 设置包含光源物体的图层（这些物体会产生光照，且会被 JFA 处理）。
*   **Receivers Layer Mask**: 设置接收光照的物体图层。
*   **Blend Mode**:
    *   `Additive`: 线性减淡（增加亮度）。
    *   `AlphaBlend`: 透明混合（适用于受光面遮盖背景）。
    *   `Multiply`: 正片叠底（适用于阴影或暗调效果）。

### 2. 使用材质

*   **光源物体**: 使用 URP 默认的 `Unlit` 或 `Lit` 着色器即可。
*   **接收物体**: 必须使用 `RainRust/RainRustLighting` 着色器。
    *   **注意**: 标记为 `RainRustLighting` 的物体将不再由 URP 默认流程绘制，而是由 RainRust 特性统一管理合成，从而实现自定义光照效果。

### 3. 调节参数 (Volume)

在场景中创建或使用现有的 `Global Volume`，添加 `Rain Rust Volume` 组件：

*   **Resolution Scalar**: 降采样比例，用于优化性能。
*   **Light Samples**: 光照采样数，越高光照越柔和平滑。
*   **Light Intensity**: 光照强度。
*   **Falloff Alpha/Gamma**: 控制光照随距离衰减的曲线。
*   **Noise Settings**: 支持 Texture 或 Shader 噪波，增加光照的细节和抖动感。

## 目录结构

*   `RenderFeatures/`: 包含主逻辑入口 `RainRustLighting.cs`。
*   `RenderPasses/`: 渲染管线的各个子阶段实现。
*   `Shaders/`:
    *   `RainRustLighting.shader`: 接收者使用的材质着色器。
    *   `RainRustComposition.shader`: 最终合成着色器。
    *   `RayTracing.shader`: 核心光场计算着色器。
    *   `JumpFloodAlgorithm.shader`: JFA 算法实现。

## 注意事项

*   **Depth Buffer**: 系统使用了自定义深度图来处理遮挡，确保渲染层级正确。
*   **性能**: JFA 具有固定步数，性能开销与屏幕分辨率相关。如遇卡顿，请尝试降低 Volume 中的 `Resolution Scalar`。