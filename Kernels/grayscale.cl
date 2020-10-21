__constant sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_FILTER_NEAREST | CLK_ADDRESS_CLAMP_TO_EDGE;

__kernel void grayscale(const float rFactor, const float gFactor, const float bFactor, __read_only image2d_t in, __write_only image2d_t out)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	
	int2 pos = (int2)(x, y);
	
	const uint4 pixel = read_imageui(in, sampler, pos);
	
	float avg = pixel.r * rFactor + pixel.g * gFactor + pixel.b * bFactor;
	
	write_imageui(out, pos, (uint4)(avg,avg,avg,pixel.a));
}