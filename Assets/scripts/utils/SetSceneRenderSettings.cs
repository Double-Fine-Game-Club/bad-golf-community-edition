using UnityEngine;

public class SetSceneRenderSettings : MonoBehaviour
{
	public bool fog = false;
	public Color fogColor = new Color(1f, 1f, 1f, 1f);
	public FogMode fogMode = FogMode.ExponentialSquared;
	public float fogDensity = .01f;
	public float linearFogStart = 0;
	public float linearFogEnd = 300;
	public Color ambientLight = new Vector4(.5f, .5f, .5f, 1f);
	public Material skyboxMaterial = null;
	public float haloStrength = .5f;
	public float flareStrength = 1;
	
	void OnEnable()
	{
		applyToRenderSettings();
	}
	
	public void getFromRenderSettings()
	{
		fog = RenderSettings.fog;
		fogColor = RenderSettings.fogColor;
		fogMode = RenderSettings.fogMode;
		fogDensity = RenderSettings.fogDensity;
		linearFogStart = RenderSettings.fogStartDistance;
		linearFogEnd = RenderSettings.fogEndDistance;
		ambientLight = RenderSettings.ambientLight;
		skyboxMaterial = RenderSettings.skybox;
		haloStrength = RenderSettings.haloStrength;
		flareStrength = RenderSettings.flareStrength;
	}
	
	public void applyToRenderSettings()
	{
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogMode = fogMode;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogStartDistance = linearFogStart;
		RenderSettings.fogEndDistance = linearFogEnd;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.skybox = skyboxMaterial;
		RenderSettings.haloStrength = haloStrength;
		RenderSettings.flareStrength = flareStrength;	
	}
}
