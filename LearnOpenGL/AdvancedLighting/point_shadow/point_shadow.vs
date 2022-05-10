#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec2 TexCoords;

out VS_OUT{
    vec3 frag_pos; // FragPos in world space
    vec3 normal;
    vec2 tex_coords;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform bool reverse_normals;

void main()
{
    vs_out.frag_pos = vec3(model * vec4(aPos, 1.0));
    if(reverse_normals) // a slight hack to make sure the outer large cube displays lighting from the 'inside' instead of the default 'outside'.
        vs_out.normal = transpose(inverse(mat3(model))) * (-1.0 * aNormal);
    else
        vs_out.normal = inverse(transpose(mat3(model))) * aNormal;
    vs_out.tex_coords = aTexCoords;
    gl_Position = projection * view * model * vec4(aPos, 1.0); // FragPos in camera space
}
