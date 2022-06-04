using UnityEngine;

public class Particals : MonoBehaviour
{

    public int particleCount;
    public float h = 1f;
    public float restDensity = 0.02f;
    public float kStiffness = 2000.0f;
    public float viscosity = 10.0f;
    public float radius = 1;
    public float mass = 1;
    public int rowSize = 100;

    [Range(-1,0)]
    public float DAMPING_COEFFICIENT = -0.5f;

    public Mesh mesh;
    public Material material;

    Particle[] particlesArray;
    ComputeBuffer particlesBuffer;

    public ComputeShader shader;
    int kernelComputeDensityPressure;
    int kernelComputeForces;
    int kernelIntegrate;

    private static Vector4 gravity = new Vector4(0.0f, -9.81f, 0.0f, 2000.0f);
    private const float deltaTime = 0.0008f;
    int groupSize;


    private struct Particle
    {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 force;

        public float density;
        public float pressure;

        public Particle(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            force = Vector3.zero;
            density = 0.0f;
            pressure = 0.0f;
        }
    }
    int SIZE_SPHPARTICLE = 11 * sizeof(float);


    // Start is called before the first frame update
    void Start()
    {
        InitSPH();
        InitShader();
    }

    private void InitSPH()
    {
        
        kernelComputeDensityPressure = shader.FindKernel("CumputeDensityPressure");

        uint numThreadsX;
        shader.GetKernelThreadGroupSizes(kernelComputeDensityPressure, out numThreadsX, out _, out _);
        groupSize = Mathf.CeilToInt((float)particleCount / (float)numThreadsX);
        int amount = (int)numThreadsX * groupSize;

        particlesArray = new Particle[amount];
        float center = rowSize * 0.5f;

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = new Vector3();
            pos.x = (i % rowSize) + UnityEngine.Random.Range(-0.1f, 0.1f) - center;
            pos.y = 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            pos.z = ((i / rowSize) % rowSize) + UnityEngine.Random.Range(-0.1f, 0.1f) - center;
            pos *= radius;

            particlesArray[i] = new Particle(pos);
        }
    }

    private void InitShader()
    {
        kernelComputeForces = shader.FindKernel("ComputeForces");
        kernelIntegrate = shader.FindKernel("Integrate");

        float hSq = h * h;

        particlesBuffer = new ComputeBuffer(particlesArray.Length, SIZE_SPHPARTICLE);
        particlesBuffer.SetData(particlesArray);

        shader.SetInt("particleCount", particlesArray.Length);
        shader.SetFloat("smoothingRadius", h);
        shader.SetFloat("smoothingRadiusSq", hSq);
        shader.SetFloat("kStiffness", kStiffness);
        shader.SetFloat("restDensity", restDensity);
        shader.SetFloat("radius", radius);
        shader.SetFloat("mass", mass);
        shader.SetFloat("particleViscosity", viscosity);
        shader.SetFloat("deltaTime", deltaTime);
        shader.SetVector("gravity", gravity);
        shader.SetFloat("DAMPING_COEFFICIENT", DAMPING_COEFFICIENT);

        shader.SetBuffer(kernelComputeDensityPressure, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeForces, "particles", particlesBuffer);
        shader.SetBuffer(kernelIntegrate, "particles", particlesBuffer);
      
    }

    private void Update()
    {
        shader.Dispatch(kernelComputeDensityPressure, groupSize, 1, 1);
        shader.Dispatch(kernelComputeForces, groupSize, 1, 1);
        shader.Dispatch(kernelIntegrate, groupSize, 1, 1);

        particlesBuffer.GetData(particlesArray);

        foreach (Particle particle in particlesArray)
        {
            Graphics.DrawMesh(mesh, particle.position, Quaternion.identity, material, 0);
        }

    }


    private void OnDestroy()
    {
        particlesBuffer.Dispose();
    }

}
