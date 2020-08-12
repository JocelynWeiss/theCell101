using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HmdSettings : MonoBehaviour
{
    MeshRenderer m_Renderer;


    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    private void OnTriggerEnter(Collider other)
    {
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.green);
        }

        //Debug.Log($"===========Setting HMD ID to {name}");
        TheCellGameMgr.instance.SetHmdId(name);
    }


    [ContextMenu("Trigger action")]
    void TriggerAction()
    {
        this.OnTriggerEnter(null);
    }
}
