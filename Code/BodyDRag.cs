using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;
using WorldsEngine.Editor;
using System;
using System.Collections.Generic;
using WorldsEngine.Input;

namespace wefs;

[Component]
public class BodyDrag : Component, ISimulatedComponent
{
    public float DragForce;

    public void Simulate()
    {
        if (this.Entity.TryGetComponent<RigidBody>(out var rb))
        {
            rb.AddForce(-rb.Velocity * rb.Velocity.Length * this.DragForce, autowake: false);
        }
    }
}
