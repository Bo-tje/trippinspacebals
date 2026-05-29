using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slingShot : MonoBehaviour
{
    public float power = 10f;
    public Rigidbody2D rb;

    public Vector2 minPower;
    public Vector2 maxPower;
    public TrajectoryLine trajectoryLine;
    
    private Camera _cam;
    private Vector2 _force;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        trajectoryLine = GetComponent<TrajectoryLine>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _startPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
            _startPoint.z = 15;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 currentPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
            currentPoint.z = 15;
            trajectoryLine.RenderLine( _startPoint, currentPoint);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            _endPoint = _cam.ScreenToWorldPoint(Input.mousePosition);
            _endPoint.z = 15;

            _force = new Vector2(Mathf.Clamp(_startPoint.x - _endPoint.x, minPower.x, maxPower.x), Mathf.Clamp(_startPoint.y - _endPoint.y, minPower.y, maxPower.y));
            rb.AddForce(_force * power, ForceMode2D.Impulse);
            print(_endPoint);
            trajectoryLine.EndLine();
        }
    }
}
