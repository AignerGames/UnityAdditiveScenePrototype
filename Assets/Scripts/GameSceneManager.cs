using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Currently this scene manager works with the essential scene + one additive scene
/// The script can be changed to support multiple additive scenes
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    private int _essentialsSceneIndex = 0;

    [SerializeField]
    private int _cubeSceneIndex = 1;

    [SerializeField]
    private int _sphereSceneIndex = 2;

    /// <summary>
    /// The scene that should be loaded if no other scene was dragged into the scene view in the editor
    /// </summary>
    [SerializeField]
    private int _debugInitialSceneIndex = 1;

    [SerializeField]
    private GameObject _loadingPanel = default;

    /// <summary>
    /// Additional delay during loading to fake long loading times
    /// </summary>
    [SerializeField]
    private float _debugLoadingDelay = 1f;

    [SerializeField]
    private Text _activeSceneText;

    private int _currentLoadedSceneIndex = -1;

    private bool _isLoading;

    void Start()
    {
        if(SceneManager.sceneCount == 1)
        {
            Debug.Log("No additive scene found, loading initial additive scene");
            ChangeScene(_debugInitialSceneIndex);
        }
        else
        {
            Debug.Log("Additive scene found, setting new active scene");

            // Find the other scene, that was drag & dropped in the editor
            var sceneCount = SceneManager.sceneCount;
            for(var i = 0; i < sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);

                if(scene.buildIndex == _essentialsSceneIndex)
                {
                    continue;
                }

                if(_currentLoadedSceneIndex != -1)
                {
                    Debug.LogError("Currently this scene manager only supports the essentials scene and one additive scene, but multiple scenes were found");
                }

                _currentLoadedSceneIndex = scene.buildIndex;
            }

            // The additive scene should be set as the active scene
            UpdateActiveScene();
        }
    }

    public void ChangeToCubeScene()
    {
        Debug.Log("ChangeToCubeScene clicked");
        ChangeScene(_cubeSceneIndex);
    }

    public void ChangeToSphereScene()
    {
        Debug.Log("ChangeToSphereScene clicked");
        ChangeScene(_sphereSceneIndex);
    }

    private void ChangeScene(int sceneIndex)
    {
        if(_isLoading || _currentLoadedSceneIndex == sceneIndex)
        {
            Debug.Log("Currently loading or the same scene is already loaded");
            return;
        }

        StartCoroutine(LoadScene(sceneIndex));
    }

    private IEnumerator LoadScene(int sceneIndex)
    {
        Debug.Log("Loading...");

        _isLoading = true;
        _loadingPanel.SetActive(true);

        // Disable player, input etc to prevent the player from doing something during loading

        if(_currentLoadedSceneIndex != -1)
        {
            Debug.Log($"Unloading scene: {_currentLoadedSceneIndex}");

            yield return SceneManager.UnloadSceneAsync(_currentLoadedSceneIndex);
        }

        Debug.Log($"Loading scene: {sceneIndex}");

        yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        Debug.Log($"Debug delay: {_debugLoadingDelay}");

        yield return new WaitForSeconds(_debugLoadingDelay);

        Debug.Log("Loading finished");

        _currentLoadedSceneIndex = sceneIndex;

        // Set the new loaded scene as active
        UpdateActiveScene();

        _loadingPanel.SetActive(false);
        _isLoading = false;
    }

    private void UpdateActiveScene()
    {
        // The active scene will be used to spawn new objects (unless they have a parent, then it will be added to the given parent as usual)
        // It depends on the game which scene should be set as active, but for this prototype I set the additive scene as active

        var scene = SceneManager.GetSceneByBuildIndex(_currentLoadedSceneIndex);
        if(!SceneManager.SetActiveScene(scene))
        {
            Debug.Log($"Failed to set active scene: {_currentLoadedSceneIndex}");
        }

        _activeSceneText.text = SceneManager.GetActiveScene().name;

        Debug.Log($"ActiveScene: {_activeSceneText.text}");
    }
}
