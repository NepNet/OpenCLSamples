__kernel void add(__global float *a, __global float *b, __global float *out)
{
	int i = get_global_id(0);
	
	out[i] = a[i] + b[i];
}