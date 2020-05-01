using Godot;
using System;


public class Chunk : MeshInstance
{
	private uint size;
	private Vector2 position;
	private OpenSimplexNoise height_noise;
	private OpenSimplexNoise hydro_noise;
	private OpenSimplexNoise heat_noise;

	private static ShaderMaterial base_shader;

	public void Init(uint s, Vector2 p, OpenSimplexNoise height, OpenSimplexNoise hydro, OpenSimplexNoise heat)
	{
		size = s;
		position = p;
		height_noise = height;
		hydro_noise = hydro;
		heat_noise = heat;
	}

	public override void _Ready()
	{
		var mesh = new PlaneMesh();
		mesh.Size = new Vector2(size, size);
		mesh.SubdivideDepth = 3;
		mesh.SubdivideWidth = (int)position.x == 0 ? (int)size : 3;

		var surface_tool = new SurfaceTool();
		surface_tool.CreateFrom(mesh, 0);
		var mesh_tool = new MeshDataTool();
		mesh_tool.CreateFromSurface(surface_tool.Commit(), 0);

		var biome = GetBiome(/*hydro_noise.GetNoise2dv(position / size), */heat_noise.GetNoise2d(position.x / size / 10.0f, position.y));

		for (int i = 0; i < mesh_tool.GetVertexCount(); ++i)
		{
			var vertex = mesh_tool.GetVertex(i);
			var vertex_global_pos = position + new Vector2(vertex.x, vertex.z);

			var height_noise_val = height_noise.GetNoise2dv(vertex_global_pos);
			vertex.y = height_noise_val * 20;
			var color_factor = (height_noise_val + 1) / 2.0f;

			var hydro_val = (int)Math.Round(hydro_noise.GetNoise2dv(vertex_global_pos));

			if ((int)vertex.x == 0 && (int)position.x == 0)
			{
				mesh_tool.SetVertexColor(i, new Color(color_factor, color_factor / 2, 0.0f));
			}
			else if (hydro_val == -1)
			{
				mesh_tool.SetVertexColor(i, biome.dry_color * color_factor);
			}
			else if (hydro_val == 1)
			{
				mesh_tool.SetVertexColor(i, biome.humid_color * color_factor);
			}
			else
			{
				mesh_tool.SetVertexColor(i, biome.ground_color * color_factor);
			}
			mesh_tool.SetVertex(i, vertex);
		}

		/*if (base_tree_mesh == null && ResourceLoader.Exists("res://assets/tree.obj"))
		{
			base_tree_mesh = ResourceLoader.Load<Mesh>("res://assets/tree.obj");
		}
		if (base_tree_material == null && ResourceLoader.Exists("res://assets/tree.tres"))
		{
			Console.WriteLine("OK");
			base_tree_material = ResourceLoader.Load<SpatialMaterial>("res://assets/tree.tres");
		}

		MultiMesh trees = new MultiMesh();
		trees.Mesh = base_tree_mesh;
		trees.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;

		if ((int)position.x == 0)
		{
			var points1 = Utility.UniformPoissonDiskSampler.SampleRectangle(-new Vector2(size, size) / 2, new Vector2(-5, size / 2.0f), biome.tree_spacing);
			var points2 = Utility.UniformPoissonDiskSampler.SampleRectangle(new Vector2(5, 0), new Vector2(size, size) / 2, biome.tree_spacing);
			trees.InstanceCount = points1.Count + points2.Count;

			int i = 0;
			foreach (var p in points1)
			{
				trees.SetInstanceTransform(i, Transform.Identity.Scaled(new Vector3(1, biome.tree_size, 1)).Translated(new Vector3(p.x, height_noise.GetNoise2dv(position + p) * 20, p.y)));
				++i;
			}

			foreach (var p in points2)
			{
				trees.SetInstanceTransform(i, Transform.Identity.Scaled(new Vector3(1, biome.tree_size, 1)).Translated(new Vector3(p.x, height_noise.GetNoise2dv(position + p) * 20, p.y)));
				++i;
			}
		}
		else
		{
			var points = Utility.UniformPoissonDiskSampler.SampleRectangle(-new Vector2(size, size) / 2, new Vector2(size, size) / 2, biome.tree_spacing);
			trees.InstanceCount = points.Count;
			int i = 0;
			foreach (var p in points)
			{
				trees.SetInstanceTransform(i, Transform.Identity.Scaled(new Vector3(1, biome.tree_size, 1)).Translated(new Vector3(p.x, height_noise.GetNoise2dv(position + p) * 20, p.y)));
				++i;
			}
		}

		MultiMeshInstance child = new MultiMeshInstance();
		child.Multimesh = trees;
		child.MaterialOverride = base_tree_material;
		AddChild(child);*/

		var array = new ArrayMesh();
		mesh_tool.CommitToSurface(array);
		Mesh = array;

		if (base_shader == null && ResourceLoader.Exists("res://assets/chunk_shader.tres"))
		{
			base_shader = ResourceLoader.Load<ShaderMaterial>("res://assets/chunk_shader.tres");
		}
		var shader = base_shader;

		MaterialOverride = shader;
	}

