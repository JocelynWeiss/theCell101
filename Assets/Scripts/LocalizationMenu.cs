using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationMenu : MonoBehaviour
{
    public GameObject m_StartButton;
    //public GameObject m_FlagFr;
    //public GameObject m_FlagUk;
    public Flags m_FlagFr;
    public Flags m_FlagUk;


    // ---
    private void Awake()
    {
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }


    // ---
    private void FixedUpdate()
    {
        /*
        if (m_FlagFr.m_IsSelected)
        {
            if (m_FlagUk.m_IsSelected)
            {

            }
        }        
        */
    }


    public void SwitchLanguage(Flags newFlag)
    {
        if (m_FlagFr.m_IsSelected && newFlag != m_FlagFr)
        {
            m_FlagFr.SetOnOff(false);
        }

        if (m_FlagUk.m_IsSelected && newFlag != m_FlagUk)
        {
            m_FlagUk.SetOnOff(false);
        }

        if (newFlag.m_IsSelected == false)
        {
            newFlag.SetOnOff(true);
        }
    }
}
