#version 330 core

struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float shininess;
};

struct Light {
    // basic light properties
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    // for flash light
    vec3 direction;
    float cutOff;
    float outerCutOff;
    // 衰减系数
    float constant;
    float linear;
    float quadratic;
};

uniform Light light;
uniform Material material;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

out vec4 FragColor;

void main()
{
    // 环境光
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    
    // 漫反射，顺便计算随距离衰减的系数
    vec3 Pos2Light = normalize(light.position - FragPos);
    float distance_light2pos = length(light.position - FragPos); // distance
    float attenuation = 1.0 / (light.constant + light.linear * distance_light2pos +
    light.quadratic * (distance_light2pos * distance_light2pos));
    vec3 lightDir = normalize(light.direction);
    vec3 norm = normalize(Normal);
    float cos_theta = max(dot(Pos2Light, norm), 0.0f); // 反射角度
    vec3 diffuse = cos_theta * light.diffuse * vec3(texture(material.diffuse, TexCoords));
    
    // 高光
    float specularWeight = 0.5f;
    vec3 viewDir = normalize(-light.direction);
    vec3 reflectDir = reflect(-Pos2Light, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), material.shininess);
    vec3 specular = vec3(texture(material.specular, TexCoords)) * spec * light.specular;
    
    // 软化光源照明边界
    float theta     = dot(Pos2Light, -lightDir);
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    
    ambient = ambient * attenuation;
    diffuse = diffuse * attenuation * intensity;
    specular = specular * attenuation * intensity;
    
    vec3 result = ambient + diffuse + specular;
    
    FragColor = vec4(result, 1.0f);
}
