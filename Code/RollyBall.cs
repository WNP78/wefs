using WorldsEngine;
using WorldsEngine.ECS;
using WorldsEngine.Input;
using WorldsEngine.Math;

namespace wefs;

// Add the Component attribute to mark something as a component
// Deriving from Component is optional but gives you access to the Entity property
// ISimulatedComponent and IUpdateableComponent are interfaces that show we want to run
// stuff in Update() and Simulate() respectively
[Component]
public class RollyBall : Component, ISimulatedComponent, IUpdateableComponent
{
    private Vector3 _torqueInput;
    
    // Simulate is called by default at a fixed rate of 100hz before each physics tick
    public void Simulate()
    {
        // Unlike what you're probably used to, GetComponent is more or less free
        // Call it whenever you want :)
        var rb = Entity.GetComponent<RigidBody>();
        
        // Shuffle round the torque vector
        var torque = new Vector3(_torqueInput.z, 0.0f, -_torqueInput.x);
        rb.AddTorque(torque * 20f, ForceMode.Acceleration);
    }

    // Update is called every single frame
    public void Update()
    {
        _torqueInput = Vector3.Zero;
        
        if (Keyboard.KeyHeld(KeyCode.W))
        {
            _torqueInput.z += 1.0f;
        }
        
        if (Keyboard.KeyHeld(KeyCode.S))
        {
            _torqueInput.z -= 1.0f;
        }
        
        if (Keyboard.KeyHeld(KeyCode.A))
        {
            _torqueInput.x += 1.0f;
        }
        
        if (Keyboard.KeyHeld(KeyCode.D))
        {
            _torqueInput.x -= 1.0f;
        }
        
        // There's no camera entity, so you just set the camera position + rotation
        Camera.Main.Position = Entity.Transform.Position + new Vector3(0.0f, 3.0f, -2.0f);
        Camera.Main.Rotation =
            Quaternion.LookAt(Camera.Main.Position.DirectionTo(Entity.Transform.Position), Vector3.Up);
    }
}