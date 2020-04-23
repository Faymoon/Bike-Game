using Godot;
using System;

public class Chunk : Spatial
{
	private uint size;
	private Vector2 position;
	private OpenSimplexNoise height_noise;
	private OpenSimplexNoise biome_noise;

	private static ShaderMaterial base_shader;

	public void Init(uint s, Vector2 p, OpenSimplexNoise height, OpenSimplexNoise biome)
	{
		size = s;
		position = p;
		height_noise = height;
		biome_noise = biome;
	}

	public override void _Ready()
	{
		var mesh = new PlaneMesh();
		mesh.Size = new Vector2(size, size);
		mesh.SubdivideDepth = 3;
		mesh.SubdivideWidth = 3;

		var surface_tool = new SurfaceTool();
		surface_tool.CreateFrom(mesh, 0);
		var mesh_tool = new MeshDataTool();
		mesh_tool.CreateFromSurface(surface_tool.Commit(), 0);

		for (int i = 0; i < mesh_tool.GetVertexCount(); ++i)
		{
			var vertex = mesh_tool.GetVertex(i);
			var height_noise_val = height_noise.GetNoise2dv(position + new Vector2(vertex.x, vertex.z));
			vertex.y = height_noise_val * 20;
			mesh_tool.SetVertex(i, vertex);
			var color_factor = (height_noise_val + 1) / 2.0f;
			
			if ((int)vertex.x == 0 && (int)position.x == 0)
			{
				mesh_tool.SetVertexColor(i, new Color(color_factor, color_factor / 2, 0.0f));
			}
			else
			{
				mesh_tool.SetVertexColor(i, ((biome_noise.GetNoise2dv(position + new Vector2(vertex.x, vertex.z)) > 0) ? new Color(0.0f, color_factor, 0.0f) : new Color(1.0f, color_factor, 0.0f)));
			}
		}

		var child = new MeshInstance();
		var array = new ArrayMesh();
		mesh_tool.CommitToSurface(array);
		child.Mesh = array;

		if (base_shader == null && ResourceLoader.Exists("res://assets/chunk_shader.tres"))
		{
			base_shader = ResourceLoader.Load<ShaderMaterial>("res://assets/chunk_shader.tres");
		}
		var shader = base_shader;

		child.MaterialOverride = shader;

		AddChild(child);
	}
}
