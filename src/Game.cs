using Godot;
using System;

public class Game : Spatial
{
	private Godot.Object bluetooth;

	private uint chunk_size = 32;
	private OpenSimplexNoise hydro;
	private OpenSimplexNoise heat;
	public OpenSimplexNoise height;

	private System.Collections.Generic.Dictionary<(int, int), Chunk> chunks = new System.Collections.Generic.Dictionary<(int, int), Chunk>() { };
	private System.Collections.Generic.Dictionary<(int, int), Chunk> chunks_cache = new System.Collections.Generic.Dictionary<(int, int), Chunk>() { };

	private Player player;

	public float rpm = 0.0f;

	public override void _Ready()
	{
		if (Engine.HasSingleton("GodotBluetooth"))
		{

			MobileVRInterface vr = (MobileVRInterface)ARVRServer.FindInterface("Native mobile");
			if (vr != null && vr.Initialize())
			{
				GetViewport().Arvr = true;

				// My VR headset values, consider modifyong this with yours ;)
				/*
				vendor: "T.T. International"
				model: "3D VR Viewer Deluxe"
				screen_to_lens_distance: 0.063
				inter_lens_distance: 0.04
				left_eye_field_of_view_angles: 50
				left_eye_field_of_view_angles: 50
				left_eye_field_of_view_angles: 50
				left_eye_field_of_view_angles: 50
				tray_to_lens_distance: 0.035
				distortion_coefficients: 0.01
				distortion_coefficients: 0.01
				has_magnet: true
				vertical_alignment: CENTER
				primary_button: MAGNET
				*/
				vr.DisplayToLens = 6.3f;
				vr.Iod = 4;
				vr.K1 = 0.1f;
				vr.K2 = 0.1f;
			}

			bluetooth = Engine.GetSingleton("GodotBluetooth");
			bluetooth.Call("init", GetInstanceId(), true);
			GetTree().Paused = true;
			connect();
		}
		else
		{
			GetNode<Camera>("Player/Camera").Current = true;
		}

		var gen = new Random();

		hydro = new OpenSimplexNoise();
		hydro.Seed = gen.Next();
		hydro.Period = 30;
		hydro.Octaves = 1;
		
		heat = new OpenSimplexNoise();
		heat.Seed = gen.Next();
		heat.Period = 300;
		heat.Octaves = 1;

		height = new OpenSimplexNoise();
		height.Seed = gen.Next();
		height.Period = 200;
		height.Octaves = 2;
		height.Persistence = 0.5f;


		Chunk.Test(hydro, heat);
		//heat.GetImage(120 * (int)chunk_size, 120 * (int)chunk_size).SavePng("heat.png");
		//hydro.GetImage(120 * (int)chunk_size, 120 * (int)chunk_size).SavePng("hydro.png");

		var init_pos = new Vector3(-0.5f, height.GetNoise1d(16) * 20, 0);

		player = GetNode<Player>("Player");
		player.Translate(init_pos);
	}

	public void connect()
	{
		bluetooth.Call("listPairedDevices");
	}

	public void _on_single_device_found(String deviceName, String deviceAddress, String deviceID)
	{
		if (deviceName.Contains("HC-06")) // HC-06
		{
			if (Engine.HasSingleton("GodotToast"))
				Engine.GetSingleton("GodotToast").Call("sendToast", "device scanning " + deviceID);

			bluetooth.Call("connect", deviceID.ToInt());
		}
	}

	public void _on_connected(String deviceName, String deviceAddress)
	{
		GetTree().Paused = false;
	}

	public void _on_connected_error()
	{
		connect();
	}

	public void _on_disconnected()
	{
		connect();
	}

	public void _on_data_received(byte[] dataReceived)
	{
		rpm = dataReceived[0];
	}

	public override void _Process(float delta)
	{
		var player_pos = player.Translation;
		for (int i = (int)((player_pos.x / chunk_size) - 8); i < (int)((player_pos.x / chunk_size) + 8); ++i)
		{
			for (int j = (int)((player_pos.z / chunk_size) + 12); j > (int)((player_pos.z / chunk_size) - 12); --j)
			{
				if (!chunks_cache.ContainsKey((i, j)))
				{
					chunks.Add((i, j), new Chunk());
					var p = new Vector2(i, j) * chunk_size;
					chunks[(i, j)].Init(chunk_size, p, height, hydro, heat);
					chunks[(i, j)].Translate(new Vector3(p.x, 0, p.y));
					AddChild(chunks[(i, j)]);
				}
				else
				{
					chunks.Add((i, j), chunks_cache[(i, j)]);
					chunks_cache.Remove((i, j));
				}
			}
		}

		foreach (var pair in chunks_cache)
		{
			pair.Value.QueueFree();
		}
		chunks_cache.Clear();

		chunks_cache = chunks;
		chunks = new System.Collections.Generic.Dictionary<(int, int), Chunk>();
	}
}
