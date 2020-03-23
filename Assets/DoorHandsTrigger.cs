using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandsTrigger : MonoBehaviour
{
    // Which door is it opening
    public TheCellGameMgr.CardinalPoint cardinal;

    private Renderer m_renderer;
    // First item is left hand, second item is right hand
    private OVRHand[] m_hands;
    // True if an index tip is inside the cube, false otherwise.
    // First item is left hand, second item is right hand
    private bool[] m_isIndexStaying;

    private float m_goingOutStartTime = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_hands = new OVRHand[]
        {
            GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRHand>(),
            GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>()
        };
        m_isIndexStaying = new bool[2] { false, false };
        m_goingOutStartTime = 0.0f;
    }


    // Update is called once per frame
    void Update()
    {

    }


    private void FixedUpdate()
    {
        if ((m_goingOutStartTime != 0.0f) && (Time.fixedTime - m_goingOutStartTime > 2.0f))
        {
            m_goingOutStartTime = 0.0f;

            switch (cardinal)
            {
                case TheCellGameMgr.CardinalPoint.North:
                    TheCellGameMgr.instance.MovePlayerNorth();
                    break;
                case TheCellGameMgr.CardinalPoint.East:
                    TheCellGameMgr.instance.MovePlayerEast();
                    break;
                case TheCellGameMgr.CardinalPoint.South:
                    TheCellGameMgr.instance.MovePlayerSouth();
                    break;
                case TheCellGameMgr.CardinalPoint.West:
                    TheCellGameMgr.instance.MovePlayerWest();
                    break;
            }
        }
    }


    private void OnTriggerEnter(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = GetIndexFingerHandId(collider);
        //m_goingOutStartTime = 0.0f;

        //if there is an associated hand, it means that an index of one of two hands is entering the cube
        //change the color of the cube accordingly (blue for left hand, green for right one)
        if (handIdx != -1)
        {
            //m_renderer.material.color = handIdx == 0 ? m_renderer.material.color = Color.blue : m_renderer.material.color = Color.green;
            m_isIndexStaying[handIdx] = true;

            if ((m_isIndexStaying[0] == true) && (m_isIndexStaying[1] == true))
            {
                m_renderer.material.SetColor("_TintColor", Color.red);
                if (m_goingOutStartTime == 0.0f)
                {
                    m_goingOutStartTime = Time.fixedTime;
                }
            }
            else if (m_isIndexStaying[0] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.blue);
                m_goingOutStartTime = 0.0f;
            }
            else if (m_isIndexStaying[1] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.green);
                m_goingOutStartTime = 0.0f;
            }
            else
            {
                m_renderer.material.SetColor("_TintColor", Color.cyan);
                m_goingOutStartTime = 0.0f;
            }
        }
    }


    private void OnTriggerExit(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = GetIndexFingerHandId(collider);
        //m_goingOutStartTime = 0.0f;

        //if there is an associated hand, it means that an index of one of two hands is levaing the cube,
        //so set the color of the cube back to white, or to the one of the other hand, if it is in
        if (handIdx != -1)
        {
            m_isIndexStaying[handIdx] = false;
            //m_renderer.material.color = m_isIndexStaying[0] ? m_renderer.material.color = Color.blue :
            //                          (m_isIndexStaying[1] ? m_renderer.material.color = Color.green : Color.white);
            if (m_isIndexStaying[0] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.blue);
            }
            else if (m_isIndexStaying[1] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.green);
            }
            else
            {
                m_renderer.material.SetColor("_TintColor", Color.cyan);
            }
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
