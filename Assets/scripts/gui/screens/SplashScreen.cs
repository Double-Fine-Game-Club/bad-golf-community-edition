using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour {

	public Sprite[] splashScreens;

	// Use this for initialization
	void Start () {
		int i = Random.Range(0, splashScreens.Length);
		SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
		renderer.sprite = splashScreens[i];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
