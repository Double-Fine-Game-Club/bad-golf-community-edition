using UnityEngine;
using System.Collections;

//Attach to networkObject so it can communicate with server/client control scripts
//Requires one of controlServer.cs or controlClient.cs
public class netTransferToSwing : MonoBehaviour {

	public GameObject ball, cart;
	private Behaviour ballGlow;
	private GameObject swingIcon;
	private bool inHittingRange = false;
	networkVariables nvs;
	PlayerInfo myInfo;

	bool isInitialized=false;

	void Start()
	{
		nvs = GetComponent ("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		ball = nvs.myInfo.ballGameObject;
		cart = nvs.myInfo.cartGameObject;

		//Create swing Icon for player
		swingIcon = Instantiate (Resources.Load ("swing_icon")) as GameObject;

		//Get glow effect on ball
		ballGlow = ball.GetComponent ("Halo") as Behaviour;
		ballGlow.enabled = false;

		attemptInitialize ();	//for connecting swingIcon to the in level HUD
	}	

	void Update ()
	{
		if (!isInitialized) {
			attemptInitialize();
			return;
		}

		float distance = Vector3.Distance (cart.transform.position, ball.transform.position);
		if (distance < 5) 
		{
			inHittingRange = true;
			ballGlow.enabled=true;
			swingIcon.SetActive (true);
		} 
		else 
		{
			inHittingRange = false;
			ballGlow.enabled=false;
			swingIcon.SetActive (false);
		}
		if (!myInfo.playerIsPaused && inHittingRange) 
		{
			bool attemptGolfActionPressed = Input.GetKeyUp (KeyCode.E);
			#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
				if (Input.touchCount > 0)
				{
					for (int c = 0; c < Input.touchCount; c++) 
					{
						if (Input.GetTouch(c).phase == TouchPhase.Ended) 
						{
						attemptGolfActionPressed = true;
							break;
						}
					}
				}
			#endif

			if ( attemptGolfActionPressed )
			{ 
				//But character at the golf ball
				gameObject.SendMessage("switchToBall");
				ballGlow.enabled=false;
				swingIcon.SetActive (false);
				this.enabled=false;
			}
		}
	}
	
	void onUserGamePadButton()
	{
		if (inHittingRange) 
		{
			gameObject.SendMessage("switchToBall");
			ballGlow.enabled=false;
			swingIcon.SetActive (false);
			this.enabled=false;
		}
	}

	void attemptInitialize(){
		//Attach icon to the HUD
		GameObject hud = GameObject.FindGameObjectWithTag("PlayerHUD");
		if(hud==null){return;}
		swingIcon.transform.parent = hud.transform;
		swingIcon.transform.localPosition = new Vector3 (0f, -3f, 1f);	
		swingIcon.transform.localRotation = Quaternion.identity;
		swingIcon.SetActive (false);	//starts hidden
		isInitialized = true;
	}

	void OnDisable(){
		//ballGlow.enabled = false;
		//swingIcon.SetActive (false);
	}

	void OnDestroy(){
		Destroy (swingIcon);
	}
}
