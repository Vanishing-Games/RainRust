using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RainRust.Rendering
{
    public class TextureHandlePingPong
    {
        private TextureHandle now;
        private TextureHandle pre;

        public TextureHandlePingPong(TextureHandle first, TextureHandle second)
        {
            now = first;
            pre = second;
        }

        public TextureHandle Current() => now;
        public TextureHandle Previous() => pre;
        public void Swap() => (pre, now) = (now, pre);
    }

    public class RainRustContextData : ContextItem
    {
        public TextureHandle mainRt;
        public TextureHandlePingPong jfaRt;
        public TextureHandle distanceRt;
        public TextureHandle lightingRt;

        public override void Reset()
        {
            mainRt = TextureHandle.nullHandle;
            jfaRt = new TextureHandlePingPong(TextureHandle.nullHandle, TextureHandle.nullHandle);
            distanceRt = TextureHandle.nullHandle;
            lightingRt = TextureHandle.nullHandle;
        }
    }
}
