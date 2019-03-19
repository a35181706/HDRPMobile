using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndirectTest : MonoBehaviour {

    public Mesh mesh;
    public Material mat;

    public int swordCout = 1000 * 10;
    public static int sphereSize = 40;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private ComputeBuffer argsBuffer;
    private ComputeBuffer transformBuffer;
    Matrix4x4[] matrixbuffer;
    // Use this for initialization
    void Start () {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
        transformBuffer = new ComputeBuffer(swordCout, size);

        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)swordCout;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        //CPU暴力算一波矩阵，排除computshader干扰。
        matrixbuffer = new Matrix4x4[swordCout];
        for (int i = 0; i < swordCout; i++)
        {
            Vector3 pos = UnityEngine.Random.insideUnitSphere * sphereSize;
            pos.y = 20;
            matrixbuffer[i] = Matrix4x4.TRS(pos, Quaternion.Euler(90, 0, 0), Vector3.one);
        }

        transformBuffer.SetData(matrixbuffer);
        argsBuffer.SetData(args);
        mat.SetBuffer("transformBuffer", transformBuffer);
        UnityEngine.Rendering.CommandBuffer cmdBuffer = new UnityEngine.Rendering.CommandBuffer();

        if (Camera.main.actualRenderingPath == RenderingPath.Forward)
        {
            Camera.main.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterForwardOpaque, cmdBuffer);
        }
        else if (Camera.main.actualRenderingPath == RenderingPath.DeferredShading)
        {
            Camera.main.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterGBuffer, cmdBuffer);
        }

        cmdBuffer.DrawMeshInstancedIndirect(mesh, 0, mat,
            -1, argsBuffer, 0, null);
    }

    private void OnDestroy()
    {
        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }

        if (transformBuffer != null)
        {
            transformBuffer.Release();
        }
        matrixbuffer = null;
        Camera.main.RemoveAllCommandBuffers();

    }

}
