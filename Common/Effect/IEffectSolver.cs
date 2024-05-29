namespace fms;

public interface IEffectSolver
{
    /// <summary>
    /// </summary>
    /// <param name="effect"></param>
    public void AddEffect(EffectBase effect);

    /// <summary>
    /// </summary>
    public void SolveEffect();
}