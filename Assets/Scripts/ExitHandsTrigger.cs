using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHandsTrigger : MonoBehaviour
{
    private bool m_actionTriggered = false;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;
    [ViewOnly] public GameObject m_HatchModel;


    private void Awake()
    {
        //--- attached the instanciated model
        GameObject exitmodel = TheCellGameMgr.instance.m_CentreModels.m_ExitCell.gameObject;
        GameObject trap_exit = exitmodel.transform.Find("trap_exit").gameObject;
        GameObject hatch = trap_exit.transform.Find("door_exit").gameObject;
        if (hatch != null)
        {
            m_HatchModel = hatch;
            Debug.Log($"[ExitHandsTrigger] Awake. {transform.name}, model: {m_HatchModel.name} in {m_HatchModel.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[ExitHandsTrigger] Awake. {transform.name}, model: {m_HatchModel.name} couldn't be found...");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (m_actionTriggered)
            return;

        bool actionning = false;
        if (Input.GetKey(KeyCode.B))
        {
            //Debug.Log($"ExitPos = {transform.position} at {Time.fixedTime}");
            actionning = true;
        }

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

        if ((actionning) && (!m_actionTriggered))
        {
            if (transform.position.y > 1.0f)
            {
                if (m_actionTriggered)
                {
                    return;
                }
                else
                {
                    m_actionTriggered = TriggerAction();
                }
            }
            Vector3 pos = transform.position;
            //transform.position.Set(pos.x, pos.y + Time.deltaTime * 1.0f, pos.z);
            transform.position = pos + new Vector3(0.0f, Time.deltaTime * 0.1f, 0.0f);

            m_HatchModel.transform.RotateAround(m_HatchModel.transform.position, transform.up, Time.deltaTime * -45.0f);
        }
    }


    bool TriggerAction()
    {
        // Do whatever you need to do when trap is opening
        Debug.Log($"Exit trap is open now = {transform.position} at {Time.fixedTime}");

        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        OVRScreenFade _screenFadeScript = rig.GetComponent<OVRScreenFade>();
        if (_screenFadeScript != null)
        {
            //_screenFadeScript.SetFadeLevel(fadeLevel * MaxFade);
            _screenFadeScript.FadeOut();
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
