#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform float tilingX;
uniform float tilingY;

out vec4 finalColor;

void main()
{
	vec2 t = vec2(tilingX, tilingY);
    vec2 uv = fract(fragTexCoord * t); //tile this regardless of wrap mode from outside
    vec4 texel = texture(texture0, uv);
	
    finalColor = texel * fragColor;
}