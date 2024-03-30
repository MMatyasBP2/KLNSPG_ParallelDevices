namespace GPUStatistics
{
    public interface ICalculations
    {
        (float, double) CalculateSum(float[] array);
        (float, double) CalculateAverage(float[] array);
        (float, double) CalculateMin(float[] array);
        (float, double) CalculateMax(float[] array);
        (float, double) CalculateMedian(float[] array);
    }
}
