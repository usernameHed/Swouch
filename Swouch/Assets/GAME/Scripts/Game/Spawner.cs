using swouch.extension.propertyAttribute.noNull;
using swouch.extension.runtime.ui.animate.timeline;
using swouch.time;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace swouch.entity
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private float _minTime = 1f;
        [SerializeField] private float _maxTime = 4f;

        [SerializeField] private Transform[] _poolPositions = new Transform[50];
        [SerializeField, NoNull] private Swoutcher _enemy = default;
        [SerializeField, NoNull] private PlayerUpdater _player = default;

        private FrequencyCoolDown _frequencySpawn = new FrequencyCoolDown();

        private void Update()
        {
            if (_frequencySpawn.IsNotRunning())
            {
                Transform position = PickRandom(_poolPositions);

                Swoutcher swoutcher = GameObject.Instantiate(_enemy, position.position, position.rotation, transform);
                swoutcher.Build(_player.transform);
                _frequencySpawn.StartCoolDown(UnityEngine.Random.Range(_minTime, _maxTime));
            }
        }

        /// <summary>
        /// pick a random number in array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public T PickRandom<T>(T[] collection)
        {
            return (collection[UnityEngine.Random.Range(0, collection.Length)]);
        }
    }
}