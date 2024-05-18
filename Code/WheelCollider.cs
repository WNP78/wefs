using System;
using System.Reflection.PortableExecutable;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Editor;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class WheelCollider : Component, IComponentGizmoDrawer, ISimulatedComponent, IStartListener
{
    public Entity Rigidbody;
    public float Radius;
    public float TurnAngle;
    public float SpringForce;
    public float DamperForce;

    public Vector4 ForwardSlipCurve;
    public Vector4 SidewaysSlipCurve;

    public bool IsGrounded;

    private float _lastPenetration;

    public Quaternion GlobalTireRotation => this.Entity.Transform.Rotation * Quaternion.AngleAxis(this.TurnAngle, Vector3.Up);

    public void Start()
    {
        this._lastPenetration = 0f;
    }

    public void DrawGizmos()
    {
        var wheelRotation = this.GlobalTireRotation;
        var rayDir = wheelRotation * Vector3.Down;
        var position = this.Entity.Transform.Position - (rayDir * this.Radius);
        DebugShapes.DrawCircle(position + (rayDir * (this.Radius - this._lastPenetration)), this.Radius, wheelRotation * Quaternion.AngleAxis(MathF.PI / 2f, Vector3.Forward), new(0f, 1f, 0f, 1f));
        DebugShapes.DrawLine(position, position + rayDir * this.Radius * 2f, new(0f, 1f, 0f, 1f));
    }

    public void Simulate()
    {
        if (!this.Rigidbody.IsValid)
            return;
        if (!this.Rigidbody.TryGetComponent(out RigidBody rb))
            return;

        var wheelRotation = this.GlobalTireRotation;
        var rayDir = wheelRotation * Vector3.Down;
        var position = this.Entity.Transform.Position - (rayDir * this.Radius);
        if (Physics.Raycast(position, rayDir, out var hit, this.Radius * 2f, PhysicsLayerMask.Player) && hit.HitEntity != this.Rigidbody)
        {
            float penetration = (2f * this.Radius) - hit.Distance;
            float rate = (penetration - this._lastPenetration) / Time.DeltaTime;
            float force = (penetration * this.SpringForce) + (rate * this.DamperForce);
            rb.AddForceAtPosition(force * hit.Normal, hit.WorldHitPos);
            this._lastPenetration = penetration;
            this.IsGrounded = true;
        }
        else
        {
            this._lastPenetration = 0f;
            this.IsGrounded = false;
        }
    }

    private static float EvaluateSlipCurve(float slip, Vector4 curve)
    {
        return EvaluateSlipCurve(slip, curve.x, curve.y, curve.z, curve.w);
    }

    private static float EvaluateSlipCurve(float slip, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue)
    {
        float sign = MathF.Sign(slip);
        if (sign == 0f) return 0f;

        slip *= sign;

        float result;

        if (slip <= extremumSlip)
        {
            float t = slip / extremumSlip;
            result = MathFX.Lerp(t * extremumValue, extremumValue, t);
        }
        else if (slip <= asymptoteSlip)
        {
            float t = (slip - extremumSlip) / asymptoteSlip;
            // bez( e, e, a, a )
            // lerp( bez(e, e, a), bez(e, a, a) )
            // lerp( lerp( e, lerp(e, a) ), lerp( lerp(e, a), a ) )
            float l1 = MathFX.Lerp(extremumValue, MathFX.Lerp(extremumValue, asymptoteValue, t), t);
            float l2 = MathFX.Lerp(MathFX.Lerp(extremumValue, asymptoteValue, t), asymptoteValue, t);
            result = MathFX.Lerp(l1, l2, t);
        }
        else
        {
            result = asymptoteValue;
        }

        return result * sign;
    }
}