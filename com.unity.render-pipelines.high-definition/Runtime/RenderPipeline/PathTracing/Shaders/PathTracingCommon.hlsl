float Length2(float3 v)
{
	return dot(v, v);
}

float Sqr(float x)
{
	return x * x;
}

float Average(float3 v)
{
	return (v.x + v.y + v.z) / 3.0;
}

// float Luminance(float3 v)
// {
// 	return 0.2126 * v.x + 0.7152 * v.y + 0.0722 * v.z;
// }

float Min(float3 v)
{
	return min(v.x, min(v.y, v.z));
}

float Max(float3 v)
{
	return max(v.x, max(v.y, v.z));
}

float Min(float x, float y, float z)
{
	return min(x, min(y, z));
}

float Max(float x, float y, float z)
{
	return max(x, max(y, z));
}
