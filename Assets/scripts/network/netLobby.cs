using UnityEngine;
using System.Collections;

public class netLobby : MonoBehaviour {
	networkVariables nvs;
	int IcartModel=0, IballModel=0, IcharacterModel=0;
	float timer = 0;
	bool changeNeeded = false;

	// Use this for initialization
	void Start () {
		nvs = GetComponent("networkVariables") as networkVariables;

		// set models
		nvs.myInfo.cartModel = nvs.buggyModels[0];
		nvs.myInfo.ballModel = nvs.ballModels[0];
		nvs.myInfo.characterModel = nvs.characterModels[0];
	}
	
	// Update is called once per frame
	void Update () {
		// don't spam on change
		timer += Time.deltaTime;
		if (timer > 0.1 && changeNeeded) {
			timer = 0;
			changeNeeded = false;
			networkView.RPC("ChangeModels", RPCMode.Server, nvs.myInfo.cartModel, nvs.myInfo.ballModel, nvs.myInfo.characterModel);
		}
	}

	
	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width/4,40,Screen.width/2,20), nvs.buggyModelNames[IcartModel]))
		{
			// change to next model
			IcartModel += 1;
			IcartModel %= nvs.buggyModels.Length;
			nvs.myInfo.cartModel = nvs.buggyModels[IcartModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,80,Screen.width/2,20), nvs.ballModelNames[IballModel]))
		{
			// change to next model
			IballModel += 1;
			IballModel %= nvs.ballModels.Length;
			nvs.myInfo.ballModel = nvs.ballModels[IballModel];
			changeNeeded = true;
		}
		if (GUI.Button(new Rect(Screen.width/4,120,Screen.width/2,20), nvs.characteryModelNames[IcharacterModel]))
		{
			// change to next model
			IcharacterModel += 1;
			IcharacterModel %= nvs.characterModels.Length;
			nvs.myInfo.characterModel = nvs.characterModels[IcharacterModel];
			changeNeeded = true;
		}
		if (Network.isServer) {
			if (GUI.Button(new Rect(Screen.width/4,160,Screen.width/2,20), "Start!"))
			{
				GetComponent("networkManagerServer").SendMessage("StartGame");
				this.enabled = false;
			}
		}
	}


	// blanks
	[RPC]
	void ChangeModels(string a, string b, string c) {}
}
