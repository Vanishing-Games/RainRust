using Core;
using UnityEngine;

namespace Demos.LDtkDemo.RoomManager
{
    public class RoomTransitionStartEvent : IEvent
    {
        public RoomTransitionStartEvent(Room current, Room target, Portal exit, Portal targetPortal)
        {
            CurrentRoom = current;
            TargetRoom = target;
            ExitPortal = exit;
            TargetPortal = targetPortal;
        }

        public Room CurrentRoom { get; }
        public Room TargetRoom { get; }
        public Portal ExitPortal { get; }
        public Portal TargetPortal { get; }
    }

    public class RoomTransitionEndEvent : IEvent
    {
        public RoomTransitionEndEvent(Room newRoom)
        {
            NewRoom = newRoom;
        }

        public Room NewRoom { get; }
    }
}
