using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    // Start is called before the first frame update

    RaycastHit2D[] m_tHits = new RaycastHit2D[360];
    public float m_fDetectionDistance, m_fSpeed;
    public Goal m_oGoal;
    LayerMask m_oNotTheRobot;
    
    private void Start()
    {
        m_oNotTheRobot =~ LayerMask.GetMask("Robot");
    }

    private void Update()
    {


        Vector3 vGoalPosition = m_oGoal.transform.position;
        RaycastHit2D oGoalHit = Physics2D.Raycast(transform.position, vGoalPosition - transform.position, m_fDetectionDistance);

        //No obstacles
        if(oGoalHit.collider == null || oGoalHit.collider == m_oGoal.m_oCollider)
        {
            MoveToPosition(m_oGoal.transform.position);
            Debug.DrawLine(transform.position, m_oGoal.transform.position, Color.green);
            Debug.Log("Moving to goal");
        }
        else
        {
            for (int i = 0; i < m_tHits.Length; i++)
            {
                Vector3 vRotatedVector = Quaternion.AngleAxis(i, Vector3.forward) * Vector3.up;
                RaycastHit2D oHit = Physics2D.Raycast(transform.position, vRotatedVector, m_fDetectionDistance, m_oNotTheRobot);
                m_tHits[i] = oHit;
                Debug.DrawLine(transform.position, oHit.point, Color.red);
            }
            RaycastHit2D oClosestHit = GetClosestHitToGoal();
            MoveToPosition(oClosestHit.point);
            if(oClosestHit.transform != null)
            {
                Debug.Log("Moving to Obstacle " + oClosestHit.transform.gameObject.name);
            }
        }
    }

    void MoveToPosition(Vector3 _vPosition)
    {
        Vector3 vDirection = (_vPosition - transform.position).normalized;
        transform.position += vDirection * m_fSpeed * Time.deltaTime;
    }

    RaycastHit2D  GetClosestHitToGoal()
    {
        RaycastHit2D oClosestHit = new RaycastHit2D();
        float fMinDistance = Mathf.Infinity;
        for (int i = 0; i < m_tHits.Length; i++)
        {
            if (CheckPointIsBorder(i))
            {
                if (oClosestHit.collider != null)
                {
                    float fCurrentDistance = (m_tHits[i].point - new Vector2(m_oGoal.transform.position.x, m_oGoal.transform.position.y)).sqrMagnitude;
                    if (fCurrentDistance < fMinDistance)
                    {
                        fMinDistance = fCurrentDistance;
                        oClosestHit = m_tHits[i];
                        Debug.Log("Closest hit is from " + oClosestHit.collider.gameObject);
                    }
                }
                else
                {
                    oClosestHit = m_tHits[i];
                }
            }
        }
        Debug.Log("min distance: " + fMinDistance);
        Debug.DrawLine(transform.position, oClosestHit.point, Color.yellow);
        return oClosestHit;
    }
    

    bool CheckPointIsBorder (int _iHitIndex)
    {
        if(m_tHits[_iHitIndex].collider !=  null)
        {
            int iSuperior = _iHitIndex + 1;
            int iInferior = _iHitIndex - 1;
            if (iSuperior >= m_tHits.Length)
            {
                iSuperior = 0;
            }
            if(iInferior < 0)
            {
                iInferior = m_tHits.Length - 1;
            }
            return m_tHits[iSuperior].collider == null || m_tHits[iInferior].collider == null;
        }
        else
        {
            return false;
        }
    }
}
