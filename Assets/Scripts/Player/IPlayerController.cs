using UnityEngine;
using System;

public interface IPlayerController
{
    event Action<bool, float> GroundedChanged;
    event Action Jumped;
    Vector2 FrameInput { get; }
}
