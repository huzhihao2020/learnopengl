#version 330 core

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};

struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform Light light;
uniform Material material;
uniform vec3 viewPos;

in vec3 FragPos;
in vec3 Normal;
in vec2  TexCoords;

out vec4 FragColor;

void main()
{
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    
    vec3 lightDir = normalize(light.position - FragPos);
    vec3 norm = normalize(Normal);
    float cos_theta = max(dot(lightDir, norm), 0.0f);
    vec3 diffuse = cos_theta * light.diffuse * vec3(texture(material.diffuse, TexCoords));
    
    float specularWeight = 0.5f;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
    vec3 specular = vec3(texture(material.specular, TexCoords)) * spec * light.specular;
    
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0f);
}
