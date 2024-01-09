#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D screenTex;

out vec4 finalColor;

vec3 BlendOverlay(vec3 base, vec3 blend) {
    return vec3(
        base.r < 0.5 ? (2.0 * base.r * blend.r) : (1.0 - 2.0 * (1.0 - base.r) * (1.0 - blend.r)),
        base.g < 0.5 ? (2.0 * base.g * blend.g) : (1.0 - 2.0 * (1.0 - base.g) * (1.0 - blend.g)),
        base.b < 0.5 ? (2.0 * base.b * blend.b) : (1.0 - 2.0 * (1.0 - base.b) * (1.0 - blend.b))
    );
}

void main()
{
    vec3 lightColor = texture(texture0, fragTexCoord).rgb;
    vec3 screenColor = texture(screenTex, fragTexCoord).rgb;

    vec3 texelColor = BlendOverlay(lightColor, screenColor);
    finalColor = vec4(texelColor, 1);
}