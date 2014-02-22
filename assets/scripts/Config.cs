using UnityEngine;
using System.IO;
using System.Xml;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using Xml = System.Xml;
using ConfigWriter = System.Xml.XmlWriter;
using ConfigReader = System.Xml.XmlDocument;


public class Config : MonoBehaviour {
	
	private string configPath = "assets/config.xml";
	
	// Use this for initialization
	void Start () {
		if (System.IO.File.Exists (configPath) == false){
			Xml.XmlWriterSettings settings = new Xml.XmlWriterSettings();
			settings.Indent = true;
			
			string levelsPath = "assets/scenes/";
			string[] loaderPath = System.IO.Directory.GetFiles (levelsPath);
			ConfigWriter cfgFile = ConfigWriter.Create(configPath,settings);
			cfgFile.WriteStartDocument();
			cfgFile.WriteStartElement("Config");
			cfgFile.WriteStartElement("Levels");
			cfgFile.WriteAttributeString ("LevelsPath",levelsPath.ToString());
			
			foreach (string file in loaderPath){
				
				if (file.EndsWith(".meta") == false)
				{
					cfgFile.WriteElementString("Map",Path.GetFileNameWithoutExtension(file));
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
		cfgFile.Load(configPath);
		
		GameObject gObj = GameObject.FindGameObjectWithTag ("LevelID");
		LevelSelect levelSel = gObj.GetComponent (typeof(LevelSelect)) as LevelSelect;
		XmlNode node = cfgFile.DocumentElement.SelectSingleNode ("Levels");
		XmlNodeList mapNode = node.SelectNodes ("Map");
		string pathToLevels = node.Attributes.GetNamedItem("LevelsPath").Value;

		int mapCount = mapNode.Count;
		
		levelSel.levels = new string[mapCount];
		int itn = 0;
		string[] levelsPath = new string[mapCount];
		
		foreach (Xml.XmlElement nodes in mapNode){
			string path = pathToLevels;

			XmlNode relativePathNode = nodes.Attributes.GetNamedItem("RelativePath");
			if (relativePathNode != null)
			{
				path = Path.Combine(pathToLevels, relativePathNode.Value);
			}

			path = Path.Combine(path, nodes.InnerText + ".unity");
			levelsPath[itn] = path;
			levelSel.levels[itn] = nodes.InnerText;
			itn++;
		}

#if UNITY_EDITOR

		EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[levelSel.levels.Length];
		
		for (int i = 0; i < levelsPath.Length; i++) {
			EditorBuildSettingsScene sceneL = new EditorBuildSettingsScene(levelsPath[i],true);
			newSettings[i]=sceneL;
		}
		
		EditorBuildSettings.scenes = newSettings;
#endif
	}
	
}