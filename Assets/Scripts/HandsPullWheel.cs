using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandsPullWheel : MonoBehaviour
{
    MeshRenderer m_Renderer;
    bool m_rightIndexIn = false;
    bool m_leftIndexIn = false;
    Collider m_rightCollider;
    Vector3 m_rightStartPos;
    Collider m_leftCollider;
    Vector3 m_leftStartPos;
    float m_forceActive;


    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        transform.localRotation = Quaternion.identity; // Should start from the identity
    }


    private void OnTriggerEnter(Collider other)
    {
        //get hand associated with finger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Middle2);

//#if HAND_DEBUG
        GameObject text = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(0).gameObject;
        text.GetComponent<TextMeshProUGUI>().text = $"Hand= {handIdx}";
//#endif

        if (handIdx == 0)
        {
            m_leftIndexIn = true;
            m_leftCollider = other;
            m_leftStartPos = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.position;
        }
        else if (handIdx == 1)
        {
            m_rightIndexIn = true;
            m_rightCollider = other;
            m_rightStartPos = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.position;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(other, OVRPlugin.BoneId.Hand_Middle2);

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


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        //m_forceActive = 0.0f;
        if (Input.GetKey(KeyCode.I))
        {
            m_forceActive = 10.0f;
        }
        if (Input.GetKey(KeyCode.U))
        {
            m_forceActive = -10.0f;
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

        if ((m_forceActive >= 1.0f) || (m_forceActive <= -1.0f))
        {
            active = true;
        }

        if (active)
        {
            if (m_rightCollider)
            {
                //Vector3 handPos = m_rightCollider.transform.position;
                Vector3 handPos = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.position;

                GameObject text = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
                text.GetComponent<TextMeshProUGUI>().text = $"{m_rightCollider.name}= {handPos.x.ToString("0.0")} {handPos.y.ToString("0.0")} {handPos.z.ToString("0.0")}";

                //Vector3 hp = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.position;
                //float d = Vector3.Distance(m_rightStartPos, hp);
                Vector3 vi = m_rightStartPos - transform.position;
                Vector3 vf = handPos - transform.position;
                float d = Vector3.Dot(vi, vf);
                transform.RotateAround(transform.position, transform.forward, Time.fixedDeltaTime * 1000.0f * d);
            }
            if (m_leftCollider)
            {
                Vector3 handPos = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.position;

                GameObject text = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(1).gameObject;
                text.GetComponent<TextMeshProUGUI>().text = $"{m_leftCollider.name}= {handPos.x.ToString("0.0")} {handPos.y.ToString("0.0")} {handPos.z.ToString("0.0")}";

                //float d = Vector3.Distance(m_leftStartPos, handPos);
                Vector3 vi = m_leftStartPos - transform.position;
                Vector3 vf = handPos - transform.position;
                float d = Vector3.Dot(vi, vf);

                transform.RotateAround(transform.position, transform.forward, Time.fixedDeltaTime * 1000.0f * d);
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

                }
                else
                {

                }

                Debug.Log($"{gameObject.name} m_startTrigger= {Time.fixedTime}s qw= {qw}");
                TheCellGameMgr.instance.Audio_Bank[1].Play();
                //m_leftStartZ = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandLeft).hand.PointerPose.localRotation.z;
                //m_rightStartZ = TheCellGameMgr.instance.GetHand(OVRHand.Hand.HandRight).hand.PointerPose.localRotation.z;
                m_rightCollider = null;
                m_leftCollider = null;
                m_rightIndexIn = false;
                m_leftIndexIn = false;
                m_forceActive = 0.0f;
                transform.localRotation = Quaternion.identity;
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
}
