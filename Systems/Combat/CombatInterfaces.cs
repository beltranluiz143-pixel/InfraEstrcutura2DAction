using UnityEngine;

namespace Infra2DAction
{
    public interface IHittable
    {
        void ReceiveHit(int damage, Vector2 knockbackDirection);
    }

    public interface IPacifiable
    {
        string ID { get; }
        void Pacify();
    }

    public interface IDamageSource
    {
        int GetDamage();
        Vector2 GetKnockbackOrigin();
    }
}
