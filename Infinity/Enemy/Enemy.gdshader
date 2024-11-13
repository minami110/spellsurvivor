shader_type canvas_item;
render_mode blend_mix;

// Parms
uniform float hit = 0.0;

// Varyings
uniform vec2 offset = vec2(30.0, 30.0);
uniform vec4 modulate : source_color = vec4(0, 0, 1, 1);

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	vec2 ps = TEXTURE_PIXEL_SIZE;
	vec4 shadow = vec4(modulate.rgb, texture(TEXTURE, UV - offset * ps).a * modulate.a);
	COLOR = mix(shadow, col, col.a);
	COLOR.rgb += hit;
}
