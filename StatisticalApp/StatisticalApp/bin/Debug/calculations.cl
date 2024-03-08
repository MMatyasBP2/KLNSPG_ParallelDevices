__kernel void calc_min_reduced(__global const float* input, __global float* output, const int count, __local float* localMin) {
    int id = get_global_id(0);
    int local_id = get_local_id(0);
    float min_val = INFINITY;

    // Calculate local minimums
    for (int i = id; i < count; i += get_global_size(0)) {
        min_val = fmin(min_val, input[i]);
    }
    localMin[local_id] = min_val;

    barrier(CLK_LOCAL_MEM_FENCE);

    // Perform reduction in local memory
    for(int stride = get_local_size(0) / 2; stride > 0; stride >>= 1) {
        if (local_id < stride) {
            localMin[local_id] = fmin(localMin[local_id], localMin[local_id + stride]);
        }
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    // Write result from first thread of each block
    if(local_id == 0) {
        output[get_group_id(0)] = localMin[0];
    }
}