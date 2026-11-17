// Something that reacts when the player's weapon hits it, e.g. an enemy that flinches or
// loses poise. PlayerCombat calls this after the damage is dealt (not for ripostes).
public interface IHitReceiver
{
    void OnHit(int damage);
}
