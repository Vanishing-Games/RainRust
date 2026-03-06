namespace PlayerControlByOris
{
    public enum PlayerControlTickOrder
    {
        InitialSet,
        CollisionStartCheck,
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
