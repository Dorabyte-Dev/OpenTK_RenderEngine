#version 330 core
layout (location = 0) in vec3 aPosition;
layout(location = 2) in vec2 aTexCoord;

uniform vec4 baseColor;

out vec4 BaseColor;
out vec2 texCoord;


void main()
{
    gl_Position = vec4(aPosition, 1.0);
    texCoord = aTexCoord;
    BaseColor = baseColor;
}