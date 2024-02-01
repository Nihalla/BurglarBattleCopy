using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition instance { get; private set; }
    [SerializeField] private bool _startFadedIn = true;
    [SerializeField] private CanvasGroup _canvasPanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private GameObject _teamPanelParent;
    [SerializeField] private GameObject _loadingHelp;
    [SerializeField] private GameObject _gameTipText;
    [SerializeField] private GameObject _collectTipText;
    private float durationBetweenHelp = 3f;

    private float _textChangeRate = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _loadingHelp.SetActive(false);
    }

    // Start is called before the first frame update
    async void Start()
    {
        if (_startFadedIn)
        {
            _canvasPanelCanvasGroup.alpha = 1;

            await Task.Delay(100);
            StartCoroutine(FadeOutRoutine());
        }
        else
        {
            _canvasPanelCanvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// Fades the loading screen in using a Coroutine
    /// </summary>
    public void FadeIn(bool useTeams = false)
    {
        if (useTeams)
        {
            //_teamPanelParent.SetActive(true);
            StartCoroutine(DisplayHelp());
        }
        else
        {
            _loadingHelp.SetActive(false);

        }

        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator DisplayHelp()
    {
        _teamPanelParent.SetActive(true);
        yield return new WaitForSeconds(durationBetweenHelp);
        _teamPanelParent.SetActive(false);
        _gameTipText.SetActive(false);
        _collectTipText.SetActive(false);
        _loadingHelp.SetActive(true);

    }
    /// <summary>
    /// Fades the loading screen out using a Coroutine
    /// </summary>
    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        while (_canvasPanelCanvasGroup.alpha < 1)
        {
            _canvasPanelCanvasGroup.alpha += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        while (_canvasPanelCanvasGroup.alpha > 0)
        {
            _canvasPanelCanvasGroup.alpha -= Time.deltaTime;

            if (_canvasPanelCanvasGroup.alpha < 0.1f)
            {
                _loadingHelp.SetActive(false);
            }

            yield return null;
        }
    }
}
