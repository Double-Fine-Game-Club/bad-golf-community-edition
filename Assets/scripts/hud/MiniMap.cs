using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour {

	public Texture2D flagIcon;
	public Texture2D playerIcon;
	public Texture2D playerDirectionIcon;
	public Texture2D ballIcon;
	public float iconScale = 0.2f;
	public float dirArrowScale = 1.2f;

	//TODO This color should be set to be the client player's color
	public Color playerColor = new Color(0,1,1,1);
	
	public Color opponentColor = new Color(1,0,0,1);

	public Transform flag;
	public GameObject level;
	public Camera mapCamera;

	private Rect playerRect;
	private Rect playerDirRect;

	private List<Rect> opponentRects = null;

	private Rect ballRect;
	private Rect flagRect;
	private float playerAngle;

	private Vector3 camMin;
	private Vector3 camMax;

	private networkVariables nvs;

	// Use this for initialization
	void Start () {

		UpdateCameraBounds();

		float size = (camMax.x-camMin.x) * iconScale;

		playerRect       = new Rect(0,0,size,size);
		playerDirRect    = new Rect(0,0,size,size);
		ballRect         = new Rect(0,0,size,size);
		flagRect         = new Rect(0,0,size,size);

		playerAngle = 0;

		UpdateIconSize();

		nvs = FindObjectOfType<networkVariables>();

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

	float UpdateIconSize() {
		float size = (camMax.x-camMin.x) * iconScale;
		
		playerRect.width = size;
		playerRect.height = size;
		playerDirRect.width = size * dirArrowScale;
		playerDirRect.height = size * dirArrowScale;
		ballRect.width = size;
		ballRect.height = size;
		flagRect.width = size;
		flagRect.height = size;

		return size;
	}
	
	// Update is called once per frame
	void LateUpdate () {

		// TODO : Checking if ball object exists 
		if( !nvs.myInfo.ballGameObject ) {
			return;
		}

		opponentRects = new List<Rect>();

		float size = UpdateIconSize();
		float halfSize = size / 2.0f;

		foreach( PlayerInfo opponent in nvs.players ) {
			if( opponent != nvs.myInfo ) {
				Vector2 c = NormalizedPosition( opponent.cartGameObject.transform.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );
				opponentRects.Add( new Rect(c.x-halfSize,c.y-halfSize,size,size) );
			}
		}

		Transform player = nvs.myInfo.cartGameObject.transform;
		Transform ball = nvs.myInfo.ballGameObject.transform;

		playerAngle = FullAngle(new Vector2(-Vector3.forward.x, -Vector3.forward.z), new Vector2(player.forward.x, player.forward.z ));

		// Alterative approach : Get ScreenPoint directly from world positions
		//Vector3 camPlayer = mapCamera.WorldToScreenPoint( player.position );
		//Vector3 camBall   = mapCamera.WorldToScreenPoint( ball.position );
		//Vector3 camFlag   = mapCamera.WorldToScreenPoint( flag.position );

		//playerPos.center = new Vector2( camPlayer.x, Screen.height - camPlayer.y );
		//ballPos.center   = new Vector2( camBall.x, Screen.height - camBall.y );
		//flagPos.center   = new Vector2( camFlag.x, Screen.height - camFlag.y - flagPos.height * 0.5f );

		playerRect.center = NormalizedPosition( player.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );
		playerDirRect.center = playerRect.center;
		ballRect.center = NormalizedPosition( ball.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );
		flagRect.center = NormalizedPosition( flag.position, level.collider.bounds.min, level.collider.bounds.max, camMin, camMax );

		flagRect.center = new Vector2( flagRect.center.x, flagRect.center.y - flagRect.height * 0.5f);

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

		// TODO : Checking if ball object exists 
		if( !nvs.myInfo.ballGameObject ) {
			return;
		}

		GUI.DrawTexture( flagRect,flagIcon,ScaleMode.ScaleToFit );

		if( opponentRects.Count > 0 ) {
			GUI.color = opponentColor;
			foreach( Rect opponent in opponentRects ) {
				GUI.DrawTexture( opponent, playerIcon, ScaleMode.ScaleToFit );
			}
		}

		GUI.color = playerColor;
		GUI.DrawTexture( playerRect, playerIcon, ScaleMode.ScaleToFit );
		GUI.DrawTexture( ballRect, ballIcon, ScaleMode.ScaleToFit );

		GUIUtility.RotateAroundPivot(playerAngle, playerRect.center); 
		GUI.DrawTexture( playerDirRect, playerDirectionIcon, ScaleMode.ScaleToFit );
	}
}
