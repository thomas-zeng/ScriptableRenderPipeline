namespace UnityEngine.Rendering.HighDefinition
{
    public abstract class SkyRenderer
    {
        /// <summary>
        /// Called on startup. Create resources used by the renderer (shaders, materials, etc).
        /// </summary>
        public abstract void Build();

        /// <summary>
        /// Called on cleanup. Release resources used by the renderer.
        /// </summary>
        public abstract void Cleanup();

        /// <summary>
        /// Called every frame. Implement it if your SkyRenderer needs to iterate independently from the user defined update frequency (see SkySettings UpdateMode)
        /// </summary>
        /// <returns>True if the update determines that sky lighting needs to be re-rendered. False otherwise.</returns>
        public virtual bool Update(BuiltinSkyParameters builtinParams) { return false; }

        /// <summary>
        /// Implements actual rendering of the sky. This will be called when rendering the sky into a cubemap (for lighting) and also during the main frame rendering.
        /// </summary>
        /// <param name="builtinParams">Engine parameters provided to users for rendering the sky.</param>
        /// <param name="renderForCubemap">This will be true when the sky is being rendered into a cubemap for lighting. This is useful when the sky renderer needs a different implementation in this case.</param>
        /// <param name="renderSunDisk">If the sky renderer supports the rendering of a sun disk, it must not render it if this is set to false.</param>
        public abstract void RenderSky(BuiltinSkyParameters builtinParams, bool renderForCubemap, bool renderSunDisk);

        /// <summary>
        /// Sky Renderer validity check.
        /// </summary>
        /// <returns>Returns true if the current sky is valid. False otherwise.</returns>
        public abstract bool IsValid();

        /// <summary>
        /// Returns exposure setting for the provided SkySettings. This will also take debug exposure into accound
        /// </summary>
        /// <param name="skySettings">SkySettings for which exposure is required.</param>
        /// <param name="debugSettings">Current debug display settings</param>
        /// <returns>Returns SkySetting exposure.</returns>
        protected static float GetExposure(SkySettings skySettings, DebugDisplaySettings debugSettings)
        {
            float debugExposure = 0.0f;
            if (debugSettings != null && debugSettings.DebugNeedsExposure())
            {
                debugExposure = debugSettings.data.lightingDebugSettings.debugExposure;
            }
            return ColorUtils.ConvertEV100ToExposure(-(skySettings.exposure.value + debugExposure));
        }

        /// <summary>
        /// Setup global parameters for the sky renderer.
        /// </summary>
        /// <param name="cmd">Command buffer provided to setup shader constants.</param>
        public virtual void SetGlobalSkyData(CommandBuffer cmd)
        {

        }
    }
}
