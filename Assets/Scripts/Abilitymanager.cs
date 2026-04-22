using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    private IAbility[] abilities;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        abilities = new IAbility[]
        {
            GetComponent<Ability_Invincibility>(),
            GetComponent<Ability_Projectile>(),
            GetComponent<Ability_Shrink>()
        };
    }

    public void TryActivateAbility(int index)
    {
        if (abilities == null || index < 0 || index >= abilities.Length) return;
        var ability = abilities[index];
        if (ability == null) return;

        if (!ability.IsReady()) return;

        int cost = ability.GetPointCost();
        if (ScoreManager.Instance == null) return;
        if (ScoreManager.Instance.CurrentScore < cost) return;

        ScoreManager.Instance.SpendPoints(cost);
        ability.Activate();
    }

    public IAbility GetAbility(int index)
    {
        if (abilities == null || index < 0 || index >= abilities.Length) return null;
        return abilities[index];
    }
}

public interface IAbility
{
    bool IsReady();
    int GetPointCost();
    void Activate();
    float GetCooldownRemaining();
    bool IsActive { get; }
}