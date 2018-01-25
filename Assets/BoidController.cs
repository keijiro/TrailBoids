using UnityEngine;
using System.Collections.Generic;

namespace TrailBoids
{
    public class BoidController : MonoBehaviour
    {
        #region Editable properties

        [SerializeField] int _spawnCount = 10;
        [SerializeField] float _spawnRadius = 4;

        [SerializeField] float _velocity = 6;
        [SerializeField, Range(0, 1)] float _velocityVariance = 0.5f;
        [SerializeField] Vector3 _scroll = Vector3.zero;

        [SerializeField] float _rotationSpeed = 4;
        [SerializeField] float _neighborDistance = 2;

        #endregion

        #region Boid array

        class Boid
        {
            public Vector3 position;
            public Quaternion rotation;
            public float noiseOffset;
            public GameObject gameObject;
        }

        List<Boid> _boids = new List<Boid>();

        #endregion

        #region Boid behavior

        // Calculates a separation vector from a boid with another boid.
        Vector3 GetSeparationVector(Boid self, Boid target)
        {
            var diff = target.position - self.position;
            var diffLen = diff.magnitude;
            var scaler = Mathf.Clamp01(1 - diffLen / _neighborDistance);
            return diff * scaler / diffLen;
        }

        // Reynolds' steering behavior
        void SteerBoid(Boid self)
        {
            // Steering vectors
            var separation = Vector3.zero;
            var alignment = transform.forward;
            var cohesion = transform.position;

            // Looks up nearby boids.
            var neighborCount = 0;
            foreach (var neighbor in _boids)
            {
                if (neighbor == self) continue;

                var dist = Vector3.Distance(self.position, neighbor.position);
                if (dist > _neighborDistance) continue;

                // Influence from this boid
                separation += GetSeparationVector(self, neighbor);
                alignment += neighbor.rotation * Vector3.forward;
                cohesion += neighbor.position;

                neighborCount++;
            }

            // Normalization
            var div = 1.0f / (neighborCount + 1);
            alignment *= div;
            cohesion = (cohesion * div - self.position).normalized;

            // Calculate the target direction and convert to quaternion.
            var direction = separation + alignment * 0.667f + cohesion;
            var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

            // Applys the rotation with interpolation.
            if (rotation != self.rotation)
            {
                var ip = Mathf.Exp(-_rotationSpeed * Time.deltaTime);
                self.rotation = Quaternion.Slerp(rotation, self.rotation, ip);
            }
        }

        // Position updater
        void AdvanceBoid(Boid self)
        {
            var noise = Mathf.PerlinNoise(Time.time * 0.5f, self.noiseOffset) * 2 - 1;
            var velocity = _velocity * (1 + noise * _velocityVariance);
            var forward = self.rotation * Vector3.forward;
            self.position += (forward * velocity + _scroll) * Time.deltaTime;
        }

        #endregion

        #region Public methods

        GameObject _template;

        public void Spawn()
        {
            Spawn(transform.position + Random.insideUnitSphere * _spawnRadius);
        }

        public void Spawn(Vector3 position)
        {
            var go = Instantiate(_template);
            go.transform.parent = transform;
            go.SetActive(true);

            _boids.Add(new Boid() {
                position = position,
                rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f),
                noiseOffset = Random.value * 10,
                gameObject = go
            });
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _template = transform.GetChild(0).gameObject;
            _template.SetActive(false);

            for (var i = 0; i < _spawnCount; i++) Spawn();
        }

        void Update()
        {
            foreach (var boid in _boids) SteerBoid(boid);
            foreach (var boid in _boids) AdvanceBoid(boid);

            foreach (var boid in _boids)
            {
                var tr = boid.gameObject.transform;
                tr.position = boid.position;
                tr.rotation = boid.rotation;
            }
        }

        #endregion
    }
}
