using WorldsEngine;
using WorldsEngine.Math;

namespace wefs;

public struct ForcePair
{
    public Vector3 Force;
    public Vector3 Torque;

    public void AddOffsetForce(Vector3 force, Vector3 offset)
    {
        this.Force += force;
        this.Torque += Vector3.Cross(offset, force);
    }

    public readonly void ApplyLocalSpace(RigidBody rb, ForceMode mode = ForceMode.Force, bool autowake = true)
    {
        var rot = rb.Pose.Rotation;
        rb.AddForce(rot * this.Force, mode, autowake);
        rb.AddTorque(rot * this.Torque, mode, autowake);
    }

    public readonly void Apply(RigidBody rb, ForceMode mode = ForceMode.Force, bool autowake = true)
    {
        rb.AddForce(this.Force, mode, autowake);
        rb.AddTorque(this.Torque, mode, autowake);
    }
}