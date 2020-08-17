using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandsTrigger : MonoBehaviour
{
    // Which door is it opening
    public TheCellGameMgr.CardinalPoint m_cardinal;
    public float m_timeToTrigger = 1.0f; // in sec

    private Renderer m_renderer;

    // True if an index tip is inside the cube, false otherwise.
    // First item is left hand, second item is right hand
    private bool[] m_isIndexStaying;

    float m_bothIn = 0.0f;
    private float m_goingOutStartTime = 0.0f;


    private void Awake()
    {
        m_renderer = GetComponent<Renderer>();
    }


    // Start is called before the first frame update
    void Start()
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }
        m_renderer.enabled = false;

        m_isIndexStaying = new bool[2] { false, false };
        m_goingOutStartTime = 0.0f;
        m_bothIn = 0.0f;
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if ((Input.GetKeyUp(KeyCode.UpArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.North))
        {
            TheCellGameMgr.instance.Audio_Bank[17].Play();
            m_renderer.material.SetColor("_TintColor", Color.green);
            m_isIndexStaying[0] = true;
            m_isIndexStaying[1] = true;
            m_bothIn = Time.fixedTime;
            TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true);
        }
        if ((Input.GetKeyUp(KeyCode.RightArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.East))
        {
            TheCellGameMgr.instance.Audio_Bank[17].Play();
            m_renderer.material.SetColor("_TintColor", Color.green);
            m_isIndexStaying[0] = true;
            m_isIndexStaying[1] = true;
            m_bothIn = Time.fixedTime;
            TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true);
        }
        if ((Input.GetKeyUp(KeyCode.DownArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.South))
        {
            TheCellGameMgr.instance.Audio_Bank[17].Play();
            m_renderer.material.SetColor("_TintColor", Color.green);
            m_isIndexStaying[0] = true;
            m_isIndexStaying[1] = true;
            m_bothIn = Time.fixedTime;
            TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true);
        }
        if ((Input.GetKeyUp(KeyCode.LeftArrow)) && (m_cardinal == TheCellGameMgr.CardinalPoint.West))
        {
            TheCellGameMgr.instance.Audio_Bank[17].Play();
            m_renderer.material.SetColor("_TintColor", Color.green);
            m_isIndexStaying[0] = true;
            m_isIndexStaying[1] = true;
            m_bothIn = Time.fixedTime;
            TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true);
        }
