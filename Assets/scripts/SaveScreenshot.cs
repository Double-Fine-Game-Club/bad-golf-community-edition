using UnityEngine;
using System.Collections;

public class SaveScreenshot : MonoBehaviour
{
	private int count = 0;
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown("f12")) //TODO: Get this binding out to somewhere useful
		{
			string fname;
			do
			{
				count ++;
				fname = "screenshot_" + count.ToString("D5") + ".png";

			} while (System.IO.File.Exists(Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar + fname));

			Application.CaptureScreenshot(Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar + fname);
		}
	}
}
