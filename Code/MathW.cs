using System;
using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;

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

    public static void AddForceAtPositionLocal(this RigidBody rb, Vector3 force, Vector3 localPosition, ForceMode mode = ForceMode.Force, bool autowake = true)
    {
        // velocity = cross(angVel, offset)
        // force = cross(torque, offset)
        // torque = cross(offset, force)
        rb.AddForce(force, mode, autowake);
        rb.AddTorque(Vector3.Cross(rb.Pose.Rotation * localPosition, force), mode, autowake);
    }

    public static Vector3 Project(Vector3 vector, Vector3 line)
    {
        return vector * Vector3.Dot(vector, line);
    }

    public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 plane)
    {
        return vector - vector * Vector3.Dot(vector, plane);
    }
}
