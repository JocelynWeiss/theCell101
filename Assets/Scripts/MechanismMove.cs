using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismMove : MonoBehaviour
{
    // Which orientation is it facing
    public TheCellGameMgr.CardinalPoint cardinal;
    private bool m_actionTriggered = false;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;


    // Update is called once per frame
    void Update()
    {
        bool actionning = false;
        if (Input.GetKey(KeyCode.A))
        {
            //Debug.Log($"a = {transform.eulerAngles}");
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

        if (actionning)
        {
            if (transform.rotation.eulerAngles.x > 355.0f)
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
            transform.RotateAround(transform.position, transform.right, Time.deltaTime * -90.0f);
        }
    }


    bool TriggerAction()
    {
        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                TheCellGameMgr.instance.MoveColumn(true);
                break;
            case TheCellGameMgr.CardinalPoint.East:
                TheCellGameMgr.instance.MoveRow(true);
                break;
            case TheCellGameMgr.CardinalPoint.South:
                TheCellGameMgr.instance.MoveColumn(false);
                break;
            case TheCellGameMgr.CardinalPoint.West:
                TheCellGameMgr.instance.MoveRow(false);
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
