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
	
	// Use this for initialization
	void Start () {
		if (System.IO.File.Exists (configPath) == false){
			Xml.XmlWriterSettings settings = new Xml.XmlWriterSettings();
			settings.Indent = true;
			
			string levelsPath = "Assets/scenes/";
			string[] loaderPath = System.IO.Directory.GetFiles (levelsPath);
			ConfigWriter cfgFile = ConfigWriter.Create(configPath,settings);
			cfgFile.WriteStartDocument();
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
	}
	
	public void cfgLoadLevels(){
		ConfigReader cfgFile = new ConfigReader ();
		cfgFile.Load (configPath);
		
		GameObject gObj = GameObject.FindGameObjectWithTag ("LevelID");
		LevelSelect levelSel = gObj.GetComponent (typeof(LevelSelect)) as LevelSelect;
		Xml.XmlNodeList mapNode = cfgFile.GetElementsByTagName ("Map");
		levelSel.pathToLevels = cfgFile.DocumentElement.FirstChild.Attributes[0].Value;
		
		
		int mapCount = mapNode.Count;
		
		levelSel.levels = new string[mapCount];
		int itn = 0;
		
		foreach (Xml.XmlElement nodes in mapNode){
			levelSel.levels[itn] = nodes.InnerText;
			itn = itn+1;
		}
		
		
		string[] levelsPath = new string[levelSel.levels.Length];

#if UNITY_EDITOR

		EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[levelSel.levels.Length];
		
		for (int i = 0; i < levelsPath.Length; i++) {
			levelsPath[i] = levelSel.pathToLevels+"/"+levelSel.levels[i]+".unity";	
			EditorBuildSettingsScene sceneL = new EditorBuildSettingsScene(levelsPath[i],true);
			newSettings[i]=sceneL;
		}
		
		EditorBuildSettings.scenes = newSettings;
#endif

		for (int i = 0; i < levelsPath.Length; i++) {
			levelsPath[i] = levelSel.pathToLevels+"/"+levelSel.levels[i]+".unity";
		}
	}
	
}