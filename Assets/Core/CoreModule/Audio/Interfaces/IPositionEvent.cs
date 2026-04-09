using UnityEngine;

namespace Core
{
	public interface IPositionEvent : IEvent
	{
		Vector3 Position { get; }
	}
}
