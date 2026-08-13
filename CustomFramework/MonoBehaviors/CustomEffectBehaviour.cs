using CustomFramework.CustomEffects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Utils.NonAllocLINQ;
using Logger = LabApi.Features.Console.Logger;

namespace CustomFramework.MonoBehaviors
{
    public class CustomEffectBehaviour : MonoBehaviour
    {
        public static bool Debug => CustomFrameworkPlugin.Debug;

        public ReferenceHub Owner;

        public List<CustomEffect> ActiveEffects = new List<CustomEffect>();

        public void Start()
        {
            Owner = GetComponent<ReferenceHub>();
        }

        public void Update()
        {
            foreach (var effect in ActiveEffects.ToList())
            {
                effect._timeTillTick -= Time.deltaTime;
                if (effect._timeTillTick <= 0f)
                {
                    effect._timeTillTick += effect.TimeBetweenTicks;
                    effect.Tick();
                }
                if (effect.Duration != 0f)
                {
                    effect.Duration -= Time.deltaTime;
                    if (effect.Duration <= 0f)
                    {
                        DisableEffect(effect);
                    }
                }
            }
        }

        public void ChangeEffect(CustomEffect effect, byte intensity, float duration, bool addDuration = false)
        {
            effect.Intensity = intensity;
            if (addDuration)
            {
                Logger.Debug("Add duration.", Debug);
                effect.Duration += duration;
            }
            else
            {
                Logger.Debug("Set duration.", Debug);
                effect.Duration = duration;
            }
        }

        public void ChangeEffect<T>(byte intensity, float duration, bool addDuration = false) where T : CustomEffect
        {
            if (ActiveEffects.TryGetFirst(t => t is T, out var eff))
            {
                ChangeEffect(eff, intensity, duration, addDuration);
            }
        }

        public CustomEffect EnableEffect(CustomEffect effect, byte intensity, float duration, bool addDuration = false)
        {
            bool made = false;
            if (!ActiveEffects.TryGetFirst(t => t.Id == effect.Id, out effect))
            {
                effect = (CustomEffect)Activator.CreateInstance(effect.GetType());
                effect.Hub = Owner;
                ActiveEffects.Add(effect);
                made = true;
            }
            ChangeEffect(effect, intensity, duration, addDuration);
            if (made)
                effect.Enable();
            return effect;
        }

        public T EnableEffect<T>(byte intensity, float duration, bool addDuration = false) where T : CustomEffect, new()
        {
            CustomEffect effect;
            bool made = false;
            if (!ActiveEffects.TryGetFirst(t => t is T, out effect))
            {
                effect = new T() { Hub = Owner };
                ActiveEffects.Add(effect);
                made = true;
            }
            ChangeEffect(effect, intensity, duration, addDuration);
            if (made)
                effect.Enable();
            return (T)effect;
        }

        public T GetEffect<T>() where T : CustomEffect
        {
            if (ActiveEffects.TryGetFirst(t => t is T, out var eff))
            {
                return (T)eff;
            }
            return null;
        }

        public void DisableEffect(CustomEffect eff)
        {
            Logger.Debug("Disabling effect", Debug);

            eff.Disable();
            ActiveEffects.Remove(eff);
        }

        public void DisableEffect<T>() where T : CustomEffect
        {
            if (ActiveEffects.TryGetFirst(t => t is T, out var eff))
                DisableEffect(eff);
        }
    }
}
