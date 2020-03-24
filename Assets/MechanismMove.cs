using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismMove : MonoBehaviour
{
    // Which orientation is it facing
    public TheCellGameMgr.CardinalPoint cardinal;
    // First item is left hand, second item is right hand
    private OVRHand[] m_hands;
    private bool m_actionTriggered = false;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;

    // Start is called before the first frame update
    void Start()
    {
        m_hands = new OVRHand[]
        {
            GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRHand>(),
            GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>()
        };
    }


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
            if (m_hands[0].GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                actionning = true;
            }
            if (m_hands[1].GetFingerIsPinching(OVRHand.HandFinger.Index))
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
        int handIdx = GetIndexFingerHandId(collider);

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
        int handIdx = GetIndexFingerHandId(collider);

        if (handIdx == 0)
        {
            m_leftIndexIn = false;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = false;
        }
    }


    /// <summary>
    /// Gets the hand id associated with the index finger of the collider passed as parameter, if any
    /// </summary>
    /// <param name="collider">Collider of interest</param>
    /// <returns>0 if the collider represents the finger tip of left hand, 1 if it is the one of right hand, -1 if it is not an index fingertip</returns>
    private int GetIndexFingerHandId(Collider collider)
    {
        //Checking Oculus code, it is possible to see that physics capsules gameobjects always end with _CapsuleCollider
        if (collider.gameObject.name.Contains("_CapsuleCollider"))
        {
            //get the name of the bone from the name of the gameobject, and convert it to an enum value
            string boneName = collider.gameObject.name.Substring(0, collider.gameObject.name.Length - 16);
            OVRPlugin.BoneId boneId = (OVRPlugin.BoneId)Enum.Parse(typeof(OVRPlugin.BoneId), boneName);

            //if it is the tip of the Index
            if (boneId == OVRPlugin.BoneId.Hand_Index3)
                //check if it is left or right hand, and change color accordingly.
                //Notice that absurdly, we don't have a way to detect the type of the hand
                //so we have to use the hierarchy to detect current hand
                if (collider.transform.IsChildOf(m_hands[0].transform))
                {
                    return 0;
                }
                else if (collider.transform.IsChildOf(m_hands[1].transform))
                {
                    return 1;
                }
        }

        return -1;
    }
}
