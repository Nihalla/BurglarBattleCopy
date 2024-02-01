using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthNormalsRenderFeature : ScriptableRendererFeature
{
  private class Pass : ScriptableRenderPass
  {
    private Material _depthMat;
    private List<ShaderTagId> _shaderTags;
    private  FilteringSettings _filteringSettings;
    private RenderTargetHandle _destHandle;

    public Pass(Material depthMat)
    {
      _depthMat = depthMat;
      
      _shaderTags = new List<ShaderTagId>()
      {
        new ShaderTagId("DepthOnly"),
      };
      
      _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
      _destHandle.Init("_DepthNormalsTexture");
    }

    
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
      cmd.GetTemporaryRT(_destHandle.id, cameraTextureDescriptor, FilterMode.Point);
      ConfigureTarget(_destHandle.Identifier());
      ConfigureClear(ClearFlag.All, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      DrawingSettings drawSettings  = CreateDrawingSettings(_shaderTags, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
      drawSettings.overrideMaterial = _depthMat;
      context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref _filteringSettings);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
      cmd.ReleaseTemporaryRT(_destHandle.id);
    }
  }

  private Pass _renderPass;

  /// <inheritdoc/>
  public override void Create()
  {
    Material material = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");
    _renderPass = new Pass(material);
    _renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;

    // Configures where the render pass should be injected.
    _renderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
  }

  // Here you can inject one or multiple render passes in the renderer.
  // This method is called when setting up the renderer once per-camera.
  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
  {
    renderer.EnqueuePass(_renderPass);
  }
}