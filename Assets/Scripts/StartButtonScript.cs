using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonScript : MonoBehaviour
{
    MeshRenderer m_Renderer;

    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();

        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.cyan);
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.red);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.cyan);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
