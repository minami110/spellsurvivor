shader_type canvas_item;
render_mode blend_mix;

// Varyings
varying vec2 vertex_world;
varying flat vec4 modulate;

void vertex() {
	vertex_world = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
	modulate = COLOR;
}

void fragment() {
	COLOR = texture(TEXTURE, UV);
	COLOR *= modulate;
}

void light() {
	// ドットのスパンを計算
	const float px = 7.0;
	vec2 v = mod(vertex_world, px) / px;
	float d = distance(v, vec2(0.5, 0.5));
	
	// ドットのサイズを調整
	d = floor(clamp(1.0 - (d / 0.3), 0.0, 1.0) * 80.0);
	
	// カゲの長さを調整 (第2引数をデカくすると短くなる)
	float a = pow(LIGHT_COLOR.r, 6);
	
	// Default Light:
	// LIGHT = vec4(LIGHT_COLOR.rgb * COLOR.rgb * LIGHT_ENERGY, LIGHT_COLOR.a);
  	
	LIGHT = vec4(0, 0, 0 ,a);
	SHADOW_MODULATE = vec4(-d * -0.3, -d * -0.9, -d * 1.0, 1);
}
