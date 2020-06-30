using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismMove : MonoBehaviour
{
    // Which orientation is it facing
    public TheCellGameMgr.CardinalPoint cardinal;
    public float m_BackSpeedFactor = 100.0f;
    [ViewOnly] public bool m_modelSet = false;
    [ViewOnly] public bool m_forceActionning = false;   // Will do as if player was actionning the mechanism
    private bool m_actionTriggered = false;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;
    public bool m_IsOn = true;


    private void Awake()
    {
        /*
        //--- attached the instanciated model
        GameObject genAModel = TheCellGameMgr.instance.m_CentreModels.m_GenCellA.gameObject;
        GameObject h1 = null;
        GameObject h2 = null;

        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                genAModel = TheCellGameMgr.instance.m_NorthModels.m_GenCellA.gameObject;
                h1 = genAModel.transform.Find("Trap_1").gameObject;
                h2 = h1.transform.Find("manche_base").gameObject;
                break;
            case TheCellGameMgr.CardinalPoint.East:
                genAModel = TheCellGameMgr.instance.m_EastModels.m_GenCellA.gameObject;
                h1 = genAModel.transform.Find("trap_3").gameObject;
                h2 = h1.transform.Find("manche_base 3").gameObject;
                break;
            case TheCellGameMgr.CardinalPoint.South:
                genAModel = TheCellGameMgr.instance.m_SouthModels.m_GenCellA.gameObject;
                h1 = genAModel.transform.Find("trap_0").gameObject;
                h2 = h1.transform.Find("manche_base 1").gameObject;
                break;
            case TheCellGameMgr.CardinalPoint.West:
                genAModel = TheCellGameMgr.instance.m_WestModels.m_GenCellA.gameObject;
                h1 = genAModel.transform.Find("trap_2").gameObject;
                h2 = h1.transform.Find("manche_base 2").gameObject;
                break;
        }

        if (h2 != null)
        {
            m_Model = h2;
            //Debug.Log($"[MechanismMove] Awake. {transform.name}\\{m_Model.name} in {m_Model.transform.position}");
            Debug.Log($"[MechanismMove] Awake-> {transform.name}\\{transform.parent.name}");
        }
        //*/

        //Debug.Log($"[MechanismMove] Awake-> {transform.name}\\{transform.parent.name}");
        //gameObject.SetActive(false);
    }


    private void OnEnable()
    {
    }


    private void OnDisable()
    {
        //Debug.Log($"{gameObject.name} -> {cardinal}");
    }


    // Update is called once per frame
    void Update()
    {
        bool actionning = false;
        /*
        if (Input.GetKey(KeyCode.A))
        {
            if (cardinal == TheCellGameMgr.CardinalPoint.North)
            {
                Debug.Log($"{gameObject.name} euler = {transform.eulerAngles}");
            }
            actionning = true;
        }
        */

        // Check for hands
        if ((m_leftIndexIn) || (m_rightIndexIn))
        {
            if (TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                actionning = true;
            }
            if (TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                actionning = true;
            }
        }

        AudioSource snd = TheCellGameMgr.instance.Audio_UseLevers;
        if ((actionning) || (m_forceActionning))
        {
            //--- Snd ---
            if ((snd.isPlaying == false) && (m_actionTriggered == false))
            {
                snd.transform.SetParent(transform);
                snd.transform.localPosition = Vector3.zero;
                snd.Play();
            }
            //--- Snd ---

            float ex = transform.localRotation.x;
            if (ex < -0.65f)
            {
                if (m_actionTriggered)
                {
                    return;
                }
                else
                {
                    if (snd.isPlaying)
                    {
                        snd.Stop();
                    }
                    
                    m_actionTriggered = TriggerAction();
                }
            }

            //Debug.Log($"==============-> {transform.name}: {m_IsOn} {transform.rotation.eulerAngles}   {transform.localRotation}");

            transform.RotateAround(transform.position, transform.right, Time.deltaTime * -90.0f);
        }
        else
        {
            // Slowly go back in position
            float ex = transform.rotation.eulerAngles.x;
            bool inPlace = true;
            if (ex < 355.0f)
            {
                transform.RotateAround(transform.position, transform.right, Time.deltaTime * Mathf.Sqrt(ex * m_BackSpeedFactor));
                inPlace = false;
            }
            if ((inPlace == false) && (transform.rotation.eulerAngles.x > 355.0f))
            {
                if (snd.isPlaying)
                {
                    snd.Stop();
                    //Debug.Log($"[MechanismMove] Stop sound-> {transform.name}\\{transform.parent.name} = {snd.name} at {ex}");
                }
            }
            
            
            if ((m_actionTriggered == true) && (inPlace == false))
            {
                if (transform.rotation.eulerAngles.x > 355.0f)
                {
                    m_actionTriggered = false; // System is back and can be re used
                    Debug.Log($"[MechanismMove] System back-> {transform.name}\\{transform.parent.name} = {transform.rotation.eulerAngles.x}");
                }
            }
        }
    }


    public bool TriggerAction()
    {
        if (m_IsOn == false)
        {
            TheCellGameMgr.instance.Audio_Bank[12].Play();
            return true;
        }

        OneCellClass current = TheCellGameMgr.instance.GetCurrentCell();
        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                TheCellGameMgr.instance.MoveColumn(true);
                current.MechanismSouth.m_IsOn = false;
                break;
            case TheCellGameMgr.CardinalPoint.East:
                TheCellGameMgr.instance.MoveRow(true);
                current.MechanismWest.m_IsOn = false;
                break;
            case TheCellGameMgr.CardinalPoint.South:
                TheCellGameMgr.instance.MoveColumn(false);
                current.MechanismNorth.m_IsOn = false;
                break;
            case TheCellGameMgr.CardinalPoint.West:
                TheCellGameMgr.instance.MoveRow(false);
                current.MechanismEast.m_IsOn = false;
                break;
        }

        return true;
    }


    private void OnTriggerEnter(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index3);

        if (handIdx == 0)
        {
            m_leftIndexIn = true;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = true;
        }
    }


    private void OnTriggerExit(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index3);

        if (handIdx == 0)
        {
            m_leftIndexIn = false;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = false;
        }
    }
}
