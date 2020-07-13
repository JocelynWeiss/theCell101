using UnityEngine;

public class LocalizationMenu : MonoBehaviour
{
    public StartButtonScript m_StartButton;
    public Flags m_FlagFr;
    public Flags m_FlagUk;


    // ---
    private void FixedUpdate()
    {
        if (m_StartButton != null)
        {
            if (m_StartButton.m_IsReady)
            {
                switch (TheCellGameMgr.m_Language)
                {
                    case TheCellGameMgr.GameLanguages.English:
                        TheCellGameMgr.instance.LoadLocalizedText("localized_en.json");
                        break;
                    case TheCellGameMgr.GameLanguages.French:
                    default:
                        TheCellGameMgr.instance.LoadLocalizedText("localized_fr.json");
                        break;
                }

                TheCellGameMgr.instance.TeleportToStart(true);
                m_StartButton.m_IsReady = false;
                GameObject.Find("LocalizationMenu").gameObject.SetActive(false); // Hide loc
                TheCellGameMgr.instance.m_codes.SetActive(true);
            }
        }
    }


    // Highlight the flag and set startButton to clickable
    public void ChangeLanguageSelection(Flags newFlag)
    {
        if (m_FlagFr.m_IsSelected && newFlag != m_FlagFr)
        {
            m_FlagFr.SetOnOff(false);
        }

        if (m_FlagUk.m_IsSelected && newFlag != m_FlagUk)
        {
            m_FlagUk.SetOnOff(false);
        }

        if (newFlag.m_IsSelected == false)
        {
            newFlag.SetOnOff(true);
            TheCellGameMgr.m_Language = newFlag.m_Language;
            m_StartButton.SetClickable();
        }
    }
}
