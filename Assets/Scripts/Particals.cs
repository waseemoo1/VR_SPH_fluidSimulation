using System;
using System.Collections.Generic;
using UnityEngine;

public class Particals : MonoBehaviour
{
    List<Particle> particle_list = new List<Particle>();
    public int particleCount;
    public GameObject cubePrefab;
    public float h = 1f;
    public float restDensity = 0.02f;
    public float kStiffness = 1.0f;
    public float viscosity = 10.0f;
    public Vector3 gravity = new Vector3(0, -9.8f, 0);
    int count = 0;

    public float DAMPING_COEFFICIENT = -0.1f;


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
        public float mass = 0.2f;
        public float pressure = 1.0f;
        public float density = 1.0f;

        public List<KeyValuePair<Particle,float>> neighbors = new List<KeyValuePair<Particle, float>>();
    }


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("addParticle", 1.0f, 3.0f);
    }

    void addParticle()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 temp = this.transform.position + new Vector3(i*0.5f, 0, i * 0.5f);
            GameObject particleView = Instantiate(cubePrefab, temp, Quaternion.identity);
            particleView.name = "particle " + count.ToString();
            count++;

            Particle newParticle = new Particle(particleView);
            particle_list.Add(newParticle);
        }
    }


    private void FixedUpdate()
    {
        foreach (Particle particle in particle_list)
        {
            getNeighbors(particle);
        }
        foreach (Particle particle in particle_list)
        {
            particle.density = getDensity(particle);
            particle.pressure = getPressure(particle.density);
        }
        foreach (Particle particle in particle_list)
        {
            particle.force = getPessureforce(particle) + getViscocityforce(particle)+ getGravityForce(particle);
            
        }
        foreach (Particle particle in particle_list)
        {
            particle.velocity = particle.velocity + ((Time.fixedDeltaTime * particle.force) / particle.mass);
            particle.position = particle.position + ((Time.fixedDeltaTime * particle.velocity));


            
            //boundary condition
            //down
            if (particle.position.y < 1)
            {
                particle.velocity.y *= DAMPING_COEFFICIENT;
                particle.position.y = 1;
            }
            //left and right
            if (particle.position.x < 4)
            {
                particle.velocity.x *= DAMPING_COEFFICIENT;
                particle.position.x = 4;
            }
            if (particle.position.x > 20)
            {
                particle.velocity.x *= DAMPING_COEFFICIENT;
                particle.position.x = 20;
            }

            //up and down
            if (particle.position.z > -5.5f)
            {
                particle.velocity.z *= DAMPING_COEFFICIENT;
                particle.position.z = -5.5f;
            }
            if (particle.position.z < -27.5f)
            {
                particle.velocity.z *= DAMPING_COEFFICIENT;
                particle.position.z = -27.5f;
            }

            particle.particleView.transform.position = particle.position;
        }
    }


    private float getDensity(Particle particle)
    {
        float sum = 0;
        foreach (var neighbor in particle.neighbors)
        {
            sum += SphSpikyKernel3(neighbor.Value);
        }
        return sum * particle.mass;
    }

    private void getNeighbors(Particle particle)
    {
        particle.neighbors.Clear();
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
            return (3.0f / (2.0f * Mathf.PI)) * (2.0f / 3.0f) - Mathf.Pow(q, 2) +(0.5f * Mathf.Pow(q, 3));
        }

        else if (q >= 1 && q < 2)
        {
            return (3.0f / (2.0f * Mathf.PI)) * (1.0f / 6.0f) * Mathf.Pow(2.0f - q, 3);
        }
        else
            return 0;
    }

    private float SphSpikyKernel3(float distance)
    {
        if (distance >= h)
        {
            return 0.0f;
        }
        else
        {
            float x = 1.0f - distance / h;
            return (15.0f / (Mathf.PI * (h * h * h))) * x * x * x;
        }
    }

    private float firstDerivative(float distance)
    {
        if (distance >= h)
        {
            return 0.0f;
        }
        else
        {
            float x = 1.0f - distance / h;
            return -45.0f / (Mathf.PI * Mathf.Pow(h,4) ) * x * x;
        }
    }

    private Vector3 gradient(float distance,Vector3 directionToCenter)
    {
        return -firstDerivative(distance) * directionToCenter;
    }

    private float secondDerivative(float distance)
    {
        if (distance >= h)
        {
            return 0.0f;
        }
        else
        {
            float x = 1.0f - distance / h;
            return 90.0f / (Mathf.PI * Mathf.Pow(h,5) ) * x;
         }
    }

        private float standardKernal(float distance)
    {
        if(distance >= h)
        {
            return 0.0f;
        }
        else
        {
            float x = 1.0f - distance * distance / h;
            return 315.0f / (64.0f * Mathf.PI * h * h * h) * x * x * x;
        }
    }

    /*private Vector3 interpolate(Particle particle)
    {
        Vector3 sum;

    }*/

    private float kernal(float d)
    {
        float q = d / h;
        return (1.0f / Mathf.Pow(h, 3)) * fQ(q);
    }

    private float getPressure(float density)
    {
        //float p = kStiffness * (density - restDensity);
        float p = kStiffness * (Mathf.Pow(density / restDensity,7) - 1);

        /*if(p<0)
        {
            p *= -0.9f; 
        }*/

        return p;
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
            Vector3 dir = (particle.position - neighbor.Key.position)/ neighbor.Value;
            float coef = (particle.pressure / (particle.density * particle.density)
                + neighbor.Key.pressure / (neighbor.Key.density * neighbor.Key.density));
            //print(particle.density);
            pressureForce += coef
                *gradient(neighbor.Value, dir);

            
            //float cof = neighbor.Key.mass * ( (particle.pressure / Mathf.Pow(particle.density, 2)) + (neighbor.Key.pressure / Mathf.Pow(neighbor.Key.density, 2)) );
            //print(particle.density);
            //pressureForce += cof * GradientSpiky(particle.position - neighbor.Key.position);
            
        }
        return -1* particle.mass * particle.mass * pressureForce;

        //return -particle.mass*pressureForce;
    }
    public Vector3 getViscocityforce(Particle particle)
    {
        Vector3 viscocityForce = Vector3.zero;

        foreach (var neighbor in particle.neighbors)
        {
            Vector3 r = particle.position - neighbor.Key.position;
            Vector3 v = neighbor.Key.velocity - particle.velocity;
            Vector3 coef = viscosity * particle.mass * particle.mass
                * (v / neighbor.Key.density);

            viscocityForce += coef
                * secondDerivative(neighbor.Value);

            /*viscocityForce += (neighbor.Key.mass / neighbor.Key.density) * v
                * ( Vector3.Dot(r, GradientSpiky(r)) / (Vector3.Dot(r, r) + 0.01f * (float)Math.Pow(h, 2)));*/
        }

        return viscocityForce;
    }

    public Vector3 getGravityForce(Particle particle)
    {
        return particle.mass * gravity;
    }

    public string showInfo(Particle particle)
    {
        string info = "";
        info += "force: " + particle.force + "\n";
        info += "velocioty: " + particle.velocity + "\n";
        return info;
    }

    

}
