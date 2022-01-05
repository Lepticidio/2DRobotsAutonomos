using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    // Start is called before the first frame update

    public bool m_bPaused = true;
    RaycastHit2D[] m_tHits = new RaycastHit2D[360];
    public float m_fDetectionDistance, m_fSpeed;
    public Goal m_oGoal;
    LayerMask m_oNotTheRobot;
    bool m_bFollowingWall;
    Vector3 m_vMinWallPoint;
    Vector3 m_vCurrentWallPoint;
    Vector3 m_vGeneralDirection;
    Vector3 m_vPastPosition, m_vEvenPreviousPosition;

    private void Start()
    {
        m_oNotTheRobot = ~LayerMask.GetMask("Robot");
    }

    private void Update()
    {
        if(!m_bPaused)
        {
            Vector3 vGoalPosition = m_oGoal.transform.position;
            RaycastHit2D oGoalHit = Physics2D.Raycast(transform.position, vGoalPosition - transform.position, m_fDetectionDistance, m_oNotTheRobot);
            if (!m_bFollowingWall)
            {

                //No obstacles
                if (oGoalHit.collider == null || oGoalHit.collider == m_oGoal.m_oCollider)
                {
                    MoveToPosition(m_oGoal.transform.position);
                    Debug.DrawLine(transform.position, m_oGoal.transform.position, Color.green);
                    Debug.Log("Moving to goal");
                }
                else
                {
                    ScanSorroundings();
                    RaycastHit2D oClosestHit = GetClosestHitToGoal();
                    m_vMinWallPoint = oClosestHit.point;
                    m_vCurrentWallPoint = m_vMinWallPoint;
                    MoveToPosition(m_vMinWallPoint);
                    if (oClosestHit.transform != null)
                    {
                        Debug.Log("Encountered Obstacle " + oClosestHit.transform.gameObject.name);
                    }
                    m_bFollowingWall = true;
                }
            }
            else
            {
                ScanSorroundings();
                FollowWall();
            }
        }
    }

    void ScanSorroundings()
    {
        for (int i = 0; i < m_tHits.Length; i++)
        {
            Vector3 vRotatedVector = Quaternion.AngleAxis(i, Vector3.forward) * Vector3.up;
            RaycastHit2D oHit = Physics2D.Raycast(transform.position, vRotatedVector, m_fDetectionDistance, m_oNotTheRobot);
            m_tHits[i] = oHit;
            //Debug.DrawLine(transform.position, oHit.point, Color.red);
        }

    }

    void MoveToPosition(Vector3 _vPosition)
    {
        m_vEvenPreviousPosition = m_vPastPosition;
        m_vPastPosition = transform.position;
        Vector3 vDirection = (_vPosition - transform.position).normalized;
        transform.position += vDirection * m_fSpeed * Time.deltaTime;
        m_vGeneralDirection = transform.position - m_vEvenPreviousPosition;
    }

    RaycastHit2D GetClosestHitToGoal()
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
                        //Debug.Log("Closest hit is from " + oClosestHit.collider.gameObject);
                    }
                }
                else
                {
                    oClosestHit = m_tHits[i];
                }
            }
        }
        //Debug.Log("min distance: " + fMinDistance);
        //Debug.DrawLine(transform.position, oClosestHit.point, Color.yellow);
        return oClosestHit;
    }

    List<Vector3> GetBorders()
    {
        List<Vector3> tBordersList = new List<Vector3>();
        for (int i = 0; i < m_tHits.Length; i++)
        {
            if (CheckPointIsBorder(i))
            {
                tBordersList.Add(m_tHits[i].point);
            }
        }
        return tBordersList;
    }

    void FollowWall()
    {
        Debug.Log("Following wall");
        Vector2 vInitialDirection = m_vCurrentWallPoint - transform.position;
        Vector3 oBestPoint = new Vector3();
        List<Vector3> tBordersList = GetBorders();
        float fMinAngle = 360;
        for (int i = 0; i < tBordersList.Count; i++)
        {
            Vector3 vNewDirection = tBordersList[i] - transform.position;
            float fAngle = Vector2.Angle(vInitialDirection, vNewDirection);
            if (fAngle < fMinAngle)
            {
                oBestPoint = tBordersList[i];
                fMinAngle = fAngle;
            }
        }
        Debug.DrawLine(transform.position, oBestPoint, Color.red);

        m_vCurrentWallPoint = oBestPoint;
        Vector3 vWallDirection = m_vCurrentWallPoint - transform.position;
        Debug.DrawLine(transform.position, m_vCurrentWallPoint, Color.magenta);
        Debug.DrawLine(transform.position, transform.position + m_vGeneralDirection.normalized * vWallDirection.magnitude, Color.yellow);
        Vector3 vTarget = (m_vCurrentWallPoint + (transform.position + m_vGeneralDirection.normalized * vWallDirection.magnitude))/2;
        Debug.DrawLine(transform.position, vTarget, Color.white);
        MoveToPosition(vTarget);
        if ((m_vCurrentWallPoint - m_oGoal.transform.position).sqrMagnitude < (m_vMinWallPoint - m_oGoal.transform.position).sqrMagnitude)
        {
            m_bFollowingWall = false;
        }
    }

    int ReturnEmptyBorderNextToIndex(int _iHitIndex)
    {

        int iSuperior = _iHitIndex + 1;
        int iInferior = _iHitIndex - 1;
        if (iSuperior >= m_tHits.Length)
        {
            iSuperior = 0;
        }
        if (iInferior < 0)
        {
            iInferior = m_tHits.Length - 1;
        }
        Debug.Log("index: " + _iHitIndex + " superior: " + iSuperior + " inferior: " + iInferior);
        if (m_tHits[iSuperior].collider == null)
        {
            return iSuperior;
        }
        else
        {
            return iInferior;
        }

    }

    bool CheckPointIsBorder(int _iHitIndex)
    {
        if (m_tHits[_iHitIndex].collider != null)
        {
            int iSuperior = _iHitIndex + 1;
            int iInferior = _iHitIndex - 1;
            if (iSuperior >= m_tHits.Length)
            {
                iSuperior = 0;
            }
            if (iInferior < 0)
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
