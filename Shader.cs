using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GameEngine;
using System.Xml.Linq;

public class Shader
{
    public int Handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        //Declaring and Creating Shaders

        string vertexShaderSource = File.ReadAllText("Shaders/" + vertexPath);
        string fragmentShaderSource = File.ReadAllText("Shaders/" + fragmentPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);

        //Compiling Shaders

        GL.CompileShader(vertexShader);

        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexSuccess);
        if (vertexSuccess == 0)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentSuccess);
        if (fragmentSuccess == 0)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }

        //Attaching Shaders to a program

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        GL.UseProgram(Handle);

        int location = GL.GetUniformLocation(Handle, name);

        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVector3(string name, Vector3 value)
    {
        GL.UseProgram(Handle);

        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform3(location, value);
    }

    public void SetColor(string name, Color4 value)
    {
        GL.UseProgram(Handle);

        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform4(location, value);
    }

    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform1(location, value);
    }

    private bool _disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        GL.DeleteProgram(Handle);

        _disposedValue = true;
    }

    ~Shader()
    {
        if (!_disposedValue)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
}