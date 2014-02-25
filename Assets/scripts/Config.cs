using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using Xml = System.Xml;
using ConfigWriter = System.Xml.XmlWriter;
using ConfigReader = System.Xml.XmlDocument;


public class Config : MonoBehaviour {

	private string configPath = "Assets/config.xml";

	static public Dictionary<string, string[]> colorsDictionary = new Dictionary<string, string[]>();

	// Use this for initialization
	void Start () {
		if (System.IO.File.Exists (configPath) == false){
			Xml.XmlWriterSettings settings = new Xml.XmlWriterSettings();
			settings.Indent = true;

			string levelsPath = "Assets/scenes/";
			string[] loaderPath = System.IO.Directory.GetFiles (levelsPath);
			ConfigWriter cfgFile = ConfigWriter.Create(configPath,settings);
			cfgFile.WriteStartElement("Config");
			cfgFile.WriteStartElement("Levels");
			cfgFile.WriteAttributeString ("LevelsPath",levelsPath.ToString());
			
			foreach (string file in loaderPath){
				
				if (file.EndsWith(".meta") == false)
				{
					cfgFile.WriteElementString("Map",file.Remove(0,levelsPath.Length).Remove(file.Length-(levelsPath.Length+6)).ToString());
				}
				
			}
			
			cfgFile.WriteEndElement();
			cfgFile.WriteEndElement ();	
			cfgFile.WriteEndDocument();
			cfgFile.Flush ();
			cfgFile.Close ();
		}
		else
		{
			if ( colorsDictionary.Count > 0 )
				return;

			//load colors 
			ConfigReader cfgFile = new ConfigReader ();
			cfgFile.Load (configPath);

			Xml.XmlNodeList colorNodes = cfgFile.GetElementsByTagName ("Color");
			foreach (Xml.XmlElement nodes in colorNodes)
			{
				colorsDictionary.Add( nodes.GetAttribute("colorName"), new string[]{
																  nodes.GetAttribute("one"),
																  nodes.GetAttribute("two"), 
																  nodes.GetAttribute("three"), 
																  nodes.GetAttribute("four")  });
			}
		}

		//Debug.Log ( "config says hello");
	}
	
	public void cfgLoadLevels(){
		ConfigReader cfgFile = new ConfigReader ();
		cfgFile.Load (configPath);
		
		LevelSelect levelSel = GetComponent<LevelSelect>();
		Xml.XmlNodeList mapNode = cfgFile.GetElementsByTagName ("Map");
		
		int mapCount = mapNode.Count;
		
		levelSel.levels = new string[mapCount];
		int itn = 0;
		
		foreach (Xml.XmlElement nodes in mapNode){
			levelSel.levels[itn] = nodes.InnerText;
			itn = itn+1;
		}
		
#if UNITY_EDITOR
/*
		
		string[] levelsPath = new string[levelSel.levels.Length];


		EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[levelSel.levels.Length];
		
		for (int i = 0; i < levelsPath.Length; i++) {
			levelsPath[i] = levelSel.pathToLevels+"/"+levelSel.levels[i]+".unity";	
			EditorBuildSettingsScene sceneL = new EditorBuildSettingsScene(levelsPath[i],true);
			newSettings[i]=sceneL;
		}
		
		EditorBuildSettings.scenes = newSettings;
*/
#endif

	}
	
}
