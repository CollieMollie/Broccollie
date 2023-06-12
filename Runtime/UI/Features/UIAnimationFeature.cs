using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Presets;
using UnityEngine;

namespace Broccollie.UI
{
    [DisallowMultipleComponent]
    public class UIAnimationFeature : UIBaseFeature
    {
        #region Variable Field
        [Header("Animation Feature")]
        [SerializeField] private Element[] _elements = null;

        private AnimatorOverrideController _overrideController = null;

        #endregion

        #region Override Functions
        protected override List<Task> GetFeatures(UIStates state, CancellationToken ct)
        {
            List<Task> features = new List<Task>();
            if (_elements == null) return features;

            for (int i = 0; i < _elements.Length; i++)
            {
                if (!_elements[i].IsEnabled || _elements[i].Preset == null) continue;

                UIAnimationPreset.AnimationSetting setting = Array.Find(_elements[i].Preset.Settings, x => x.ExecutionState == state);
                if (setting == null || !setting.IsEnabled) continue;

                features.Add(PlayAnimationAsync(state.ToString(), _elements[i], setting, ct));
            }
            return features;
        }

        protected override List<Action> GetFeaturesInstant(UIStates state)
        {
            return base.GetFeaturesInstant(state);
        }

        #endregion

        #region Private Functions
        private async Task PlayAnimationAsync(string executionState, Element element, UIAnimationPreset.AnimationSetting setting, CancellationToken ct)
        {
            if (_overrideController == null)
            {
                _overrideController = new AnimatorOverrideController(element.Preset.OverrideAnimator);
                element.Animator.runtimeAnimatorController = _overrideController;
            }

            AnimatorOverrideController animator = (AnimatorOverrideController)element.Animator.runtimeAnimatorController;
            if (animator[executionState] != setting.Animation)
            {
                animator[executionState] = setting.Animation;
                element.Animator.runtimeAnimatorController = animator;
            }

            if (executionState != UIStates.Hover.ToString())
            {
                List<string> animationStates = new List<string>();
                animationStates.Add(UIStates.Default.ToString());
                animationStates.Add(UIStates.Show.ToString());
                animationStates.Add(UIStates.Hide.ToString());
                animationStates.Add(UIStates.Interactive.ToString());
                animationStates.Add(UIStates.NonInteractive.ToString());
                animationStates.Add(UIStates.Press.ToString());
                animationStates.Add(UIStates.Click.ToString());

                foreach (string animationState in animationStates)
                {
                    if (animationState == executionState) continue;
                    element.Animator.SetBool(animationState, false);
                }
                element.Animator.SetBool(setting.ExecutionState.ToString(), true);
            }
            else
            {
                element.Animator.SetBool(UIStates.Press.ToString(), false);
                element.Animator.SetBool(UIStates.Default.ToString(), true);
                element.Animator.SetBool(UIStates.Hover.ToString(), true);
            }

            if (executionState == UIStates.Default.ToString() || executionState == UIStates.Click.ToString())
                element.Animator.SetBool(UIStates.Hover.ToString(), false);

            await Task.Delay(TimeSpan.FromSeconds(setting.Animation.length).Milliseconds, ct);
        }

        private async Task PlayAnimationInstant()
        {

        }

        #endregion
    }

    [Serializable]
    public class Element
    {
        public bool IsEnabled;
        public Animator Animator = null;
        public UIAnimationPreset Preset = null;
    }
}
