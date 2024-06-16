
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Profiling;

/// <summary>
/// https://cloud.tencent.com/developer/article/1759417
/// 参考/// 
/// </summary> <summary>
/// 
/// </summary>
partial class CameraRenderer
{
    partial void DrawGizmos();
    ScriptableRenderContext context;
    Camera camera;
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer { name = bufferName };
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    Lighting lighting = new Lighting();

    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
#if UNITY_EDITOR
    string SampleName { get; set; }
    static Material errorMaterial;
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")

    };
    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }
    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName = bufferName;
#endif
    public void Render(ScriptableRenderContext context, Camera camera ,ShadowSetting shadowSetting)
    {
        this.context = context;
        this.camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSetting.maxDistance))
        {
            return;
        }
        Setup();
        lighting.Setup(context, cullingResults);
        DrawVisibleGeometry();
#if UNITY_EDITOR
        DrawUnsupportedShaders();
#endif
        DrawGizmos();
        lighting.Cleanup();
        Submit();

    }


    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera)
        {
            //调整绘制顺序
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        /// <summary>
        /// 渲染天空球,需要在相机上选择skybox
        /// </summary>
        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

#if UNITY_EDITOR
    void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial
        };
        for (int i = 0; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
#endif
    /// <summary>
    /// 将摄像机的属性应用于上下文,没有这个步骤相机渲染的是1/4的大小
    /// </summary> <summary>
    /// 
    /// </summary>
    void Setup()
    {
        //设置相机属性 设置相机矩阵
        context.SetupCameraProperties(camera);

        CameraClearFlags flags = camera.clearFlags;

        //设置清除颜色
        //清除深度和颜色 
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
            );
        //命令缓冲区开始的位置
        buffer.BeginSample(SampleName);

        //执行缓存区命令 
        ExecuteBuffer();

    }

    /// <summary>
    /// 必须通过在上下文上调用Submit来提交排队的工作才会执行渲染
    /// </summary> <summary>
    /// 
    /// </summary>
    void Submit()
    {
        //命令缓冲区截止的位置
        buffer.EndSample(SampleName);
        //执行缓存区命令 必须要执行
        ExecuteBuffer();

        //提交给GPU
        context.Submit();
    }

    void ExecuteBuffer()
    {
        //执行缓存区命令
        context.ExecuteCommandBuffer(buffer);
        //清空缓存区 两者是连续的
        buffer.Clear();
    }

    /// <summary>
    /// 剔除参数
    /// </summary>
    CullingResults cullingResults;
    bool Cull(float maxShadowDistance)
    {
        //剔除  相机矩阵
        ScriptableCullingParameters p;
        if (camera.TryGetCullingParameters(out  p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}
