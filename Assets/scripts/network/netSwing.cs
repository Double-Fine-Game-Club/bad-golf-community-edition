using UnityEngine;
using System.Collections;


//Can refactor this class out since it is a duplicate
/*	//Here for reference
public abstract class SwingBehaviour : MonoBehaviour
{
	public GameObject cameraObject, cart;	//cart is unused
	public int hitMultiplier = 10;
	public const int k_maxShotPower = 500;
	public const int k_maxArcAngle = 80;
	public const int k_minArcAngle = 35;
	public const int k_shotBoost = 3;
	
	public abstract float GetShowPower ();
}
*/

//Attach this script to a network object
public class netSwing : SwingBehaviour {
	public const int k_angleBoost = 5;
	public const float k_swingTimeToHitBall = 0.75f;
	private Vector3 cameraPos = new Vector3 (0, 2, -7);
	private bool showGui = false;
	private float shotPower = 0.0f;
	private float shotAngle = 0.0f;
	private Rect guiBoxPosition = new Rect (100, 200, 100, 100);
	private bool isSwinging=false;
	networkVariables nvs;
	PlayerInfo myInfo;
	PowerMeter meter;

	//Server variables for smoothing
	private float lastAngle = 0.0f;

	// Use this for initialization
	void Start () {
		nvs = FindObjectOfType<networkVariables> ();
		myInfo = nvs.myInfo;
		cameraObject = nvs.myCam.gameObject;	//may not be set yet
		meter = myInfo.characterGameObject.AddComponent ("PowerMeter") as PowerMeter;
		meter.m_objectToCircle = myInfo.characterGameObject;
		meter.m_markerPrefab = Instantiate (Resources.Load ("powerMeterPrefab")) as GameObject;	//	:(
		meter.m_swingScript = this;
	}

	void Update(){
		if(isSwinging){return;}

		// This is where the swing happens.
		if ( myInfo.currentMode==1 && 		//At ball
		    !myInfo.playerIsPaused  		//Not paused
		    )
		{
			bool attemptSwingActionPressed = Input.GetKeyUp(KeyCode.Space); 	//Hit ball key
			#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
			if (Input.touchCount > 0)
			{
				for (int c = 0; c < Input.touchCount; c++) 
				{
					if (Input.GetTouch(c).phase == TouchPhase.Ended) 
					{
						attemptSwingActionPressed = true;
						break;
					}
				}
			}
			#endif
			
			if ( attemptSwingActionPressed )
			{

				isSwinging=true;
				if (shotPower > k_maxShotPower)
					shotPower = k_maxShotPower;
	
				networkView.RPC("GolfSwing", RPCMode.All, shotPower, shotAngle, myInfo.player, myInfo.characterGameObject.transform.parent.rotation.eulerAngles.y);
				shotPower = 0;	
				meter.HideArc();
				//meter.enabled=false; //parent is hidden no reason to hide child
	
				showGui=false;
				//This doesn't disable here because the RPC handlers are needed :/
			}
		}else if ( myInfo.currentMode==1 && 		//At ball
		     !myInfo.playerIsPaused && 		//Not paused
		     Input.GetKeyUp(KeyCode.E)) 	//Hit ball key
		{
			//Leave without swinging
			shotPower = 0;	
			meter.HideArc();
			//meter.enabled=false; //parent is hidden no reason to hide child
			
			//Switch back to cart
			gameObject.SendMessage("switchToCart");
			(GetComponent ("netTransferToSwing") as netTransferToSwing).enabled = true;
			showGui=false;
			//This doesn't disable here because the RPC handlers are needed :/

		}else if(myInfo.currentMode==1){	//not elegant
			meter.enabled=true;	
			showGui=true;
		}
	}

