using UnityEngine;
using System.Collections;

public class networkLevelLoad : MonoBehaviour {
	public bool NetworkStart;
	networkVariables nvs;
	PlayerInfo myInfo;
	bool playerStarted=false;

	// Use this for initialization
	void Start () {
		if (NetworkStart==true) {
			gameObject.AddComponent (typeof(NetworkView));
			NetworkView netView = gameObject.GetComponent (typeof(NetworkView)) as NetworkView;
			netView.gameObject.AddComponent (typeof(networkManager));
			nvs = netView.gameObject.AddComponent (typeof(networkVariables)) as networkVariables;
		}
	}

	void OnPlayerConnected(){
		playerStarted = true;
	}

	void Update(){
		if (playerStarted==true) {
			myInfo = nvs.myInfo as PlayerInfo;
			myInfo.cartGameObject.transform.position = new Vector3 (-17, 7, 116);
			myInfo.ballGameObject.transform.position = new Vector3 (-17,7,116);


			// player's cam initialization
			GameObject camObj = Resources.Load("playerCam") as GameObject;
//			FollowPlayerScript flwCam = camObj.GetComponent(typeof(FollowPlayerScript)) as FollowPlayerScript;
//			flwCam.target = nvs.myInfo.cartGameObject.transform;
			nvs.myCam = camObj.camera;
//			(myInfo.cartGameObject.GetComponent (typeof(ScriptToggler)) as ScriptToggler).camera = nvs.myCam.gameObject;
//			(myInfo.ballGameObject.GetComponent (typeof(ScriptToggler)) as ScriptToggler).camera = nvs.myCam.gameObject;

//			// set the follow camera for car audio 
//			(myInfo.cartGameObject.GetComponent (typeof(CarAudio)) as CarAudio).followCamera = nvs.myCam;

			// add script to make camera follow player
			FollowPlayerScript flwPlayer = GameObject.Find("playerCam").AddComponent (typeof(FollowPlayerScript)) as FollowPlayerScript;
			flwPlayer.target = myInfo.cartGameObject.transform;

//			// add InControllSwing
//			InControlSwingMode swingBall = myInfo.ballGameObject.AddComponent (typeof(InControlSwingMode)) as InControlSwingMode;
//			swingBall.camera = nvs.myCam.gameObject;

//			// set ScriptTogglers camera
//			(myInfo.cartGameObject.GetComponent (typeof(ScriptToggler)) as ScriptToggler).camera = nvs.myCam.gameObject;
//			(myInfo.ballGameObject.GetComponent (typeof(ScriptToggler)) as ScriptToggler).camera = nvs.myCam.gameObject;

			// add the transfer to swing script to allow the player to go from cart to ball.
			TransferToSwing transfSwing = myInfo.cartGameObject.GetComponent (typeof(TransferToSwing)) as TransferToSwing;
			transfSwing.ball = myInfo.ballGameObject;
			TransferToSwing transfSwingBall = myInfo.ballGameObject.GetComponent (typeof(TransferToSwing)) as TransferToSwing;
			transfSwingBall.ball = myInfo.ballGameObject;

//			// add the script to the ball
//			PowerMeter bpm = myInfo.ballGameObject.AddComponent ("PowerMeter") as PowerMeter;
//
//			// set things that the script needs
//			bpm.m_objectToCircle = myInfo.ballGameObject;
//			bpm.m_markerPrefab = Resources.Load ("powerMeterPrefab") as GameObject;
//			bpm.enabled = false;
			playerStarted = false;
		}
	}

	public void StartLoad(bool startLoad){
		NetworkStart = startLoad;
	}
}
