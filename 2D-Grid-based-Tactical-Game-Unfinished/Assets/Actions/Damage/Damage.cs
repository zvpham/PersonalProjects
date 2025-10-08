[System.Serializable]
public class Damage 
{
    public int minDamage;
    public int maxDamage;
    public DamageTypes damageType;

    public Damage(Damage oldDamage)
    {
        minDamage = oldDamage.minDamage;
        maxDamage = oldDamage.maxDamage;
        damageType = oldDamage.damageType;
    }

    public Damage(int minDamage, int maxDamage, DamageTypes damageType)
    {
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.damageType = damageType;
    }
}
