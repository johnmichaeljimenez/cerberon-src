#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D lightTex;

uniform vec4 colDiffuse;
out vec4 finalColor;

vec3 contrast(vec3 col, float amt)
{
	return vec3(
		pow(col.r, amt),
		pow(col.g, amt),
		pow(col.b, amt)
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
	
	vec4 lightColor = texture(lightTex, fragTexCoord);
	lightColor.rgb *= 2;
	lightColor.rgb = contrast(lightColor.rgb, 2);
	
	finalColor = texelColor * lightColor; //basic light
}