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
using Newtonsoft.Json;

public class Shader
{
    [JsonIgnore]public int Handle;
    [JsonProperty("vertexPath")] private string _vertexPath;
    [JsonProperty("geometryPath")] private string _geometryPath;
    [JsonProperty("fragmentPath")] private string _fragmentPath;

    [JsonConstructor]
    public Shader(string vertexPath, string fragmentPath, string geometryPath = null)
    {
        //Declaring and Creating Shaders
        _vertexPath = vertexPath;
        _geometryPath = geometryPath;
        _fragmentPath = fragmentPath;
    }

    public void Compile() //Just In Time method
    {
        if (Handle != 0) return;


        Console.WriteLine("Compiling shader: " + _vertexPath + ", " + _geometryPath + ", " + _fragmentPath);

        bool hasGeometryShader = _geometryPath != null;

        string vertexShaderSource = File.ReadAllText("Shaders/" + _vertexPath);

        string geometryShaderSource = null;
        if (hasGeometryShader)
        {
            geometryShaderSource = File.ReadAllText("Shaders/" + _geometryPath);
        }

        string fragmentShaderSource = File.ReadAllText("Shaders/" + _fragmentPath);

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        int geometryShader = -1;
        if (hasGeometryShader)
        {
            geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, geometryShaderSource);
        }

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

        if (hasGeometryShader)
        {
            GL.CompileShader(geometryShader);
            GL.GetShader(geometryShader, ShaderParameter.CompileStatus, out int geometrySuccess);
            if (geometrySuccess == 0)
            {
                string infolog = GL.GetShaderInfoLog(geometryShader);
                Console.WriteLine(infolog);
            }
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
        if (hasGeometryShader) GL.AttachShader(Handle, geometryShader);

        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }

        GL.DetachShader(Handle, vertexShader);
        if (hasGeometryShader) GL.DetachShader(Handle, geometryShader);
        GL.DetachShader(Handle, geometryShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        if (hasGeometryShader) GL.DeleteShader(geometryShader);
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