using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Xml = System.Xml;
using ConfigWriter = System.Xml.XmlWriter;
using ConfigReader = System.Xml.XmlDocument;


public class Config : MonoBehaviour {
	
	private string configPath = "Assets\\badgolf\\config.xml";
	
	// Use this for initialization
	void Start () {
		if (System.IO.File.Exists (configPath) == false){
			Xml.XmlWriterSettings settings = new Xml.XmlWriterSettings();
			settings.Indent = true;
			
			string levelsPath = "Assets//badgolf//scenes";
			string[] loaderPath = System.IO.Directory.GetFiles (levelsPath);
			ConfigWriter cfgFile = ConfigWriter.Create(configPath,settings);
			cfgFile.WriteStartDocument();
			cfgFile.WriteStartElement("Config");
			cfgFile.WriteStartElement("Levels");
			cfgFile.WriteAttributeString ("LevelsPath",levelsPath.ToString());
			
			foreach (string file in loaderPath){
				
				if (file.EndsWith(".meta") == false)
				{
					cfgFile.WriteElementString("Map",file.Remove(0,levelsPath.Length+1).Remove(file.Length-(levelsPath.Length+7)).ToString());
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
		Xml.XmlNodeList node = cfgFile.GetElementsByTagName ("Map");
		int mapCount = node.Count;
		
		levelSel.levels = new string[mapCount];
		int itn = 0;
		
		foreach (Xml.XmlElement nodes in node){
			levelSel.levels[itn] = nodes.InnerText;
			itn = itn+1;
		}
	}
	
}