using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Xml = System.Xml;
using ConfigWriter = System.Xml.XmlWriter;
using ConfigReader = System.Xml.XmlDocument;

public class Config : MonoBehaviour 
{
	private const string configFileName = "config";
	private const string fileExtension = ".xml";
	
	static private string result;
	static private ConfigReader xmlResult;
	static private bool loaded = false;

	static public string[] levels;
	static public Dictionary<string, string[]> colorsDictionary = new Dictionary<string, string[]>();

	// setup the nvs here
	public networkVariables nvs;

	void Start () 
	{
		// get nvs before anything else
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;

		if ( loaded ) 
			// this gets called anyway in the coroutine?
			cfgLoadOptions();
		else 
			StartCoroutine(retrieveFileFromPath( Application.persistentDataPath));
	}

	private IEnumerator retrieveFileFromPath( string basePath)
	{ 
		string filePath = System.IO.Path.Combine(basePath, configFileName + fileExtension);
		result = "";
		
		if (filePath.Contains("://")) 
		{
			WWW www = new WWW(filePath);
			yield return www;
			if ( www.error != null)
			{
				result = Resources.Load<TextAsset>(configFileName).text;
			}
			else
			{
				result = www.text;
			}
		} 
		else
		{
			#if !UNITY_WEBPLAYER
			try
			{
				if ( System.IO.File.Exists( filePath) )
					result = System.IO.File.ReadAllText(filePath);
				if ( result.Length < 5) //some random lenght more than 0
				{
					TextAsset textAsset = Resources.Load<TextAsset>(configFileName);
					result = textAsset.text;
				}
			}
			catch (System.IO.FileNotFoundException e)
			{
				Debug.Log (e.Data );
				
				result = Resources.Load<TextAsset>(configFileName).text;
			}
			catch( System.IO.IOException e)
			{
				Debug.Log (e.Data );
				
				result = Resources.Load<TextAsset>(configFileName).text;
			}
			#endif
		}

#if UNITY_EDITOR
		// only look at the one in Resources if it's in the editor
		result = Resources.Load<TextAsset>(configFileName).text;
#endif
		xmlResult = new ConfigReader ();
		xmlResult.LoadXml(result);
		
		cfgLoadLevels();
		cfgLoadColors();
		cfgLoadOptions();
		cfgLoadModels();

		loaded = true;
	}

	void cfgLoadLevels()
	{
		Xml.XmlNodeList mapNode = xmlResult.GetElementsByTagName ("Map");
		
		int mapCount = mapNode.Count;
		
		levels = new string[mapCount];
		int itn = 0;
		
		foreach (Xml.XmlElement nodes in mapNode)
		{
			levels[itn] = nodes.InnerText;
			itn = itn+1;
		}
	}

	void cfgLoadColors()
	{	
		Xml.XmlNodeList colorNodes = xmlResult.GetElementsByTagName ("Color");
		foreach (Xml.XmlElement nodes in colorNodes)
		{
			colorsDictionary.Add( nodes.GetAttribute("colorName"), 
								  new string[]{
												nodes.GetAttribute("one"),
												nodes.GetAttribute("two"), 
												nodes.GetAttribute("three"), 
												nodes.GetAttribute("four") 
											  });
		}
	}

	void cfgLoadOptions()
	{
		Xml.XmlNodeList optionNodes = xmlResult.GetElementsByTagName ("Option");
		foreach (Xml.XmlElement nodes in optionNodes)
		{
			string attributeName = nodes.GetAttribute("optionName");
			
			switch (attributeName)
			{
			case "SoundVolume":
				SoundManager.Get().setSoundVolume( float.Parse(nodes.GetAttribute("value")));
				break;
			case "MusicVolume":
				SoundManager.Get().setMusicVolume( float.Parse(nodes.GetAttribute("value")));
				break;
			case "SoundIsOn":
				SoundManager.Get().setSoundOnOff ( bool.Parse(nodes.GetAttribute("value")));
				break;
			case "MusicIsOn":
				SoundManager.Get().setMusicOnOff ( bool.Parse(nodes.GetAttribute("value")));
				break;
			}
		}

		// load server settings
		Xml.XmlNodeList serverInfo = xmlResult.GetElementsByTagName ("ServerInfo");
		foreach (Xml.XmlElement node in serverInfo)
		{
			//should only be 1 node so loops fine - also I couldn't find a way to convert between XmlNode and XmlElement
			nvs.serverVersion = node.GetAttribute("version");						// server version
			nvs.myInfo.name = node.GetAttribute("playername");						// player name
			if (nvs.myInfo.name=="") nvs.myInfo.name = SystemInfo.deviceName;		// make sure it's not blank
			nvs.serverName = node.GetAttribute("hostname");							// server name
			if (nvs.serverName=="") nvs.serverName = nvs.myInfo.name + "'s Server";	// make sure it's not blank
		}
	}

