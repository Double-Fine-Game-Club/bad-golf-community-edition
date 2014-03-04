using UnityEngine;
using System.Collections;


//Can refactor this class out since it is a duplicate
/*	//Here for reference
public abstract class SwingBehaviour : MonoBehaviour
{
	public GameObject camera, cart;	//cart is unused
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
	private Vector3 cameraPos = new Vector3 (0, 2, -7);
	private bool flying = false;
	private bool showGui = false;
	private float shotPower = 0.0f;
	private float shotAngle = 0.0f;
	private float timer=0.0f;
	private Rect guiBoxPosition = new Rect (100, 200, 100, 100);
	networkVariables nvs;
	PlayerInfo myInfo;
	PowerMeter meter;

	//Server variables for smoothing
	private float lastAngle = 0.0f;
	private double lastTime = 0.0;

	// Use this for initialization
	void Start () {
		nvs = FindObjectOfType<networkVariables> ();
		myInfo = nvs.myInfo;
		camera = nvs.myCam.gameObject;	//may not be set yet
		meter = myInfo.characterGameObject.AddComponent ("PowerMeter") as PowerMeter;
		meter.m_objectToCircle = myInfo.characterGameObject;
		meter.m_markerPrefab = Instantiate (Resources.Load ("powerMeterPrefab")) as GameObject;	//	:(
		meter.m_swingScript = this;
	}

	void Update(){
		// This is where the swing happens.
		if ( myInfo.currentMode==1 && 		//At ball
		    !myInfo.playerIsPaused && 		//Not paused
		    Input.GetKeyUp(KeyCode.Space)) 	//Hit ball key
		{
			flying = true;
			if (shotPower > k_maxShotPower)
				shotPower = k_maxShotPower;
			networkView.RPC("GolfSwing", RPCMode.Server, shotPower, shotAngle, myInfo.player, myInfo.characterGameObject.transform.parent.rotation);

			myInfo.ballGameObject.transform.rotation = myInfo.characterGameObject.transform.parent.rotation;	//hack_answers

			Vector3 arc = Vector3.forward;
			arc.y = 0;
			arc.Normalize ();
			arc.y = Mathf.Sin (shotAngle * Mathf.Deg2Rad);
			myInfo.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
			myInfo.ballGameObject.rigidbody.AddForce (myInfo.ballGameObject.transform.localRotation * arc * shotPower * k_shotBoost);

			shotPower = 0;	
			meter.HideArc();
			meter.enabled=false;

			//Switch back to cart
			gameObject.SendMessage("switchToCart");
			(GetComponent ("netTransferToSwing") as netTransferToSwing).enabled = true;
			showGui=false;
			//This doesn't disable here because the RPC handlers are needed :/

		}else if ( myInfo.currentMode==1 && 		//At ball
		     !myInfo.playerIsPaused && 		//Not paused
		     Input.GetKeyUp(KeyCode.E)) 	//Hit ball key
		{
			//Leave without swinging
			shotPower = 0;	
			meter.HideArc();
			meter.enabled=false;
			
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

		float powerInput = Input.GetAxis("Vertical");
		float rotationInput = Input.GetAxis ("Horizontal");
		//GameObject rotationObject = myInfo.ballGameObject;
		GameObject rotationObject = myInfo.characterGameObject.transform.parent.gameObject;	//hack_answers

		// Rotate ball with 'a' and 'd'.
		rotationObject.transform.Rotate (0f, rotationInput, 0f);
		myInfo.ballGameObject.rigidbody.freezeRotation = true;

		// Crappy camera script taken from the original movement.cs. Makes rotation around the ball possible.
		//Vector3 newPos = rotationObject.transform.position + rotationObject.transform.localRotation * cameraPos;
		Vector3 newPos = rotationObject.transform.position + rotationObject.transform.localRotation * cameraPos;	
		float lerper = Mathf.Min ((camera.transform.position - newPos).sqrMagnitude / 100, 1);
		camera.transform.position = (1 - lerper) * camera.transform.position + lerper * newPos;
		camera.transform.rotation = Quaternion.Lerp (camera.transform.rotation, Quaternion.LookRotation (rotationObject.transform.position - camera.transform.position), lerper);
		
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

	public override float GetShowPower ()
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
	void GolfSwing(float power, float angle, NetworkPlayer swinger, Quaternion ballFacing){
		if (Network.isClient)	return;	//Ball will be synchronized by the server

		PlayerInfo player = null;
		foreach (PlayerInfo p in nvs.players) {
			if(p.player==swinger){
				player=p;
				break;
			}
		}
		player.ballGameObject.transform.rotation = ballFacing;

		if (power > k_maxShotPower)
			power = k_maxShotPower;
		Vector3 arc = Vector3.forward;
		arc.y = 0;
		arc.Normalize ();
		arc.y = Mathf.Sin (angle * Mathf.Deg2Rad);
		player.ballGameObject.rigidbody.constraints = RigidbodyConstraints.None;
		player.ballGameObject.rigidbody.AddForce (player.ballGameObject.transform.rotation * arc * power * k_shotBoost);
	}
	
}