#endif

        if (m_goingOutStartTime != 0.0f)
        {
            Vector3 vel = Vector3.zero;
            switch (m_cardinal)
            {
                case TheCellGameMgr.CardinalPoint.North:
                    vel.z = -0.5f;
                    break;
                case TheCellGameMgr.CardinalPoint.East:
                    vel.x = -0.5f;
                    break;
                case TheCellGameMgr.CardinalPoint.South:
                    vel.z = 0.5f;
                    break;
                case TheCellGameMgr.CardinalPoint.West:
                    vel.x = 0.5f;
                    break;
            }
            TheCellGameMgr.instance.m_CentreModels.transform.position += vel * Time.deltaTime;
            TheCellGameMgr.instance.m_NorthModels.transform.position += vel * Time.deltaTime;
            TheCellGameMgr.instance.m_EastModels.transform.position += vel * Time.deltaTime;
            TheCellGameMgr.instance.m_SouthModels.transform.position += vel * Time.deltaTime;
            TheCellGameMgr.instance.m_WestModels.transform.position += vel * Time.deltaTime;

            if (TheCellGameMgr.instance.m_FxLasers.activeSelf == true)
            {
                TheCellGameMgr.instance.m_FxLasers.transform.position += vel * Time.deltaTime;
            }
        }
    }


    private void FixedUpdate()
    {
        if (m_goingOutStartTime != 0.0f)
        {
            float intensity = 3.0f;
            TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;

            if (Time.fixedTime - m_goingOutStartTime > 2.0f)
            {
                m_goingOutStartTime = 0.0f;
                TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity = 0.6f;
                TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity = 0.6f;
                TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity = 0.6f;
                TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity = 0.6f;
                TheCellGameMgr.instance.m_CentreModels.transform.position = Vector3.zero;
                TheCellGameMgr.instance.m_NorthModels.transform.position = new Vector3(0.0f, 0.0f, 2.9f);
                TheCellGameMgr.instance.m_EastModels.transform.position = new Vector3(2.9f, 0.0f, 0.0f);
                TheCellGameMgr.instance.m_SouthModels.transform.position = new Vector3(0.0f, 0.0f, -2.9f);
                TheCellGameMgr.instance.m_WestModels.transform.position = new Vector3(-2.9f, 0.0f, 0.0f);
                m_renderer.material.SetColor("_TintColor", Color.cyan);
                //m_renderer.enabled = true;
                TheCellGameMgr.instance.m_codes.SetActive(true);

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

        if (m_bothIn != 0.0f)
        {
            if ((m_isIndexStaying[0] == false) || (m_isIndexStaying[1] == false))
            {
                m_bothIn = 0.0f;
                return;
            }

            if (Time.fixedTime - m_bothIn < m_timeToTrigger)
            {
                //wait
            }
            else
            {
                StartAction();
            }
        }
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (m_goingOutStartTime != 0.0f)
            return;

        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index3);
        //m_goingOutStartTime = 0.0f;

        //if there is an associated hand, it means that an index of one of two hands is entering the cube
        //change the color of the cube accordingly (blue for left hand, green for right one)
        if (handIdx != -1)
        {
            m_isIndexStaying[handIdx] = true;

            if (TheCellGameMgr.instance.Audio_Bank[17].isPlaying == false)
            {
                TheCellGameMgr.instance.Audio_Bank[17].Play();
            }

            if ((m_isIndexStaying[0] == true) && (m_isIndexStaying[1] == true))
            {
                m_renderer.material.SetColor("_TintColor", Color.green);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true);
                if (m_goingOutStartTime == 0.0f)
                {
                    m_bothIn = Time.fixedTime;
                }
            }
            else if (m_isIndexStaying[0] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.blue);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true, 0);
            }
            else if (m_isIndexStaying[1] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.blue);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, true, 1);
            }
            else
            {
                m_renderer.material.SetColor("_TintColor", Color.cyan);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, false);
            }
        }
    }


    private void OnTriggerExit(Collider collider)
    {
        //get hand associated with trigger
        int handIdx = TheCellGameMgr.instance.GetFingerHandId(collider, OVRPlugin.BoneId.Hand_Index3);

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
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, false, 0);
            }
            else if (m_isIndexStaying[1] == true)
            {
                m_renderer.material.SetColor("_TintColor", Color.green);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, false, 1);
            }
            else
            {
                m_renderer.material.SetColor("_TintColor", Color.cyan);
                TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, false);
            }
        }
    }


    void StartAction()
    {
        m_bothIn = 0.0f;
        m_goingOutStartTime = Time.fixedTime;
        TheCellGameMgr.instance.Audio_Bank[10].Play();
        m_renderer.enabled = false;
        TheCellGameMgr.instance.m_MovingOut = true;
        Debug.Log($"Player start action going out cardinal {m_cardinal} @ {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}s");
        TheCellGameMgr.instance.m_FxDust_N.SetActive(false);
        TheCellGameMgr.instance.m_FxDust_E.SetActive(false);
        TheCellGameMgr.instance.m_FxDust_S.SetActive(false);
        TheCellGameMgr.instance.m_FxDust_W.SetActive(false);
        TheCellGameMgr.instance.m_FxDust_Top.SetActive(false);
        TheCellGameMgr.instance.m_Console_N.SetActive(false);
        TheCellGameMgr.instance.m_Console_E.SetActive(false);
        TheCellGameMgr.instance.m_Console_S.SetActive(false);
        TheCellGameMgr.instance.m_Console_W.SetActive(false);
        TheCellGameMgr.instance.SwitchHandConsole(m_cardinal, false);
        TheCellGameMgr.instance.m_codes.SetActive(false);
        TheCellGameMgr.instance.AnimateShuttersOpen(m_cardinal);
        TheCellGameMgr.instance.m_FxTeleporter.SetActive(false);
        TheCellGameMgr.instance.m_FXDeathRespawn.SetActive(false);
        TheCellGameMgr.instance.m_GroupElements.SetActive(false);
        TheCellGameMgr.instance.m_PlayaModel.SetActive(false);
    }


    // ---
    public static IEnumerator LitupConsole(GameObject obj)
    {
        DoorHandsTrigger door = obj.GetComponent<DoorHandsTrigger>();
        if (door == null)
            yield return null;

        float startTime = Time.time;
        float intensity = 0.0f;
        while (intensity < 0.3f)
        {
            intensity += Time.fixedDeltaTime;
            door.m_renderer.material.SetFloat("_Intensity", intensity);
            yield return new WaitForFixedUpdate();
        }
    }


    // ---
}
