#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D lightTex;
uniform sampler2D visionTex;

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
    vig = pow(vig, 1.1);

    vec3 screenColor = texture(texture0, fragTexCoord).rgb;
	vec3 lightColor = texture(lightTex, fragTexCoord).rgb;
	vec3 visColor = texture(visionTex, fragTexCoord).rgb;
	lightColor = contrast(lightColor, 1.1);
	
    vec3 texelColor = lightColor * (screenColor + (screenColor * lightColor * 2)); //basic light with intentional overblown lights
    float lum = pow(dot(texelColor, vec3(0.299, 0.587, 0.114)), 3);
    vec3 screenGrayColor = vec3(lum, lum, lum);
	
	vec3 finColor = mix(screenGrayColor, texelColor, visColor.r);
	finalColor = vec4(finColor * vig, 1);
}