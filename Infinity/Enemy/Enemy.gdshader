shader_type canvas_item;

// Parms
uniform float hit = 0.0;

void vertex() {
}

void fragment() {
	COLOR = texture(TEXTURE, UV);
	COLOR.rgb += hit;
}
