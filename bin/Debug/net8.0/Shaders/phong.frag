#version 330
struct Material {
    sampler2D diffuse;
    sampler2D specular;

    float shininess;
    int isBlinn;
};

struct DirLight {
    vec3 direction;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform DirLight dirLight;

struct PointLight {    
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;  

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};
#define NR_SPOT_LIGHTS 2
uniform SpotLight spotLights[NR_SPOT_LIGHTS];

uniform Material material;
uniform vec3 viewPos;

in vec3 Normal;
in vec3 FragPos;
in vec2 texCoord;

out vec4 FragColor;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    // properties
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    // phase 1: Directional lighting
    vec3 result = CalcDirLight(dirLight, norm, viewDir);

    // phase 2: Point lights
    for(int i = 0; i < NR_POINT_LIGHTS; i++)
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);   
        
    // phase 3: Spot light
    for(int i = 0; i < NR_SPOT_LIGHTS; i++)
        result += CalcSpotLight(spotLights[i], norm, FragPos, viewDir);    
    
    float gamma = 2.2;
    result = pow(result, vec3(1.0/gamma));

    FragColor = vec4(result, 1.0);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    if (length(light.direction) < 0.001)
        return vec3(0.0);

    vec3 lightDir = normalize(-light.direction);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    float spec;
    float kPi = 3.14159265f;

    if(material.isBlinn == 0)
    {
        float kEnergyConservation = ( 2.0 + material.shininess ) / ( 2.0 * kPi ); 
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = kEnergyConservation * pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    }
    else
    {
        float kEnergyConservation = ( 8.0 + material.shininess ) / ( 8.0 * kPi ); 
		vec3 halfwayDir = normalize(lightDir + viewDir);  
		spec = kEnergyConservation * pow(max(dot(normal, halfwayDir), 0.0), material.shininess * 2);
        
	}

    // combine results
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));

    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    if (light.constant < 0.001)
            return vec3(0.0);
    vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    float spec;
    float kPi = 3.14159265f;

    if(material.isBlinn == 0)
    {
        float kEnergyConservation = ( 2.0 + material.shininess ) / ( 2.0 * kPi ); 
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = kEnergyConservation * pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    }
    else{
        float kEnergyConservation = ( 8.0 + material.shininess ) / ( 8.0 * kPi ); 
		vec3 halfwayDir = normalize(lightDir + viewDir);  
		spec = kEnergyConservation * pow(max(dot(normal, halfwayDir), 0.0), material.shininess * 2);
	}
    

    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    

    // combine results
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{   
    if (light.constant < 0.001 || light.cutOff == light.outerCutOff) //Check later
        return vec3(0.0);

    vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    float spec;
    float kPi = 3.14159265f;

    if(material.isBlinn == 0)
    {
        float kEnergyConservation = ( 2.0 + material.shininess ) / ( 2.0 * kPi ); 
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = kEnergyConservation * pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    }
    else{
        float kEnergyConservation = ( 8.0 + material.shininess ) / ( 8.0 * kPi ); 
		vec3 halfwayDir = normalize(lightDir + viewDir);  
		spec = kEnergyConservation * pow(max(dot(normal, halfwayDir), 0.0), material.shininess * 2);
	}

    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    

    //Spotlight
    float theta     = dot(lightDir, normalize(-light.direction));
    float epsilon   = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    // combine results
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));
    
    ambient  *= attenuation;
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;

    return(ambient + diffuse + specular);
}