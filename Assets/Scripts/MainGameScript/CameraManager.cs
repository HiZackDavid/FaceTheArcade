using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineBrain cameraBrain;

    [SerializeField] private CinemachineCamera primaryCamera;

    [SerializeField] private CinemachineCamera startingCamera;

    [SerializeField] private CinemachineCamera[] allCameras;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        } else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void SetStartingCamera()
    {
        SwitchToCamera(startingCamera);
        ControllerManager.instance.DeactivateController();
    }

    public void SwitchToPrimaryCamera()
    {
        SwitchToCamera(primaryCamera);
    }


    private void SwitchToCamera(CinemachineCamera targetCamera)
    {
        ControllerManager.instance.DeactivateController();

        foreach (CinemachineCamera cam in allCameras) {
            cam.enabled = targetCamera == cam;
        }

        StartCoroutine(WaitForBlend());

        ControllerManager.instance.ActivateController();
    }

    IEnumerator WaitForBlend()
    {
        yield return new WaitUntil(() => cameraBrain.IsBlending);

        yield return new WaitUntil(() => !cameraBrain.IsBlending);
    }
}
