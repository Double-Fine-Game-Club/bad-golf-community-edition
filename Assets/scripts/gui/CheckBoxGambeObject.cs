using UnityEngine;
using System.Collections;

public class CheckBoxGambeObject : MonoBehaviour 
{
	public bool startsChecked = true;	
	public GameObject targetSwitchSprite;

	public GameObject messageTarget;
	public string messageName;
	

	private bool isChecked = true;

	void onCheckBox()
	{
		isChecked = !isChecked;
		
		targetSwitchSprite.SetActive( isChecked);
		doMessageSend();
	}

	void doMessageSend()
	{
		if ( messageTarget != null)
		{
			messageTarget.SendMessage(messageName, isChecked);
		}
		else
		{
			SendMessageUpwards( messageName, isChecked);		
		}
	}

	public void forceMessageSend()
	{
		doMessageSend();
	}

	public void forceCheckBoxAction()
	{
		onCheckBox();
	}

	public void setCheckBox ( bool value)
	{
		isChecked = value;
		targetSwitchSprite.SetActive( isChecked);
		forceMessageSend();
	}
}