	// setup models
	void cfgLoadModels()
	{
		// should change everything to use this format really in case of duplicate names - actually don't worry it seems ok for now
		//Xml.XmlElement xmodels = xmlResult.GetElementsByTagName("Models").Item(0);	// get all models
		//foreach xmodels.ChildNodes;
		
		Xml.XmlNodeList xcarts = xmlResult.GetElementsByTagName("Cart");			// get all carts
		nvs.buggyModelNames = new string[xcarts.Count];								// make some arrays
		nvs.buggyModels = new string[xcarts.Count];
		int count = 0;
		foreach (Xml.XmlElement node in xcarts)
		{
			// get the type
			string ctype = node.GetAttribute("type").ToLower();
			switch(ctype) {
			case "body":	// if it's a body section
				//Debug.Log("Body found - don't worry it was dead when we got here");
				if (Resources.Load(node.GetAttribute("location"))!=null) {
					// make sure the resource exists
					nvs.buggyModelNames[count] = node.GetAttribute("name");
					nvs.buggyModels[count] = node.GetAttribute("location");
				} else {
					// if it doesn't then complain
					Debug.LogError("Tried to load model at '" + node.GetAttribute("location") + "' but it doesn't exist!");
				}
				break;
				
			default:
				Debug.LogError("Tried to load model type '" + ctype + "' and I don't know how to do that!");
				break;
			}
			count++;
		}
		
		Xml.XmlNodeList xchars = xmlResult.GetElementsByTagName("Character");		// get all characters
		nvs.characteryModelNames = new string[xchars.Count];						// make some arrays
		nvs.characterModels = new string[xchars.Count];
		count = 0;
		foreach (Xml.XmlElement node in xchars)
		{
			// get the type
			string ctype = node.GetAttribute("type").ToLower();
			switch(ctype) {
			case "body":	// if it's a body section
				//Debug.Log("Body found - don't worry it was dead when we got here");
				if (Resources.Load(node.GetAttribute("location"))!=null) {
					// make sure the resource exists
					nvs.characteryModelNames[count] = node.GetAttribute("name");
					nvs.characterModels[count] = node.GetAttribute("location");
				} else {
					// if it doesn't then complain
					Debug.LogError("Tried to load model at '" + node.GetAttribute("location") + "' but it doesn't exist!");
				}
				break;
				
			default:
				Debug.LogError("Tried to load model type '" + ctype + "' and I don't know how to do that!");
				break;
			}
			count++;
		}
		
		Xml.XmlNodeList xballs = xmlResult.GetElementsByTagName("Ball");		// get all balls
		nvs.ballModelNames = new string[xballs.Count];							// make some arrays
		nvs.ballModels = new string[xballs.Count];
		count = 0;
		foreach (Xml.XmlElement node in xballs)
		{
			if (Resources.Load(node.GetAttribute("location"))!=null) {
				// make sure the resource exists
				nvs.ballModelNames[count] = node.GetAttribute("name");
				nvs.ballModels[count] = node.GetAttribute("location");
			} else {
				// if it doesn't then complain
				Debug.LogError("Tried to load model at '" + node.GetAttribute("location") + "' but it doesn't exist!");
			}
			count++;
		}
	}

	void saveOptions()
	{
		Xml.XmlNodeList optionNodes = xmlResult.GetElementsByTagName ("Option");
		foreach (Xml.XmlElement nodes in optionNodes)
		{
			string attributeName = nodes.GetAttribute("optionName");
			
			switch (attributeName)
			{
			case "SoundVolume":
				nodes.SetAttribute("value", SoundManager.Get().sfxVolume.ToString());
				break;
			case "MusicVolume":
           		nodes.SetAttribute("value", SoundManager.Get().musicVolume.ToString());
				break;
			case "SoundIsOn":
				nodes.SetAttribute("value", (!SoundManager.Get().muteSfx).ToString());
				break;
			case "MusicIsOn":
				nodes.SetAttribute("value", (!SoundManager.Get().muteMusic).ToString());
				break;
			}
		}

		// save the server settings
		Xml.XmlNodeList serverInfo = xmlResult.GetElementsByTagName ("ServerInfo");
		foreach (Xml.XmlElement node in serverInfo)
		{
			//should only be 1 node so loops fine - also I couldn't find a way to convert between XmlNode and XmlElement
			// save player name
			node.SetAttribute("playername", nvs.myInfo.name);
			// save server name
			node.SetAttribute("hostname", nvs.serverName);
		}
	}
	
	void OnDestroy()
	{
		// only save if it's not in the editor otherwise editting config.xml does nothing
#if !UNITY_EDITOR
		saveOptions();

		string filePath = System.IO.Path.Combine(Application.persistentDataPath, configFileName + fileExtension);
		System.IO.File.WriteAllText(filePath, xmlResult.OuterXml);
		Debug.Log ( "Saved user config to :" + filePath );
#endif
	}
}
