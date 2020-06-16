using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flags : MonoBehaviour
{
    public TheCellGameMgr.GameLanguages m_Language;
    public bool m_IsSelected = false;
    MeshRenderer m_Renderer;
    Color m_ColUnselected = new Color(0.25f, 0.25f, 0.25f);
    Color m_ColSelected = new Color(1.0f, 1.0f, 1.0f);


    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();

        if (m_Renderer)
        {
            m_Renderer.material.SetColor("_BaseColor", m_ColUnselected);
        }
    }


    // ---
    private void OnTriggerEnter(Collider other)
    {
        TheCellGameMgr.instance.m_LocMenu.ChangeLanguageSelection(this);
    }


    // ---
    public void SetOnOff(bool switchOn)
    {
        if (switchOn)
        {
            if (m_IsSelected == false)
            {
                m_IsSelected = true;
                m_Renderer.material.SetColor("_BaseColor", m_ColSelected);
            }
        }
        else
        {
            if (m_IsSelected == true)
            {
                m_IsSelected = false;
                m_Renderer.material.SetColor("_BaseColor", m_ColUnselected);
            }
        }
    }


    [ContextMenu("SelectLanguage")]
    void SelectLanguage()
    {
        TheCellGameMgr.instance.m_LocMenu.ChangeLanguageSelection(this);
    }
}
