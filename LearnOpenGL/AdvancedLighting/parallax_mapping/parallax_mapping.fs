#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec2 TexCoords;
    vec3 FragPos;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D diffuse_texture;
uniform sampler2D normal_map;
uniform sampler2D depth_map;

uniform float height_scale;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    float height =  texture(depth_map, texCoords).r;
    return texCoords - viewDir.xy * (height * height_scale);
}

void main()
{
    vec3 light_dir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    vec3 view_dir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    
    // apply depth_map to modify (u, v)
    vec2 texCoords = fs_in.TexCoords;
    texCoords = ParallaxMapping(fs_in.TexCoords,  view_dir);
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;
    
    vec3 normal = texture(normal_map, texCoords).rgb;
    normal = normalize(normal * 2.0 - 1);
    vec3 color = texture(diffuse_texture, texCoords).rgb;

    vec3 light = vec3(1.0);
    vec3 ambient, diffuse, specular;
    ambient = 0.1 * light;
    diffuse = light * color * max(dot(light_dir, normal), 0.0f);
    vec3 half_vec = normalize(light_dir+view_dir);
    specular = 0.2 * light * pow(max(dot(half_vec, normal), 0.0f), 32);
    
    vec3 lighting = ambient + diffuse + specular;
    FragColor = vec4(lighting, 1.0);
}
