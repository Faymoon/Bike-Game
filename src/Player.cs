using Godot;
using System;

public class Player : Godot.Spatial
{
	private Game root;

	private bool arvr = false;
	private Spatial cam_origin;
	private ARVRCamera camera;

	public override void _Ready()
	{
		root = GetNode<Game>("/root/Root");

		arvr = GetViewport().Arvr;
		if (arvr)
		{
			cam_origin = GetNode<Spatial>("Spatial");
			camera = GetNode<ARVRCamera>("Spatial/ARVROrigin/ARVRCamera");
		}
	}

	public override void _Process(float delta)
	{
		if ((int)root.rpm != 0)
		{
			var curve = new Curve3D();
			curve.UpVectorEnabled = true;
			for (int i = 0; i < 15; i += 5)
			{
				curve.AddPoint(new Vector3(Translation.x, root.height.GetNoise2d(Translation.x, Translation.z - i) * 20, Translation.z - i));
			}
			var offset = ((root.rpm * (float)Math.PI * 2.0f) / 60.0f) * 0.60f * delta;
			Translation = curve.InterpolateBaked(offset);
			LookAt(Translation + curve.InterpolateBakedUpVector(offset).Rotated(Vector3.Right, -(float)Math.PI / 2), Vector3.Up);
		}
		else if (Input.IsKeyPressed((int)KeyList.Z))
		{
			var curve = new Curve3D();
			curve.UpVectorEnabled = true;
			for (int i = 0; i < 15; i += 5)
			{
				curve.AddPoint(new Vector3(Translation.x, root.height.GetNoise2d(Translation.x, Translation.z - i) * 20, Translation.z - i));
			}
			Translation = curve.InterpolateBaked(16 * delta);
			LookAt(Translation + curve.InterpolateBakedUpVector(16 * delta).Rotated(Vector3.Right, -(float)Math.PI / 2), Vector3.Up);
		}
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (!arvr)
			return;

		if (evt is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed)
			{
				cam_origin.Rotation = new Vector3(0, -camera.Rotation.y, 0);
				//origin.RotateY(-GetNode<ARVRCamera>("Spatial/ARVROrigin/ARVRCamera").Rotation.y - origin.Rotation.y);
			}
		}
	}
}
