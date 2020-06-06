/*
Note from ModelShark Studio:
Permission is granted to include this script in your Unity assets for commercial or non-commercial use.
Permission is also granted to modify this script so long as it does not include other code from our DestroyIt product.
The purpose of this permission is to allow you to sell DestroyIt-Ready assets without needing to include DestroyIt code dependencies.
*/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace DestroyItReady
{
    public class HitEffectsStub : MonoBehaviour
    {
        public List<HitEffect> effects;
    }

    [Serializable]
    public class HitEffect
    {
        public HitBy hitBy;
        public GameObject effect;
    }

    [Flags]
    public enum HitBy
    {
        Bullet = (1 << 0),
        Cannonball = (1 << 1),
        Axe = (1 << 2)
    }
}