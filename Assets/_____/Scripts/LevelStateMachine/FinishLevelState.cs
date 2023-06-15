public class FinishLevelState : LevelState
{
    private readonly FinishScreenView _finishScreenView;
    private readonly SceneLoaderWrapper _sceneLoader;

    public override LevelStateType Type => LevelStateType.Finish;

    public FinishLevelState(LevelStateMachine.ServicesPack pack)
    {
        _finishScreenView = pack.Ui.FinishScreenView;
        _sceneLoader = pack.SceneLoader;
    }
    public override void Start()
    {
        _finishScreenView.gameObject.SetActive(true);
        _finishScreenView.AgainButton.onClick.AddListener(OnAgainButtonClicked);
    }

    private void OnAgainButtonClicked()
    {
        _sceneLoader.LoadLevel(1);
    }

    public override void Stop()
    {
        _finishScreenView.gameObject.SetActive(false);
        _finishScreenView.AgainButton.onClick.RemoveListener(OnAgainButtonClicked);
    }

    public override void Update()
    {
        
    }
    public override void FixedUpdate()
    {
        
    }
}
