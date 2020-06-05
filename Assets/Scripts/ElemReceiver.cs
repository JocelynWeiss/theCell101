using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OculusSampleFramework;

public class ElemReceiver : MonoBehaviour
{
    [Tooltip("UID of this receiver, from 1 to 4.")]
    [Range(1,4)] public int m_ReceiverId; // Uniq receiver ID
    [ViewOnly] public TheCellGameMgr.Elements m_ElementType; // Element type it needs to be validated
    [ViewOnly] public bool m_Validated = false;

    private MeshRenderer m_renderer;
    private Collider m_LastCollider = null;


    // Start is called before the first frame update
    void Start()
    {
        m_Validated = false;
        m_renderer = this.GetComponent<MeshRenderer>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        GameObject t0 = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(0).gameObject;
        //t0.GetComponent<TextMeshProUGUI>().text = other.tag;
        //t0.GetComponent<TextMeshProUGUI>().text = other.name;

        if (other.tag == "ElementsCube")
        {
            m_LastCollider = other;
            Material meshMaterial = m_renderer.material;
            meshMaterial.SetColor("_BaseColor", Color.red);
            t0.GetComponent<TextMeshProUGUI>().text = other.name;
        }
    }


    // --- 
    private void OnTriggerExit(Collider other)
    {
        m_LastCollider = null;
        m_renderer.material.SetColor("_BaseColor", Color.grey);
    }


    // --- 
    private void FixedUpdate()
    {
        if (m_LastCollider == null)
        {
            return;
        }

        if (m_Validated)
        {
            return;
        }
        
        Deactivate(m_LastCollider);
    }


    void Deactivate(Collider other)
    {
        Vector3 dist = transform.position - other.transform.position;
        float length = dist.magnitude;
        Color color = Color.red * (length * 10.0f);

        m_renderer.material.SetColor("_BaseColor", color);

        GameObject t2 = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(2).gameObject;
        t2.GetComponent<TextMeshProUGUI>().text = $"d: {length}";

        if (length < 0.025f)
        {
            ColorGrabbable grabbable = other.gameObject.GetComponent<ColorGrabbable>();
            if (grabbable == null)
            {
                t2.GetComponent<TextMeshProUGUI>().text = $"Can't find ColorGrabbable {Time.fixedTime}s";
                return;
            }

            OVRGrabber grabber = grabbable.grabbedBy;
            if (grabber == null)
            {
                t2.GetComponent<TextMeshProUGUI>().text = $"Can't find the grabber at {Time.fixedTime}s'";
                /*
                if (!m_Validated)
                {
                    m_Validated = true;
                    transform.localScale *= 0.75f;
                }
                */
                return;
            }
            grabber.ForceRelease(grabbable);

            // Deactivate this receiver, it's feeded.
            BoxCollider bc = this.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.isTrigger = false;
                bc.enabled = false;
            }
            else
            {
                t2.GetComponent<TextMeshProUGUI>().text = $"Can't find boxcollider {Time.fixedTime}s'";
            }
            if (!m_Validated)
            {
                transform.localScale *= 0.75f;
            }

            TheCellGameMgr.Elements type = other.gameObject.GetComponent<ElemCubeClass>().m_ElemType;
            m_ElementType = type;
            m_Validated = true;
            CodeUtils.BitfieldSet(m_ReceiverId, true, ref TheCellGameMgr.instance.m_CodeFinalSet);

            t2.GetComponent<TextMeshProUGUI>().text = $"Released by '{grabber.name} at {Time.fixedTime}s'";
            other.enabled = false;
                
            // Snap the object and reset physics
            other.transform.SetPositionAndRotation(transform.position, transform.rotation);
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            grabbable.Deactivate();

            m_LastCollider = null;

        }
    }
}
