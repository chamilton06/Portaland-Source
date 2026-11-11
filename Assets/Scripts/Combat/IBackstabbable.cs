// Something the player can backstab, like EnemyMelee: hitting it from behind while it's open
// deals critical damage (PlayerCombat's Backstab Multiplier) and knocks it off its feet.
public interface IBackstabbable
{
    // False while it can't be backstabbed, e.g. mid-attack, staggered or dead.
    bool CanBeBackstabbed { get; }

    void OnBackstabbed();
}
