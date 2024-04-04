﻿using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.Types;
using static UnityEngine.GraphicsBuffer;

namespace GoldSprite.UFsm {
    [Serializable]
    public class Fsm : SerializedMonoBehaviour, IFsm {
        public bool TranRun = true;
        public bool StateRun = true;
        public bool ConditionRun = true;
        [ShowInInspector]
        protected virtual SortedDictionary<Type, IState> states { get; set; }
        [ShowInInspector]
        public IState CState { get; protected set; }
        public IState DefaultState { get; protected set; }
        public int LastPriority { get; private set; }
        public IProps Props { get; }


        protected void InitState(IState state)
        {
            var comparer = new TypeComparer();
            states = new SortedDictionary<Type, IState>(comparer);
            DefaultState = CState = state;
            AddState(state, 0);
        }

        protected void AddState(IState state, int priority = 1)
        {
            states.Add(state.GetType(), state);
            LastPriority += priority;
            state.Priority = LastPriority;
        }

        protected void AddStateFix(IState state, int priority)
        {
            states.Add(state.GetType(), state);
            state.Priority = priority;
            LastPriority = states.Values.OrderBy(p => p.Priority).Last().Priority;
        }

        public void Begin()
        {
            CState.OnEnter();
        }


        public bool UpdateNextState()
        {
            var exit = false;
            if (CState.Exit()) { OnEnterState(DefaultState); exit = true; }

            var targetState = CState;
            foreach (var state in states.Values) {
                if (EnterState(state, targetState)) targetState = state;
            }
            var enter = targetState != CState || (targetState.CanTranSelf && targetState.Enter());
            if (enter) OnEnterState(targetState);
            return enter && exit;
        }

        protected bool EnterState(IState target, IState current)
        {
            if ((target == CState && !target.CanTranSelf) || current.Priority > target.Priority) return false;
            return target.Enter();
        }

        protected void OnEnterState(IState state)
        {
            CState.OnExit();
            CState = state;
            CState.OnEnter();
        }

        public T GetState<T>() where T : IState
        {
            if (states.TryGetValue(typeof(T), out IState state)) return (T)state;
            return default;
        }

        public bool IsDefaultState() { return CState == DefaultState; }

        public virtual void Update()
        {
            if (ConditionRun)
                UpdateConditions();

            if (TranRun)
                UpdateNextState();

            if (StateRun) CState.Update();
        }

        public void UpdateConditions()
        {
            foreach(var state in states.Values) {
                state.UpdateCondition();
            }
        }

        public virtual void FixedUpdate()
        {
            if (StateRun) CState.FixedUpdate();
        }


        class TypeComparer : IComparer<Type> {
            public int Compare(Type x, Type y)
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
}
