using UnityEngine;
using System.Collections;

public class networkPlayerLoad : MonoBehaviour {
	public networkVariables nvs;
	PlayerInfo myInfo;

	// Use this for initialization
	void Start () {
		myInfo = nvs.myInfo as PlayerInfo;
		myInfo.cartGameObject.transform.position = new Vector3 (-17, 7, 116);
		myInfo.ballGameObject.transform.position = new Vector3 (-17,7,116);
		myInfo.characterGameObject.transform.position = new Vector3 (-17, 6, 116);
		
		// player's cam initialization
		GameObject camObj = Instantiate(Resources.Load("playerCam"),new Vector3(-17,7,116), Quaternion.identity) as GameObject;
		FollowPlayerScript flwCam = camObj.GetComponent(typeof(FollowPlayerScript)) as FollowPlayerScript;
		flwCam.target = nvs.myInfo.cartGameObject.transform;
		nvs.myCam = camObj.camera;

		PowerMeter bpm = myInfo.ballGameObject.AddComponent (typeof(PowerMeter)) as PowerMeter;
		bpm.m_objectToCircle = myInfo.ballGameObject;
		bpm.m_markerPrefab = Resources.Load ("powerMeterPrefab") as GameObject;
		bpm.enabled = false;

		// set the follow camera for car audio 
		(myInfo.cartGameObject.GetComponent (typeof(CarAudio)) as CarAudio).followCamera = nvs.myCam;
	}

	public void CarScriptToggler(PlayerInfo pInfo,bool isEnabled){
		(pInfo.cartGameObject.GetComponent(typeof(CarController)) as CarController).enabled = isEnabled;
		(pInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl).enabled = isEnabled;
	}
	
	public void BallScriptToggler(PlayerInfo pInfo,bool isEnabled){
		(pInfo.ballGameObject.GetComponent(typeof(PowerMeter)) as PowerMeter).enabled = isEnabled;
		(pInfo.ballGameObject.GetComponent(typeof(InControlSwingMode)) as InControlSwingMode).enabled = isEnabled;
		myInfo.ballGameObject.transform.FindChild("hit_ball_camera").gameObject.SetActive(isEnabled);
		PowerMeter bpm = myInfo.ballGameObject.GetComponent(typeof(PowerMeter)) as PowerMeter;
		bpm.enabled = isEnabled;
	}

	public void CarScriptToggler(networkVariables netVars,bool isEnabled){
		(netVars.myInfo.cartGameObject.GetComponent(typeof(CarController)) as CarController).enabled = isEnabled;
		(netVars.myInfo.cartGameObject.GetComponent(typeof(CarUserControl)) as CarUserControl).enabled = isEnabled;
	}

	public void BallScriptToggler(networkVariables netVars,bool isEnabled){
		(netVars.myInfo.ballGameObject.GetComponent(typeof(PowerMeter)) as PowerMeter).enabled = isEnabled;
		(netVars.myInfo.ballGameObject.GetComponent(typeof(InControlSwingMode)) as InControlSwingMode).enabled = isEnabled;
		netVars.myInfo.ballGameObject.transform.FindChild("hit_ball_camera").gameObject.SetActive(isEnabled);
		PowerMeter bpm = netVars.myInfo.ballGameObject.GetComponent(typeof(PowerMeter)) as PowerMeter;
		bpm.enabled = isEnabled;
	}
}
