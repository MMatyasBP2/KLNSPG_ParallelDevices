__kernel void Sum(__global float* array, const int length) {
    int global_id = get_global_id(0);
    float sum = 0;
    if (global_id < length) {
        sum += array[global_id];
    }
    array[global_id] = sum;
}

__kernel void Min(__global float* array, const int length) {
    int global_id = get_global_id(0);
    float min_val = INFINITY;
    if (global_id < length) {
        min_val = array[global_id];
    }
    array[global_id] = min_val;
}

__kernel void Max(__global float* array, const int length) {
    int global_id = get_global_id(0);
    float max_val = -INFINITY;
    if (global_id < length) {
        max_val = array[global_id];
    }
    array[global_id] = max_val;
}

__kernel void Average(__global float* array, const int length) {
    int global_id = get_global_id(0);
    float sum = 0;
    if (global_id < length) {
        sum = array[global_id];
    }
    array[global_id] = sum;
}

__kernel void Median(__global float* array, const int length) {
    int global_id = get_global_id(0);

    // Tömb elemek rendezése növekvő sorrendbe
    for (int i = 0; i < length - 1; i++) {
        for (int j = 0; j < length - i - 1; j++) {
            if (array[j] > array[j + 1]) {
                float temp = array[j];
                array[j] = array[j + 1];
                array[j + 1] = temp;
            }
        }
    }

    // Számoljuk ki a mediánt, ha a tömb hossza páros
    if (length % 2 == 0) {
        array[global_id] = (array[length / 2 - 1] + array[length / 2]) / 2.0;
    }
    // Számoljuk ki a mediánt, ha a tömb hossza páratlan
    else {
        array[global_id] = array[length / 2];
    }
}