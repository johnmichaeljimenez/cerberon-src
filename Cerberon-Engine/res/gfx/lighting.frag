#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

vec4 BlendOverlay(vec4 base, vec4 blend) {
    return vec4(
        base.r < 0.5 ? (2.0 * base.r * blend.r) : (1.0 - 2.0 * (1.0 - base.r) * (1.0 - blend.r)),
        base.g < 0.5 ? (2.0 * base.g * blend.g) : (1.0 - 2.0 * (1.0 - base.g) * (1.0 - blend.g)),
        base.b < 0.5 ? (2.0 * base.b * blend.b) : (1.0 - 2.0 * (1.0 - base.b) * (1.0 - blend.b)),
        base.a < 0.5 ? (2.0 * base.a * blend.a) : (1.0 - 2.0 * (1.0 - base.a) * (1.0 - blend.a))
    );
}

void main()
{
    // Texel color fetching from texture sampler
    vec3 texelColor = texture(texture0, fragTexCoord).rgb;

    // texelColor = BlendOverlay(fragColor, texelColor);
    finalColor = vec4(fragColor.r,0,0,0.3);// vec4(texelColor* 5, 1.0);
}