	struct Biome
	{
		// Was a test dunno how i will add trees (TODO)
		//public float tree_spacing; // tree are calcuted every chunk so higher than size(=32) is useless 
		//public float tree_size;
		public Color ground_color;
		public Color humid_color;
		public Color dry_color;

		//for arid/non arid implementation
		public static Biome ARID = new Biome() { ground_color = new Color(1, 0.96f, 0.6f), humid_color = new Color(0.93f, 0.62f, 0.42f), dry_color = new Color(0.95f, 0.88f, 0.66f) };
		public static Biome NON_ARID = new Biome() { ground_color = new Color(0.54f, 1, 0.18f), humid_color = new Color(0.13f, 0.55f, 0.13f), dry_color = new Color(0.78f, 0.78f, 0.45f) };

		// for Circle implementation
		/*public static Biome GRASSLAND = new Biome() { ground_color = new Color(0, 1, 0) };
		public static Biome DESERT = new Biome() { ground_color = new Color(1, 1, 0) };
		public static Biome SAVANNAH = new Biome() { ground_color = new Color(1, 0.7f, 0) };
		public static Biome FOREST = new Biome() { ground_color = new Color(0, 0.5f, 0.2f) };*/

		public static Biome InterpolateBiomes(Biome r, Biome l, float factor)
		{
			return new Biome()
			{
				ground_color = r.ground_color.LinearInterpolate(l.ground_color, factor)
			};
		}
	}

	private static Biome GetBiome(/*float hydro, */float heat)
	{
		Biome biome = new Biome();

		if (heat >= 0)
			biome = Biome.NON_ARID;
		else
			biome = Biome.ARID;


		// Circle implementation
		// use heat and hydro as cos and sin to get a value in rad (seems to work without normalizing vec2(hydro, heat) that would give possible sin and cos values)
		/*var value = Math.Atan2(hydro, heat);

		if (value <= -7 * (float)Math.PI / 10)
		{
			// major savannah minor desert
			biome = Biome.InterpolateBiomes(Biome.SAVANNAH, Biome.DESERT, 0.5f);
		}
		else if (value <= -6 * (float)Math.PI / 10)
		{
			// savannah
			biome = Biome.SAVANNAH;
		}
		/*else if (value <= -(float)Math.PI / 2)
		{
			// major savannah minor forest
			biome = Biome.InterpolateBiomes(Biome.SAVANNAH, Biome.FOREST, 0.33f);
		}
		else if (value <= -4 * (float)Math.PI / 10)
		{
			// major forest minor savannah
			biome = Biome.InterpolateBiomes(Biome.FOREST, Biome.SAVANNAH, 0.5f);
		}
		else if (value <= -(float)Math.PI / 10)
		{
			// forest
			biome = Biome.FOREST;
		}
		/*else if (value <= 0)
		{
			// major forest minor grassland
			biome = Biome.InterpolateBiomes(Biome.FOREST, Biome.GRASSLAND, 0.33f);
		}
		else if (value <= (float)Math.PI / 10)
		{
			// major grassland minor forest
			biome = Biome.InterpolateBiomes(Biome.GRASSLAND, Biome.FOREST, 0.5f);
		}
		else if (value <= 4 * (float)Math.PI / 10)
		{
			// grassland
			biome = Biome.GRASSLAND;
		}
		/*else if (value <= (float)Math.PI / 2)
		{
			// major grassland minor desert
			biome = Biome.InterpolateBiomes(Biome.GRASSLAND, Biome.DESERT, 0.33f);
		}
		else if (value <= 6 * (float)Math.PI / 10)
		{
			// major desert minor grassland
			biome = Biome.InterpolateBiomes(Biome.DESERT, Biome.GRASSLAND, 0.5f);
		}
		else if (value <= 9 * (float)Math.PI / 10)
		{
			// desert
			biome = Biome.DESERT;
		}
		else
		{
			// major desert minor savannah
			biome = Biome.InterpolateBiomes(Biome.DESERT, Biome.SAVANNAH, 0.5f);
		}*/

		return biome;
	}
}
