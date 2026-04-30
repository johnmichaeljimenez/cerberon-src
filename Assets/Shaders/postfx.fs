#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform sampler2D lightTex;
uniform sampler2D visionTex;
uniform float time;

uniform float fadeAmt = 0.0; //0 = none, 1 = full
uniform float nightAmt = 0.0;

const float blurRadius = 1.5;

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

float luminance(vec3 c)
{
    return dot(c, vec3(0.299, 0.587, 0.114));
}

vec3 fade(vec3 col, float n)
{
    return max(vec3(0.0), col - n * 4.5); //simple fade (mix to black) is not enough, this one is the long-lost art common in retro consoles
}

vec3 nightVision(vec3 tex, vec3 light)
{
    vec3 col = tex;
    col = contrast(col,0.9);
    col = col + (light * 2);

    const vec3 nightTint = vec3(0.1, 1.0, 0.2); //I used green phosphor here instead of white phosphor. so red and dark red colors are hard to see like IRL
    col *= nightTint;

    float grain = fract(
        sin( dot( fragTexCoord.xy + vec2(time*1.0, 0.0),
                  vec2(12.9898,78.233) ) ) * 43758.5453
    );

    col += (grain - 0.5) * 0.4;

    return col;
}

vec3 blur(sampler2D tex, vec2 uv, float radius)
{
    vec2 texelSize = 1.0 / vec2(textureSize(tex, 0));
    vec3 color = vec3(0.0);
    float weightSum = 0.0;

    for (int x = -2; x <= 2; ++x)
    {
        for (int y = -2; y <= 2; ++y)
        {
            //TODO: optimize by caching radius and weights
            float weight = exp(-(x*x + y*y) / (2.0 * radius * radius)); // Gaussian falloff
            vec2 offset = vec2(x, y) * texelSize * radius;
            color += texture(tex, uv + offset).rgb * weight;
            weightSum += weight;
        }
    }
    return color / weightSum;
}

void main() {
    vec2 uv = fragTexCoord.xy;
    uv *=  1.0 - uv.yx;
    float vig = uv.x*uv.y * 15.0;
    vig = pow(vig, 1.1);

    vec3 screenColor = texture(texture0, fragTexCoord).rgb;
    vec3 blurColor = blur(texture0, fragTexCoord, blurRadius); //focus blur effect
	vec3 lightColor = texture(lightTex, fragTexCoord).rgb;
	vec3 visColor = texture(visionTex, fragTexCoord).rgb;

	lightColor = contrast(lightColor, 1.1);
    vec3 lightBlur = blur(lightTex, fragTexCoord, 2.5).rgb;
    lightBlur = contrast(lightBlur, 5); //bloom
	
    vec3 texelColor = lightColor * (screenColor + (screenColor * lightColor * 2));
    float lum = pow(luminance(blurColor), 2.5);
    vec3 screenGrayColor = vec3(lum, lum, lum);
	
    vec3 nightCol = nightVision(screenColor, lightColor);         
	vec3 finColor = mix(screenGrayColor, mix(texelColor, nightCol, nightAmt), visColor.r);
    
	float dither = ditherOffset(fragTexCoord);
    finColor += lightBlur * 0.1;
	finColor += dither; //remove color banding

    finColor = fade(finColor, fadeAmt/4.5);
    finColor *= vig;

	finalColor = vec4(finColor, 1);
}