using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<LevelStateMachine>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TacticalCameraController>().AsSingle().NonLazy();
        Container.BindFactory<Vector3,bool,Transform, List<PawnController>, PawnController, PawnController.Factory>().AsSingle().NonLazy();
    }
}
