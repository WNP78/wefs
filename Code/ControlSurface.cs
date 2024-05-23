using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;
using WorldsEngine.Editor;
using WorldsEngine.Input;

namespace wefs;

[Component]
public class ControlSurface : Component, IUpdateableComponent, IComponentGizmoDrawer
{
    public float startSpan;
    public float endSpan;
    public float startChord;
    public float endChord;
    public InputAxis inputAxis;
    public bool invertInput;
    public float maxDeflection;
    public float deflectionRate;

    private float _currentInput;

    public float Deflection => this._currentInput * this.maxDeflection * MathFX.DegreesToRadians;

    public bool IsPresent(float spanPosition) => spanPosition > this.startSpan && spanPosition < this.endSpan;

    public float ChordPercent(float spanPosition) => MathFX.Remap(spanPosition, this.startSpan, this.endSpan, this.startChord, this.endChord);

    public KeyCode PositiveBtn => this.inputAxis switch
    {
        InputAxis.Pitch => KeyCode.W,
        InputAxis.Roll => KeyCode.D,
        InputAxis.Yaw => KeyCode.E,
        _ => default,
    };

    public KeyCode NegativeBtn => this.inputAxis switch
    {
        InputAxis.Pitch => KeyCode.S,
        InputAxis.Roll => KeyCode.A,
        InputAxis.Yaw => KeyCode.Q,
        _ => default,
    };

    public void DrawGizmos()
    {
        if (!this.Entity.Parent.IsValid || !this.Entity.Parent.TryGetComponent<WingPanel>(out var wing)) return;

        float startSpan = MathFX.Clamp(this.startSpan, 0f, wing.span);
        float endSpan = MathFX.Clamp(this.endSpan, 0f, wing.span);
        float t = this.startSpan / wing.span;
        float chordLength = MathFX.Lerp(wing.rootChord, wing.tipChord, t);
        float startZ = wing.sweep * t - (0.5f - this.startChord) * chordLength;
        float startTrailingZ = wing.sweep * t - 0.5f * chordLength;

        t = this.endSpan / wing.span;
        chordLength = MathFX.Lerp(wing.rootChord, wing.tipChord, t);
        float endZ = wing.sweep * t - (0.5f - this.endChord) * MathFX.Lerp(wing.rootChord, wing.tipChord, t);
        float endTrailingZ = wing.sweep * t - 0.5f * chordLength;

        var axis = new Vector3(endSpan - startSpan, 0f, endZ - startZ).Normalized;
        var rotCenter = new Vector3(startSpan, 0f, startZ);
        var rotation = Quaternion.AngleAxis(this.Deflection, axis);

        Vector3 tr(Vector3 p)
        {
            p -= rotCenter;
            p = rotation * p;
            return p + rotCenter;
        }

        wing.DebugDrawLocalQuad(
            tr(new(startSpan, 0f, startTrailingZ)),
            tr(new(startSpan, 0f, startZ)),
            tr(new(endSpan, 0f, endZ)),
            tr(new(endSpan, 0f, endTrailingZ)),
            new(1f, 0.5f, 0f, 1f));
    }

    public void Update()
    {
        float input = 0f;
        if (Keyboard.KeyHeld(this.PositiveBtn)) input++;
        if (Keyboard.KeyHeld(this.NegativeBtn)) input--;

        if (this.invertInput) input = -input;

        this._currentInput = MathW.MoveTowards(this._currentInput, input, Time.DeltaTime * this.deflectionRate);

        this.DrawGizmos();
    }
}

public enum InputAxis
{
    Pitch = 0,
    Roll = 1,
    Yaw = 2,
}