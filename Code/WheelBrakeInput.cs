using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class WheelBrakeInput : Component, IUpdateableComponent, IStartListener
{
    public float MaxTorque;
    public float ApplySpeed;

    private float _smoothInput;

    public void Start()
    {
        this._smoothInput = 0f;
    }

    public void Update()
    {
        if (this.Entity.TryGetComponent<WheelCollider>(out var wc))
        {
            float input = (Keyboard.KeyHeld(KeyCode.B) || Controller.ButtonHeld(ControllerButton.B)) ? 1f : 0f;
            this._smoothInput = MathW.MoveTowards(this._smoothInput, input, this.ApplySpeed * Time.DeltaTime);
            wc.BrakeTorque = this._smoothInput * this.MaxTorque;
        }
    }
}
