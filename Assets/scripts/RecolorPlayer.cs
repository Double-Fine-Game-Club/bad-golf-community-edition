using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecolorPlayer : MonoBehaviour {

	public static void recolorPlayerBody(Renderer bodyRenderer, string color){
		Dictionary<string, string[]> colors = Config.colorsDictionary;
		if(colors == null){ Debug.Log("Color dictionary not found"); return;}
		if(!colors.ContainsKey(color)){ Debug.Log("Color:" + color + " is not supported"); return; }

		string[] myColor = colors [color];		
		Material[] mats = bodyRenderer.materials;
		for(int iter=0; iter<mats.Length; iter++){
			if(iter==3)continue;	//skin color
			Material mat = mats[iter];
			
			string[] split = myColor[0].Split( new char[]{','}); 
			//Debug.Log ( split[0]+","+split[1]+","+split[2] );
			mat.SetColor("_Color01", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
			
			split = myColor[1].Split( new char[]{','}); 
			//Debug.Log ( split[0]+","+split[1]+","+split[2] );
			mat.SetColor("_Color02", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
			
			split = myColor[2].Split( new char[]{','}); 
			//Debug.Log ( split[0]+","+split[1]+","+split[2] );
			mat.SetColor("_Color03", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
			
			split = myColor[3].Split( new char[]{','}); 
			//Debug.Log ( split[0]+","+split[1]+","+split[2] );
			mat.SetColor("_Color04", new Color( float.Parse(split[0])/255, float.Parse(split[1])/255, float.Parse(split[2])/255));
			bodyRenderer.material = mat;
		}

	}

}
