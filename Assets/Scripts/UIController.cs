using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI m_oText;
    public GameObject m_oObstaclePrefab, m_oObstaclePanel, m_oGeneralPanel;
    public Robot m_oRobot;
    public Goal m_oGoal;
    public TMP_Dropdown m_oDropdown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && m_oRobot.m_bPaused)
        {
            if((Input.mousePosition.x/ Screen.width> 0.275f  || Input.mousePosition.y/Screen.height < 0.65f) && Input.mousePosition.y / Screen.height < 0.9f)
            {
                if (m_oDropdown.value == 0)
                {
                    Vector3 vMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
                    Obstacle oObstacle = Instantiate(m_oObstaclePrefab, vMousePos, Quaternion.identity).GetComponent<Obstacle>();
                    oObstacle.m_oDropdown = m_oDropdown;
                }
                else if (m_oDropdown.value == 2)
                {
                    m_oRobot.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
                }
                else if (m_oDropdown.value == 3)
                {
                    m_oGoal.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
                }
            }
        }
    }

    public void PlayStop()
    {
        if(m_oRobot.m_bPaused)
        {
            m_oGeneralPanel.SetActive(false);
            m_oRobot.m_bPaused = false;
            m_oText.text = "GO BACK";
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ChangedDropdownValue()
    {
        if(m_oDropdown.value == 0)
        {
            m_oObstaclePanel.SetActive(false);
        }
        else
        {
            m_oObstaclePanel.SetActive(false);
        }
    }
}
