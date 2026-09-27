using System.Collections;
using UnityEngine;

namespace Infra2DAction
{
    public class PooledObject : MonoBehaviour
    {
        public string AssetID { get; set; }
        public PoolManager Owner { get; set; }

        private Coroutine _autoDespawnCoroutine;

        public void Despawn()
        {
            if (Owner != null) Owner.Despawn(gameObject);
            else Destroy(gameObject);
        }

        public void OnSpawn()
        {
            if (_autoDespawnCoroutine != null)
                StopCoroutine(_autoDespawnCoroutine);
        }

        public void AutoDespawn(float delay)
        {
            if (_autoDespawnCoroutine != null)
                StopCoroutine(_autoDespawnCoroutine);

            _autoDespawnCoroutine = StartCoroutine(DespawnAfterDelay(delay));
        }

        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Despawn();
        }

        public void OnDespawn()
        {
            if (_autoDespawnCoroutine != null)
            {
                StopCoroutine(_autoDespawnCoroutine);
                _autoDespawnCoroutine = null;
            }
        }
    }
}
