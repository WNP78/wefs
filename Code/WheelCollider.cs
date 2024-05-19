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
    public float BrakeTorque;

    public float SpringForce;
    public float DamperForce;

    public float ForwardExtremumSlip = 0.4f;
    public float ForwardExtremumValue = 1f;
    public float ForwardAsymptoteSlip = 0.8f;
    public float ForwardAsymptoteValue = 0.5f;
    
    public float SidewaysExtremumSlip = 0.4f;
    public float SidewaysExtremumValue = 1f;
    public float SidewaysAsymptoteSlip = 0.8f;
    public float SidewaysAsymptoteValue = 0.5f;

    public bool IsGrounded;

    public float RotationSpeed;
    public float AngularMassDensity = 0.1f;
    public float AngularDragFactor = 0.1f;

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
            float contactForce = (penetration * this.SpringForce) + (rate * this.DamperForce);
            Vector3 totalForce = contactForce * hit.Normal;
            this._lastPenetration = penetration;
            this.IsGrounded = true;

            var velocity = rb.GetVelocityAtPoint(hit.WorldHitPos);
            if (hit.HitEntity.TryGetComponent<RigidBody>(out var hitRB))
                velocity -= hitRB.GetVelocityAtPoint(hit.WorldHitPos);
            else hitRB = null;

            var forwards = MathW.ProjectOnPlane(wheelRotation * Vector3.Forward, hit.Normal).Normalized;
            var right = Vector3.Cross(forwards, hit.Normal);

            float sidewaysSlip = Vector3.Dot(velocity, right);
            float sidewaysForce = contactForce * EvaluateSlipCurve(-sidewaysSlip, this.SidewaysExtremumSlip, this.SidewaysExtremumValue, this.SidewaysAsymptoteSlip, this.SidewaysAsymptoteValue);
            totalForce += right * sidewaysForce;

            float forwardSlip = Vector3.Dot(velocity, forwards);
            forwardSlip -= this.RotationSpeed * this.Radius;
            float forwardForce = contactForce * EvaluateSlipCurve(-forwardSlip, this.ForwardExtremumSlip, this.ForwardExtremumValue, this.ForwardAsymptoteSlip, this.ForwardAsymptoteValue);

            totalForce += forwards * forwardForce;
            this.RotationSpeed -= forwardForce * Time.DeltaTime / (this.AngularMassDensity * this.Radius); // force * radius / (density * radius^2)

            rb.AddForceAtPosition(totalForce, hit.WorldHitPos);
            hitRB?.AddForceAtPosition(-totalForce, hit.WorldHitPos);
        }
        else
        {
            this._lastPenetration = 0f;
            this.IsGrounded = false;
        }

        float brakeTorque = this.BrakeTorque + (this.AngularDragFactor * MathF.Abs(this.RotationSpeed));
        this.RotationSpeed = MathW.MoveTowards(this.RotationSpeed, 0f, brakeTorque * Time.DeltaTime);
    }

    private static float EvaluateSlipCurve(float slip, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue)
    {
        float sign = MathF.Sign(slip);
        if (sign == 0f) return 0f;

        slip *= sign;

        float result;

        // https://www.desmos.com/calculator/gtyayrytl3
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