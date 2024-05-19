using System;
using System.Runtime.CompilerServices;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;
using static WorldsEngine.Math.MathFX;

namespace wefs;

public static class MathW
{
    public static float MoveTowards(float a, float b, float t)
    {
        float d = b - a;
        if (MathF.Abs(d) > t)
            return a + MathF.Sign(d) * t;
        else
            return b;
    }

    public static Vector3 GetVelocityAtPoint(this RigidBody rb, Vector3 worldPoint)
    {
        return rb.Velocity + Vector3.Cross(rb.AngularVelocity, worldPoint - rb.WorldSpaceCenterOfMass.Position);
    }

    public static Vector3 GetVelocityAtPointLocal(this RigidBody rb, Vector3 localOffset)
    {
        var world = rb.Velocity + Vector3.Cross(rb.AngularVelocity, rb.Pose.Rotation * (localOffset - rb.CenterOfMassLocalPose.Position));
        return rb.Pose.Rotation.Inverse * world;
    }

    public static void AddForceAtPositionLocal(this RigidBody rb, Vector3 force, Vector3 localPosition, ForceMode mode = ForceMode.Force, bool autowake = true)
    {
        var offset = rb.Pose.Rotation * (localPosition - rb.CenterOfMassLocalPose.Position);

        // velocity = cross(angVel, offset)
        // force = cross(torque, offset)
        // torque = cross(offset, force)
        rb.AddForce(force, mode, autowake);
        rb.AddTorque(Vector3.Cross(offset, force), mode, autowake);
    }

    public static Vector3 Project(Vector3 vector, Vector3 line)
    {
        return vector * Vector3.Dot(vector, line);
    }

    public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 plane)
    {
        return vector - vector * Vector3.Dot(vector, plane);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Bezier(float a, float b, float c, float t)
    {
        return Lerp(Lerp(a, b, t), Lerp(b, c, t), t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Bezier(float a, float b, float c, float d, float t)
    {
        return Lerp(Bezier(a, b, c, t), Bezier(b, c, d, t), t);
    }

    public static float Ease(float start, float startGrad, float end, float endGrad, float t, float maxT)
    {
        float k = maxT / 3f;
        return Bezier(start, start + k * startGrad, end - k * endGrad, end, t / maxT);
    }
}
