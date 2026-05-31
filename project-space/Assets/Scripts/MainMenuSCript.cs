using UnityEngine;
using UnityEngine.SceneManagement;



public class MainMenuSCript : MonoBehaviour
{
public int scene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(scene);

    }
}
