#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;

void main()
{
	//TODO: optimize and set resolution and blur amount as parameter
	vec2 texelSize = 1.0 / vec2(256,256);
	vec4 result = vec4(0.0);
	
	//5 tap kernel
	float weights[3];
	weights[0] = 0.382928; 
	weights[1] = 0.241732; 
	weights[2] = 0.060598; 
	
	float totalWeight = 0.0;

	vec4 test = texture(texture0, fragTexCoord).rgba;
	
	// Sample a 5x5 grid
	for (int x = -2; x <= 2; x++)
	{
		for (int y = -2; y <= 2; y++)
		{
			vec2 offset = vec2(float(x), float(y)) * texelSize * 4;
			
			int ax = x < 0 ? -x : x;
			int ay = y < 0 ? -y : y;
			
			float weight = weights[ax] * weights[ay];
			result += texture(texture0, fragTexCoord + offset) * weight;
			totalWeight += weight;
		}
	}
	
	vec4 col = fragColor;
	// col.r = blurAmount/6.0;
	// col.g = 0;
	// col.b = 0;
	finalColor = (result / totalWeight) * col;
}