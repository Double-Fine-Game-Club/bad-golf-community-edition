﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif

public class PlatformSpecificContent : MonoBehaviour {

	enum BuildTargetGroup {
		Standalone,
		Mobile
    }

	[SerializeField] BuildTargetGroup showOnlyOn;
	[SerializeField] GameObject[] content = new GameObject[0];
	[SerializeField] bool childrenOfThisObject;
    
	#if !UNITY_EDITOR
	void OnEnable()
	{
		CheckEnableContent();
	}
	#endif

	#if UNITY_EDITOR
	
    void OnEnable () {
		EditorUserBuildSettings.activeBuildTargetChanged += Update;
		EditorApplication.update += Update;
    }

    void OnDisable()
    {
		EditorUserBuildSettings.activeBuildTargetChanged -= Update;
		EditorApplication.update -= Update;
    }

	void Update()
    {
		CheckEnableContent();

    }
	#endif

    void CheckEnableContent()
	{
		#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY )
		if (showOnlyOn == BuildTargetGroup.Mobile)
		{
			EnableContent(true);
		} else {
			EnableContent(false);
		}
		#endif
		
		#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY )
		if (showOnlyOn == BuildTargetGroup.Mobile)
		{
            EnableContent(false);
        } else {
            EnableContent(true);
        }
        #endif
        
    }

	void EnableContent(bool enabled)
	{
		if (content.Length > 0)
		{
			foreach (var g in content)
			{
				if (g != null)
				{
					g.SetActive( enabled );
				}
			}
		}
		if (childrenOfThisObject)
		{
			foreach (Transform t in transform)
			{
				t.gameObject.SetActive( enabled );
			}
		}
	}
	

}


