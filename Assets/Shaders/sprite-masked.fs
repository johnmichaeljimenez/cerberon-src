#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D visionTex;

out vec4 finalColor;

void main() {
	vec2 uv2 = gl_FragCoord.rg/vec2(800,450); //TODO: pass the virtual resolution properly

    vec4 texColor = texture(texture0, fragTexCoord);
	vec3 visColor = texture(visionTex, uv2).rgb;

	finalColor = vec4(texColor.rgb, texColor.a * visColor.r);
}