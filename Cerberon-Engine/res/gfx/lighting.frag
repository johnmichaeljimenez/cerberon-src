#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D screenTex;
uniform sampler2D effectTex;

out vec4 finalColor;

const mat4 dither_table = mat4(
    -4.0, 0.0, -3.0, 1.0,
     2.0, -2.0, 3.0, -1.0,
    -3.0, 1.0, -4.0, 0.0,
     3.0, -1.0, 2.0, -2.0
);

const float depth = 24;

float quantize(float value, float levels)
{
    return floor(value * levels) / (levels - 1.0);
}

vec3 BlendOverlay(vec3 base, vec3 blend) {
    return vec3(
        base.r < 0.5 ? (2.0 * base.r * blend.r) : (1.0 - 2.0 * (1.0 - base.r) * (1.0 - blend.r)),
        base.g < 0.5 ? (2.0 * base.g * blend.g) : (1.0 - 2.0 * (1.0 - base.g) * (1.0 - blend.g)),
        base.b < 0.5 ? (2.0 * base.b * blend.b) : (1.0 - 2.0 * (1.0 - base.b) * (1.0 - blend.b))
    );
}

void main()
{
    vec2 uv = fragTexCoord.xy;
    uv *=  1.0 - uv.yx;
    float vig = uv.x*uv.y * 50.0;
    vig = pow(vig, 0.8);
    
    vec3 effectColor = texture(effectTex, fragTexCoord).rgb;

    vec3 lightColor = texture(texture0, fragTexCoord).rgb;
    vec3 screenColor = texture(screenTex, fragTexCoord).rgb;
    vec3 texelColor = BlendOverlay(lightColor, screenColor);

    //PS1 dither effect from Jax on Raylib Discord server
    vec2 fragCoord = gl_FragCoord.xy;
    ivec2 fragCoordInt = ivec2(fragCoord);
    vec2 scaledCoord = fragCoord.xy * 1;

    texelColor.rgb += dither_table[int(scaledCoord.x) % 4][int(scaledCoord.y) % 4] * 0.05;
    texelColor.r = quantize(texelColor.r, depth);
    texelColor.g = quantize(texelColor.g, depth);
    texelColor.b = quantize(texelColor.b, depth);
    texelColor *= vig;

    float lum =  dot(texelColor, vec3(0.299, 0.587, 0.114));
    vec3 screenGrayColor = vec3(lum, lum, lum);
    
    finalColor = vec4(mix(screenGrayColor, texelColor, effectColor.r), 1);
}