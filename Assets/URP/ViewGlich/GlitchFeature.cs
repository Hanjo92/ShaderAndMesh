using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Almond
{
	public class GlitchFeature : ScriptableRendererFeature
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		public Material glitchMaterial;
		private class GlitchRenderPass : ScriptableRenderPass
		{
			private RenderTargetIdentifier identifier;
			private static int PixelBufferID = Shader.PropertyToID("_PixelBuffer");
			RenderTargetIdentifier tmpIdentifier;
			private Material effect;
			public GlitchRenderPass(Material pixelizeMaterial) : base()
			{
				effect = pixelizeMaterial;
				tmpIdentifier = new RenderTargetIdentifier(PixelBufferID);
			}

			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				identifier = renderingData.cameraData.renderer.cameraColorTarget;
			}
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			{
				RenderTextureDescriptor descriptor = cameraTextureDescriptor;
				cmd.GetTemporaryRT(PixelBufferID, descriptor, FilterMode.Bilinear);
			}
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				if(renderingData.cameraData.camera.cameraType != CameraType.Game)
					return;
				CommandBuffer cmd = CommandBufferPool.Get("GlitchRenderPass");

				Blit(cmd, identifier, tmpIdentifier, effect);
				Blit(cmd, tmpIdentifier, identifier);

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			public override void OnCameraCleanup(CommandBuffer cmd)
			{
				base.OnCameraCleanup(cmd);
				cmd.ReleaseTemporaryRT(PixelBufferID);
			}
		}

		private GlitchRenderPass glitchPass;

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(glitchPass);
		}

		public override void Create()
		{
			glitchPass = new GlitchRenderPass(glitchMaterial);
			glitchPass.renderPassEvent = renderPassEvent;
		}
	}
}