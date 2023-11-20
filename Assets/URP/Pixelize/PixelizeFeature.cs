using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeFeature : ScriptableRendererFeature
{
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public Material pixelizeMaterial;
    class PixelizeRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier identifier;
        private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");
        RenderTargetIdentifier tmpIdentifier;
        private Material effect;
        public PixelizeRenderPass(Material pixelizeMaterial ) : base()
        {
            effect = pixelizeMaterial;
            tmpIdentifier = new RenderTargetIdentifier( pixelBufferID );
        }

		public override void OnCameraSetup( CommandBuffer cmd, ref RenderingData renderingData )
        {
            identifier = renderingData.cameraData.renderer.cameraColorTarget;
        }
		public override void Configure( CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor )
		{
			RenderTextureDescriptor descriptor = cameraTextureDescriptor;
			cmd.GetTemporaryRT( pixelBufferID, descriptor, FilterMode.Bilinear );
		}
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if( renderingData.cameraData.camera.cameraType != CameraType.Game )
                return;
            CommandBuffer cmd = CommandBufferPool.Get("PixelizeRenderPass");

            Blit( cmd, identifier, tmpIdentifier, effect );
			Blit( cmd, tmpIdentifier, identifier );

			context.ExecuteCommandBuffer( cmd );
            CommandBufferPool.Release( cmd );
        }
		public override void OnCameraCleanup( CommandBuffer cmd )
		{
            cmd.ReleaseTemporaryRT( pixelBufferID );
        }
	}

    PixelizeRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PixelizeRenderPass( pixelizeMaterial );
        m_ScriptablePass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData )
	{
        renderer.EnqueuePass( m_ScriptablePass );
    }
}