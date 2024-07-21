using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "Shadows";
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    CommandBuffer buffer = new CommandBuffer() { name = bufferName };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSetting setting;
    const int maxShadowedDirectionalLightCount = 1;
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
        // public Vector3 directionWS;
        // public float normalBias;
    }
    ShadowedDirectionalLight[] shadowedDirectionalLights = 
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    int shadowedDirectionalLightCount =1;
    public void ReserveDirectionalShadows(Light light , int visibleLightIndex)
    {
        if(shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
        && light.shadows != LightShadows.None
        && light.shadowStrength > 0f
        && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] = 
            new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex
            };
            // shadowedDirectionalLightCount++;
        }
    }


    public void Render()
    {
        if (shadowedDirectionalLightCount >0)
        {
            RenderDirectionalShadows();
        }
    }
    //渲染阴影贴图
    void RenderDirectionalShadows()
    {
        int atlasSize = (int)setting.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId,atlasSize,atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(false,true,Color.clear);
        ExecuteBuffer();
    }
    public void Setup(
        ScriptableRenderContext context,
        CullingResults cullingResults,
        ShadowSetting setting)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.setting = setting;
        shadowedDirectionalLightCount = 0;

    }

    public void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    public void Cleanup()
    {
        if(shadowedDirectionalLightCount>0)
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
        
    }


}
