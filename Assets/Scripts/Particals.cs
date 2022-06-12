using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Particals : MonoBehaviour
{
    public bool showParticle = true;
    //public bool showVolume = true; 
    private float ParticleVolume;
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

    public Mesh mesh = null;
    public Material material;
    public Material m_volumeMat;

    Bounds argBounds = new Bounds(Vector3.zero, Vector3.one * 0);
    uint[] argsArray = { 0, 0, 0, 0, 0 };
    ComputeBuffer argsBuffer;




    //grid
    public GameObject gridBounds;

    public bool useGrid;
    const int SIZE_GRID_CELL = 4 * sizeof(int);
    ComputeBuffer gridBuffer;

    Particle[] particlesArray;
    ComputeBuffer particlesBuffer;

    public ComputeShader shader;

    int kernelClearGrid;
    int kernelPopulateGrid;
    int kernelComputeDensityPressure;
    int kernelComputeForces;
    int kernelIntegrate;
    int kernalComputeVolume;

    private static Vector4 gravity = new Vector4(0.0f, -9.81f, 0.0f, 2000.0f);
    private const float deltaTime = 0.0008f;

    int groupSize;
    int gridGroupSize;
    int gridCount;
    Vector4 gridDimensions;
    Vector4 gridStartPosition;

    private struct Particle
    {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 force;

        public float density;
        public float pressure;


        public Vector3Int gridLocation;
        public int gridIndex;

        public Vector3Int voxel;
        public int voxelW;

        public Particle(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            force = Vector3.zero;
            density = 0.0f;
            pressure = 0.0f;
            gridLocation = Vector3Int.zero;
            gridIndex = 0;
            voxel = Vector3Int.zero;
            voxelW = 0;
        }
    }
    int SIZE_SPHPARTICLE = 11 * sizeof(float) + 8 * sizeof(int);

    private struct Obstacle
    {
        public Vector3 position;
        public Vector3 right;
        public Vector3 up;
        public Vector2 scale;

        public Obstacle(Transform _transform)
        {
            position = _transform.position;
            right = _transform.right;
            up = _transform.up;
            scale = new Vector2(_transform.lossyScale.x / 2f, _transform.lossyScale.y / 2f);
        }
    }
    int SIZE_SPHCOLLIDER = 11 * sizeof(float);

    public float particleDrag = 0.025f;
    private const float BOUND_DAMPING = -0.5f;

    Obstacle[] collidersArray;
    ComputeBuffer collidersBuffer;
    int kernelComputeColliders;


    void Start()
    {
        InitSPH();
        InitGrid();
        InitShader();
        InitArags();
        InitRenderTexture();
    }

    private void InitArags()
    {
        argsArray[0] = mesh.GetIndexCount(0);
        argsArray[1] = (uint)particlesArray.Length;
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(argsArray);

        material.SetBuffer("particles", particlesBuffer);
        material.SetFloat("_Radius", radius);
    }

    public RenderTexture Volume;
    private Vector3Int RenderTextureGroups;
    private Bounds bounds;
    private void InitRenderTexture()
    {
        Vector3 min = new Vector3(-((gridBounds.transform.localScale.x / 2) - gridBounds.transform.localPosition.x)
            , -((gridBounds.transform.localScale.y / 2) - gridBounds.transform.localPosition.y)
            , -((gridBounds.transform.localScale.z / 2) - gridBounds.transform.localPosition.z));

        Vector3 max = new Vector3( ((gridBounds.transform.localScale.x / 2) + gridBounds.transform.localPosition.x)
            , ((gridBounds.transform.localScale.y / 2) + gridBounds.transform.localPosition.y)
            , ((gridBounds.transform.localScale.z / 2) + gridBounds.transform.localPosition.z));

        bounds = new Bounds();
        bounds.SetMinMax(min , max);

        int THREADS = 8;

        ParticleVolume = (4.0f / 3.0f) * Mathf.PI * Mathf.Pow(radius, 3);

        int width =(int) gridDimensions.x;
        int height = (int)gridDimensions.y;
        int depth = (int)gridDimensions.z;

        int groupsX = width / THREADS;
        if (width % THREADS != 0) groupsX++;

        int groupsY = height / THREADS;
        if (height % THREADS != 0) groupsY++;

        int groupsZ = depth / THREADS;
        if (depth % THREADS != 0) groupsZ++;

        RenderTextureGroups = new Vector3Int(groupsX, groupsY, groupsZ);

        Volume = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        Volume.dimension = TextureDimension.Tex3D;
        Volume.volumeDepth = depth;
        Volume.useMipMap = false;
        Volume.enableRandomWrite = true;
        Volume.wrapMode = TextureWrapMode.Clamp;
        Volume.filterMode = FilterMode.Bilinear;
        Volume.Create();

        CreateMesh(m_volumeMat);

        kernalComputeVolume = shader.FindKernel("ComputeVolume");

        shader.SetTexture(kernalComputeVolume, "Volume", Volume);
        shader.SetFloat("ParticleVolume", ParticleVolume);

        shader.SetBuffer(kernalComputeVolume, "particles", particlesBuffer);
        shader.SetBuffer(kernalComputeVolume, "grid", gridBuffer);

    }

    void InitGrid()
    {
        gridDimensions.Set((int)gridBounds.transform.localScale.x,(int)gridBounds.transform.localScale.y,(int)gridBounds.transform.localScale.z,0);

        float cellSize = h;
        gridDimensions /= cellSize;
        gridDimensions.w = gridDimensions.x * gridDimensions.y * gridDimensions.z;

        Vector3 halfSize = new Vector3(gridDimensions.x, gridDimensions.y, gridDimensions.z) * cellSize * 0.5f;
        Vector3 pos = gridBounds.transform.position - halfSize;
        gridStartPosition.Set(pos.x, pos.y, pos.z, cellSize);

        kernelClearGrid = shader.FindKernel("ClearGrid");

        uint numThreadsX;
        shader.GetKernelThreadGroupSizes(kernelClearGrid, out numThreadsX, out _, out _);
        gridGroupSize = Mathf.CeilToInt(gridDimensions.w / (float)numThreadsX);

        gridCount = (int)numThreadsX * gridGroupSize;

        gridBuffer = new ComputeBuffer(gridCount, SIZE_GRID_CELL);
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
            pos += this.transform.position;
            pos *= radius;


            particlesArray[i] = new Particle(pos);
        }
    }

    private void InitShader()
    {
        kernelPopulateGrid = shader.FindKernel("PopulateGrid");
        kernelComputeForces = shader.FindKernel("ComputeForces");
        kernelIntegrate = shader.FindKernel("Integrate");
        kernelComputeColliders = shader.FindKernel("ComputeColliders");

        float hSq = h * h;

        particlesBuffer = new ComputeBuffer(particlesArray.Length, SIZE_SPHPARTICLE);
        particlesBuffer.SetData(particlesArray);

        UpdateColliders();

      

        shader.SetInt("particleCount", particlesArray.Length);
        shader.SetInt("colliderCount", collidersArray.Length);
        shader.SetInt("gridCount", gridCount);
        shader.SetFloat("smoothingRadius", h);
        shader.SetFloat("smoothingRadiusSq", hSq);
        shader.SetFloat("kStiffness", kStiffness);
        shader.SetFloat("restDensity", restDensity);
        shader.SetFloat("mass", mass);
        shader.SetFloat("particleViscosity", viscosity);
        shader.SetFloat("deltaTime", deltaTime);
        shader.SetVector("gravity", gravity);
        shader.SetFloat("DAMPING_COEFFICIENT", DAMPING_COEFFICIENT);
        shader.SetFloat("damping", BOUND_DAMPING);
        shader.SetFloat("particleDrag", particleDrag);
        shader.SetFloat("radius", radius);

        int[] gridDims = new int[] { (int)gridDimensions.x, (int)gridDimensions.y, (int)gridDimensions.z, (int)gridDimensions.w };
        shader.SetInts("gridDimensions", gridDims);
        shader.SetVector("gridStartPosition", gridStartPosition);

        shader.SetBuffer(kernelClearGrid, "grid", gridBuffer);
        shader.SetBuffer(kernelPopulateGrid, "grid", gridBuffer);
        shader.SetBuffer(kernelPopulateGrid, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeDensityPressure, "grid", gridBuffer);
        shader.SetBuffer(kernelComputeDensityPressure, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeForces, "grid", gridBuffer);

        shader.SetBuffer(kernelComputeForces, "particles", particlesBuffer);
        shader.SetBuffer(kernelIntegrate, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeColliders, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeColliders, "colliders", collidersBuffer);
 
    }



    private void Update()
    {
        UpdateColliders();
        shader.SetInt("useGrid", useGrid ? 1 : 0);

        if (useGrid)
        {
            shader.Dispatch(kernelClearGrid, gridGroupSize, 1, 1);
            shader.Dispatch(kernelPopulateGrid, groupSize, 1, 1);
        }

        shader.Dispatch(kernelComputeDensityPressure, groupSize, 1, 1);
        shader.Dispatch(kernelComputeForces, groupSize, 1, 1);
        shader.Dispatch(kernelIntegrate, groupSize, 1, 1);
        shader.Dispatch(kernelComputeColliders, groupSize, 1, 1);
        shader.Dispatch(kernalComputeVolume, RenderTextureGroups.x, RenderTextureGroups.y, RenderTextureGroups.z);

        if (showParticle)
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, argBounds, argsBuffer);
        }

    }


    private void OnDestroy()
    {
        gridBuffer.Dispose();
        particlesBuffer.Dispose();
        collidersBuffer.Dispose();
        argsBuffer.Dispose();
    }

    void UpdateColliders()
    {
        // Get colliders
        GameObject[] collidersGO = GameObject.FindGameObjectsWithTag("Obstacle");
        if (collidersArray == null || collidersArray.Length != collidersGO.Length)
        {
            collidersArray = new Obstacle[collidersGO.Length];
            if (collidersBuffer != null)
            {
                collidersBuffer.Dispose();
            }
            collidersBuffer = new ComputeBuffer(collidersArray.Length, SIZE_SPHCOLLIDER);
        }
        for (int i = 0; i < collidersArray.Length; i++)
        {
            collidersArray[i] = new Obstacle(collidersGO[i].transform);
        }
        collidersBuffer.SetData(collidersArray);
        shader.SetBuffer(kernelComputeColliders, "colliders", collidersBuffer);
    }


    private GameObject m_mesh;
    private void CreateMesh(Material material)
    {

        m_mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_mesh.GetComponent<MeshRenderer>().sharedMaterial = material;

        Vector3 min = bounds.min;
        Vector3 max = min + bounds.size * radius;

        Bounds worldBounds = new Bounds();
        worldBounds.SetMinMax(min, max);

        m_mesh.transform.position = worldBounds.center;
        m_mesh.transform.localScale = worldBounds.size;

        material.SetVector("Translate", m_mesh.transform.position);
        material.SetVector("Scale", m_mesh.transform.localScale);
        material.SetTexture("Volume", Volume);
        material.SetVector("Size", bounds.size);

    }

}





