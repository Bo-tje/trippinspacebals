using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    public LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        EndLine();
    }
    
    public void ShowTrajectory(Vector3 startPoint, Vector3 velocity, float gravityScale)
    {
        int resolution = 30;
        float timeStep = 0.05f;
        lineRenderer.positionCount = resolution;
        
        Vector3[] points = new Vector3[resolution];
        Vector3 gravity = Physics2D.gravity * gravityScale;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            // Kinematic formula: s = s0 + v0*t + 0.5*a*t^2
            points[i] = startPoint + velocity * t + 0.5f * gravity * t * t;
        }

        lineRenderer.SetPositions(points);
    }

    public void EndLine()
    {
        lineRenderer.positionCount = 0;
    }
}
