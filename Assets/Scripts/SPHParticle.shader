Shader "Custom/SPHParticle"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types  
        #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma instancing_options procedural:setup

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        float _Radius;
        float3 _Position;
        fixed4 _Color;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        struct Particle
        {
            float3 position;
            float3 velocity;
            float3 force;
            float density;
            float pressure;
            int4 gridLocation;
            int4 voxel;
        };

        StructuredBuffer<Particle> particles;
        #endif
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
            void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            Particle particle = particles[unity_InstanceID];
            _Position = particle.position;
            #endif
        }
        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex.xyz *= _Radius;
                v.vertex.xyz += _Position;
            #endif
        }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            o.Albedo = _Color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
