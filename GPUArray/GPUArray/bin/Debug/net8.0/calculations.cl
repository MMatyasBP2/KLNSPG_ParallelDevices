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

float Partition(__global float* arr, const int left, const int right) {
    float pivotValue = arr[right];
    int i = left - 1;
    
    for (int j = left; j < right; ++j) {
        if (arr[j] <= pivotValue) {
            i++;
            float temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
    
    float temp = arr[i + 1];
    arr[i + 1] = arr[right];
    arr[right] = temp;
    return i + 1;
}

void QuickSelect(__global float* arr, int left, int right, const int k) {
    while (left < right) {
        float pivotValue = arr[right];
        int i = left - 1;

        for (int j = left; j < right; ++j) {
            if (arr[j] <= pivotValue) {
                i++;
                float temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }

        float temp = arr[i + 1];
        arr[i + 1] = arr[right];
        arr[right] = temp;

        int pivotIndex = i + 1;

        if (k == pivotIndex) {
            break;
        } else if (k < pivotIndex) {
            right = pivotIndex - 1;
        } else {
            left = pivotIndex + 1;
        }
    }
}

__kernel void CalcMedian(__global float* data, __global float* result, const int arraySize) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_id = get_group_id(0);
    int group_size = get_local_size(0);
    
    // Az aktuális rész kezdőindexe
    int start = group_id * group_size;
    // Az aktuális rész végindexe
    int end = min(start + group_size, arraySize) - 1;
    
    // Lokális tömb az aktuális rész adatok tárolására
    __local float localData[256];
    
    // Adatok másolása a lokális tömbbe
    for (int i = start + local_id; i <= end; i += group_size) {
        localData[i - start] = data[i];
    }
    
    // Medián számítás az aktuális rész adatokon
    QuickSelect(data, start, end, (end - start) / 2);
    
    // Mediánok másolása a globális memóriába
    if (local_id == 0) {
        result[group_id] = data[start + (end - start) / 2];
    }
    
    // Szinkronizáció a work-groupon belül
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Az első work-group kiszámítja a részmediánok globális mediánját
    if (group_id == 0) {
        QuickSelect(result, 0, get_num_groups(0) - 1, get_num_groups(0) / 2);
    }
}