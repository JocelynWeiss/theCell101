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


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ChangeType(TheCellGameMgr.Elements newType, Material[] mats)
    {
        m_ElemType = newType;
        if (m_Renderer != null)
        {
            m_Renderer.material = mats[(int)newType];
        }
    }
}
