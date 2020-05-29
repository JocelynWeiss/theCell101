using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OculusSampleFramework;

public class ElemReceiver : MonoBehaviour
{
    private MeshRenderer m_renderer;

    // Start is called before the first frame update
    void Start()
    {
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
            Material meshMaterial = m_renderer.material;
            meshMaterial.SetColor("_BaseColor", Color.red);
            t0.GetComponent<TextMeshProUGUI>().text = other.name;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.tag == "ElementsCube")
        {
            Vector3 dist = transform.position - other.transform.position;
            float length = dist.magnitude;
            Color color = Color.red * length;

            m_renderer.material.SetColor("_BaseColor", color);

            GameObject t2 = TheCellGameMgr.instance.m_basicCanvas.transform.GetChild(2).gameObject;
            t2.GetComponent<TextMeshProUGUI>().text = $"d: {length}";

            //if (length < 0.025f)
            if (length < 0.1f)
            {
                ColorGrabbable grabbable = other.GetComponent<ColorGrabbable>();
                if (grabbable != null)
                {
                    grabbable.Deactivate();
                }
                else
                {
                    t2.GetComponent<TextMeshProUGUI>().text = $"Can't find ColorGrabbable";
                    return;
                }

                OVRGrabber grabber = grabbable.grabbedBy;
                if (grabber != null)
                {
                    grabber.ForceRelease(grabbable);
                    grabbable.enabled = false;
                    t2.GetComponent<TextMeshProUGUI>().text = $"Released by '{grabbable.grabbedBy}'";
                }
                else
                {
                    t2.GetComponent<TextMeshProUGUI>().text = $"Can't find the grabber";
                    //JowNext: We come here but we shouldn't really...
                }

                other.transform.SetPositionAndRotation(transform.position, transform.rotation);
            }
        }
    }
}
