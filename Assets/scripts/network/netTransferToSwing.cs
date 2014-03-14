using UnityEngine;
using System.Collections;

//Attach to networkObject so it can communicate with server/client control scripts
//Requires one of controlServer.cs or controlClient.cs
public class netTransferToSwing : MonoBehaviour {

	public GameObject ball, cart;
	// For updating the GUI Box.
	private bool inHittingRange = false;
	public Rect guiBoxPosition = new Rect (100, 200, 100, 100);
	public bool showGUI = false;
	networkVariables nvs;
	PlayerInfo myInfo;
	
	//private Color originalColor = Color.grey;

	void Start()
	{
		//originalColor = ball.renderer.material.GetColor("_ColorTint");
		nvs = GetComponent ("networkVariables") as networkVariables;
		myInfo = nvs.myInfo;
		ball = nvs.myInfo.ballGameObject;
		cart = nvs.myInfo.cartGameObject;
	}	

	void OnEnable(){
		showGUI = true;
	}
	
	void Update ()
	{

		float distance = Vector3.Distance (cart.transform.position, ball.transform.position);
		if (distance < 5) 
		{
			inHittingRange = true;
			//Material temp =	ball.renderer.sharedMaterial; 
			//temp.SetColor("_ColorTint", Color.red);
			//ball.renderer.sharedMaterial = temp;
		} 
		else 
		{
			inHittingRange = false;
			//Material temp =	ball.renderer.sharedMaterial; 
			//temp.SetColor("_ColorTint", originalColor);
			//ball.renderer.sharedMaterial = temp;
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
					showGUI=false;
					this.enabled=false;
			}
		}
	}
	
	// Makes the ugly GUI box that tells when you are close enough to the ball.
	void OnGUI ()
	{
		if( showGUI)
			GUI.Box (guiBoxPosition, "in range: " + inHittingRange);
	}
	
	void onUserGamePadButton()
	{
		if (inHittingRange) 
		{
			if (inHittingRange) 
			{
				gameObject.SendMessage("switchToBall");
				showGUI=false;
				this.enabled=false;
			}
		}
	}
}
