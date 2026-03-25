using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RainRust.Rendering
{
    public class TextureHandlePingPong
    {
        private TextureHandle odd;
        private TextureHandle even;

        public TextureHandlePingPong(TextureHandle first, TextureHandle second)
        {
            odd = first;
            even = second;
        }

        public TextureHandle OddSource() => odd;

        public TextureHandle EvenSource() => even;

        public TextureHandle GetByStep(int step) => (step % 2 == 0) ? even : odd;
    }

    public class RainRustContextData : ContextItem
    {
        public TextureHandle mainRt;
        public TextureHandle mainDepthRt;
        public TextureHandle receiverRt;
        public TextureHandlePingPong jfaRt;
        public TextureHandle finalJfaRt;
        public TextureHandle distanceRt;
        public TextureHandle lightingRt;

        public override void Reset()
        {
            // csharpier-ignore-start
            mainRt      = TextureHandle.nullHandle;
            mainDepthRt = TextureHandle.nullHandle;
            receiverRt  = TextureHandle.nullHandle;
            jfaRt       = new TextureHandlePingPong(TextureHandle.nullHandle, TextureHandle.nullHandle);
            finalJfaRt  = TextureHandle.nullHandle;
            distanceRt  = TextureHandle.nullHandle;
            lightingRt  = TextureHandle.nullHandle;
            // csharpier-ignore-end
        }
    }
}
