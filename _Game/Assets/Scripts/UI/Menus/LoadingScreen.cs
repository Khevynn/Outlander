using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingText;
    
    private void Start()
    {
        StartCoroutine(RedirectToCorrectScene());
        StartCoroutine(LoadingTextAnimation());
    }

    private IEnumerator LoadingTextAnimation()
    {
        loadingText.text = "Loading";
        int i = 0;
        while (true)
        {
            yield return new WaitForSeconds(.5f);
            if (i >= 3)
            {
                i = 0;
                loadingText.text = "Loading";
                continue;
            }
            
            i++;
            loadingText.text += ".";
        }
    }
    private IEnumerator RedirectToCorrectScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.Instance.SceneToLoad);
        
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress);
            progressBar.value = progressValue;

            yield return null;
        }
        
        SceneManager.UnloadSceneAsync(SceneLoader.Instance.SceneToUnload); 
    }
}
