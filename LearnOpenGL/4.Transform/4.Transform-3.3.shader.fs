#version 330 core
out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

uniform sampler2D ourTexture1;
uniform sampler2D ourTexture2;
uniform float mix_tex_factor;
uniform float mix_color_factor;

void main()
{
    FragColor = mix(texture(ourTexture1, TexCoord), texture(ourTexture2, vec2(1.0 - TexCoord.x,TexCoord.y)), mix_tex_factor) * vec4(vec3(1.0f, 1.0f, 1.0f) * mix_color_factor, 1.0f);
}
