using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocalHUDElement : MonoBehaviour
{
    public GameObject m_splitView;
    public PlayerInfo m_myPlayerInfo;
    public Camera m_myCamera;
    public List<PlayerInfo> m_players; //we don't want this list to include the player himself, so we'll have to account for that

    public bool m_initialized;

    public void Update()
    {
        if (!m_initialized) {
            virtualUpdate();
        }
    }

    //thanks for being dumb, Unity - apparently overriding Start/Update isn't quite allowed
    public bool virtualUpdate()
    {
        if (!m_initialized) {
            AttemptInitialize();
            return false;
        }
        return true;
    }

    public virtual void AttemptInitialize()
    {
        m_splitView = GameObject.FindWithTag("LocalMultiplayerView");

        //confirm ability to get network variables, else return here without setting initialization flag
        if (m_splitView == null) {
            return;
        }

        Initialize();
    }

    public virtual void Initialize()
    {
        Transform[] children = m_splitView.GetComponentsInChildren<Transform>();
        List<GameObject> playerGameObjects = new List<GameObject>();
        for(int i = 0; i < children.Length; i++) {
            if (children[i].gameObject.name.Contains("player")) {
                if (children[i] != this.transform) {
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

        //we don't want to use foreachs, for collection iteration errors on disconnect scenarios
        for (int i = 0; i < playerGameObjects.Count; i++) {
            PlayerInfo info = new PlayerInfo();
            info.ballGameObject = this.transform.FindChild("hit_mode_ball").gameObject;
            info.cartGameObject = this.transform.FindChild("buggy").gameObject;
            if (info != null) {
                m_players.Add(info);
            }
        }

        m_initialized = true;
    }

    public virtual void UpdatePositions()
    {
        //implement in subclasses as desired
    }
}
