using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerMeter : MonoBehaviour
{

    // the player that the arc will be drawn around
    public GameObject m_objectToCircle;
    // what the pieces of the arc will look like
    public GameObject m_markerPrefab;

    [HideInInspector] //expose these later for tweaking?
    public float m_meterRadius = 1.0f;
    [HideInInspector]
    public int m_meterPoints = 100;

    public SwingBehaviour m_swingScript;
    private float m_percentMaxShotPower = 0.25f;

    private List<GameObject> m_arcChunks = new List<GameObject>();

    void Start()
    {
        CreateArc(m_meterPoints, m_meterRadius, m_objectToCircle.transform.position);
    }

    void Update()
    {
        //proper behavior relies on swingScript being in the scene
		if (m_swingScript != null) {
			Debug.Log("Shot Power: " + m_swingScript.GetShowPower());
			m_percentMaxShotPower = m_swingScript.GetShowPower() / SwingBehaviour.k_maxShotPower;
            DrawArc(m_meterPoints, m_percentMaxShotPower);
        }
    }

    // instantiates the pieces of the arc on start
    void CreateArc(int points, float radius, Vector3 center)
    {
        float slice = 2 * Mathf.PI / points;


        for (int i = 0; i < points / 2; i++) { //points / 2 is used here, because we are drawing *HALF* a circle!
            float angle = slice * 0;

            if (i != points - 1) {
                angle = slice * i;
            }

            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.z + radius * Mathf.Sin(angle);

            Vector3 pos = new Vector3(x, m_objectToCircle.transform.position.y, y);

            GameObject chunk = GameObject.Instantiate(m_markerPrefab) as GameObject;
            pos.y += 0.1f;
            chunk.transform.position = pos;
            chunk.transform.LookAt(m_objectToCircle.transform.position);
            Color markerColor = Color.Lerp(Color.green, Color.red, (float)i / (points / 2));
            chunk.renderer.material.SetColor("_Color", markerColor);

            m_arcChunks.Add(chunk);
            chunk.transform.parent = this.gameObject.transform;
        }
    }

    // draws part of the arc that should be visible according to power of swing
    void DrawArc(int points, float percentToDraw)
    {
        int currIndex = 0;

        foreach (GameObject chunk in m_arcChunks) {
            if (((float)currIndex / (float)m_arcChunks.Count) < percentToDraw) {
                chunk.SetActive(true);
            } else {
                chunk.SetActive(false);
            }
            currIndex++;
        }
    }
}
