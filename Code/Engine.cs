using System;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Editor;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class Engine : Component, IUpdateableComponent, ISimulatedComponent, IComponentGizmoDrawer
{
    public float throttle;

    public float maxPower = 400f;

    public Entity Rigidbody;

    public void Update()
    {
        float ti = 0f;
        if (Keyboard.KeyHeld(KeyCode.LeftShift)) ti++;
        if (Keyboard.KeyHeld(KeyCode.LeftControl)) ti--;
        this.throttle = Math.Clamp(this.throttle + (ti * Time.DeltaTime), 0f, 1f);
    }

    public void Simulate()
    {
        if (this.Rigidbody.IsValid && this.Rigidbody.TryGetComponent<RigidBody>(out var rb))
        {
            Vector3 localPos;
            if (this.Entity.Parent == this.Rigidbody)
                localPos = this.Entity.LocalTransform.Position;
            else
                localPos = this.Rigidbody.Transform.InverseTransformPoint(this.Entity.Transform.Position);

            rb.AddForceAtPositionLocal(this.Entity.Transform.Forward * (this.maxPower * this.throttle), localPos);
        }
    }

    public void DrawGizmos()
    {
        DebugShapes.DrawLine(this.Entity.Transform.Position, this.Entity.Transform.Position + this.throttle * this.Entity.Transform.Forward, new(0f, 1f, 1f, 1f));
    }
}