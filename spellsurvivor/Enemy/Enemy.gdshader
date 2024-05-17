shader_type canvas_item;

// Parms
uniform float hit = 0.0;

void vertex() {
}

void fragment() {
	COLOR = texture(TEXTURE, UV);
	COLOR.r += hit * 0.5;
	COLOR.g += hit * 0.5;
	COLOR.b += hit * 0.5;
}
