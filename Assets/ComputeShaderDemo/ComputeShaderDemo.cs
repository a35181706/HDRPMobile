using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;
//GPU粒子的结构，用于CPU与GPU的数据传递
public struct GPUParticle
{
    public Vector2 position;
    public Vector2 velocity;
}


public class ComputeShaderDemo : MonoBehaviour
{
    public ComputeShader particleComputeShader;

    public Material particleMat;

    public Mesh mesh;
    const string computeShaderEntryPoint = "CSMain";
    const string computeShaderGPUBufferName = "GPUParticles";


    /// <summary>
    /// compute shader中numThread的数量
    /// </summary>
    const int WARP_SIZE = 512;

    /// <summary>
    /// 粒子数目
    /// </summary>
    public int ParticleCout = 1024 * 1000;

    /// <summary>
    /// 单个粒子的大小
    /// </summary>
    private int ParticleSize = 0;

    private ComputeBuffer gpuParticleBuffer = null;
    private int gpuUpdateKernelIndex = 0;
    private GPUParticle[] cpuParticleBuffer = null;

    private UnityEngine.Rendering.CommandBuffer gpuParticleRenderCommand = null;

    private int currentParticleCout = -1;

    /// <summary>
    /// GPU GroupNum
    /// </summary>
    private int gpuThreadGroupNum = -1;

    void Start()
    {

        InitGPUParticleSystem();
    }

    void InitGPUParticleSystem()
    {
        ParticleSize = Marshal.SizeOf(typeof(GPUParticle));
        //找到更新函数
        gpuUpdateKernelIndex = particleComputeShader.FindKernel(computeShaderEntryPoint);


    }

    private void OnGUI()
    {
        GUILayout.Label("FPS:" + FPSManager.fps);
        GUILayout.Label("按住屏幕滑动");
        GUILayout.Space(30);

        if (GUILayout.Button("重置粒子", GUILayout.Width(200), GUILayout.Height(150)))
        {
            RefershParticles(true);
        }
    }

    void RefershParticles(bool bforce = false)
    {
        if (ParticleCout == currentParticleCout && !bforce)
        {
            return;
        }

        if (!particleComputeShader || !particleMat)
        {
            return;
        }

        currentParticleCout = ParticleCout;
        cpuParticleBuffer = null;
        cpuParticleBuffer = new GPUParticle[currentParticleCout];
        if (gpuParticleBuffer != null)
        {
            gpuParticleRenderCommand.Release();
            gpuParticleBuffer.Release();
            gpuParticleBuffer = null;
        }
        gpuThreadGroupNum = Mathf.CeilToInt((float)ParticleCout / WARP_SIZE);
        gpuParticleBuffer = new ComputeBuffer(ParticleCout, ParticleSize);

        //初始化粒子buffer
        for (int i = 0; i < currentParticleCout; i++)
        {
            cpuParticleBuffer[i] = new GPUParticle();
            cpuParticleBuffer[i].position = Random.insideUnitCircle * 10f;
            cpuParticleBuffer[i].velocity = Vector2.zero;
        }

        //传递CPU初始化数据给GPU buffer
        gpuParticleBuffer.SetData(cpuParticleBuffer);

        //给computeshader中的函数设置数据源
        particleComputeShader.SetBuffer(gpuUpdateKernelIndex, computeShaderGPUBufferName, gpuParticleBuffer);

        //给shader设置buffer数据，用来做渲染用
        particleMat.SetBuffer(computeShaderGPUBufferName, gpuParticleBuffer);

        gpuParticleRenderCommand = new UnityEngine.Rendering.CommandBuffer();
        gpuParticleRenderCommand.name = "GPUParticle";
        gpuParticleRenderCommand.DrawProcedural(transform.localToWorldMatrix, particleMat, 0, MeshTopology.Points, 1, currentParticleCout);
        Camera.main.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterEverything, gpuParticleRenderCommand);

    }

    // Update is called once per frame
    void Update()
    {
        RefershParticles();

        if (Input.GetKeyDown(KeyCode.R))
        {
            //重置buffer 往compute shader 里更新自定义数据也是这样写
            gpuParticleBuffer.SetData(cpuParticleBuffer);
        }

        particleComputeShader.SetInt("shouldMove", Input.GetMouseButton(0) ? 1 : 0);
        //传递 int float Vector3 等数据 Vector2 和 Vector3 要转成 float[2],float[3] 才可以用，vector4可以直接用
        var mousePosition = GetMousePosition();
        particleComputeShader.SetFloats("mousePosition", mousePosition);
        particleComputeShader.SetFloat("dt", Time.deltaTime);

        //执行一次compute shader 里的函数
        particleComputeShader.Dispatch(gpuUpdateKernelIndex, gpuThreadGroupNum, 1, 1);
    }

    private void OnDestroy()
    {
        if (gpuParticleBuffer != null)
        {
            gpuParticleRenderCommand.Release();
            gpuParticleBuffer.Release();
            gpuParticleBuffer = null;
        }
    }
    /// <summary>
    /// 降vec2转换为float buffer
    /// </summary>
    /// <returns></returns>
    float[] GetMousePosition()
    {
        var mp = Input.mousePosition;
        var v = Camera.main.ScreenToWorldPoint(mp);
        return new float[] { v.x, v.y };
    }
}
