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

__kernel void SortArray(__global float* data, const int length) {
    // Bubble sort algorithm for simplicity, replace with more efficient sorting algorithm if needed
    for (int i = 0; i < length - 1; i++) {
        for (int j = 0; j < length - i - 1; j++) {
            if (data[j] > data[j + 1]) {
                float temp = data[j];
                data[j] = data[j + 1];
                data[j + 1] = temp;
            }
        }
    }
}

__kernel void CalcMedian(__global float* sortedData, __global float* median, const int length) {
    int middle = length / 2;

    if (length % 2 == 0) {
        median[0] = (sortedData[middle - 1] + sortedData[middle]) / 2.0f;
    } else {
        median[0] = sortedData[middle];
    }
}
