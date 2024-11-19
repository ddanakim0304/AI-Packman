
using UnityEngine;

public interface IGhostChase
{
    void Enable();
    void Disable();
    bool IsEnabled { get; }
}