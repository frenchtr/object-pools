using System.Collections;
using System.Collections.Generic;
using TravisRFrench.ObjectPools.Runtime;
using UnityEngine;

namespace TravisRFrench.ObjectPools.Samples
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private float interval = 3f;
        [SerializeField]
        private List<GameObject> spawnPoints;
        private IObjectPool<GameObject> pool;

        private void Awake()
        {
            this.pool = new ObjectPool<GameObject>( () => Instantiate(this.prefab), Destroy);
            this.pool.Initialize();
        }

        private void OnEnable()
        {
            this.StartCoroutine(this.Spawn());
            
            this.pool.Retrieved += this.OnInstanceRetrieved;
            this.pool.Returned += this.OnInstanceReturned;
        }

        private void OnDisable()
        {
            this.pool.Retrieved -= this.OnInstanceRetrieved;
            this.pool.Returned -= this.OnInstanceReturned;
        }

        private IEnumerator Spawn()
        {
            while (true)
            {
                yield return new WaitForSeconds(this.interval);
                var instance = this.pool.Retrieve();
                var spawnPoint = this.spawnPoints[Random.Range(0, this.spawnPoints.Count)];
                instance.transform.position = spawnPoint.transform.position;
                instance.transform.rotation = spawnPoint.transform.rotation;
            }
        }
        
        private void OnInstanceRetrieved(GameObject instance)
        {
            instance.SetActive(true);
        }
        
        private void OnInstanceReturned(GameObject instance)
        {
            instance.SetActive(false);
        }
    }
}
