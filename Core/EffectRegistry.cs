using REPORandomizer.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REPORandomizer
{
    public class EffectRegistry
    {
        private readonly List<IRandomEffect> effects = new List<IRandomEffect>();
        private readonly Random random = new Random();

        public void RegisterEffect(IRandomEffect effect)
        {
            if (effect == null) return;
            effects.Add(effect);
        }

        public IRandomEffect GetRandomEffect()
        {
            if (effects.Count == 0) return null;

            int randomIndex = random.Next(0, effects.Count);
            return effects[randomIndex];
        }

        public IRandomEffect GetEffectById(string id)
        {
            if (id == null) return null;
            var target = EffectId.Of(id);
            return effects.FirstOrDefault(e => e.Id == target);
        }

        public int Count => effects.Count;
    }
}
