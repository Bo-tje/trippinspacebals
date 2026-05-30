using UnityEngine;

public class Catapult : MonoBehaviour
{
    public TrajectoryLine trajectoryLine;
    private Transform[] _childArray;
    
    private void Awake()
    {
        trajectoryLine = GetComponent<TrajectoryLine>();
        GetChildren();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _childArray.Length; i++)
        {
            print( _childArray[i]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void GetChildren()
    {
            _childArray = GetComponentsInChildren<Transform>();
        
    }
}
