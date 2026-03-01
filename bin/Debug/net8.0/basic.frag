#version 330 core
out vec4 FragColor;

in vec4 BaseColor;
in vec2 texCoord;
uniform sampler2D texture0;
void main()
{
    //FragColor = BaseColor;
    FragColor = texture(texture0, texCoord);
}