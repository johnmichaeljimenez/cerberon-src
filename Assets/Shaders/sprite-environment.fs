#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;

uniform bool eraseVision = false;
uniform sampler2D visionTex;

uniform float tilingX;
uniform float tilingY;
uniform bool tilingMode = false;
uniform bool stochasticMode = false;

out vec4 finalColor;

void main()
{
    vec2 uv2 = gl_FragCoord.rg / vec2(800, 450);
    vec2 t = vec2(tilingX, tilingY);
    vec2 base_uv = fragTexCoord * t;
    vec2 local_uv = fragTexCoord;
    vec2 tile = vec2(0.0);

    if (tilingMode)
    {
        local_uv = fract(base_uv);
        tile = floor(base_uv);
    }

    vec4 texel;

    if (stochasticMode && tilingMode)
    {
        float hash   = fract(sin(dot(tile, vec2(12.9898, 78.233))) * 43758.5453);
        float hash2  = fract(hash * 17.313);

        int   rot    = int(hash * 4.0);
        bool  flipH  = hash2 > 0.5;

        vec2 suv = local_uv;

        if (flipH)
            suv.x = 1.0 - suv.x;

        if (rot == 1)
            suv = vec2(suv.y, 1.0 - suv.x);
        else if (rot == 2)
            suv = vec2(1.0 - suv.x, 1.0 - suv.y);
        else if (rot == 3)
            suv = vec2(1.0 - suv.y, suv.x);

        texel = texture(texture0, suv);
    }
    else
    {
        texel = texture(texture0, local_uv);
    }

    if (eraseVision)
    {
        float visValue = texture(visionTex, uv2).r;
        texel.a = mix(texel.a, texel.a * 0.5, visValue);
    }

    finalColor = texel * fragColor;
}