	// Update is called once per fiziks
	void FixedUpdate () {
		if (myInfo.currentMode != 1 || 	//Not at ball
			myInfo.playerIsPaused) { 	//Paused
			return;
		}
		if(isSwinging){
			//Camera stuff
			nvs.myCam.transform.rotation = Quaternion.Euler(
				myInfo.ballGameObject.transform.rotation.eulerAngles.x, 
				myInfo.ballGameObject.transform.rotation.eulerAngles.y,
				0);
			return;
		}
		float powerInput = Input.GetAxis("Vertical");
		float rotationInput = Input.GetAxis ("Horizontal");
		#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
			powerInput = Input.acceleration.y + .5f;
			rotationInput = Input.acceleration.x; 
		#endif

		//GameObject rotationObject = myInfo.ballGameObject;
		GameObject rotationObject = myInfo.characterGameObject.transform.parent.gameObject;	//hack_answers

		// Rotate ball with 'a' and 'd'.
		rotationObject.transform.Rotate (0f, rotationInput, 0f);
		myInfo.ballGameObject.rigidbody.freezeRotation = true;

		// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
		//Vector3 newPos = rotationObject.transform.position + rotationObject.transform.localRotation * cameraPos;
		Vector3 newPos = rotationObject.transform.position + rotationObject.transform.localRotation * cameraPos;	
		float lerper = Mathf.Min ((cameraObject.transform.position - newPos).sqrMagnitude / 100, 1);
		cameraObject.transform.position = (1 - lerper) * cameraObject.transform.position + lerper * newPos;
		cameraObject.transform.rotation = Quaternion.Lerp (cameraObject.transform.rotation, Quaternion.LookRotation (rotationObject.transform.position - cameraObject.transform.position), lerper);
		
		//add remove power with vertical axis
		shotPower += powerInput * hitMultiplier;
		shotPower = Mathf.Clamp( shotPower, 0, k_maxShotPower);
		
		// flies slow in a high arc. Needs tuning.
		// elneilios: Tuned this so that harder shots fly straighter (like real golf!)
		var angleModifier = (shotPower / k_maxShotPower);
		var angleRange = k_maxArcAngle - k_minArcAngle;
		shotAngle = k_maxArcAngle - (angleRange * angleModifier);	
	}

	// Draws the ugly GUI Box that tells you how hard you are about to hit.
	void OnGUI ()
	{
		if(showGui)
			GUI.Box (guiBoxPosition, "power: " + (int)shotPower + "\nangle: " + (int)shotAngle);
	}

	public override float GetShotPower ()
	{
		return shotPower;
	}

	//Unused due to being too stuttery
	[RPC]
	void RotatePlayer(float angle, NetworkMessageInfo info){
		if (Network.isClient)	return;	//Ball will be synchronized by the server

		//Add the assumed angle that would have been added while the packet
		//	was being delivered
		float deltaTime = (float)(info.timestamp - Network.time);
		float deltaAngle = angle - lastAngle;
		lastAngle = angle;
		float adjustedAngle = deltaAngle * deltaTime + angle;

		GameObject playersBall = null;
		foreach (PlayerInfo p in nvs.players) {
			if(p.player==info.sender){
				playersBall=p.ballGameObject;
				break;
			}
		}
		if (playersBall == null)
			throw new UnassignedReferenceException ("Recieved message from unknown NetworkPlayer");
		playersBall.transform.Rotate (0f, adjustedAngle, 0f);
		//myInfo.ballGameObject.rigidbody.freezeRotation = true;
	}

	//Quaternion math can reduce the size of this
	[RPC]
	void GolfSwing(float power, float angle, NetworkPlayer swinger, float ballFacing){
		//if (Network.isClient)	return;	//Ball will be synchronized by the server

		PlayerInfo player = null;
		foreach (PlayerInfo p in nvs.players) {
			if(p.player==swinger){
				player=p;
				break;
			}
		}
		Quaternion ballRotation = player.ballGameObject.transform.rotation;
		Vector3 eulerAnagles = ballRotation.eulerAngles;
		eulerAnagles.y = ballFacing;
		ballRotation.eulerAngles = eulerAnagles;
		player.ballGameObject.transform.rotation = ballRotation;
		
		//play out swing animation then switch to cart
		StartCoroutine(hitGolfBall(player.characterGameObject.animation.GetClip("swing").length, player, power, angle));
				
	}

	IEnumerator hitGolfBall(float time, PlayerInfo player, float power, float angle){
		//play swing animation
		player.characterGameObject.animation.Play("swing",PlayMode.StopAll);
		yield return new WaitForSeconds(k_swingTimeToHitBall);

		//detach character from ball so it doesn't move too
		player.characterGameObject.transform.parent = null;
		if(player==myInfo){
			//follow ball for a bit while it moves
			nvs.myCam.transform.parent = player.ballGameObject.transform;
			nvs.myCam.transform.position = nvs.myCam.transform.position - new Vector3(0,1.5f,0);	//looks better
		}

		Vector3 arc = Vector3.forward;
		arc.y = 0;
		arc.Normalize ();
		arc.y = Mathf.Sin (angle * Mathf.Deg2Rad);
		player.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
		if(Network.isServer){
			//Only server does this. To prevent rubber banding of ball
			player.ballGameObject.rigidbody.AddForce (player.ballGameObject.transform.localRotation * arc * power * k_shotBoost);
		}
		yield return new WaitForSeconds(time-k_swingTimeToHitBall);

		if(player==myInfo){

			//Switch back to cart
			gameObject.SendMessage("switchToCart");
			isSwinging=false;
			(GetComponent ("netTransferToSwing") as netTransferToSwing).enabled = true;
		}
	}
	
}
