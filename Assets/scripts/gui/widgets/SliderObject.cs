using UnityEngine;
using System.Collections;

public class SliderObject : MonoBehaviour 
{
	public GameObject targetSliderObject;
	public BoxCollider sliderMoveArea;

	public GameObject[] messageTarget;
	public string[] messageName;

	public Camera uiCamera;

	public enum SliderDirectionType
	{
		horizontal,
		vertical
	}

	public SliderDirectionType sliderDirection = SliderDirectionType.horizontal;
 
	private float prevSliderValue = .5f;
	public float sliderValue = .5f;

	bool isDragging = false;	

	public TextMesh numericDisplay;

	void OnEnable()
	{
		if ( sliderDirection == 0)
		{	
			positionSliderToValue( sliderValue, 
			                      new Vector3(sliderMoveArea.bounds.center.x,targetSliderObject.transform.position.y,targetSliderObject.transform.position.z) - new Vector3(sliderMoveArea.bounds.extents.x,0,0),
			                      new Vector3(sliderMoveArea.bounds.center.x,targetSliderObject.transform.position.y,targetSliderObject.transform.position.z) + new Vector3(sliderMoveArea.bounds.extents.x,0,0));
		}
		else
		{
			positionSliderToValue( sliderValue, 
			                      new Vector3(targetSliderObject.transform.position.x, sliderMoveArea.bounds.center.y, targetSliderObject.transform.position.z) - new Vector3(0,sliderMoveArea.bounds.extents.y,0),
			                      new Vector3(targetSliderObject.transform.position.x, sliderMoveArea.bounds.center.y, targetSliderObject.transform.position.z) + new Vector3(0,sliderMoveArea.bounds.extents.y,0));
		}
	}

	void OnMouseDown()
	{
		isDragging = true;
	}
	
	void OnMouseUp()
	{
		isDragging = false;		
	}	

	void Update () 
	{
		if (isDragging)
		{
			moveSliderUsingMousePosition();
		}
	}

	void moveSliderUsingMousePosition()
	{
		Vector3 point =	uiCamera.ScreenToWorldPoint( new Vector3( Input.mousePosition.x, Input.mousePosition.y, targetSliderObject.transform.position.z));
		
		if ( sliderDirection == 0)
		{
			targetSliderObject.transform.position = new Vector3( Mathf.Clamp( point.x, sliderMoveArea.bounds.center.x - sliderMoveArea.bounds.extents.x, sliderMoveArea.bounds.center.x + sliderMoveArea.bounds.extents.x),
			                                                    targetSliderObject.transform.position.y, targetSliderObject.transform.position.z);
		}
		else 
		{
			targetSliderObject.transform.position = new Vector3( targetSliderObject.transform.position.x,
			                                                    Mathf.Clamp( point.y, sliderMoveArea.bounds.center.y - sliderMoveArea.bounds.extents.y, sliderMoveArea.bounds.center.y + sliderMoveArea.bounds.extents.y), 
			                                                    targetSliderObject.transform.position.z);
		}

		calculateCurrentVal();
		updateNumericText();
		sendValueChangeMessage();
	}

	void updateNumericText()
	{
		if ( numericDisplay != null )
		{
			numericDisplay.text = (sliderValue * 100).ToString( "##") + "%";
			if ( numericDisplay.text == "%")
				numericDisplay.text = "0%";
		}
	}

	void calculateCurrentVal()
	{
		if ( sliderDirection == 0)
		{
			sliderValue = remap( targetSliderObject.transform.position.x, 
			                    sliderMoveArea.bounds.center.x - sliderMoveArea.bounds.extents.x,
			                    sliderMoveArea.bounds.center.x + sliderMoveArea.bounds.extents.x,
								0,1);
		}
		else
		{
			sliderValue = remap( targetSliderObject.transform.position.x, 
			                    sliderMoveArea.bounds.center.y - sliderMoveArea.bounds.extents.y,
			                    sliderMoveArea.bounds.center.y + sliderMoveArea.bounds.extents.y,
			                    0,1);
		}
	}

	private float remap(float val, float from1, float to1, float from2, float to2) 
	{
		return (val - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	void positionSliderToValue( float val, Vector3 start, Vector3 end)
	{
		targetSliderObject.transform.position = Vector3.Lerp( start, end, val);
		updateNumericText();
		sendValueChangeMessage();
	}

	void sendValueChangeMessage( )
	{
		if( prevSliderValue != sliderValue)
		{
			if ( messageTarget != null)
			{
				for( int i=0; i< messageTarget.Length; i++ )
				{ 
					messageTarget[i].SendMessage(messageName[i], sliderValue);
				}
			}
			else
			{
				SendMessageUpwards( messageName[0]);	
			}
			prevSliderValue = sliderValue;
		}
	}

	void onBarAreaClicked ()
	{
		moveSliderUsingMousePosition();
	}
}