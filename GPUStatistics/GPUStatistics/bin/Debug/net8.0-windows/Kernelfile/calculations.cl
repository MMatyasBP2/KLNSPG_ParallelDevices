__kernel void Sum(__global const float* array, __global float* result, const int length) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_size = get_local_size(0);

    __local float localSum[256];

    // Kezdeti összeg 0-ra inicializálása
    float sum = 0.0;

    // Minden szál összeadja a saját résztömbjének elemeit
    for(int i = global_id; i < length; i += get_global_size(0)) {
        sum += array[i];
    }

    // Az eredményt ideiglenesen a lokális memóriába írjuk
    localSum[local_id] = sum;

    // Szinkronizálás a work-groupon belül
    barrier(CLK_LOCAL_MEM_FENCE);

    // Lokális memóriában lévő adatok összegezése a work-groupon belül (redukció)
    for(int stride = group_size / 2; stride > 0; stride >>= 1) {
        if(local_id < stride) {
            localSum[local_id] += localSum[local_id + stride];
        }
        // Újabb szinkronizálás, mielőtt a következő redukciós lépést végeznénk
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    // Az összegzés végeredményének elmentése a globális memóriába
    // Csak a work-groupon belüli első szál végzi
    if(local_id == 0) {
        result[get_group_id(0)] = localSum[0];
    }
}

__kernel void CalculateMin(__global const float* array, __global float* result, const int length) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_size = get_local_size(0);

    __local float localMin[256];

    float min_val = INFINITY;

    for(int i = global_id; i < length; i += get_global_size(0)) {
        min_val = fmin(min_val, array[i]);
    }

    localMin[local_id] = min_val;

    barrier(CLK_LOCAL_MEM_FENCE);

    for(int stride = group_size / 2; stride > 0; stride >>= 1) {
        if(local_id < stride) {
            localMin[local_id] = fmin(localMin[local_id], localMin[local_id + stride]);
        }
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    if(local_id == 0) {
        result[get_group_id(0)] = localMin[0];
    }
}

__kernel void CalculateMax(__global const float* array, __global float* result, const int length) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_size = get_local_size(0);

    __local float localMax[256];

    float max_val = -INFINITY;

    for(int i = global_id; i < length; i += get_global_size(0)) {
        max_val = fmax(max_val, array[i]);
    }

    localMax[local_id] = max_val;

    barrier(CLK_LOCAL_MEM_FENCE);

    for(int stride = group_size / 2; stride > 0; stride >>= 1) {
        if(local_id < stride) {
            localMax[local_id] = fmax(localMax[local_id], localMax[local_id + stride]);
        }
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    if(local_id == 0) {
        result[get_group_id(0)] = localMax[0];
    }
}

__kernel void CalculateSumAndAverage(__global const float* array, __global float* result, const int length) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_size = get_local_size(0);

    __local float localSum[256];

    float sum = 0.0;

    for(int i = global_id; i < length; i += get_global_size(0)) {
        sum += array[i];
    }

    localSum[local_id] = sum;

    barrier(CLK_LOCAL_MEM_FENCE);

    for(int stride = group_size / 2; stride > 0; stride >>= 1) {
        if(local_id < stride) {
            localSum[local_id] += localSum[local_id + stride];
        }
        barrier(CLK_LOCAL_MEM_FENCE);
    }

    if(local_id == 0) {
        result[get_group_id(0)] = localSum[0];
    }
}

__kernel void CalcMedian(__global float* numbers, __global float* result, int length)
{
    for (int i = 0; i < get_global_id(0); i++)
    {
        for (int j = 0; j < get_global_id(0)- i - 1; j++)
        {
            if (numbers[j] > numbers[j + 1])
            {
                float temp = numbers[j];
                numbers[j] = numbers[j + 1];
                numbers[j + 1] = temp;
            }
        }
    }

    float median;
    if (length % 2 == 0)
    {
        // If the number of elements is even, take the average of the two middle elements
        int middleIndex = length / 2;
        median = (numbers[middleIndex - 1] + numbers[middleIndex]) / 2.0f;
    }
    else
        median = numbers[length / 2];

    // Store the median in the result array
    result[0] = median;
}