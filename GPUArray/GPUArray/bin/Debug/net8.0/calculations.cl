__kernel void Sum(__global const float* array, __global float* result, const int length) {
    int global_id = get_global_id(0);
    int local_id = get_local_id(0);
    int group_size = get_local_size(0);

    __local float localSum[256]; // Feltételezve, hogy a work group mérete max 256

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

__kernel void bitonicSortAndCalculateMedian(__global float* data, int length) {
    int idx = get_global_id(0);

    // Egyszerű bitonikus rendezés egy adott elemszámra.
    // Megjegyzés: Ez a kód csak illusztrációs célú, és nem alkalmazható közvetlenül.
    for (int size = 2; size <= length; size *= 2) {
        for (int stride = size / 2; stride > 0; stride /= 2) {
            int index = (idx / stride) & 1;
            if (index == 0) {
                if ((idx & size) == 0 && data[idx] > data[idx + stride]) {
                    // Csere
                    float temp = data[idx];
                    data[idx] = data[idx + stride];
                    data[idx + stride] = temp;
                }
            } else {
                if ((idx & size) != 0 && data[idx] < data[idx + stride]) {
                    // Csere
                    float temp = data[idx];
                    data[idx] = data[idx + stride];
                    data[idx + stride] = temp;
                }
            }
        }
    }

    // Medián számítása
    barrier(CLK_GLOBAL_MEM_FENCE);
    if (idx == 0) {
        float median;
        if (length % 2 == 0) {
            median = (data[length / 2 - 1] + data[length / 2]) / 2.0;
        } else {
            median = data[length / 2];
        }
        // Tegyük fel, hogy van egy módszerünk az eredmény visszaküldésére.
    }
}