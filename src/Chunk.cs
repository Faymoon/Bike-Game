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

		var biome = GetBiome(hydro_noise.GetNoise2dv(position / size), heat_noise.GetNoise2dv(position / size));

		for (int i = 0; i < mesh_tool.GetVertexCount(); ++i)
		{
			var vertex = mesh_tool.GetVertex(i);
			var height_noise_val = height_noise.GetNoise2dv(position + new Vector2(vertex.x, vertex.z));
			vertex.y = height_noise_val * 20;
			var color_factor = (height_noise_val + 1) / 2.0f;

			if ((int)vertex.x == 0 && (int)position.x == 0)
			{
				mesh_tool.SetVertexColor(i, new Color(color_factor, color_factor / 2, 0.0f));
			}
			else
			{
				mesh_tool.SetVertexColor(i, biome.ground_color * color_factor);
			}
			mesh_tool.SetVertex(i, vertex);
		}

		var child = new MeshInstance();
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

		AddChild(child);
	struct Biome
	{
		// Was a test dunno how i will add trees (TODO)
		//public float tree_spacing; // tree are calcuted every chunk so higher than size(=32) is useless 
		//public float tree_size;
		public Color ground_color;

		public static Biome GRASSLAND = new Biome() { ground_color = new Color(0, 1, 0) };
		public static Biome DESERT = new Biome() { ground_color = new Color(1, 1, 0) };
		public static Biome TUNDRA = new Biome() { ground_color = new Color(1, 1, 1) };
		public static Biome FOREST = new Biome() { ground_color = new Color(0, 0.5f, 0.2f) };

		public static Biome InterpolateBiomes(Biome r, Biome l, float factor)
		{
			return new Biome()
			{
				ground_color = r.ground_color.LinearInterpolate(l.ground_color, factor)
			};
		}
	}

	private static Biome GetBiome(float hydro, float heat)
	{
		Biome biome = new Biome();

		// use heat and hydro as cos and sin to get a value in rad (seems to work without normalizing vec2(hydro, heat) that would give possible sin and cos values)
		var value = Math.Atan2(hydro, heat);

		
		if (value <= -7 * (float)Math.PI / 8)
		{
			// major tundra minor desert
			biome = Biome.InterpolateBiomes(Biome.TUNDRA, Biome.DESERT, 0.66f);
		}
		else if (value <= -5 * (float)Math.PI / 8)
		{
			// tundra
			biome = Biome.TUNDRA;
		}
		else if (value <= -(float)Math.PI / 2)
		{
			// major tundra minor forest
			biome = Biome.InterpolateBiomes(Biome.TUNDRA, Biome.FOREST, 0.66f);
		}
		else if (value <= -3 * (float)Math.PI / 8)
		{
			// major forest minor tundra
			biome = Biome.InterpolateBiomes(Biome.FOREST, Biome.TUNDRA, 0.66f);
		}
		else if (value <= -(float)Math.PI / 8)
		{
			// forest
			biome = Biome.FOREST;
		}
		else if (value <= 0)
		{
			// major forest minor grassland
			biome = Biome.InterpolateBiomes(Biome.FOREST, Biome.GRASSLAND, 0.66f);
		}
		else if (value <= (float)Math.PI / 8)
		{
			// major grassland minor forest
			biome = Biome.InterpolateBiomes(Biome.GRASSLAND, Biome.FOREST, 0.66f);
		}
		else if (value <= 3 * (float)Math.PI / 8)
		{
			// grassland
			biome = Biome.GRASSLAND;
		}
		else if (value <= (float)Math.PI / 2)
		{
			// major grassland minor desert
			biome = Biome.InterpolateBiomes(Biome.GRASSLAND, Biome.DESERT, 0.66f);
		}
		else if (value <= 5 * (float)Math.PI / 8)
		{
			// major desert minor grassland
			biome = Biome.InterpolateBiomes(Biome.DESERT, Biome.GRASSLAND, 0.66f);
		}
		else if (value <= 7 * (float)Math.PI / 8)
		{
			// desert
			biome = Biome.DESERT;
		}
		else
		{
			// major desert minor tundra
			biome = Biome.InterpolateBiomes(Biome.DESERT, Biome.TUNDRA, 0.66f);
		}

		return biome;
	}
}
