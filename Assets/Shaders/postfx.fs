#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;
uniform vec4 colDiffuse;
out vec4 finalColor;

vec3 contrast(vec3 col)
{
	float p = 1.5;

	return vec3(
		pow(col.r, p),
		pow(col.g, p),
		pow(col.b, p)
	);
}

vec3 invert(vec3 col)
{
	return vec3(
		1.0 - col.r,
		1.0 - col.g,
		1.0 - col.b
	);
}

void main() {
	vec4 texelColor = texture(texture0, fragTexCoord);

	vec2 uv = (fragTexCoord - 0.5) * 1.08;
    float dist = length(uv);
	float vignette = smoothstep(0.8, 0.6, dist);
	texelColor.rgb *= vignette;

    // texelColor.rgb = invert(texelColor.rgb);
    texelColor.rgb = contrast(texelColor.rgb) * 2;

	// float gray = dot(texelColor.rgb, vec3(0.299, 0.587, 0.114));
    // vec4 color = vec4(vec3(gray), texelColor.a);
    // finalColor = color * colDiffuse * fragColor;

	finalColor = texelColor;
}