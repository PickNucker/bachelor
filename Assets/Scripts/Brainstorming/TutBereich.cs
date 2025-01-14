using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TutBereich : MonoBehaviour
{
    [SerializeField] UnityEvent OnJoin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnJoin.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LeaveTutorial()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void LoadTutorial()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
