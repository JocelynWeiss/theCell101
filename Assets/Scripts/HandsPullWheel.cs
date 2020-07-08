using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandsPullWheel : MonoBehaviour
{
    // Which door is it opening
    public TheCellGameMgr.CardinalPoint m_cardinal;
    MeshRenderer m_Renderer;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;
    float m_enterTime = 0.0f;
    float m_openStart = 0.0f;
    public int m_forward = 0; //0->+z, 1->+x, 2->-x, 3->-z


    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        transform.localRotation = Quaternion.identity; // Should start from the identity
    }


    private void OnTriggerEnter(Collider other)
    {
        //get hand associated with finger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Index3);

//#if HAND_DEBUG
        GameObject text = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(0).gameObject;
        text.GetComponent<TextMeshProUGUI>().text = $"Hand= {handIdx}";
//#endif

        if (handIdx == 0)
        {
            m_leftIndexIn = true;

            if (m_enterTime == 0.0f)
            {
                m_enterTime = Time.fixedTime;
            }
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = true;

            if (m_enterTime == 0.0f)
            {
                m_enterTime = Time.fixedTime;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //get hand associated with finger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Index3);

        if (handIdx == 0)
        {
            m_leftIndexIn = false;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = false;
        }

        m_enterTime = 0.0f;
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (m_cardinal == TheCellGameMgr.CardinalPoint.North)
        {
            if (Input.GetKey(KeyCode.I))
            {
                m_rightIndexIn = true;
                if (m_enterTime == 0.0f)
                {
                    m_enterTime = Time.fixedTime;
                    Debug.Log($"{gameObject.name} m_enterTime= {m_enterTime}s");
                }
            }
            if (Input.GetKey(KeyCode.U))
            {
                m_leftIndexIn = true;
                if (m_enterTime == 0.0f)
                {
                    m_enterTime = Time.fixedTime;
                    Debug.Log($"{gameObject.name} m_enterTime= {m_enterTime}s");
                }
            }
        }

        if ((Input.GetKeyUp(KeyCode.UpArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.North))
        {
            m_rightIndexIn = true;
            m_enterTime = Time.fixedTime;
        }
        if ((Input.GetKeyUp(KeyCode.RightArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.East))
        {
            m_rightIndexIn = true;
            m_enterTime = Time.fixedTime;
        }
        if ((Input.GetKeyUp(KeyCode.DownArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.South))
        {
            m_rightIndexIn = true;
            m_enterTime = Time.fixedTime;
        }
        if ((Input.GetKeyUp(KeyCode.LeftArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.West))
        {
            m_rightIndexIn = true;
            m_enterTime = Time.fixedTime;
        }
#endif
    }


    private void FixedUpdate()
    {
        if (TheCellGameMgr.instance == null)
            return;

#if HAND_DEBUG
        if (TheCellGameMgr.instance.m_basicCanvas != null)
        {
            GameObject directions = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
            //directions.GetComponent<TextMeshProUGUI>().text = $"Right= {m_rightIndexIn} \nLeft= {m_leftIndexIn}";
            Quaternion rrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.localRotation;
            //directions.GetComponent<TextMeshProUGUI>().text = $"RightLoc= {rrot.x.ToString("0.0")} {rrot.y.ToString("0.0")} {rrot.z.ToString("0.0")}";
            //Quaternion lrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.transform.localRotation; // No movement
            Quaternion lrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.localRotation;
            directions.GetComponent<TextMeshProUGUI>().text = $"rrot: {rrot.x.ToString("0.0")} {rrot.y.ToString("0.0")} {rrot.z.ToString("0.0")}\nlrot: {lrot.x.ToString("0.0")} {lrot.y.ToString("0.0")} {lrot.z.ToString("0.0")}";
        }
#endif

        bool active = m_rightIndexIn;
        if ((active == false) && (m_leftIndexIn))
        {
            active = true;
        }

        Vector3 forward = transform.forward;
        if (m_forward == 1)
            forward = transform.right;
        else if (m_forward == 2)
            forward = -transform.right;
        else if (m_forward == 3)
            forward = -transform.forward;

        if ((active) && (m_enterTime > 0.0f))
        {
            float timeIn = Time.fixedTime - m_enterTime;
            if (timeIn < 3.0f)
            {
                transform.RotateAround(transform.position, forward, Time.fixedDeltaTime * timeIn * 10.0f);
            }
            else
            {
                TriggerAction();
                m_enterTime = 0.0f;
                m_rightIndexIn = false;
                m_leftIndexIn = false;
            }
        }
        else
        {
            m_enterTime = 0.0f;

            // Handle leaving the room
            if (m_openStart != 0.0f)
            {
                if (Time.fixedTime - m_openStart < 1.5f)
                {
                    transform.RotateAround(transform.position, forward, Time.fixedDeltaTime * 100.0f);
                    TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity -= Time.fixedDeltaTime * 0.15f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity -= Time.fixedDeltaTime * 0.15f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity -= Time.fixedDeltaTime * 0.15f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity -= Time.fixedDeltaTime * 0.15f;
                }
                else
                {
                    TheCellGameMgr.instance.Audio_Bank[10].Play();
                    m_openStart = 0.0f;
                    transform.localRotation = Quaternion.identity;
                    TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity = 0.6f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity = 0.6f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity = 0.6f;
                    TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity = 0.6f;
                    Debug.Log($"{transform.parent.parent.name}//{gameObject.name} Go to next room @ {Time.fixedTime}s");
                    switch (m_cardinal)
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
        }

        if (m_Renderer)
        {
            if (m_rightIndexIn)
            {
                m_Renderer.material.EnableKeyword("_EMISSION");
                m_Renderer.material.SetColor("_EmissionColor", Color.yellow * 0.1f);
            }
            else if (m_leftIndexIn)
            {
                m_Renderer.material.EnableKeyword("_EMISSION");
                m_Renderer.material.SetColor("_EmissionColor", Color.yellow * 0.1f);
            }
            else
            {
                m_Renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }


    // ---
    public void TriggerAction()
    {
        Debug.Log($"{transform.parent.parent.name}//{gameObject.name} {m_cardinal} @ {Time.fixedTime}s");
        m_openStart = Time.fixedTime;
    }


    // ---
    
}
