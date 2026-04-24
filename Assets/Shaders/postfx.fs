#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D lightTex;
uniform sampler2D visionTex;

uniform vec4 colDiffuse;
out vec4 finalColor;

const int bayer4x4[16] = int[16](
     0,  8,  2, 10,
    12,  4, 14,  6,
     3, 11,  1,  9,
    15,  7, 13,  5
);

float bayer4x4_lookup(int x, int y)
{
    int ix = x & 3;
    int iy = y & 3;
    int idx = iy * 4 + ix;
    return float(bayer4x4[idx]) / 16.0;
}

float ditherOffset(vec2 uv)
{
    ivec2 texSize = textureSize(texture0, 0);
    ivec2 pix = ivec2(floor(uv * vec2(texSize)));
    return (bayer4x4_lookup(pix.x, pix.y) - 0.5) / 255.0;
}

vec3 contrast(vec3 col, float factor)
{
    return (col - 0.5) * factor + 0.5;
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
	float dither = ditherOffset(fragTexCoord);
	finColor += dither; //remove color banding
	
	finalColor = vec4(finColor * vig, 1);
}