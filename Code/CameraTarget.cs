using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

[Component]
public class CameraTarget : Component, IUpdateableComponent
{
    public bool Active;

    public void Update()
    {
        if (this.Active)
        {
            var t = this.Entity.Transform;
            Camera.Main.Position = t.Position;
            Camera.Main.Rotation = t.Rotation;
        }
    }
}