namespace REPORandomizer.Core
{
    public interface IRandomEffect
    {
        EffectId Id { get; }
        string Name { get; }
        string Description { get; }
        EffectType Type { get; }
        bool IsActive { get; }
        float Duration { get; }
        void Trigger();
        void Deactivate();
        void Update(float time);
        void OverrideDuration(float newDuration);
    }
}
