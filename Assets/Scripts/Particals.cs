using System;
using System.Collections.Generic;
using UnityEngine;

public class Particals : MonoBehaviour
{
    List<Particle> particle_list = new List<Particle>();
    public int particleCount;
    public GameObject cubePrefab;
    public float h = 0.7f;
    public float restDensity = 82.0f;
    [Range(20f, 100f)]
    public float kStiffness = 50f;
    public float viscosity = 0.6f;
    public Vector3 gravity = new Vector3(0, -9.8f, 0);


    public class Particle
    {
        
        public Particle(GameObject particleView)
        {
            position = particleView.transform.position;
            this.particleView = particleView;
        }

        public GameObject particleView;
        public Vector3 position = Vector3.zero;
        public Vector3 oldPosition = Vector3.zero;
        public Vector3 velocity = Vector3.zero;
        public Vector3 oldAcceleration = Vector3.zero;
        public Vector3 force = Vector3.zero;
        public float radius = 0.1f;
        public float mass = 1.0f;
        public float pressure = 0;
        public float density = 0;

        public List<KeyValuePair<Particle,float>> neighbors = new List<KeyValuePair<Particle, float>>();
    }


    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i < particleCount; i++)
        {
            Vector3 temp = this.transform.position + new Vector3( 2.0f*i ,0,0);
            GameObject particleView = Instantiate(cubePrefab, temp, Quaternion.identity);
            Particle newParticle = new Particle(particleView);
            particle_list.Add(newParticle);
        } 
    }

    
    private void getNeighbors(Particle particle)
    {
        foreach(Particle particle1 in particle_list )
        {
            float r = Vector3.Distance(particle.position, particle1.position);
            if(r == 0){
                continue;
            }

            if (r <= h)
            {
                particle.neighbors.Add(new KeyValuePair<Particle, float> (particle1,r));
            }
        }

    }

    private float fQ(float q)
    {
        if (q >= 0 && q < 1)
        {
            return (3.0f / (2.0f * Mathf.PI)) * 2.0f / 3.0f - Mathf.Pow(q, 2) + 0.5f * Mathf.Pow(q, 3);
        }

        else if (q >= 1 && q < 2)
        {
            return (3.0f / (2.0f * Mathf.PI)) * (1.0f / 6.0f) * Mathf.Pow(2.0f - q, 3);
        }
        else
            return 0;
    }

    private float kernal(float d)
    {
        return (1.0f / Mathf.Pow(h, 3)) * fQ(d / h);
    }

    private float getPressure(float density)
    {
        return kStiffness * (Mathf.Pow(density / restDensity,7)-1);
    }
    public Vector3 GradientSpiky(Vector3 r)
    {
        float coef = 45f / (Mathf.PI * Mathf.Pow(h, 6));
        float dist = r.magnitude;

        if (h < dist)
            return Vector3.zero;

        return -coef * r.normalized * Mathf.Pow(h - dist, 2);
    }

    public float ViscosityLaplacian(float r)
    {
        if (h < r)
            return 0f;

        float coef = 45f / (Mathf.PI * Mathf.Pow(h, 6));
        return coef * (h - r);
    }

    public Vector3 getPessureforce(Particle particle)
    {
        Vector3 pressureForce = Vector3.zero;

        foreach (var neighbor in particle.neighbors)
        {
            float cof = neighbor.Key.mass * ((particle.pressure / Mathf.Pow(particle.density, 2)) + (neighbor.Key.pressure / Mathf.Pow(neighbor.Key.density, 2)));
            pressureForce += cof * GradientSpiky(particle.position - neighbor.Key.position);
        }

        return -particle.mass*pressureForce;
    }
    public Vector3 getViscocityforce(Particle particle)
    {
        Vector3 viscocityForce = Vector3.zero;

        foreach (var neighbor in particle.neighbors)
        {
            Vector3 r = particle.position - neighbor.Key.position;
            Vector3 v = particle.velocity - neighbor.Key.velocity;

            viscocityForce += (neighbor.Key.mass / neighbor.Key.density) * v
                * ( Vector3.Dot(r, GradientSpiky(r)) / (Vector3.Dot(r, r) + 0.01f * (float)Math.Pow(h, 2)));

        }

        return particle.mass * viscosity * 2 * viscocityForce;
    }

    public Vector3 getGravityForce(Particle particle)
    {
        return particle.mass * gravity;
    }

    void Update()
    {
        foreach (Particle particle in particle_list)
        {
            Vector3 gravityForce = Vector3.zero;
            particle.particleView.transform.position = new Vector3(0, 8, -12);
            getNeighbors(particle);

            foreach(var neighbor in particle.neighbors)
            {
                particle.density += neighbor.Key.mass * kernal(neighbor.Value);
            }
            particle.pressure = getPressure(particle.density);
           
            particle.force = getPessureforce(particle) + getViscocityforce(particle) + getGravityForce(particle);

            particle.velocity = particle.velocity + ( (Time.deltaTime * particle.force) / particle.mass);
            particle.position = particle.position + ((Time.deltaTime * particle.velocity));
            //print(particle.position);

            particle.particleView.transform.position = particle.position;
        }

    }




}
