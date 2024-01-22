#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D effectTex;

out vec4 finalColor;

void main()
{
	vec2 uv2 = gl_FragCoord.rg/vec2(1366,768); //TODO: provide uniform resolution values

    vec3 effectColor = texture(effectTex, uv2).rgb;
    vec4 color = texture(texture0, fragTexCoord).rgba;
	vec3 outColor = color.rgb;

    finalColor = vec4(color.rgb, effectColor.r * color.a);
}