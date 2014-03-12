using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerMeter : MonoBehaviour
{
    // the player that the arc will be drawn around
    public GameObject m_objectToCircle;
    // what the pieces of the arc will look like
    public GameObject m_markerPrefab;

    [HideInInspector] //expose this later for tweaking?
    public float m_meterRadius = 1.0f;

    public SwingBehaviour m_swingScript;
    private float m_percentMaxShotPower = 0.25f;

    private List<GameObject> m_arcChunks = new List<GameObject>();

    private const int k_arcAngleOffset = 145;
    private const int k_chunksDrawnMod = 4; //this scales the number of "chunks" drawn in the power meter arc - lesser is a smoother curve

    void Start()
    {
        CreateArc(m_meterRadius, m_objectToCircle.transform.position);
    }

    void Update()
    {
        //proper behavior relies on swingScript being in the scene
		if (m_swingScript != null) {
			m_percentMaxShotPower = m_swingScript.GetShotPower() / SwingBehaviour.k_maxShotPower;
            DrawArc(m_percentMaxShotPower);
        }
    }

    // instantiates the pieces of the arc on start
    void CreateArc(float radius, Vector3 center)
    {
        for (int i = 0; i < 180; i++) { 
            float angle = i;
            if (angle % k_chunksDrawnMod == 0) {

                angle += k_arcAngleOffset; //modify where the arc is drawn in relation to the player it circles around

                float x = center.x + radius * Mathf.Cos((2 * Mathf.PI) * (angle / 360.0f));
                float y = center.z + radius * Mathf.Sin((2 * Mathf.PI) * (angle / 360.0f));

                Vector3 pos = new Vector3(x, m_objectToCircle.transform.position.y, y);

                GameObject chunk = GameObject.Instantiate(m_markerPrefab) as GameObject;
                pos.y += 0.1f;
                chunk.transform.position = pos;
                chunk.transform.LookAt(m_objectToCircle.transform.position);
                Color markerColor = Color.Lerp(Color.green, Color.red, (float)i / 180.0f);
                chunk.renderer.material.SetColor("_Color", markerColor);

                m_arcChunks.Add(chunk);
                chunk.transform.parent = this.gameObject.transform;
            }
        }
    }

    // draws part of the arc that should be visible according to power of swing
    void DrawArc(float percentToDraw)
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

	public void HideArc(){
		foreach (GameObject chunk in m_arcChunks) {
			chunk.SetActive(false);
		}
	}
}
