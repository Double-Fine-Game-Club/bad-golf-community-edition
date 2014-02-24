using UnityEngine;
using System.Collections;

public class BallMarker : MonoBehaviour {

    public GameObject m_myBallMarkerPrefab;

    private networkVariables m_nvs;
    private PlayerInfo m_myPlayerInfo;
    private GameObject m_myBall;
    private GameObject m_myBallMarker;

    private bool m_initialized = false;
    private bool m_moveUp = false;
    private float m_positionOffset = 0.0f;


	void Start () 
    {
        AttemptInitialize();
	}
	
	void Update () 
    {
        //never do anything if network variables weren't found
        if (!m_initialized) {
            AttemptInitialize();
        }

        SetPosition();
        
	}

    void SetPosition()
    {
        Vector3 ballPos = m_myBall.transform.position;
        Vector3 startingPos = new Vector3(ballPos.x, ballPos.y, ballPos.z);
        startingPos.y += (2.5f + m_positionOffset); //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;
        m_myBallMarker.transform.rotation = Camera.main.transform.rotation; //billboard ball marker towards the camera
    }

    void AttemptInitialize()
    {
        m_nvs = FindObjectOfType<networkVariables>() as networkVariables;

        //confirm ability to get network variables, else return here without setting initialization flag
        if (m_nvs == null) {
            //Debug.Log("Unable to find network variables!");
            return;
        }

        Initialize();
    }

    void Initialize()
    {
        m_myPlayerInfo = m_nvs.myInfo;
        m_myBall = m_myPlayerInfo.ballGameObject;

        m_myBallMarker = GameObject.Instantiate(m_myBallMarkerPrefab) as GameObject;

        Vector3 startingPos = m_myBall.transform.position;
        startingPos.y += 2.5f; //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;

        //child ball marker to ball 
        //m_myBallMarker.transform.parent = m_myBall.transform;

        StartCoroutine(MoveObject(0.0f, 1.0f, 0.5f));
        m_initialized = true;
    }

    IEnumerator MoveObject(float min, float max, float overTime)
    {
        m_moveUp = !m_moveUp;
        float startTime = Time.time;

        while (Time.time < startTime + overTime) {
            if (m_moveUp) {
                m_positionOffset = Mathf.Lerp(min, max, (Time.time - startTime) / overTime);
            } else {
                m_positionOffset = Mathf.Lerp(max, min, (Time.time - startTime) / overTime);
            }
            yield return null;
        }

        //finalize position (Lerp will leave it slightly off)
        if (m_moveUp) {
            m_positionOffset = max;
        } else {
            m_positionOffset = min;
        }
        
        StartCoroutine(MoveObject(0.0f, 1.0f, 0.5f));
    }
}
