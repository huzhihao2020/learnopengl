#version 330 core

out vec4 FragColor;

in vec2 TexCoords;
in vec4 FragPos;
in VS_OUT {
    vec2 TexCoords;
    vec3 FragPos;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D diffuse_texture;
uniform sampler2D normal_map;

uniform vec3 light_pos;
uniform vec3 view_pos;

void main()
{
    vec3 normal = texture(normal_map, fs_in.TexCoords).rgb;
    normal = normalize(normal * 2.0 - 1);
//    vec3 normal = normalize(Normal);
    vec3 color = texture(diffuse_texture, fs_in.TexCoords).rgb;
    
    vec3 light_dir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    vec3 view_dir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    
    vec3 light = vec3(1.0);
    vec3 ambient, diffuse, specular;
    ambient = 0.1 * light;
    
    diffuse = light * color * max(dot(light_dir, normal), 0.0f);
    
    vec3 half_vec = normalize(light_dir+view_dir);
    specular = 0.3 * light * pow(max(dot(half_vec, normal), 0.0f), 64);
    
    vec3 lighting = ambient + diffuse + specular;
    FragColor = vec4(lighting, 1.0);
}
