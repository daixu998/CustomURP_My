using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "CustomRenderPipeline", menuName = "Custom/CustomRenderPipeline")]
public class CustomRenderPiineAsset : RenderPipelineAsset
{
    public bool useDynamicBatching;
    public bool useGPUInstancing;
   public bool useSRPBatcher;

    [SerializeField]
    ShadowSetting shadow = default;
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching,useGPUInstancing,useSRPBatcher,shadow);
    }
}
