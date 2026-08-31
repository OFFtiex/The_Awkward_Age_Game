using UnityEngine;

public class Watch : MonoBehaviour
{
    public enum WatchType { Aging, Rejuvenating }

    private bool _isTriggered = false;

    [SerializeField] private WatchType typeOfWatch;
    [SerializeField] private AudioClip Watch_Clip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isTriggered) return;
        if (!collision.CompareTag("Player"))                    return;
        if (!collision.TryGetComponent<Player>(out var player)) return;
        if (player == null)                                     return;

        if (TryGetComponent<Collider2D>(out var myCollider))
        {
            _isTriggered = true;
        }

        player.PlaySFX(Watch_Clip);

        if (typeOfWatch == WatchType.Aging)
        {
            ApplyAging(player);
        }
        else
        {
            ApplyRejuvenation(player);
        }

        Destroy(gameObject);
    }

    private void ApplyAging(Player player)
    {
        // You touch the clock and you become older
        
        player.CurrentAge = player.CurrentAge switch
        {
            AgeState.Youth => AgeState.Prime,
            AgeState.Prime => AgeState.Aging,
            AgeState.Aging => HandleDeath(player, "Senescence"),
            _ => player.CurrentAge
        };
    }

    private void ApplyRejuvenation(Player player)
    {
        // You touch the clock and you become younger
       
        player.CurrentAge = player.CurrentAge switch
        {
            AgeState.Aging => AgeState.Prime,
            AgeState.Prime => AgeState.Youth,
            AgeState.Youth => HandleDeath(player, "Chronological Regression"),
            _ => player.CurrentAge
        };
    }

    private AgeState HandleDeath(Player player, string reason)
    {
        player.Kill(reason);
        return player.CurrentAge;
    }
}