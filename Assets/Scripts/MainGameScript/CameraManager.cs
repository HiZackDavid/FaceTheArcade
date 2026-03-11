using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineBrain cameraBrain;

    [SerializeField] private CinemachineCamera primaryCamera;

    [SerializeField] private CinemachineCamera startingCamera;

    [SerializeField] private CinemachineCamera[] allCameras;

    private bool reactivateController = false;
    private bool ortho = false;
    private CinemachineCamera tCam;

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
        SwitchToCamera(startingCamera, ortho: false);
        ControllerManager.instance.DeactivateController();
    }

    public void SwitchToPrimaryCamera()
    {
        SwitchToCamera(primaryCamera, ortho: false);
    }


    public void SwitchToCamera(CinemachineCamera targetCamera, bool reactivateController = true, bool ortho = true)
    {
        tCam = targetCamera;
        this.reactivateController = reactivateController;
        this.ortho = ortho;

        ControllerManager.instance.DeactivateController();

        foreach (CinemachineCamera cam in allCameras) {
            cam.enabled = targetCamera == cam;
        }


        StartCoroutine(WaitForBlend());

    }

    IEnumerator WaitForBlend()
    {
        yield return new WaitUntil(() => cameraBrain.IsBlending);

        yield return new WaitUntil(() => !cameraBrain.IsBlending);

        if (reactivateController)
            ControllerManager.instance.ActivateController();
        else
            UIManager.instace.hideHideableHUD();

        if (ortho)
        {
            tCam.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            tCam.Lens.OrthographicSize = 0.2623907f;
            tCam.Lens.NearClipPlane = 0.3f;
            tCam.Lens.FarClipPlane = 10f;
        }

    }
}
