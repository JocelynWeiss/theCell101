using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ElemCubeClass : MonoBehaviour
{
    public TheCellGameMgr.Elements m_ElemType;
    [ViewOnly] public int m_State = 0; // 0-> free, 1-> placed, 2-> fallen
    private MeshRenderer m_Renderer;
    Rigidbody m_Rb;


    private void Awake()
    {
        m_State = 0;
        m_Renderer = this.GetComponentInChildren<MeshRenderer>(true);
        m_Rb = GetComponent<Rigidbody>();
    }


    // ---
    public void ChangeType(TheCellGameMgr.Elements newType, Material[] mats)
    {
        m_ElemType = newType;
        if (m_Renderer != null)
        {
            m_Renderer.material = mats[(int)newType];
        }
    }


    public void SetUseGravity()
    {
        if (m_Rb == null)
            return;

        m_Rb.isKinematic = false;
        m_Rb.useGravity = true;
        m_State = 2;
    }
}
