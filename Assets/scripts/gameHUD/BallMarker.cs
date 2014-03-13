using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallMarker : NetworkedHUDElement {
    public GameObject m_myBall;
    public GameObject m_myBallMarker;
    public Dictionary<PlayerInfo, GameObject> m_enemyBallMarkers;

    protected bool m_moveUp = false;
    protected float m_positionOffset = 0.0f;
    protected int m_myLastMode = 0;

    protected const float k_maxBallScalar = 1.2f;
    protected const float k_heightOffsetFromBall = 3.0f;
    protected const float k_maxAlphaPercentEnemyMarkers = 0.35f;
    protected const float k_groundedSensitivity = 1.0f;

    protected GameObject m_floor;

	
	public void Update () 
    {
        if (!base.virtualUpdate()) return;

        //check what mode player is in - swinging or driving?
        CheckMode();

        //then, update marker positions
        UpdatePositions();
        
	}

    public override void Initialize()
    {
        base.Initialize();
        if (!m_initialized) return;

        m_myBall = m_myPlayerInfo.ballGameObject;
        m_myBallMarker = GameObject.Instantiate(Resources.Load("ballMarkerPrefab")) as GameObject;

        Vector3 startingPos = m_myPlayerInfo.ballGameObject.transform.position;
        startingPos.y += k_heightOffsetFromBall; //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;

        StartCoroutine(MoveObject(0.0f, 1.0f, 0.5f));

        //initialize enemy ball markers
        m_enemyBallMarkers = new Dictionary<PlayerInfo, GameObject>();
        for (int i = 0; i < m_players.Count; i++) {
            PlayerInfo player = m_players[i];
            // do NOT want to duplicate own ball marker
            if (player != null) {
                if (player != m_myPlayerInfo) {
                    GameObject playerBall = player.ballGameObject;
                    Vector3 thisBallMarkerPos = playerBall.transform.position;
                    thisBallMarkerPos.y += k_heightOffsetFromBall;
                    GameObject thisBallMarker = GameObject.Instantiate(Resources.Load("enemyBallMarkerPrefab")) as GameObject;
                    thisBallMarker.transform.position = thisBallMarkerPos;

                    m_enemyBallMarkers.Add(player, thisBallMarker);
                }
            }
        }
    }

    public override void OnCleanupPlayerList(List<PlayerInfo> toClean)
    {
        foreach (PlayerInfo player in toClean) {
            Destroy(m_enemyBallMarkers[player]);
            m_players.Remove(player);
            m_enemyBallMarkers.Remove(player);
        }
    }

    public override void OnRegisterNewPlayers(List<PlayerInfo> newPlayers)
    {
        foreach (PlayerInfo player in newPlayers) {
            if (!m_enemyBallMarkers.ContainsKey(player)) {
                GameObject playerBall = player.ballGameObject;
                Vector3 thisBallMarkerPos = playerBall.transform.position;
                thisBallMarkerPos.y += k_heightOffsetFromBall;
                GameObject thisBallMarker = GameObject.Instantiate(Resources.Load("enemyBallMarkerPrefab")) as GameObject;
                thisBallMarker.transform.position = thisBallMarkerPos;
                m_enemyBallMarkers.Add(player, thisBallMarker);
            }

            if (!m_players.Contains(player)) {
                m_players.Add(player);
            }
        }
    }

    public override void UpdatePositions()
    {
        Vector3 ballPos = m_myBall.transform.position;
        Vector3 startingPos = new Vector3(ballPos.x, ballPos.y, ballPos.z);
        startingPos.y += (k_heightOffsetFromBall + m_positionOffset); //needs to be high enough to prevent weird collision issues with ball

        m_myBallMarker.transform.position = startingPos;

        m_myBallMarker.transform.rotation = m_myCamera.transform.rotation; //billboard ball marker towards the camera

        UpdateColorScaleToDistance(m_myBallMarker, m_myBall, true);

        foreach (PlayerInfo player in m_players) {
            if (m_enemyBallMarkers.ContainsKey(player)) {
                if (player != m_myPlayerInfo) {
					GameObject playerBall = player.ballGameObject;
                    GameObject playerBallMarker = m_enemyBallMarkers[player];
                    Vector3 thisBallMarkerPos = player.ballGameObject.transform.position;
                    thisBallMarkerPos.y += k_heightOffsetFromBall;
					playerBallMarker.transform.position = thisBallMarkerPos;

					playerBallMarker.transform.rotation = m_myCamera.transform.rotation; //billboard ball marker towards the camera
					UpdateColorScaleToDistance(playerBallMarker, playerBall, false);
                }
            }
        }
    }

    public void UpdateColorScaleToDistance(GameObject objectToUpdate, GameObject associatedBall, bool myBall)
    {
        Renderer objRenderer = objectToUpdate.GetComponentInChildren<Renderer>();

        //if renderer is not obtained, bail out
        if (objRenderer == null) return;
        Color objColor = objRenderer.material.GetColor("_Color");

        if (Mathf.Abs(associatedBall.rigidbody.velocity.y) > k_groundedSensitivity) {
            objColor.a = 0.0f;
            objRenderer.material.SetColor("_Color", objColor);
        } else {
            float distance = Vector3.Distance(objectToUpdate.transform.position, m_myPlayerInfo.cartGameObject.transform.position);

            if (myBall) {
                objColor.a = Mathf.Abs(distance * 0.25f / 10.0f);
            } else {
                objColor.a = Mathf.Min(k_maxAlphaPercentEnemyMarkers, Mathf.Abs(distance * 0.25f / 10.0f));
            }

            Vector3 scale = objectToUpdate.transform.localScale;
            scale.x = Mathf.Max(distance / 15.0f * k_maxBallScalar, k_maxBallScalar);
            scale.y = Mathf.Max(distance / 15.0f * k_maxBallScalar, k_maxBallScalar);

            objectToUpdate.transform.localScale = scale;

            objRenderer.material.SetColor("_Color", objColor);
        }
    }

    //hide markers (layer 12) if swinging
    public void CheckMode()
    {
        int currMode = m_myPlayerInfo.currentMode;
        if (currMode != m_myLastMode) {
            if (currMode == 1) {
                m_myCamera.cullingMask &= ~(1 << 12);
            } else {
                m_myCamera.cullingMask |= (1 << 12);
            }
            m_myLastMode = currMode;
        }
    }

    public IEnumerator MoveObject(float min, float max, float overTime)
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
