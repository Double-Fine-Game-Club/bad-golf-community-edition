using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerMeter : MonoBehaviour {

    public GameObject m_objectToCircle;
    public GameObject m_markerPrefab;

    [HideInInspector] //expose this later for tweaking
    public float m_meterRadius = 1.0f;
    public int m_meterPoints = 100;

    private int m_meterIndex;
    private GameObject m_marker;
    private SwingMode m_swingScript;
    private float m_percentMaxShotPower = 0.25f;

    private List<GameObject> m_markerList = new List<GameObject>();

	void Start () {
        DrawCirclePoints(m_meterPoints, m_meterRadius, m_objectToCircle.transform.position);
        m_meterIndex = m_meterPoints / 4;
        m_marker = GameObject.Instantiate(m_markerPrefab) as GameObject;
        m_swingScript = FindObjectOfType<SwingMode>();
	}
	
	void Update () {
        if (m_swingScript != null) {
            m_percentMaxShotPower = m_swingScript.GetShowPower() / SwingMode.k_maxShotPower;
            DrawPowerMarker(m_meterPoints, m_percentMaxShotPower);
        }
	}

    void DrawCirclePoints(int points, float radius, Vector3 center)
    {
        float slice = 2 * Mathf.PI / points;


        for (int i = 0; i < points / 2; i++) {
            float angle = slice * 0;
            
            if (i != points - 1) {
                angle = slice * i;
            }

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.z + radius * Mathf.Sin(angle);

            Vector3 pos = new Vector3(x, m_objectToCircle.transform.position.y, y);

            GameObject tmpMarker = GameObject.Instantiate(m_markerPrefab) as GameObject;
            pos.y += 0.1f;
            tmpMarker.transform.position = pos;
            tmpMarker.transform.LookAt(m_objectToCircle.transform.position);
            Color markerColor = Color.Lerp(Color.green, Color.red, (float)i / (points / 2));
            tmpMarker.renderer.material.SetColor("_Color", markerColor);

            m_markerList.Add(tmpMarker);
            tmpMarker.transform.parent = this.gameObject.transform;
        }
    }

    void DrawPowerMarker(int points, float percentToDraw)
    {
        int currIndex = 0;

        foreach (GameObject marker in m_markerList) {
            if (((float)currIndex / (float)m_markerList.Count) < percentToDraw) {
                marker.SetActive(true);
            } else {
                marker.SetActive(false);
            }
            currIndex++;
        }
    }
}
