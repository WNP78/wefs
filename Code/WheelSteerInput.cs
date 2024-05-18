
using System;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class WheelSteerInput : Component, IUpdateableComponent, IStartListener
{
    public float MaxSteerAngle;
    public float SteeringSpeed;

    private float _smoothInput;

    public void Start()
    {
        this._smoothInput = 0f;
    }

    public void Update()
    {
        if (this.Entity.TryGetComponent<WheelCollider>(out var wc))
        {
            float input = 0f;
            if (Keyboard.KeyHeld(KeyCode.Q)) input -= 1f;
            if (Keyboard.KeyHeld(KeyCode.E)) input = 1f;
            if ((input == 1f && this._smoothInput < 0f) || (input == -1f && this._smoothInput > 0f)) this._smoothInput = 0f;
            this._smoothInput = MathW.MoveTowards(this._smoothInput, input, this.SteeringSpeed * Time.DeltaTime);
            wc.TurnAngle = -MathFX.DegreesToRadians * this._smoothInput * this.MaxSteerAngle;
        }
    }
}
