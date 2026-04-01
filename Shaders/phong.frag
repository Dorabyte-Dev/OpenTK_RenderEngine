#version 330 core
struct Material {
    sampler2D diffuse;
    sampler2D specular;
    sampler2D normalMap;
    float shininess;
    int isNotBlinn;
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
uniform int gammaCorrection;
uniform sampler2D shadowMap;
uniform samplerCube shadowCubemap;
uniform float far_plane;

in vec3 Normal;
in vec3 FragPos;
in vec2 texCoord;
in mat3 TBN;
in vec4 FragPosLightSpace;

out vec4 FragColor;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir);
float PointShadowCalculation(vec3 fragPos, vec3 lightPos);

void main()
{
    // properties
    /*vec3 norm = texture(material.normalMap, texCoord).rgb;
    norm = norm * 2.0 - 1.0;
    norm = normalize(TBN * norm);*/

    vec3 viewDir = normalize(viewPos - FragPos);

    vec3 norm = normalize(Normal);

    // phase 1: Directional lighting
    vec3 result = CalcDirLight(dirLight, norm, viewDir);

    // phase 2: Point lights
    for(int i = 0; i < NR_POINT_LIGHTS; i++)
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);   
        
    // phase 3: Spot light
    for(int i = 0; i < NR_SPOT_LIGHTS; i++)
        result += CalcSpotLight(spotLights[i], norm, FragPos, viewDir);    
    
    if(gammaCorrection == 1)
    {
        float gamma = 2.2;
        result = pow(result, vec3(1.0/gamma));
    }
    
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

    if(material.isNotBlinn == 1)
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

    float shadow = ShadowCalculation(FragPosLightSpace, normal, lightDir);

    // combine results
    vec3 ambient  = light.ambient  * vec3(texture(material.diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material.diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(material.specular, texCoord));

    return (ambient + (1.0 - shadow) * (diffuse + specular));
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

    if(material.isNotBlinn == 1)
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

    float shadow = PointShadowCalculation(fragPos, light.position);

    return (ambient + (1.0 - shadow) * (diffuse + specular));
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

    if(material.isNotBlinn == 1)
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

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    // 1. División de perspectiva (de normalizar el clip space)
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    
    // 2. Transformar de rango [-1, 1] a rango [0, 1] para leer la textura
    projCoords = projCoords * 0.5 + 0.5;

    // 3. Obtener la profundidad más cercana desde el punto de vista de la luz
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    
    // 4. Obtener la profundidad de nuestro píxel actual
    float currentDepth = projCoords.z;
    
    float bias = max(0.01 * (1.0 - dot(normal, lightDir)), 0.005);

    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    
    return shadow;
}

float PointShadowCalculation(vec3 fragPos, vec3 lightPos)
{
    vec3 fragToLight = fragPos - lightPos; 
    float closestDepth = texture(shadowCubemap, fragToLight).r;
    closestDepth *= far_plane;
    float currentDepth = length(fragToLight);
    float shadow  = 0.0;
    float bias    = 0.05; 
    float samples = 4.0;
    float offset  = 0.1;
    for(float x = -offset; x < offset; x += offset / (samples * 0.5))
    {
        for(float y = -offset; y < offset; y += offset / (samples * 0.5))
        {
            for(float z = -offset; z < offset; z += offset / (samples * 0.5))
            {
                float closestDepth = texture(shadowCubemap, fragToLight + vec3(x, y, z)).r; 
                closestDepth *= far_plane;   // undo mapping [0;1]
                if(currentDepth - bias > closestDepth)
                    shadow += 1.0;
            }
        }
    }
    shadow /= (samples * samples * samples);

    return shadow;
}  

