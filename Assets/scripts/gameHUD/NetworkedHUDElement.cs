using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkedHUDElement : MonoBehaviour {

    public networkVariables m_nvs;
    public PlayerInfo m_myPlayerInfo;
    public Camera m_myCamera;
    public List<PlayerInfo> m_players; //we don't want this list to include the player himself, so we'll have to account for that

    public bool m_initialized;

    //thanks for being dumb, Unity - apparently overriding Start/Update isn't quite allowed
    public virtual bool virtualUpdate()
    {
        if (!m_initialized) {
            AttemptInitialize();
            return false;
        }

        //first, make sure the players we expect are there, or clean up
        //appropriate containing structures
        CheckPlayerListValidity();
        return true;
	}

    public virtual void AttemptInitialize()
    {
        m_nvs = FindObjectOfType<networkVariables>() as networkVariables;

        //confirm ability to get network variables, else return here without setting initialization flag
        if (m_nvs == null) {
            return;
        }

        Initialize();
    }

    public virtual void Initialize()
    {
        //can't do anything else if we don't have PlayerInfo resources loaded!
        m_myPlayerInfo = m_nvs.myInfo;
        if (m_myPlayerInfo.cartGameObject == null || m_myPlayerInfo.ballGameObject == null) return;

        //also make sure we can get the player camera
        m_myCamera = m_nvs.myCam;
        if (m_myCamera == null) return;

        //get info from all players
        m_players = new List<PlayerInfo>();

        //we don't want to use foreachs, for collection iteration errors on disconnect scenarios
        for (int i = 0; i < m_nvs.players.Count; i++) {
            PlayerInfo player = (PlayerInfo)m_nvs.players[i];
            if (player != null) {
                // do NOT want to include the player himself
                if (player != m_myPlayerInfo) {
                    m_players.Add(player);
                }
            }
        }

        m_initialized = true;
    }

    public void CheckPlayerListValidity()
    {
        // -1 is to account for player not being in enemy marker list
        if (m_players.Count < m_nvs.players.Count - 1) {
            RegisterNewPlayers();
        } else if (m_players.Count > m_nvs.players.Count - 1) {
            CleanupPlayerList();
        }
    }

    public virtual void CleanupPlayerList()
    {
        List<PlayerInfo> toClean = new List<PlayerInfo>();
        for (int i = 0; i < m_players.Count; i++) {
            PlayerInfo player = m_players[i];
            if (!m_nvs.players.Contains(player)) {
                m_players.Remove(player);
                toClean.Add(player);
            }
        }

        OnCleanupPlayerList(toClean);
    }

    public virtual void OnCleanupPlayerList(List<PlayerInfo> toClean)
    {
        //implement in subclasses as desired
    }

    public void RegisterNewPlayers()
    {
        List<PlayerInfo> newPlayers = new List<PlayerInfo>();
        for (int i = 0; i < m_nvs.players.Count; i++) {
            PlayerInfo player = (PlayerInfo)m_nvs.players[i];
            if (player != null) {
                if (!m_players.Contains(player)) {
                    m_players.Add(player);
                    newPlayers.Add(player);
                }
            }
        }

        OnRegisterNewPlayers(newPlayers);

    }

    public virtual void OnRegisterNewPlayers(List<PlayerInfo> newPlayers)
    {
        //implement in subclasses as desired
    }

    public virtual void UpdatePositions()
    {
        //implement in subclasses as desired
    }
}
