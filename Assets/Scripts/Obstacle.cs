using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Obstacle : MonoBehaviour
{
    public Collider2D m_oCollider;
    public TMP_Dropdown m_oDropdown;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && m_oDropdown.value == 1)
        {
            Vector3 vMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 vMousePos2D = new Vector2(vMousePos.x, vMousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(vMousePos2D, Vector2.zero);
            if (hit.collider == m_oCollider)
            {
                Destroy(gameObject);
            }
        }
    }
}
