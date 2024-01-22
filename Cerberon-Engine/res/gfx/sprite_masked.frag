#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D effectTex;

out vec4 finalColor;

void main()
{
    vec3 effectColor = texture(effectTex, fragTexCoord).rgb;
    vec4 color = texture(texture0, fragTexCoord).rgba;
	vec3 outColor = color.rgb;

    finalColor = vec4(1, 0, 0, effectColor.r);
}