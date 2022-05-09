#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec2 TexCoords;

out VS_OUT{
    vec3 frag_pos; // FragPos in world space
    vec3 normal;
    vec2 tex_coords;
    vec4 frag_light_space;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 light_space_matrix;

void main()
{
    vs_out.frag_pos = vec3(model * vec4(aPos, 1.0));
    vs_out.normal = inverse(transpose(mat3(model))) * aNormal;
    vs_out.tex_coords = aTexCoords;
    vs_out.frag_light_space = light_space_matrix * vec4(vs_out.frag_pos, 1.0f); // FragPos in light space
    gl_Position = projection * view * vec4(vs_out.frag_pos, 1.0); // FragPos in camera space
}
