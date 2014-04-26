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

	// could move these over to netObj?
	static public string[] levels;
	static public Dictionary<string, string[]> colorsDictionary = new Dictionary<string, string[]>();

	// setup the nvs here
	public networkVariables nvs;

	IEnumerator Start () 
	{
		// wait for the level to load before touching anything otherwise we can get bad references
		yield return new WaitForSeconds(2);
		// get nvs before anything else
		nvs = GameObject.FindWithTag("NetObj").GetComponent("networkVariables") as networkVariables;

		if ( loaded ) {
			// this gets called anyway in the coroutine?
			cfgLoadOptions();
			cfgLoadLevels();
			cfgLoadModels();
		} else  {
			Debug.Log(Application.persistentDataPath);
			StartCoroutine(retrieveFileFromPath( Application.persistentDataPath));
		}
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

		// load xml from computer
		xmlResult = new ConfigReader ();
		xmlResult.LoadXml(result);

		// local xml from game
		ConfigReader xmlGame = new ConfigReader ();
		xmlGame.LoadXml(Resources.Load<TextAsset>(configFileName).text);

		//check version numbers
		if (xmlResult.GetElementsByTagName("Version").Count==0){
			Debug.Log("Updating Config.xml from build 'old' to build " + (xmlGame.GetElementsByTagName("Version")[0] as Xml.XmlElement).GetAttribute("build"));
			// update to latest
			cfgUpdate();
		} else if(int.Parse((xmlResult.GetElementsByTagName("Version")[0] as Xml.XmlElement).GetAttribute("build")) < int.Parse((xmlGame.GetElementsByTagName("Version")[0] as Xml.XmlElement).GetAttribute("build"))) {
			Debug.Log("Updating Config.xml from build " + (xmlResult.GetElementsByTagName("Version")[0] as Xml.XmlElement).GetAttribute("build") +
			          " to build " + (xmlGame.GetElementsByTagName("Version")[0] as Xml.XmlElement).GetAttribute("build"));
			// update to latest
			cfgUpdate();
		}

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


		/* load server settings
		Xml.XmlNodeList serverInfo = xmlResult.GetElementsByTagName ("ServerInfo");
		foreach (Xml.XmlElement node in serverInfo)
		{
			//should only be 1 node so loops fine - also I couldn't find a way to convert between XmlNode and XmlElement
			nvs.serverVersion = node.GetAttribute("version");						// server version
			nvs.myInfo.name = node.GetAttribute("playername");						// player name
			nvs.serverName = node.GetAttribute("hostname");							// server name
		}
		*/
		// no need to check these elements exist since cfgUpdate should do that
		Xml.XmlElement sevInfo = xmlResult.GetElementsByTagName("ServerInfo")[0] as Xml.XmlElement;
		nvs.myInfo.name = sevInfo.GetAttribute("playername");
		if (nvs.myInfo.name=="" || nvs.myInfo.name==null) nvs.myInfo.name = SystemInfo.deviceName;		// make sure it's not blank
		nvs.serverName = sevInfo.GetAttribute("hostname");
		if (nvs.serverName=="" || nvs.serverName==null) nvs.serverName = nvs.myInfo.name + "'s Server";	// make sure it's not blank
		string tmpNATmode = sevInfo.GetAttribute("NATmode");
		if (tmpNATmode=="" || tmpNATmode==null || !int.TryParse(tmpNATmode, out nvs.NATmode)) tmpNATmode = "-1";			// make sure it's not blank
		nvs.NATmode = int.Parse(tmpNATmode);
	}

	// setup models
	void cfgLoadModels()
	{
		// should change everything to use this format really in case of duplicate names - actually don't worry it seems ok for now
		// something like:
		//Xml.XmlElement xmodels = xmlResult.GetElementById("ModelList");	// get all models
		//foreach in xmodels.ChildNodes;
		
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
		nvs.characterModelNames = new string[xchars.Count];						// make some arrays
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
					nvs.characterModelNames[count] = node.GetAttribute("name");
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
		Xml.XmlElement sevInfo = xmlResult.GetElementsByTagName("ServerInfo")[0] as Xml.XmlElement;
		sevInfo.SetAttribute("playername",nvs.myInfo.name);
		sevInfo.SetAttribute("hostname",nvs.serverName);
		sevInfo.SetAttribute("NATmode",nvs.NATmode.ToString());
		//putting a / after the * here breaks MonoDevelop's tab system - stupid piece of
		/*xmlResult.GetElementById("sih").GetAttribute("
		Xml.XmlNodeList serverInfo = xmlResult.GetElementsByTagName ("ServerInfo");
		foreach (Xml.XmlElement node in serverInfo)
		{
			//should only be 1 node so loops fine - also I couldn't find a way to convert between XmlNode and XmlElement
			// save player name
			node.SetAttribute("playername", nvs.myInfo.name);
			// save server name
			node.SetAttribute("hostname", nvs.serverName);
		}*/
	}
	
	void OnDestroy()
	{
		saveOptions();

		string filePath = System.IO.Path.Combine(Application.persistentDataPath, configFileName + fileExtension);
		System.IO.File.WriteAllText(filePath, xmlResult.OuterXml);
		Debug.Log ( "Saved user config to : " + filePath );
	}

	// called when we need to update the config
	// currently it only saves options
	void cfgUpdate()
	{
		//make reference
		ConfigReader xmlGame = new ConfigReader();
		xmlGame.LoadXml(Resources.Load<TextAsset>(configFileName).text);
		//copy options to xmlGame
		string[] optionList = new string[4];
		Xml.XmlNodeList optionNodes = xmlResult.GetElementsByTagName ("Option");
		foreach (Xml.XmlElement nodes in optionNodes)
		{
			string attributeName = nodes.GetAttribute("optionName");
			
			switch (attributeName)
			{
			case "SoundVolume":
				optionList[0] = nodes.GetAttribute("value");
				break;
			case "MusicVolume":
				optionList[1] = nodes.GetAttribute("value");
				break;
			case "SoundIsOn":
				optionList[2] = nodes.GetAttribute("value");
				break;
			case "MusicIsOn":
				optionList[3] = nodes.GetAttribute("value");
				break;
			}
		}
		optionNodes = xmlGame.GetElementsByTagName ("Option");
		foreach (Xml.XmlElement nodes in optionNodes)
		{
			string attributeName = nodes.GetAttribute("optionName");
			
			switch (attributeName)
			{
			case "SoundVolume":
				nodes.SetAttribute("value",optionList[0]);
				break;
			case "MusicVolume":
				nodes.SetAttribute("value",optionList[1]);
				break;
			case "SoundIsOn":
				nodes.SetAttribute("value",optionList[2]);
				break;
			case "MusicIsOn":
				nodes.SetAttribute("value",optionList[3]);
				break;
			}
		}

		// copy playername, hostname and NATmode to xmlGame if they exist
		if(xmlResult.GetElementsByTagName("ServerInfo").Count!=0) {
			Xml.XmlElement sevInfo = xmlResult.GetElementsByTagName("ServerInfo")[0] as Xml.XmlElement;
			Xml.XmlElement sevInfoGame = xmlGame.GetElementsByTagName("ServerInfo")[0] as Xml.XmlElement;
			if(sevInfo.GetAttribute("playername")!=null) {
				sevInfoGame.SetAttribute("playername",sevInfo.GetAttribute("playername"));
			}
			if(sevInfo.GetAttribute("hostname")!=null) {
				sevInfoGame.SetAttribute("hostname",sevInfo.GetAttribute("hostname"));
			}
			if(sevInfo.GetAttribute("NATmode")!=null) {
				sevInfoGame.SetAttribute("NATmode",sevInfo.GetAttribute("NATmode"));
			}
		}

		// set xmlResult as xmlGame
		xmlResult = xmlGame;
	}
}
