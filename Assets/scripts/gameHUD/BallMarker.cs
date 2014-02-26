//////////////////////////////////////////////////////////////////////////////////////////////////////
//IMPORTANT NOTE:
//this script must be attached to an object created once in a player's instance, i.e. networkObject
//////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallMarker : MonoBehaviour {
    public GameObject m_myBallMarkerPrefab;
    public GameObject m_enemyBallMarkerPrefab;

    private networkVariables m_nvs;
    private PlayerInfo m_myPlayerInfo;
    private GameObject m_myBall;
    private GameObject m_myBallMarker;
    private Dictionary<PlayerInfo, GameObject> m_enemyBallMarkers;
    private Camera m_myCamera;

    private bool m_initialized = false;
    private bool m_moveUp = false;
    private float m_positionOffset = 0.0f;
    private int m_numPlayersExpected;


	void Start () 
    {
        AttemptInitialize();
	}
	
	void Update () 
    {
        //never do anything if network variables weren't found
        if (!m_initialized) {
            AttemptInitialize();
            return;
        }

        //first, make sure the players we expect are there, or clean up
        //appropriate containing structures
        CheckPlayerListValidity();
        //then, update marker positions
        UpdatePositions();
        
	}

    void CheckPlayerListValidity()
    {
        // -1 is to account for player not being in enemy marker list
        if (m_numPlayersExpected < m_nvs.players.Count - 1) {
            RegisterNewPlayers();
        } else if (m_numPlayersExpected > m_nvs.players.Count - 1) {
            CleanupPlayerList();
        }
    }

    void CleanupPlayerList()
    {
        foreach (PlayerInfo player in m_enemyBallMarkers.Keys) {
            if (!m_nvs.players.Contains(player)) {
                Destroy(m_enemyBallMarkers[player]);
                m_enemyBallMarkers.Remove(player);
                m_numPlayersExpected--;
            }
        }
    }

    void RegisterNewPlayers()
    {
        foreach (PlayerInfo player in m_nvs.players) {
            if (!m_enemyBallMarkers.ContainsKey(player)) {
                if (player != m_myPlayerInfo) {
                    GameObject playerBall = player.ballGameObject;
                    Vector3 thisBallMarkerPos = playerBall.transform.position;
                    thisBallMarkerPos.y += 2.5f;
                    GameObject thisBallMarker = GameObject.Instantiate(m_enemyBallMarkerPrefab) as GameObject;
                    thisBallMarker.transform.position = thisBallMarkerPos;

                    m_enemyBallMarkers.Add(player, thisBallMarker);
                    m_numPlayersExpected++;
                }
            }
        }
    }

    void UpdatePositions()
    {
        Vector3 ballPos = m_myBall.transform.position;
        Vector3 startingPos = new Vector3(ballPos.x, ballPos.y, ballPos.z);
        startingPos.y += (2.5f + m_positionOffset); //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;

        m_myBallMarker.transform.rotation = m_myCamera.transform.rotation; //billboard ball marker towards the camera

        UpdateColor(m_myBallMarker);

        foreach (PlayerInfo player in m_nvs.players) {
            if (m_enemyBallMarkers.ContainsKey(player)) {
                if (player != m_myPlayerInfo) {
                    GameObject playerBall = m_enemyBallMarkers[player];
                    Vector3 thisBallMarkerPos = player.ballGameObject.transform.position;
                    thisBallMarkerPos.y += 2.5f;
                    playerBall.transform.position = thisBallMarkerPos;

                    playerBall.transform.rotation = m_myCamera.transform.rotation; //billboard ball marker towards the camera
                    UpdateColor(playerBall);
                }
            }
        }
    }

    void UpdateColor(GameObject objectToColorize)
    {
        Renderer objRenderer = objectToColorize.GetComponentInChildren<Renderer>();

        //if renderer is not obtained, bail out
        if (objRenderer == null) return;
        Color objColor = objRenderer.material.GetColor("_Color");
        float distance = Vector3.Distance(objectToColorize.transform.position, m_myPlayerInfo.cartGameObject.transform.position);

        objColor.a = Mathf.Abs(distance * 0.25f / 10.0f);

        Vector3 scale = objectToColorize.transform.localScale;
        scale.x = Mathf.Max(distance / 10.0f * 1.5f, 1.5f);
        scale.y = Mathf.Max(distance / 10.0f * 1.5f, 1.5f);

        objectToColorize.transform.localScale = scale;




        objRenderer.material.SetColor("_Color", objColor);
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

        //can't do anything else if we don't have PlayerInfo resources loaded!
        if (m_myPlayerInfo.cartContainerObject == null) return;

        m_myCamera = m_myPlayerInfo.cartContainerObject.transform.FindChild("multi_buggy_cam").gameObject.camera;

        m_myBall = m_myPlayerInfo.ballGameObject;

        //need own ball and camera to be existent to initialize
        if (m_myBall == null || m_myCamera == null) {
            return;
        }

        m_myBallMarker = GameObject.Instantiate(m_myBallMarkerPrefab) as GameObject;

        Vector3 startingPos = m_myBall.transform.position;
        startingPos.y += 2.5f; //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;

        StartCoroutine(MoveObject(0.0f, 1.0f, 0.5f));

        //initialize enemy ball markers
        m_enemyBallMarkers = new Dictionary<PlayerInfo, GameObject>();
        foreach (PlayerInfo player in m_nvs.players) {
            // do NOT want to duplicate own ball marker
            if (player != m_myPlayerInfo) {
                GameObject playerBall = player.ballGameObject;
                Vector3 thisBallMarkerPos = playerBall.transform.position;
                thisBallMarkerPos.y += 2.5f;
                GameObject thisBallMarker = GameObject.Instantiate(m_enemyBallMarkerPrefab) as GameObject;
                thisBallMarker.transform.position = thisBallMarkerPos;

                m_enemyBallMarkers.Add(player, thisBallMarker);
            }
        }

        //set how many players were connected at initialization
        m_numPlayersExpected = m_nvs.players.Count - 1;
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
