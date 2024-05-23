using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;
using WorldsEngine.Editor;
using System;
using System.Collections.Generic;

namespace wefs;

[Component]
public class WingPanel : Component, IComponentGizmoDrawer, ISimulatedComponent, IUpdateableComponent
{
    public float span;
    public float rootChord;
    public float tipChord;
    public float sweep;
    public int physicsSamples;

    private List<(Vector3 Pos, Vector3 Vec, Vector4 Col)> debugLinesRbSpace = new();

    private List<ControlSurface> surfaces = new();

    public void DrawGizmos()
    {
        this.DebugDrawLocalQuad(new(0f, 0f, 0.5f * this.rootChord), new(0f, 0f, -0.5f * this.rootChord), new(this.span, 0f, -0.5f * this.tipChord + this.sweep), new(this.span, 0f, 0.5f * this.tipChord + this.sweep), new(0f, 1f, 0f, 1f));
    }

    public void Update()
    {
        if (!this.Entity.Parent.IsValid || !this.Entity.Parent.TryGetComponent<RigidBody>(out var rb)) return;
        var p = this.Entity.Parent.Transform;
        foreach (var (pos, vec, col) in this.debugLinesRbSpace)
        {
            var r = p.TransformPoint(pos);
            DebugShapes.DrawLine(r, r + p.TransformDirection(vec), col);
        }

        this.debugLinesRbSpace.Clear();
    }

    public void Simulate()
    {
        if (!this.Entity.Parent.IsValid || !this.Entity.Parent.TryGetComponent<RigidBody>(out var rb)) return;
        var rbLocalPose = this.Entity.LocalTransform;

        this.surfaces.Clear();
        foreach (var ent in this.Entity)
        {
            if (ent.TryGetComponent<ControlSurface>(out var surf)) this.surfaces.Add(surf);
        }

        this.debugLinesRbSpace.Clear();

        Vector3 rootQc = rbLocalPose.TransformPoint(new(0f, 0f, 0.25f * this.rootChord));
        Vector3 tipQc = rbLocalPose.TransformPoint(new(this.span, 0f, 0.25f * this.tipChord + this.sweep));
        Vector3 fwd = rbLocalPose.Forward;
        Vector3 up = rbLocalPose.Up;
        Vector3 spanVec = Vector3.Cross(fwd, up);

        float sampleWidth = this.span / this.physicsSamples;
        float sampleStart = sampleWidth * 0.5f;

        ForcePair forces = default;
        
        for (int i = 0; i < this.physicsSamples; i++)
        {
            const float AirDensity = 1.225f; // sea level

            float sampleSpan = sampleStart + sampleWidth * i;
            float sampleT = sampleSpan / this.span;

            float alphaOffset = 0f;
            for (int cs = 0; cs < this.surfaces.Count; cs++)
            {
                var surf = this.surfaces[cs];
                if (surf.IsPresent(sampleSpan))
                {
                    alphaOffset -= MathF.Sqrt(surf.ChordPercent(sampleSpan)) * surf.Deflection;
                }
            }

            var qcPos = Vector3.Lerp(rootQc, tipQc, sampleT);
            var vel = rb.GetVelocityAtPointLocal(qcPos);
            float speed = vel.Length;
            if (speed < 0.0001f) continue;
            var velNorm = vel / speed;
            var velFwd = Vector3.Dot(velNorm, fwd);
            var velUp = Vector3.Dot(velNorm, up);
            var alpha = MathF.Atan2(-velUp, velFwd);
            var (cl, cd) = LiftCurve.Default.Sample(alpha + alphaOffset);

            float forceScale = sampleWidth * MathFX.Lerp(this.rootChord, this.tipChord, sampleT) * speed * speed * AirDensity;
            Vector3 dragDir = -velNorm;
            Vector3 liftDir = Vector3.Cross(spanVec, velNorm);
            Vector3 force = liftDir * (cl * forceScale) + dragDir * (cd * forceScale);
            forces.AddOffsetForce(force, qcPos);

            this.DrawVec(qcPos, liftDir * (cl * forceScale * 0.02f), new(0f, 1f, 0f, 1f));
            this.DrawVec(qcPos, dragDir * (cd * forceScale * 0.02f), new(1f, 0f, 0f, 1f));
        }

        forces.ApplyLocalSpace(rb);
    }

    private void DrawVec(Vector3 pos, Vector3 vec, Vector4 color)
    {
        this.debugLinesRbSpace?.Add((pos, vec, color));
    }

    internal void DebugDrawLocalQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color)
    {
        var t = this.Entity.Transform;
        a = t.TransformPoint(a);
        b = t.TransformPoint(b);
        c = t.TransformPoint(c);
        d = t.TransformPoint(d);

        DebugShapes.DrawLine(a, b, color);
        DebugShapes.DrawLine(b, c, color);
        DebugShapes.DrawLine(c, d, color);
        DebugShapes.DrawLine(d, a, color);
    }
}


public struct LiftCurve
{
    public float stallAngle;
    public float stallLift;
    public float normalForce;
    public float stallDuration;
    public float zeroLiftDrag;
    public float liftDragFactor;

    public static LiftCurve Default { get; } = new()
    {
        stallAngle = 14f * MathFX.DegreesToRadians,
        stallDuration = 8.5f * MathFX.DegreesToRadians,
        stallLift = 0.8f,
        normalForce = 0.8f,
        zeroLiftDrag = 0.0015f,
        liftDragFactor = 0.012f,
    };

    public readonly (float Lift, float Drag) Sample(float alphaRadians)
    {
        float sign = 1f;
        if (alphaRadians < 0f)
        {
            sign = -1f;
            alphaRadians = -alphaRadians;
        }

        var (sin, cos) = MathF.SinCos(alphaRadians);
        var stall = MathF.SinCos(this.stallAngle);

        float stallAdditionalLift = this.stallLift - (stall.Sin * stall.Cos * this.normalForce);

        float lift = sin * cos * this.normalForce; // normal lift component
        if (alphaRadians < this.stallAngle)
        {
            // additional linear lift
            lift += alphaRadians * (stallAdditionalLift / this.stallAngle);
        }
        else if (alphaRadians < this.stallAngle + this.stallDuration)
        {
            // smooth ease
            var stallEnd = MathF.SinCos(this.stallAngle + this.stallDuration);
            // stall lift = sin * cos * nf
            // diff: nf * ( (sin * -sin) + (cos * cos) )
            // = nf * ( cos^2 - sin^2 )
            // = nf * ( cos^2 - sin^2 + (sin^2 + cos^2 - 1) )
            // = nf * ( 2cos^2 - 1 )
            // = nf * (2 * cos * cos - 1)
            float normGradStall = this.normalForce * (2f * stall.Cos * stall.Cos - 1);
            float normGradEnd = this.normalForce * (2f * stallEnd.Cos * stallEnd.Cos - 1);

            lift = MathW.Ease(this.stallLift, (stallAdditionalLift / this.stallAngle) + normGradStall, normGradEnd, stallEnd.Sin * stallEnd.Cos * this.normalForce, alphaRadians - this.stallAngle, this.stallDuration);
        }


        float anorm = alphaRadians / this.stallAngle;
        float drag = this.zeroLiftDrag + MathFX.Lerp(
            anorm * anorm * this.liftDragFactor, // lift induced
            this.normalForce * sin * sin, // normal force
            MathFX.Saturate(MathFX.Unlerp(this.stallAngle, this.stallAngle + this.stallDuration, alphaRadians)));


        return (lift * sign, drag);
    }
}