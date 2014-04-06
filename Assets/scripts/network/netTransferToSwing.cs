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

	void Start()
	{
		nvs = GetComponent ("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		ball = nvs.myInfo.ballGameObject;
		cart = nvs.myInfo.cartGameObject;

		//Create swing Icon for player
		swingIcon = Instantiate (Resources.Load ("swing_icon")) as GameObject;
		swingIcon.transform.parent = nvs.myCam.transform;
		swingIcon.transform.localPosition = new Vector3 (0f, -0.37f, 1.22f);	
		swingIcon.SetActive (false);	//starts hidden

		//Get glow effect on ball
		ballGlow = ball.GetComponent ("Halo") as Behaviour;
		ballGlow.enabled = false;
	}	

	void Update ()
	{

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
			swingIcon.transform.rotation = nvs.myCam.transform.rotation;	//always facing camera
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

	void OnDisable(){
		//ballGlow.enabled = false;
		//swingIcon.SetActive (false);
	}

	void OnDestroy(){
		Destroy (swingIcon);
	}
}
