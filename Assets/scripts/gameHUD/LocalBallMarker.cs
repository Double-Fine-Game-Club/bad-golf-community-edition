using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalBallMarker : BallMarker
{
    public GameObject m_splitView;
    public LayerMask m_playerLayer;

    new public void Update()
    {
        if (!virtualUpdate()) return;

        //check what mode player is in - swinging or driving?
        //CheckMode();

        //then, update marker positions
        UpdatePositions();

    }

    //thanks for being dumb, Unity - apparently overriding Start/Update isn't quite allowed
    public override bool virtualUpdate()
    {
        if (!m_initialized) {
            AttemptInitialize();
            return false;
        }
        return true;
    }

    public override void AttemptInitialize()
    {
        m_splitView = GameObject.FindWithTag("LocalMultiplayerView");

        //confirm ability to get network variables, else return here without setting initialization flag
        if (m_splitView == null) {
            return;
        }

        Initialize();
    }

    public override void Initialize()
    {
        Transform[] children = m_splitView.GetComponentsInChildren<Transform>();
        List<GameObject> playerGameObjects = new List<GameObject>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].gameObject.name.Equals("player")) {
                if (children[i].GetInstanceID() != this.transform.GetInstanceID()) {
                    playerGameObjects.Add(children[i].gameObject);
                }
            }
        }

        //can't do anything else if we don't have PlayerInfo resources loaded!
        m_myPlayerInfo = new PlayerInfo();
        m_myPlayerInfo.ballGameObject = this.transform.FindChild("hit_mode_ball").gameObject;
        m_myPlayerInfo.cartGameObject = this.transform.FindChild("buggy").gameObject;

        if (m_myPlayerInfo.cartGameObject == null || m_myPlayerInfo.ballGameObject == null) return;

        //also make sure we can get the player camera
        m_myCamera = this.transform.FindChild("player_camera").camera;
        if (m_myCamera == null) return;

        //get info from all players
        m_players = new List<PlayerInfo>();

        for (int i = 0; i < playerGameObjects.Count; i++) {
            PlayerInfo info = new PlayerInfo();
            info.ballGameObject = playerGameObjects[i].transform.FindChild("hit_mode_ball").gameObject;
            info.cartGameObject = playerGameObjects[i].transform.FindChild("buggy").gameObject;
            if (info != null) {
                m_players.Add(info);
            }
        }

        m_playerLayer = this.transform.gameObject.layer;
        m_initialized = true;

        if (!m_initialized) return;

        m_myBall = m_myPlayerInfo.ballGameObject;
        m_myBallMarker = GameObject.Instantiate(Resources.Load("ballMarkerPrefab")) as GameObject;
        m_myBallMarker.layer = m_playerLayer;
        m_myBallMarker.transform.FindChild("Cube").gameObject.layer = m_playerLayer;

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
                    thisBallMarker.layer = m_playerLayer;
                    thisBallMarker.transform.FindChild("Cube").gameObject.layer = m_playerLayer;
                    thisBallMarker.transform.position = thisBallMarkerPos;

                    m_enemyBallMarkers.Add(player, thisBallMarker);
                }
            }
        }
    }
}
