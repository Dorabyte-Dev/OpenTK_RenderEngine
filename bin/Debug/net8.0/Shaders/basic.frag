#version 330 core
out vec4 FragColor;

in vec4 BaseColor;
in vec2 texCoord;
uniform sampler2D _mainTex;
void main()
{
    //FragColor = BaseColor;
    FragColor = texture(_mainTex, texCoord);
}