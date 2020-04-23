shader_type spatial;
//render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
// ^ default values

//uniform sampler2D colors;

varying flat vec3 color;

void vertex()
{
	color = COLOR.rgb;
	//color = texture(colors, vec2(VERTEX.x, VERTEX.z)).xyz;
}

void fragment()
{
	ALBEDO = color;
}