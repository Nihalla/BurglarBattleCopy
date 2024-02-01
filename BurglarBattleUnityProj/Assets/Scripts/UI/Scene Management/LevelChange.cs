using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    [SerializeField] private string _sceneName = "BetaFloor";
    [SerializeField] private bool _delayed = false;
    [SerializeField] private Audio _playSound;

    public void ChangeScene()
    {

            if (_delayed)
            {
                AudioManager.PlayScreenSpace(_playSound);
                StartCoroutine(ChangeSceneCR());
            }
            else
            {
                SceneManager.LoadScene(_sceneName);
            }
    }

    public IEnumerator ChangeSceneCR()
    {
        yield return new WaitForSeconds(3f);
        if (FindObjectOfType<SceneManagement>() != null)
        {
            FindObjectOfType<SceneManagement>().LoadGame(_sceneName);
        }
        else
        {
            SceneManager.LoadScene(_sceneName);
            FadeTransition.instance.FadeOut();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene("GameEnd");
    }
}
