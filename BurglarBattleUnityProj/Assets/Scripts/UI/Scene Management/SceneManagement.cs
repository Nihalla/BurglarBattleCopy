using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneManagement : MonoBehaviour
{
    public enum SceneIndexes { SCENE_CONTROLLER, TITLE_SCREEN, GAME_SCREEN, END_SCREEN };
    public static SceneManagement SceneMangerInstance;
    public GameObject loadingScreen;
    public Image progressBar;
    private float totalSceneProgress;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private GameObject fadeTransition;
    private bool readyToProgress = false;
    [SerializeField] bool test = false;


    private void Awake()
    {
        SceneMangerInstance = this;
        SceneManager.LoadSceneAsync("Main Menu Scene", LoadSceneMode.Additive);
    }

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    public void LoadGame(string sceneToLoad)
    {
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex));
        scenesLoading.Add(SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive));
        fadeTransition.SetActive(false);
        //fadeTransition.StartFadeFromOne();

        StartCoroutine(GetSceneLoadProgress());
        readyToProgress = true;
    }
    public IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }
                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100f;
                loadingText.text = "Loading " + totalSceneProgress + "%";
                yield return null;
            }
        }
        Time.timeScale = 0.0f;
        loadingText.text = "Press X to continue...";
    }
    private void Update()
    {
        if (readyToProgress)
        {
            for (int i = 0; i < InputDevices.MAX_DEVICE_COUNT; i++)
            {
                if (InputDevices.Devices[i].Actions.PlayerController.Jump.WasPerformedThisFrame())
                {
                    //fadeTransition.gameObject.SetActive(false);
                    loadingScreen.SetActive(false);
                    readyToProgress = false;
                    Time.timeScale = 1.0f;
                }
            }
        }
        if(test)
        {
            LoadGame("AlphaScene");
            test = false;
        }
    }
}
