using Core;

namespace GameMain.Editor
{
    public static class GameEditorTestEvents
    {
        public struct GETE_PlayCoin : IEvent { }

        public struct GETE_PlayExplosion : IEvent { }

        public struct GETE_PlayJump : IEvent { }

        public struct GETE_PlayBgm : IEvent { }

        public struct GETE_StopBgm : IEvent { }
    }
}
