#version 330 core
#define DISPLAY_DEPTH_ONLY false

out vec4 FragColor;
in vec2 TexCoords;
uniform sampler2D texture1;

float near = 0.1;
float far  = 30.0;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // back to NDC
    return (2.0 * near * far) / (far + near - z * (far - near));
}

void main()
{
    if(DISPLAY_DEPTH_ONLY) {
        float depth = LinearizeDepth(gl_FragCoord.z) / far; // divide by far for demonstration
        FragColor = vec4(vec3(depth), 1.0);
    }
    else {
        FragColor = texture(texture1, TexCoords);
    }
}
