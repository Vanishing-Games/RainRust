namespace GameMain.RunTime
{
    public enum PlayerControlTickOrder
    {
        InitialSet,
        CollisionStartCheck,
        DeathControl,
        StateStartSet,
        WhistleControl,
        ThrowControl,
        DashControl,
        HorizontalControl,
        GrabGravity,
        GravityControl,
        GrabJumpControl,
        JumpControl,
        CollisionEndCheck,
        StateEndSet,
    }
}
