using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatBubble : NetworkedHUDElement {

    public static ChatBubble Instance;

    private Dictionary<PlayerInfo, GameObject> m_chatBubbles;

    //tweak this for offest of chat bubble over cart
    private const float k_heightOverCart = 4.2f;

    void Start()
    {
        if (Instance == null) {
            Instance = this;
        }
    }

    void Update()
    {
        base.virtualUpdate();
        //then, update marker positions
        UpdatePositions();

    }

    public override void OnCleanupPlayerList(List<PlayerInfo> toClean)
    {
        foreach(PlayerInfo player in toClean) {
            Destroy(m_chatBubbles[player]);
            m_chatBubbles.Remove(player);
        }
    }

    public override void OnRegisterNewPlayers(List<PlayerInfo> newPlayers)
    {
        foreach(PlayerInfo player in newPlayers) {
            if (!m_chatBubbles.ContainsKey(player)) {
                GameObject playerCart = player.cartGameObject;
                Vector3 thisChatBubblePos = playerCart.transform.position;
                thisChatBubblePos.y += k_heightOverCart;
                GameObject thisChatBubble = GameObject.Instantiate(Resources.Load("chatBubblePrefab")) as GameObject;
                thisChatBubble.transform.position = thisChatBubblePos;
                Renderer objRenderer = thisChatBubble.GetComponentInChildren<Renderer>();

                //if renderer is not obtained, bail out
                if (objRenderer != null) {
                    Color objColor = objRenderer.material.GetColor("_Color");

                    objColor.a = 0.0f;

                    objRenderer.material.SetColor("_Color", objColor);
                }

                m_chatBubbles.Add(player, thisChatBubble);
            }
        }
    }

    public override void UpdatePositions()
    {
        for (int i = 0; i < m_nvs.players.Count; i++) {
            PlayerInfo player = (PlayerInfo)m_nvs.players[i];
            if (player != null) {
                if (m_chatBubbles.ContainsKey(player)) {
                    GameObject playerChatBubble = m_chatBubbles[player];
                    Vector3 thisChatBubblePos = player.cartGameObject.transform.position;
                    thisChatBubblePos.y += k_heightOverCart;
                    playerChatBubble.transform.position = thisChatBubblePos;

                    playerChatBubble.transform.rotation = m_myCamera.transform.rotation; //billboard ball marker towards the camera
                }
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        if (!m_initialized) return;
        
        //initialize enemy ball markers
        m_chatBubbles = new Dictionary<PlayerInfo, GameObject>();

        for (int i = 0; i < m_players.Count; i++) {
            PlayerInfo player = m_players[i];
            if (player != null) {
                GameObject playerCart = player.cartGameObject;
                Vector3 thisChatBubblePos = playerCart.transform.position;
                thisChatBubblePos.y += k_heightOverCart;
                GameObject thisChatBubble = GameObject.Instantiate(Resources.Load("chatBubblePrefab")) as GameObject;
                Renderer objRenderer = thisChatBubble.GetComponentInChildren<Renderer>();

                //if renderer is not obtained, bail out
                if (objRenderer != null) {
                    Color objColor = objRenderer.material.GetColor("_Color");

                    objColor.a = 0.0f;

                    objRenderer.material.SetColor("_Color", objColor);
                }
                thisChatBubble.transform.position = thisChatBubblePos;

                m_chatBubbles.Add(player, thisChatBubble);
            }
        }
    }
    
    //called in from wherever a netChat message is received, giving network ID of player to display bubble over
    public static void DisplayChat(NetworkViewID ID)
    {
        //Debug.Log("Passed ID: " + ID);
        for (int i = 0; i < Instance.m_nvs.players.Count; i++) {
            PlayerInfo player = (PlayerInfo)Instance.m_nvs.players[i];
            if (player != null) {
                //Debug.Log("My ID: " + player.ballViewID);
                if (player.ballViewID == ID) {
                    if (Instance.m_chatBubbles.ContainsKey(player)) {
                        Instance.StartCoroutine(Instance.Display(Instance.m_chatBubbles[player], 1.0f));
                        break;
                    }
                }
            }
        }
    }

    //display chat bubble over a players head, for time "overTime"
    IEnumerator Display(GameObject chatBubble, float overTime)
    {
        float startTime = Time.time;
        Renderer objRenderer = chatBubble.GetComponentInChildren<Renderer>();

        //if renderer is not obtained, bail out
        if (objRenderer != null) {
            Color objColor = objRenderer.material.GetColor("_Color");

            objColor.a = 1.0f;

            objRenderer.material.SetColor("_Color", objColor);

            while (Time.time < startTime + overTime) {
                yield return null;
            }

            if (objRenderer != null) {
                objColor.a = 0.0f;
                objRenderer.material.SetColor("_Color", objColor);
            }
        }
    }
}
