using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Xml = System.Xml;
using ConfigWriter = System.Xml.XmlWriter;
using ConfigReader = System.Xml.XmlDocument;

public class Config : MonoBehaviour 
{
	private string configFileName = "config";
	private string fileExtension = ".xml";
	private string result;
	private ConfigReader xmlResult;
	
	static private bool loaded = false;
	static public string[] levels;
	static public Dictionary<string, string[]> colorsDictionary = new Dictionary<string, string[]>();

	void Start () 
	{
		if ( loaded)
			return;

		StartCoroutine(retrieveFromPath( Application.persistentDataPath));
	}

	private IEnumerator retrieveFromPath( string basePath)
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

		xmlResult = new ConfigReader ();
		xmlResult.LoadXml(result);
		
		cfgLoadLevels();
		cfgLoadColors();
		cfgLoadOptions();

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
	}
	
	void OnDestroy()
	{
		saveOptions();

		string filePath = System.IO.Path.Combine(Application.persistentDataPath, configFileName + fileExtension);
		System.IO.File.WriteAllText(filePath, xmlResult.OuterXml);
		Debug.Log ( "Saved user config to :" + filePath );
	}
}
