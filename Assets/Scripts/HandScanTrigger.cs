using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandScanTrigger : MonoBehaviour
{
    // Which orientation is it facing
    public TheCellGameMgr.CardinalPoint m_cardinal;
    bool m_rightIndexIn = false;
    bool m_RightPinkyIn = false;
    bool m_leftIndexIn = false;
    bool m_leftPinkyIn = false;


    // ---
    private void FixedUpdate()
    {
        if ((m_leftIndexIn && m_leftPinkyIn) || (m_rightIndexIn && m_RightPinkyIn))
        {
            TriggerAction();
        }
    }


    private void OnTriggerEnter(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index2);

        if (handIdx == 0)
        {
            m_leftIndexIn = true;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = true;
        }

        handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Pinky2);

        if (handIdx == 0)
        {
            m_leftPinkyIn = true;
        }
        else if (handIdx == 1)
        {
            m_RightPinkyIn = true;
        }
    }


    private void OnTriggerExit(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index2);

        if (handIdx == 0)
        {
            m_leftIndexIn = false;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = false;
        }

        handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Pinky2);

        if (handIdx == 0)
        {
            m_leftPinkyIn = false;
        }
        else if (handIdx == 1)
        {
            m_RightPinkyIn = false;
        }
    }


    [ContextMenu("TriggerAction")]
    bool TriggerAction()
    {
        AudioSource.PlayClipAtPoint(TheCellGameMgr.instance.Audio_Bank[11].clip, transform.position);

        TheCellGameMgr.instance.ScanTriggerAction(m_cardinal);
        return true;
    }
}
