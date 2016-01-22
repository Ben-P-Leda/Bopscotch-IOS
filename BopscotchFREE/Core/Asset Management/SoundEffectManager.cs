using System.Collections.Generic;

using Microsoft.Xna.Framework.Audio;

namespace Leda.Core.Asset_Management
{
    public static class SoundEffectManager
    {
        private static Dictionary<string, SoundEffect> _effects = null;
        private static Dictionary<string, SoundEffectInstance> _effectInstances = null;

        public static bool Muted = false;

        public static void AddEffect(string effectName, SoundEffect effect, bool playFromInstance)
        {
            if (playFromInstance)
            {
                if (_effectInstances == null) { _effectInstances = new Dictionary<string, SoundEffectInstance>(); }

                if (!_effectInstances.ContainsKey(effectName)) { _effectInstances.Add(effectName, effect.CreateInstance()); }
                else { _effectInstances[effectName] = effect.CreateInstance(); }
            }
            else
            {
                if (_effects == null) { _effects = new Dictionary<string, SoundEffect>(); }

                if (!_effects.ContainsKey(effectName)) { _effects.Add(effectName, effect); }
                else { _effects[effectName] = effect; }
            }
        }

        public static SoundEffectInstance PlayEffect(string effectName)
        {
            return PlayEffect(effectName, 0.0f);
        }

        public static SoundEffectInstance PlayEffect(string effectName, float pitch)
        {
            if ((!Muted) && (!MuteDueToObscuring))
            {
                if ((_effectInstances != null) && (_effectInstances.ContainsKey(effectName)))
                {
                    _effectInstances[effectName].Play();
                    return _effectInstances[effectName];
                }
                else if ((_effects != null) && (_effects.ContainsKey(effectName)))
                {
                    _effects[effectName].Play(1.0f, pitch, 0.0f);
                }
            }

            return null;
        }
        public static SoundEffectInstance PlayEffect(string effectName)
        {
            if (!Muted) 
            {
				if ((_effectInstances != null) && (_effectInstances.ContainsKey(effectName))) { _effectInstances[effectName].Play(); return _effectInstances[effectName]; }
				if ((_effects != null) && (_effects.ContainsKey(effectName))) { _effects[effectName].Play(); }
            
				//AVAudioPlayer player = AVAudioPlayer.FromUrl (NSUrl.FromFilename("Content//SoundEffects//menu-select.wav"));
				//player.CurrentTime = player.Duration * 2;
				//player.NumberOfLoops = 1;
				//player.Volume = 1.0f;
				//player.PrepareToPlay();
				//player.Play ();
			}

            return null;
        }
    }
}
