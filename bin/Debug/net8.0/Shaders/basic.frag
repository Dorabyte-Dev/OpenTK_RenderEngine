#version 330 core
out vec4 FragColor;

in vec4 BaseColor;
in vec2 texCoord;
uniform sampler2D _mainTex;
uniform float hasTexture; 
void main()
{
    vec4 texColor = texture(_mainTex, texCoord);
    FragColor = BaseColor;
}