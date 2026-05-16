#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;

uniform bool eraseVision = false;
uniform sampler2D visionTex;

uniform float tilingX;
uniform float tilingY;
uniform bool tilingMode = false;

out vec4 finalColor;

void main()
{
	vec2 uv2 = gl_FragCoord.rg/vec2(800,450); 
    vec2 t = vec2(tilingX, tilingY);
    vec2 base_uv = fragTexCoord * t;
    vec2 local_uv = fragTexCoord;

	if (tilingMode)
		local_uv = fract(base_uv); 

    vec4 texel = texture(texture0, local_uv);
	if (eraseVision)
	{
	    float visValue = texture(visionTex, uv2).r;
		texel.a = mix(texel.a, texel.a * 0.5, visValue);
	}

	finalColor = texel * fragColor;
}