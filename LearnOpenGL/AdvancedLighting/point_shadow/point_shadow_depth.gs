#version 330 core
layout (triangles) in;
layout (triangle_strip, max_vertices=18) out;

uniform mat4 shadow_matrices[6];

out vec4 FragPos;

void main()
{
    for(int face=0; face<6; face++) {
        // gl_Layer is built-in variable that specifies which cubemap face we emit our primitive to
        gl_Layer = face;
        for(int i=0; i<3; i++) {
            // output:  FragPos is vertex position in world space
            //          gl_Position is vertex position in light space
            FragPos = gl_in[i].gl_Position;
            gl_Position = shadow_matrices[face] * FragPos;
            EmitVertex();
        }
        EndPrimitive();
    }
}
