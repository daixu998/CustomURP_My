using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// https://cloud.tencent.com/developer/article/1759417
/// 参考/// 
/// </summary> <summary>
/// 
/// </summary>
public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    const string buferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer{name = buferName};
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        Setup();
        DrawVisibleGeometry();

        Submit();
    }

    /// <summary>
    /// 渲染天空球,需要在相机上选择skybox
    /// </summary> <summary>
    /// 
    /// </summary>
    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }

    /// <summary>
    /// 将摄像机的属性应用于上下文,没有这个步骤相机渲染的是1/4的大小
    /// </summary> <summary>
    /// 
    /// </summary>
    void Setup()
    {
        //设置相机属性 设置相机矩阵
        context.SetupCameraProperties(camera);
        //清除深度和颜色 
        buffer.ClearRenderTarget(true,true, Color.clear);
        //命令缓冲区开始的位置
        buffer.BeginSample(buferName);
        
        //执行缓存区命令 
        ExecuteCommandBuffer();
        
    }

    /// <summary>
    /// 必须通过在上下文上调用Submit来提交排队的工作才会执行渲染
    /// </summary> <summary>
    /// 
    /// </summary>
    void Submit()
    {
        //命令缓冲区截止的位置
        buffer.EndSample(buferName);
        //执行缓存区命令 必须要执行
        ExecuteCommandBuffer();

        //提交给GPU
        context.Submit();
    }

    void ExecuteCommandBuffer()
    {
        //执行缓存区命令
        context.ExecuteCommandBuffer(buffer);
        //清空缓存区 两者是连续的
        buffer.Clear();
    }
}
