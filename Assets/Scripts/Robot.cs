using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    // Start is called before the first frame update

    public bool m_bPaused = true, m_bFinished = false;
    RaycastHit2D[] m_tHits = new RaycastHit2D[360];
    public float m_fDetectionDistance, m_fSpeed;
    public Goal m_oGoal;
    LayerMask m_oNotTheRobot;
    bool m_bFollowingWall;
    Vector3 m_vMinWallPoint;
    Vector3 m_vCurrentWallPoint;
    Vector3 m_vGeneralDirection;
    Vector3 m_vPastPosition, m_vEvenPreviousPosition;

    public LineRenderer m_oLineRenderer1, m_oLineRenderer2, m_oLineRenderer3;

    private void Start()
    {
        m_oNotTheRobot = ~LayerMask.GetMask("Robot");
    }

    void OnTriggerEnter2D()
    {
        m_bFinished = true;
    }

    private void Update()
    {
        m_oLineRenderer1.enabled = false;
        m_oLineRenderer2.enabled = false;
        m_oLineRenderer3.enabled = false;
        if (!m_bPaused && !m_bFinished)
        {
            Vector3 vGoalPosition = m_oGoal.transform.position;
            RaycastHit2D oGoalHit = Physics2D.Raycast(transform.position, vGoalPosition - transform.position, m_fDetectionDistance, m_oNotTheRobot);
            if (!m_bFollowingWall)
            {

                //No obstacles
                if (oGoalHit.collider == null || oGoalHit.collider == m_oGoal.m_oCollider)
                {
                    DrawLine(m_oLineRenderer1, transform.position, m_oGoal.transform.position, Color.green);
                    MoveToPosition(m_oGoal.transform.position);
                    Debug.DrawLine(transform.position, m_oGoal.transform.position, Color.green);
                }
                else
                {
                    ScanSorroundings();
                    RaycastHit2D oClosestHit = GetClosestHitToGoal();
                    m_vMinWallPoint = oClosestHit.point;
                    m_vCurrentWallPoint = m_vMinWallPoint;
                    DrawLine(m_oLineRenderer1, transform.position, m_vMinWallPoint, new Color(1, 0, 0, 0.5f));
                    DrawLine(m_oLineRenderer2, m_vMinWallPoint, m_oGoal.transform.position, new Color(1, 0.5f, 0, 0.25f));
                    MoveToPosition(m_vMinWallPoint);
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
                    }
                }
                else
                {
                    oClosestHit = m_tHits[i];
                }
            }
        }
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
        Vector3 vTarget = (m_vCurrentWallPoint + (transform.position + m_vGeneralDirection.normalized * vWallDirection.magnitude))/2;
        DrawLine(m_oLineRenderer1, transform.position, m_vMinWallPoint, new Color(1,0,0, 0.5f));
        DrawLine(m_oLineRenderer3, transform.position, vTarget, new Color(1, 1f, 0, 0.5f));
        DrawLine(m_oLineRenderer2, m_vMinWallPoint, m_oGoal.transform.position, new Color(1, 0.5f, 0, 0.25f));
        MoveToPosition(vTarget);
        if ((m_vCurrentWallPoint - m_oGoal.transform.position).sqrMagnitude < (m_vMinWallPoint - m_oGoal.transform.position).sqrMagnitude)
        {
            m_bFollowingWall = false;
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

    public void DrawLine(LineRenderer _oRenderer, Vector3 _vOrigin, Vector3 _vEnd, Color _oColor)
    {
        _oRenderer.enabled = true;
        Vector3[] tPositions = new Vector3[] { _vOrigin, _vEnd };
        _oRenderer.startColor = _oColor;
        _oRenderer.endColor = _oColor;
        _oRenderer.SetPositions(tPositions);
    }
}
