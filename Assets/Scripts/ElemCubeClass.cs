using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElemCubeClass : MonoBehaviour
{
    public TheCellGameMgr.Elements m_ElemType;
    private MeshRenderer m_Renderer;


    private void Awake()
    {
        m_Renderer = this.GetComponentInChildren<MeshRenderer>(true);
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
}
