#version 330 core

out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform samplerCube skybox;
uniform vec3 cameraPos;

void main()
{
    // Reflection
//    vec3 view = normalize(cameraPos - Position);
//    vec3 R = reflect(-view, normalize(Normal));
//    FragColor = vec4(texture(skybox, R).rgb, 1.0);

    // Refraction
    float ratio = 1.00 / 1.52;
    vec3 I = normalize(Position - cameraPos);
    vec3 R = refract(I, normalize(Normal), ratio);
    FragColor = vec4(texture(skybox, R).rgb, 1.0);
}
