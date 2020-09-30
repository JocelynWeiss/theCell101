using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEventScript : MonoBehaviour
{
    public int m_someInteger = 1;
    public UnityEvent m_myEvent;
    public Color m_colorOn = Color.blue * 0.25f;
    public Color m_colorOff = Color.blue * 0.1f;
    public Color m_colorHardOff = Color.red * 0.1f;
    MeshRenderer m_renderer;    


    private void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        if (m_renderer)
        {
            m_renderer.material.SetColor("_BaseColor", m_colorOff);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Start now integer {m_someInteger++}");
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    // A method that can be set in the inspector
    public void TriggerAction()
    {
        Debug.Log($"Coucou it's me! integer {m_someInteger}");
    }


    public void ChangeGameDifficulty()
    {
        TheCellGameMgr.instance.m_GameDifficulty++;
        if (TheCellGameMgr.instance.m_GameDifficulty >= 3 )
        {
            TheCellGameMgr.instance.m_GameDifficulty = 1;
        }
        Debug.Log($"ChangeGameDifficulty! difficulty {TheCellGameMgr.instance.m_GameDifficulty}");
    }


    private void OnTriggerEnter(Collider other)
    {
        // For more accuracy we limit interaction with one capsule only
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Index3);
        if (handIdx >= 0)
        {
            SimulateEnter();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Index3);
        if (handIdx >= 0)
        {
            SimulateExit();
        }
    }


    [ContextMenu("SimulateEnter")]
    void SimulateEnter()
    {
        if (TheCellGameMgr.instance.Audio_Bank[19].isPlaying == false)
            TheCellGameMgr.instance.Audio_Bank[19].Play();

        if (m_renderer)
        {
            m_renderer.material.SetColor("_BaseColor", m_colorOn);
        }
    }


    [ContextMenu("SimulateExit")]
    void SimulateExit()
    {
        if (TheCellGameMgr.instance.Audio_Bank[18].isPlaying == false)
            TheCellGameMgr.instance.Audio_Bank[18].Play();
        m_someInteger++;

        this.m_myEvent.Invoke();

        if (m_renderer)
        {
            if (TheCellGameMgr.instance.m_GameDifficulty == 2)
                m_renderer.material.SetColor("_BaseColor", m_colorHardOff);
            else
                m_renderer.material.SetColor("_BaseColor", m_colorOff);
        }
    }
}
