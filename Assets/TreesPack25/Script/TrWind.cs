using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//IL.ranch, 2025. ILonion32@gmail.com
namespace ILranch
{
    public class TrWind : MonoBehaviour
    {
        [System.Serializable]
        public class WindValues
        {
            public Material SharedMaterial;
            public float Power;
            [HideInInspector]
            public float Scatter;
        }
        [Header("Simple wind")]
        public WindValues[] _WindValues;

        void Awake()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks * 1000);
        }

        void Start()
        {
            for (int k0 = 0; k0 < _WindValues.Length; k0++)
            {
                _WindValues[k0].Scatter = UnityEngine.Random.Range(0.1f, 4f);
            }
        }

        void FixedUpdate()
        {
            for (int k0 = 0; k0 < _WindValues.Length; k0++)
            {
                _WindValues[k0].SharedMaterial.SetFloat("_HeightAmplitude", _WindValues[k0].Power * Mathf.Sin(Time.time * _WindValues[k0].Scatter));
            }
        }
    }
}
