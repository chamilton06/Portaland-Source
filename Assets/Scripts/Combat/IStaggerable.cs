// Something a parry can knock off balance, like DummyAttacker. Hitting it while it's
// staggered is a riposte: PlayerCombat deals extra damage and the opening ends.
// Give real enemies this too so the player can riposte them.
public interface IStaggerable
{
    bool IsStaggered { get; }

    // Called when a riposte lands, so one opening gives one critical hit.
    void EndStagger();
}
