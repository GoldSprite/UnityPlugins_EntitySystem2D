using GoldSprite.GUtils;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GoldSprite.UFsm {
    [Serializable]
    public class RoamState : AIState {
        private Coroutine task;
        private bool stop;
        public bool EnterRoam;
        public float RoamVel;
        public float IdleRate = 0.3f;
        public Vector2 RoamVelMinMax = new Vector2(0.3f, 1f);
        [ShowInInspector]
        public Vector2 Direction { get => Fsm.ctrlFsm.Props.Direction; set => Fsm.ctrlFsm.Props.Direction = value; }


        public override bool CanTranSelf { get; protected set; } = false;
        public RoamState(AIFsm fsm) : base(fsm)
        {
            task = fsm.StartCoroutine(RoamTask());
        }

        public override bool Enter() => EnterRoam;
        public override bool Exit() => !EnterRoam;

        public override void Update()
        {
            //(Props.MoveState)?.Move();
        }

        public IEnumerator RoamTask()
        {
            while (!stop) {
                var randomTime = Random.Range(1f, 3f);
                yield return new WaitForSeconds(randomTime);

                EnterRoam = true;
                var roamVel = Random.Range(RoamVelMinMax.x, RoamVelMinMax.y);
                if (MathTool.rand.NextDouble() < IdleRate) RoamVel = 0f;
                else RoamVel = MathTool.rand.NextDouble() > 0.5f ? roamVel : -roamVel;
                var dir = Direction;
                dir.x = RoamVel;
                Direction = dir;

                randomTime = Random.Range(1f, 3f);
                yield return new WaitForSeconds(randomTime);
                EnterRoam = false;
                dir = Direction;
                dir.x = 0;
                Direction = dir;
            }
        }
    }
}