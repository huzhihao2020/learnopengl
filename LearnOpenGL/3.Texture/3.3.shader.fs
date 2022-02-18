#version 330 core
out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

uniform sampler2D ourTexture1;
uniform sampler2D ourTexture2;

void main()
{
//    FragColor = mix(texture(ourTexture2, TexCoord), texture(ourTexture1, TexCoord) * vec4(ourColor, 1.0), 0.8);
    FragColor = mix(texture(ourTexture2, TexCoord), texture(ourTexture1, TexCoord), 0.8);
}
