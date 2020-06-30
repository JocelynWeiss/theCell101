using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonScript : MonoBehaviour
{
    MeshRenderer m_Renderer;
    public bool m_Clickable = false;
    public bool m_IsReady = false;


    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();

        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.red);
        }

        transform.gameObject.GetComponent<BoxCollider>().enabled = false;
        transform.gameObject.GetComponent<BoxCollider>().isTrigger = false;
    }


    // Set the button as clickable
    public void SetClickable()
    {
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.green);
            m_Clickable = true;
            transform.gameObject.GetComponent<BoxCollider>().enabled = true;
            transform.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        }
    }


    // ---
    private void OnTriggerEnter(Collider other)
    {
        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", Color.yellow);
            StartCoroutine(LaunchGame());
        }
    }


    // ---
    IEnumerator LaunchGame()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // No force applied
        rb.useGravity = true;
        TheCellGameMgr.instance.Audio_Bank[2].Play();

        yield return new WaitForSecondsRealtime(2.0f);
        m_IsReady = true;
    }


    [ContextMenu("LaunchGame")]
    void LaunchTheGame()
    {
        if (m_Clickable)
        {
            StartCoroutine(LaunchGame());
        }
    }
}
