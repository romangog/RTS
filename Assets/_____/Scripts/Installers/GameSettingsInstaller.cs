using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GameSettingsInstaller", menuName = "Installers/GameSettingsInstaller")]
public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
{
    [HideLabel]
    [TabGroup("Game Mod")]
    public GameModSettings GameMod;

    [HideLabel]
    [TabGroup("GameSettings")]
    public GameSettings GameSettings;
    

    [HideLabel]
    [TabGroup("Prefabs")]
    public Prefabs Prefabs;

    public override void InstallBindings()
    {
        Container.BindInstance(GameMod).AsSingle().NonLazy();
        Container.BindInstance(GameSettings).AsSingle().NonLazy();
        Container.BindInstance(Prefabs).AsSingle().NonLazy();
    }
}