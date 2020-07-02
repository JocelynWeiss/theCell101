using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Http.Headers;

public class HandsRotate : MonoBehaviour
{
    MeshRenderer m_Renderer;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;
    Collider m_rightCollider;
    float m_rightStartZ;
    Collider m_leftCollider;
    float m_leftStartZ;
    public float m_rotSpeed = 100.0f;
    public float m_backingTime = 1.0f; // in sec
    float m_startTrigger = 0.0f;
    float m_forceActive;
    public float m_blockButtonDuration = 2.0f; // Amount of sec to block the button after activation
    float m_blockButtonEndTime; // Time at which to re activate button
    public bool m_onRow = true;
    public int m_HeadId = 0;


    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        //Quaternion qw = transform.localRotation;
        //Debug.Log($"{gameObject.name} Awake qw= {qw}");
        transform.localRotation = Quaternion.identity; // Should start from the identity
    }


    // --- 
    private void OnTriggerEnter(Collider other)
    {
        //get hand associated with finger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Thumb3); // thumb

#if HAND_DEBUG
        GameObject text = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(0).gameObject;
        text.GetComponent<TextMeshProUGUI>().text = $"Hand= {handIdx}";
#endif

        if (handIdx == 0)
        {
            m_leftIndexIn = true;
            m_leftCollider = other;
            Quaternion lrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.localRotation;
            m_leftStartZ = lrot.z;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = true;
            m_rightCollider = other;
            Quaternion rrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.localRotation;
            m_rightStartZ = rrot.z;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Thumb3);

        if (handIdx == 0)
        {
            m_leftIndexIn = false;
            m_leftCollider = null;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = false;
            m_rightCollider = null;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        //m_forceActive = 0.0f;
        if (Input.GetKey(KeyCode.I))
        {
            m_forceActive = -10.0f;
        }
        if (Input.GetKey(KeyCode.U))
        {
            m_forceActive = 10.0f;
        }
#endif
    }


    // --- Physics ---
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

        // Do we need to turn the button back in pose
        if (m_startTrigger > Time.fixedTime)
        {
            Quaternion qw = transform.localRotation;
            transform.RotateAround(transform.position, transform.forward, Time.fixedDeltaTime * m_rotSpeed * qw.z);

            // Detect the end of the comeback
            if (m_startTrigger < Time.fixedTime + Time.fixedDeltaTime * 1.5f)
            {
                transform.localRotation = Quaternion.identity;
                //qw = transform.localRotation;
                //Debug.Log($"{gameObject.name} TriggerEnd= {m_startTrigger} qw= {qw}");
            }
            
            return;
        }

        // Block the mechanism after use
        if (m_blockButtonEndTime > Time.fixedTime)
        {
            return;
        }

        bool active = m_rightIndexIn;
        if ((active == false) && (m_leftIndexIn))
        {
            active = true;
        }

        if ((m_forceActive >= 1.0f) || (m_forceActive <= -1.0f))
        {
            active = true;
        }

        if (active)
        {
            if (m_rightCollider)
            {
                Vector3 handRot = m_rightCollider.transform.rotation.eulerAngles;

                GameObject directions = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
                directions.GetComponent<TextMeshProUGUI>().text = $"{m_rightCollider.name}= {handRot.x.ToString("0.0")} {handRot.y.ToString("0.0")} {handRot.z.ToString("0.0")}";

                //Vector3 cubeRot = transform.rotation.eulerAngles;
                //transform.localRotation = Quaternion.Euler(cubeRot.x, cubeRot.y, handRot.x);
                Quaternion objRot = transform.localRotation;
                objRot.z = handRot.x;
                //objRot.z = handRot.y; // NOK
                Quaternion rrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.localRotation;
                //objRot.z = rrot.z + 0.5f;
                objRot.z = m_rightStartZ - rrot.z;
                transform.localRotation = objRot;
            }
            if (m_leftCollider)
            {
                //Vector3 handRot = m_leftCollider.transform.rotation.eulerAngles;
                Quaternion handRot = m_leftCollider.transform.rotation;

                GameObject directions = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
                //directions.GetComponent<TextMeshProUGUI>().text = $"{m_leftCollider.name}= {handRot.x.ToString("0.0")} {handRot.y.ToString("0.0")} {handRot.z.ToString("0.0")}";
                Quaternion q = m_leftCollider.transform.rotation;
                directions.GetComponent<TextMeshProUGUI>().text = $"{q.x.ToString("0.0")}= {q.y.ToString("0.0")} {q.z.ToString("0.0")} {q.w.ToString("0.0")}";

                //Vector3 cubeRot = transform.rotation.eulerAngles;
                //transform.localRotation = Quaternion.Euler(cubeRot.x, cubeRot.y, handRot.x);
                Quaternion objRot = transform.localRotation;
                Quaternion lrot = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.localRotation;
                //objRot.z = m_leftStartZ - lrot.z;
                objRot.z = lrot.z - m_leftStartZ;
                transform.localRotation = objRot;
            }

            Quaternion qw = transform.localRotation;
            if ((m_forceActive >= 1.0f) || (m_forceActive <= -1.0f))
            {
                transform.RotateAround(transform.position, transform.forward, Time.fixedDeltaTime * 10.0f * m_forceActive);
                GameObject txt1 = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
                txt1.GetComponent<TextMeshProUGUI>().text = $"{qw.x.ToString("0.0")} {qw.y.ToString("0.0")} {qw.z.ToString("0.0")} {qw.w.ToString("0.0")}";
            }

            if ((qw.z > 0.6f) || (qw.z < -0.6f))
            {
                if (qw.z > 0.6f)
                {
                    if (m_onRow)
                    {
                        TheCellGameMgr.instance.MoveRow(m_HeadId, false);
                    }
                    else
                    {
                        TheCellGameMgr.instance.MoveColumn(m_HeadId, true);
                    }
                }
                else
                {
                    if (m_onRow)
                    {
                        TheCellGameMgr.instance.MoveRow(m_HeadId, true);
                    }
                    else
                    {
                        TheCellGameMgr.instance.MoveColumn(m_HeadId, false);
                    }
                }

                m_startTrigger = Time.fixedTime + m_backingTime;
                m_blockButtonEndTime = m_startTrigger + m_blockButtonDuration;
                Debug.Log($"{gameObject.name} m_startTrigger= {m_startTrigger} qw= {qw}");
                TheCellGameMgr.instance.Audio_Bank[1].Play();
                m_leftStartZ = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.localRotation.z;
                m_rightStartZ = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.localRotation.z;
                m_rightCollider = null;
                m_leftCollider = null;
                m_rightIndexIn = false;
                m_leftIndexIn = false;
                m_forceActive = 0.0f;
            }
        }

        if (m_Renderer)
        {
            if ((m_rightIndexIn) || (m_forceActive > 0.0f))
            {
                m_Renderer.material.EnableKeyword("_EMISSION");
                m_Renderer.material.SetColor("_EmissionColor", Color.yellow * 0.25f);
            }
            else if ((m_leftIndexIn) || (m_forceActive < 0.0f))
            {
                m_Renderer.material.EnableKeyword("_EMISSION");
                m_Renderer.material.SetColor("_EmissionColor", Color.yellow * 0.25f);
            }
            else
            {
                m_Renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }


    [ContextMenu("ActivatOnRight")]
    void ActivateOnRight()
    {
        this.m_forceActive = -10.0f;
    }


    [ContextMenu("ActivatOnLeft")]
    void ActivateOnLeft()
    {
        this.m_forceActive = 10.0f;
    }
}
