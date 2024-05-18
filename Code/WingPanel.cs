using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Math;
using WorldsEngine.Editor;

namespace wefs;

[Component]
public class WingPanel : Component, IComponentGizmoDrawer
{
    public float span;
    public Vector3 spanDirection;

    public void DrawGizmos()
    {
        
    }
}


public struct LiftCurve
{
    public float stallAngle;
    public float stallLift;
    public float normalForce;

    public static LiftCurve Default { get; } = new()
    {
        stallAngle = 14f,
        stallLift = 0.8f,
        normalForce = 1f,
    };
}