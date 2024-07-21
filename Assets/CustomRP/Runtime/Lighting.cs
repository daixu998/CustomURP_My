
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
public class Lighting
{
    const string bufferName = "Lighting";
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    static int dirLightColosrId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    const int maxDirLightCount = 4;
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    CullingResults cullingResults;
    Shadows shadows = new Shadows();
    // ShadowSetting shadowSetting;
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults , ShadowSetting shadowSetting)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        // SetupDirectionaLight();
        shadows.Setup(context, cullingResults,shadowSetting);
        SetupLight();
        shadows.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();


    }
    void SetupLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {



            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionaLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }

        }
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColosrId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
    }
    void SetupDirectionaLight(int index, ref VisibleLight light)
    {
        dirLightColors[index] = light.finalColor;
        dirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
        shadows.ReserveDirectionalShadows
            (light.light ,index);
    }
    public void Cleanup()
    {
        shadows.Cleanup();
    }

}
