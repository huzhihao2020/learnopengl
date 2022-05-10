#version 330 core
#define DISPLAY_DEPTH_ONLY false

out vec4 FragColor;
in vec2 TexCoords;

in VS_OUT{
    vec3 frag_pos; // FragPos in world space
    vec3 normal;
    vec2 tex_coords;
} fs_in;

uniform sampler2D diffuse_texture;
uniform samplerCube shadow_cubemap;

uniform vec3 light_pos;
uniform vec3 view_pos;
uniform float far_plane;
uniform bool shadows;

vec3 sampleOffsetDirections[20] = vec3[]
(
   vec3( 1,  1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1,  1,  1),
   vec3( 1,  1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1,  1, -1),
   vec3( 1,  1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1,  1,  0),
   vec3( 1,  0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1,  0, -1),
   vec3( 0,  1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0,  1, -1)
);

float shadow_calculation(vec3 frag_pos, float bias) {
    // remember we use ortho_projection for light's view
    float shadow = 0.0f;
    vec3 light_to_frag = frag_pos - light_pos;
    float current_depth = length(light_to_frag); // real depth from frag to light
    int samples = 20;
    float view_distance = length(view_pos - frag_pos);
    float disk_radius = (1.0 + (view_distance / far_plane)) / 25.0;
    for(int i=0; i<samples; i++) {
        float closest_depth = texture(shadow_cubemap, light_to_frag + sampleOffsetDirections[i] * disk_radius).r;
        closest_depth *= far_plane;   // undo mapping [0;1]
        if(current_depth - bias > closest_depth)
            shadow += 1.0;
    }
    shadow /= float(samples);
    return shadow;
    //     display closestDepth as debug (to visualize depth cubemap)
    //    FragColor = vec4(vec3(closest_depth / far_plane), 1.0);
}

void main()
{
    vec3 ambient, diffuse, specular;
    vec3 tex_color = texture(diffuse_texture, fs_in.tex_coords).rgb;
    vec3 normal = normalize(fs_in.normal);
    vec3 light_color = vec3(0.3);
    //ambient
    ambient = 0.3 * light_color;
    // diffuse
    vec3 light_dir = normalize(light_pos - fs_in.frag_pos);
    diffuse = light_color * max(dot(normal, light_dir), 0);
    // specular
    vec3 view_dir = normalize(view_pos - fs_in.frag_pos);
    vec3 half_dir = normalize(light_dir + view_dir);
    specular = light_color * pow(max(dot(normal, half_dir), 0.0), 64.0);
    
    float bias = max(0.15*(1-dot(light_dir, normal)), 0.15);
    float shadow = shadows ? shadow_calculation(fs_in.frag_pos, bias) : 0.0;
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse +  specular)) * tex_color;
    
    FragColor = vec4(lighting, 1.0);
}
