using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OculusSampleFramework;

public class ElemReceiver : MonoBehaviour
{
    public TheCellGameMgr.Elements m_ElementType; // Element type it needs to be validated

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
    }


    // --- 
    private void FixedUpdate()
    {
        if (m_LastCollider == null)
        {
            return;
        }

        Deactivate(m_LastCollider);
    }


    // Don't use OnTriggerStay as it mess up with hands grabber
#if _test_
    private void OnTriggerStay(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (m_Validated)
        {
            return;
        }

        if (other.tag == "ElementsCube")
        {
            Deactivate(other);

            /*
            //Check type first
            TheCellGameMgr.Elements type = other.gameObject.GetComponent<ElemCubeClass>().m_ElemType;
            if (type != m_ElementType)
            {
                return;
            }

            Vector3 dist = transform.position - other.transform.position;
            float length = dist.magnitude;
            Color color = Color.red * length;

            m_renderer.material.SetColor("_BaseColor", color);

            GameObject t2 = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(2).gameObject;
            t2.GetComponent<TextMeshProUGUI>().text = $"d: {length}";

            if (length < 0.025f)
            {
                ColorGrabbable grabbable = other.GetComponent<ColorGrabbable>();
                if (grabbable != null)
                {
                    grabbable.Deactivate();
                }
                else
                {
                    t2.GetComponent<TextMeshProUGUI>().text = $"Can't find ColorGrabbable {Time.fixedTime}s";
                    return;
                }

                OVRGrabber grabber = grabbable.grabbedBy;
                if (grabber != null)
                {
                    grabber.ForceRelease(grabbable);
                    grabbable.enabled = false;
                    t2.GetComponent<TextMeshProUGUI>().text = $"Released by '{grabbable.grabbedBy.name} at {Time.fixedTime}s'";
                    other.enabled = false;
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
                    m_Validated = true;
                }
                else
                {
                    t2.GetComponent<TextMeshProUGUI>().text = $"Can't find the grabber at {Time.fixedTime}s'";
                }

                // Snap the object and reset physics
                other.transform.SetPositionAndRotation(transform.position, transform.rotation);
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
            }
            */
        }
    }
#endif


    void Deactivate(Collider other)
    {
        //Check type first
        TheCellGameMgr.Elements type = other.gameObject.GetComponent<ElemCubeClass>().m_ElemType;
        if (type != m_ElementType)
        {
            return;
        }

        Vector3 dist = transform.position - other.transform.position;
        float length = dist.magnitude;
        Color color = Color.red * length;

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
            m_Validated = true;

            //grabbable.enabled = false;
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
