#version 330 core
#define DISPLAY_DEPTH_ONLY false

out vec4 FragColor;
in vec2 TexCoords;

in VS_OUT{
    vec3 frag_pos; // FragPos in world space
    vec3 normal;
    vec2 tex_coords;
    vec4 frag_light_space;
} fs_in;

uniform sampler2D diffuse_texture;
uniform sampler2D shadow_map;

uniform vec3 light_pos;
uniform vec3 view_pos;

float shadow_calculation(vec4 frag_light_space, float bias) {
    // remember we use ortho_projection for light's view
    float shadow = 0.0f;
    vec3 project_coords = frag_light_space.xyz / frag_light_space.w;
    project_coords = project_coords * 0.5 + 0.5;
    float closest_depth = texture(shadow_map, project_coords.xy).r; // depth recorded on shadow map
    float current_depth = project_coords.z; // real depth from frag to light
    if(project_coords.z > 1.0)
        return 0;
    vec2 shadow_map_size = 1.0 / textureSize(shadow_map, 0);
    for(int x=-1; x<=1; x++) {
        for(int y=-1; y<=1; y++) {
            float closest_depth = texture(shadow_map, project_coords.xy + vec2(x,y) * shadow_map_size).r;
            shadow += current_depth-bias > closest_depth ? 1.0 : 0.0;
        }
    }
    return shadow / 9.0f;
}

void main()
{
    vec3 ambient, diffuse, specular;
    vec3 tex_color = texture(diffuse_texture, fs_in.tex_coords).rgb;
    vec3 normal = normalize(fs_in.normal);
    vec3 light_color = vec3(1.0);
    //ambient
    ambient = 0.15 * light_color;
    // diffuse
    vec3 light_dir = normalize(light_pos - fs_in.frag_pos);
    diffuse = light_color * max(dot(normal, light_dir), 0);
    // specular
    vec3 view_dir = normalize(view_pos - fs_in.frag_pos);
    vec3 half_dir = normalize(light_dir + view_dir);
    specular = light_color * pow(max(dot(normal, half_dir), 0.0), 64.0);
    
    float bias = max(0.05*(1-dot(light_dir, normal)), 0.005);
    float shadow = shadow_calculation(fs_in.frag_light_space, bias);
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse +  specular)) * tex_color;
    
    FragColor = vec4(lighting, 1.0);
}
