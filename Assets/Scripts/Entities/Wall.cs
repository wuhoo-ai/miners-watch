using UnityEngine;

namespace MinersWatch
{
    /// <summary>Wall: blocks enemy movement. Stackable.</summary>
    public class Wall : DefenseEntity
    {
        public bool BlocksMovement => !IsDestroyed;
    }
}
