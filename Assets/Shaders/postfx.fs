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
    vec2 uv = fragTexCoord.xy;
    uv *=  1.0 - uv.yx;
    float vig = uv.x*uv.y * 15.0;
    vig = pow(vig, 1.0);

    vec3 screenColor = texture(texture0, fragTexCoord).rgb;
	vec3 lightColor = texture(lightTex, fragTexCoord).rgb;
    vec3 texelColor = lightColor * (screenColor + (screenColor * lightColor * 2)); //basic light
	
	finalColor = vec4(texelColor * vig, 1);
}