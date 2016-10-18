using Engine;
using Engine.Behaviors;

namespace GravityGame
{
    public class StageCompletionTrigger : Behavior
    {
        private GameObjectQuerySystem _goqs;
        private LevelLoadTrigger _levelLoadTrigger;
        private SceneLoaderSystem _sls;

        public override void Update(float deltaSeconds)
        {
        }

        protected override void Start(SystemRegistry registry)
        {
            _levelLoadTrigger = GameObject.GetComponent<LevelLoadTrigger>();
            _levelLoadTrigger.LevelLoadTriggered += OnLevelLoadTriggered;
            _goqs = registry.GetSystem<GameObjectQuerySystem>();
            _sls = registry.GetSystem<SceneLoaderSystem>();
        }

        private void OnLevelLoadTriggered(GameObject go)
        {
            if (EnabledInHierarchy)
            {
                PointCollector pointCollector = go.GetComponent<PointCollector>();
                TimeDisplay timer = _goqs.FindByName("TimeDisplay").GetComponent<TimeDisplay>();
                PlayerStageProgress psp = PlayerStageProgress.Instance;

                psp.StageCompleted(_sls.LoadedScene.Name, pointCollector.CollectedPoints, timer.Elapsed, _levelLoadTrigger.LoadedScene.ID);
            }
        }
    }
}
