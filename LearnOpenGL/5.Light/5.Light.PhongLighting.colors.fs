#version 330 core
out vec4 FragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform int specularPow;

in vec3 FragPos;
in vec3 Normal;

void main()
{
    float ambientWeight = 0.1f;
    vec3 ambient = ambientWeight * lightColor;
    
    vec3 lightDir = normalize(lightPos - FragPos);
    vec3 norm = normalize(Normal);
    float cos_theta = max(dot(lightDir, norm), 0.0f);
    vec3 diffuse = cos_theta * lightColor;
    
    float specularWeight = 0.5f;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0f), specularPow);
    vec3 specular = specularWeight * spec * lightColor;
    
    vec3 result = (ambient + diffuse + specular) * objectColor;
    FragColor = vec4(result, 1.0f);
}
