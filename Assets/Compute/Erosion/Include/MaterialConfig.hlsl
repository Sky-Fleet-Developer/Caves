#ifndef EROSION_MATERIAL
#define EROSION_MATERIAL

struct MaterialConfig
{
    float looseness;
    float looseness_inverted;
};

StructuredBuffer<MaterialConfig> material_properties_config; 

float GetLooseness(float value, uint materialIndex)
{
    return material_properties_config[materialIndex].looseness * (1 - min(sign(-value), 0));
}

float GetLoosenessInverted(float value, uint materialIndex)
{
    return material_properties_config[materialIndex].looseness_inverted * (1 - min(sign(-value), 0));
}

#endif