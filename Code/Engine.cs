using System;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class Engine : Component, IUpdateableComponent, ISimulatedComponent
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
            rb.AddForce(this.Entity.Transform.Forward * (this.maxPower * this.throttle));
        }
    }
}