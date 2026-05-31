using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;



public class MainMenuSCript : MonoBehaviour
{
public int scene = 0;
public Image letter;
public Button letterCloser;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        letter.enabled = false;
        letterCloser.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void revealLetter() 
    {
        letter.enabled = true;
        letterCloser.gameObject.SetActive(true);
    }

    public void closeLetter()
    {
        SceneManager.LoadScene(scene + 1);
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(scene + 1);

    }
}
