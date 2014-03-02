﻿using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour {

	public Texture2D flagIcon;
	public Texture2D playerIcon;
	public Texture2D playerDirectionIcon;
	public Texture2D ballIcon;
	public float iconScale = 0.3f;

	public Color playerColor = new Color(1,0,0,1);
	
	public Transform player;
	public Transform flag;
	public Transform ball;
	public GameObject level;
	public Camera mapCamera;

	private Rect playerPos;
	private Rect ballPos;
	private Rect flagPos;
	private float playerAngle;

	private Vector3 camMin;
	private Vector3 camMax;

	// Use this for initialization
	void Start () {

		UpdateCameraBounds();

		float size = (camMax.x-camMin.x) * iconScale;

		playerPos       = new Rect(0,0,size,size);
		ballPos         = new Rect(0,0,size,size);
		flagPos         = new Rect(0,0,size,size);

		playerAngle = 0;

		UpdateIconSize();

	}

	// Finds full angle of seperation for any 2D vectors
	float FullAngle( Vector2 a, Vector2 b ) {
		float angle = Vector2.Angle(a,b);
		float cross = a.x*b.y - a.y*b.x;
		if (cross > 0) {
			angle = 360 - angle;
		}
		return angle;
	}

	// Obtains the screenspace bounds of the minimap camera
	void UpdateCameraBounds() {
		camMin = mapCamera.WorldToScreenPoint( level.collider.bounds.min );
		camMax = mapCamera.WorldToScreenPoint( level.collider.bounds.max );
	}

	void UpdateIconSize() {
		float size = (camMax.x-camMin.x) * iconScale;
		
		playerPos.width = size;
		playerPos.height = size;
		ballPos.width = size;
		ballPos.height = size;
		flagPos.width = size;
		flagPos.height = size;
	}
	
	// Update is called once per frame
	void LateUpdate () {

		playerAngle = FullAngle(new Vector2(-Vector3.forward.x, -Vector3.forward.z), new Vector2(player.forward.x, player.forward.z ));

		// Alterative approach : Get ScreenPoint directly from world positions
		//Vector3 camPlayer = mapCamera.WorldToScreenPoint( player.position );
		//Vector3 camBall   = mapCamera.WorldToScreenPoint( ball.position );
		//Vector3 camFlag   = mapCamera.WorldToScreenPoint( flag.position );

		//playerPos.center = new Vector2( camPlayer.x, Screen.height - camPlayer.y );
		//ballPos.center   = new Vector2( camBall.x, Screen.height - camBall.y );
		//flagPos.center   = new Vector2( camFlag.x, Screen.height - camFlag.y - flagPos.height * 0.5f );

		playerPos.center = NormalizedPosition( player.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );
		ballPos.center = NormalizedPosition( ball.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );
		flagPos.center = NormalizedPosition( flag.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );

		flagPos.center = new Vector2( flagPos.center.x, flagPos.center.y - flagPos.height * 0.5f);

	}

	Vector2 NormalizedPosition( Vector3 p, Vector3 wMin, Vector3 wMax, Vector3 sMin, Vector3 sMax )
	{
		Vector3 delta = p - wMin;
		Vector3 extent = sMax-sMin;

		Vector2 screenPos = new Vector2( delta.x * extent.x / (wMax.x-wMin.x) + sMin.x, delta.z * extent.y / (wMax.z-wMin.z) + sMin.y);

		// flip y axis
		screenPos.y = Screen.height-screenPos.y;

		return screenPos;
	}

	void OnGUI() {
		GUI.DrawTexture( flagPos,flagIcon,ScaleMode.ScaleToFit );

		GUI.color = playerColor;
		GUI.DrawTexture( playerPos, playerIcon, ScaleMode.ScaleToFit );
		GUI.DrawTexture( ballPos, ballIcon, ScaleMode.ScaleToFit );

		GUIUtility.RotateAroundPivot(playerAngle, playerPos.center); 
		GUI.DrawTexture( playerPos, playerDirectionIcon, ScaleMode.ScaleToFit );
	}
}
