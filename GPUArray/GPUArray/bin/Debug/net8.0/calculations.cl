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

__kernel void StandardDeviation(__global float* array, const int length, __global float* result) {
    int global_id = get_global_id(0);

    // Számítsuk ki az átlagot
    float sum = 0;
    for (int i = 0; i < length; i++) {
        sum += array[i];
    }
    float mean = sum / length;

    // Számítsuk ki a szórás négyzetet
    float variance = 0;
    for (int i = 0; i < length; i++) {
        variance += pow(array[i] - mean, 2);
    }
    variance /= length;

    // A szórás a négyzetgyök a szórásnégyzetből
    float standardDeviation = sqrt(variance);

    // Az eredményt tároljuk a megadott tömbben
    result[global_id] = standardDeviation;
}

__kernel void Variance(__global float* array, const int length, __global float* result) {
    int global_id = get_global_id(0);

    // Számítsuk ki az átlagot
    float sum = 0;
    for (int i = 0; i < length; i++) {
        sum += array[i];
    }
    float mean = sum / length;

    // Számítsuk ki a szórás négyzetet
    float variance = 0;
    for (int i = 0; i < length; i++) {
        variance += pow(array[i] - mean, 2);
    }
    variance /= length;

    // Az eredményt tároljuk a megadott tömbben
    result[global_id] = variance;
}