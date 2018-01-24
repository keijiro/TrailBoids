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
        [SerializeField, Range(0, 1)] float _velocityRandomness = 0.5f;
        [SerializeField] float _rotationSpeed = 4;
        [SerializeField] float _neighborDistance = 2;

        #endregion

        #region Internal properties

        GameObject Template {
            get {
                foreach (Transform tr in transform) return tr.gameObject;
                return null;
            }
        }

        #endregion

        #region Boid array

        class Boid
        {
            public Vector3 position;
            public Quaternion rotation;
            public float noiseOffset;
            public GameObject gameObject;
        }

        List<Boid> _boids;

        #endregion

        #region Boid behavior

        // Calculates a separation vector from a boid with another.
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
            var velocity = _velocity * (1 + noise * _velocityRandomness);
            var forward = self.rotation * Vector3.forward;
            self.position += forward * velocity * Time.deltaTime;
        }

        #endregion

        #region Public methods

        public void Spawn()
        {
            Spawn(transform.position + Random.insideUnitSphere * _spawnRadius);
        }

        public void Spawn(Vector3 position)
        {
            var go = Instantiate(Template);

            var boid = new Boid() {
                position = position,
                rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f),
                noiseOffset = Random.value * 10,
                gameObject = go
            };

            _boids.Add(boid);
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _boids = new List<Boid>();
            for (var i = 0; i < _spawnCount; i++) Spawn();
        }

        void Update()
        {
            foreach (var boid in _boids)
            {
                SteerBoid(boid);
                AdvanceBoid(boid);

                boid.gameObject.transform.position = boid.position;
                boid.gameObject.transform.rotation = boid.rotation;
            }
        }

        #endregion
    }
}
