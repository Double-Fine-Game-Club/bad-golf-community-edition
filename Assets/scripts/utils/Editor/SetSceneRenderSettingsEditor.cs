using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetSceneRenderSettings))]
public class SetSceneRenderSettingsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Get From Render Settings"))
		{
			((SetSceneRenderSettings)target).getFromRenderSettings();	
		}
		
		if(GUILayout.Button("Apply to Render Settings"))
		{
			((SetSceneRenderSettings)target).applyToRenderSettings();	
		}
	}

}