using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public static GameObject[] createSplitScreenCameras(int numPlayers){
		if(numPlayers<0 || numPlayers>4){ return null; }

		//For hiding UI elements for other players
		int layerMask = 1 << LayerMask.NameToLayer("localmulti_player1")
				      | 1 << LayerMask.NameToLayer("localmulti_player2")	
					  | 1 << LayerMask.NameToLayer("localmulti_player3")
					  | 1 << LayerMask.NameToLayer("localmulti_player4");

		GameObject[] cams = new GameObject[numPlayers];
		for(int i=0; i<numPlayers; ++i){
			GameObject camObj = new GameObject("player_camera");
			Camera cam = camObj.AddComponent<Camera>() as Camera;
			cam.cullingMask = int.MaxValue 
							^ layerMask 
							| 1 << LayerMask.NameToLayer(("localmulti_player" + (i+1).ToString()));
			cams[i] = camObj;
			switch(numPlayers){
			case 1:	
				//full-screen
				break;	
			case 2:
				if(i==0){	//top screen
					cam.rect = new Rect(0f,0f,1f,.5f);
				}else if(i==1){	//bottom screen
					cam.rect = new Rect(0f,0.5f,1f,.5f);
				}
				break;
			case 3:
			case 4:
				if(i==0){		//top-left
					cam.rect = new Rect(0f,0.5f,.5f,.5f);
				}else if(i==1){	//top-right
					cam.rect = new Rect(.5f,0.5f,.5f,.5f);
				}else if(i==2){	//bottom-left
					cam.rect = new Rect(0f,0f,.5f,.5f);
				}else if(i==3){	//bottom-right
					cam.rect = new Rect(.5f,0f,.5f,.5f);
				}
				break;
			default:	//something went wrong
				return null;
			}
		}
		return cams;
	}
}
