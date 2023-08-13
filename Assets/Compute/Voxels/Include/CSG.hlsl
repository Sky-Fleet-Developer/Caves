#ifndef TUNTENFISCH_VOXELS_CSG
#define TUNTENFISCH_VOXELS_CSG

// Parts below are based on articles by Inigo Quilez (https://www.iquilezles.org/).
#include "Assets/Compute/Include/Enumeration.hlsl"
#include "Assets/Compute/Voxels/Include/Voxel.hlsl"

ENUM CSGOperatorIndex
{
    static const uint Union = 0;
    static const uint Intersection = 1;
    static const uint Difference = 2;
    static const uint SmoothUnion = 3;
    static const uint SmoothIntersection = 4;
    static const uint SmoothDifference = 5;
    static const uint DominatedUnion = 6;
    static const uint Include = 7;
};

ENUM CSGPrimitiveType
{
    static const uint Sphere = 0;
    static const uint Cuboid = 1;
};

Voxel Union(Voxel lhs, Voxel rhs)
{
    if(lhs.GetValue() < rhs.GetValue())
    {
        return lhs;
    }
    return rhs;
}

Voxel DominatedUnion(Voxel lhs, Voxel rhs)
{
    if(lhs.GetValue() < 0.0f)
    {
        return lhs;
    }
    return rhs;
}

Voxel Intersection(Voxel lhs, Voxel rhs)
{
    lhs.valueAndGradient *= -1.0f;
    rhs.valueAndGradient *= -1.0f;
    const uint lhsMat = lhs.materialIndex;
    lhs.materialIndex = rhs.materialIndex;
    rhs.materialIndex = lhsMat;
    Voxel voxel = Union(lhs, rhs);
    voxel.valueAndGradient *= -1.0f;

    return voxel;
}

Voxel Include(Voxel lhs, Voxel rhs) // TODO: remove if
{
    if(lhs.GetValue() > 0.0f)
    {
        return lhs;
    }else
    {
        if(rhs.GetValue() < 0.0f)
        {
            return rhs;
        }
    }
    return lhs;
}
/*
Voxel voxel = Intersection(lhs, rhs);
return DominatedUnion(voxel, lhs);
*/
/*
Voxel voxel = DominatedUnion(rhs, lhs);
if(lhs.GetValue() >= 0.0f && rhs.GetValue() <= 0.0f) voxel.valueAndGradient *= -1.0f;
return voxel;
*/

Voxel Difference(Voxel lhs, Voxel rhs)
{
    rhs.materialIndex = lhs.materialIndex;
    rhs.valueAndGradient *= -1.0f;
    return Intersection(lhs, rhs);
}

Voxel SmoothUnion(Voxel lhs, Voxel rhs, float smoothing)
{
    float h = max(smoothing - abs(lhs.GetValue() - rhs.GetValue()), 0.0f);
    float m = 0.25f * h * h / smoothing;
    float n = 0.50f * h / smoothing;

    Voxel voxel = Voxel::Create();
    voxel.valueAndGradient = float4(min(lhs.GetValue(), rhs.GetValue()) - m, lerp(lhs.GetGradient(), rhs.GetGradient(), lhs.GetValue() < rhs.GetValue() ? n : 1.0f - n));
    voxel.materialIndex = lhs.GetValue() < rhs.GetValue() ? lhs.materialIndex : rhs.materialIndex;

    return voxel;
}

Voxel SmoothIntersection(Voxel lhs, Voxel rhs, float smoothing)
{
    lhs.valueAndGradient *= -1.0f;
    rhs.valueAndGradient *= -1.0f;
    const uint lhsMat = lhs.materialIndex;
    lhs.materialIndex = rhs.materialIndex;
    rhs.materialIndex = lhsMat;
    Voxel voxel = SmoothUnion(lhs, rhs, smoothing);
    voxel.valueAndGradient *= -1.0f;

    return voxel;
}

Voxel SmoothDifference(Voxel lhs, Voxel rhs, float smoothing)
{
    rhs.materialIndex = lhs.materialIndex;
    rhs.valueAndGradient *= -1.0f;
    
    return SmoothIntersection(lhs, rhs, smoothing);
}

struct CSGOperator
{
    uint operatorIndex;
    float smoothing;
};

Voxel ApplyCSGOperator(Voxel lhs, Voxel rhs, CSGOperator csgOperator)
{
    [branch]
    switch(csgOperator.operatorIndex)
    {
        case CSGOperatorIndex::Union:
            return Union(lhs, rhs);

        case CSGOperatorIndex::Intersection:
            return Intersection(lhs, rhs);

        case CSGOperatorIndex::Difference:
            return Difference(lhs, rhs);

        case CSGOperatorIndex::SmoothUnion:
            return SmoothUnion(lhs, rhs, csgOperator.smoothing);

        case CSGOperatorIndex::SmoothIntersection:
            return SmoothIntersection(lhs, rhs, csgOperator.smoothing);
        
        case CSGOperatorIndex::DominatedUnion:
            return DominatedUnion(lhs, rhs);

        case CSGOperatorIndex::Include:
            return Include(lhs, rhs);
        
        default:
            return SmoothDifference(lhs, rhs, csgOperator.smoothing);
    }
}

// When combining a primitive (effectively an SDF) with other SDFs, the primitive
// will perturb the shape of nearby SDFs and hence alter the generated mesh's 
// geometry. But the mesh's geometry is even affected when the the isosurface the 
// primitive is describing isn't intersecting any of the other isosurfaces at all.
//
// This is due to small values, i.e. values close to 0, near the primitive's isosurface
// "winning" when the primitive is combined with other SDFs. Multiplying the
// value of the primitive by a "large" factor effectively narrows the interval
// of small values the primitive's SDFs has and therefore limits the region in which
// it affects other SDFs when combined. The isosurface, which is generated at the 0
// transition of the SDF isn't affect.
//
// At least that's my theory behind it...
static const float csgPrimitiveValueMultiplier = 7.5f;

struct CSGPrimitive
{
    uint primitiveType;
};

float4 EvaluateCSGSphere(float3 position)
{
    float4 valueAndGradient;
    valueAndGradient.x = length(position) - 0.5f;
    valueAndGradient.x *= csgPrimitiveValueMultiplier;
    valueAndGradient.yzw = position;

    return valueAndGradient;
}

float4 EvaluateCSGCuboid(float3 position)
{
    float4 valueAndGradient;
    float3 d = abs(position) - 0.5f;
    float3 smoothing = sign(position);
    float g = max(d.x, max(d.y, d.z));
    
    valueAndGradient.x = length(max(d, 0.0f)) + min(max(d.x, max(d.y, d.z)), 0.0f);
    valueAndGradient.x *= csgPrimitiveValueMultiplier;
    valueAndGradient.yzw = smoothing * (g > 0.0f ? normalize(max(d, 0.0f)) : step(d.yzx, d.xyz) * step(d.zxy, d.xyz));

    return valueAndGradient;
}

float4 EvaluateCSGPrimitive(float3 position, CSGPrimitive primitive)
{
    [branch]
    switch(primitive.primitiveType)
    {
        case CSGPrimitiveType::Sphere:
            return EvaluateCSGSphere(position);

        default:
            return EvaluateCSGCuboid(position);
    }
}

#endif