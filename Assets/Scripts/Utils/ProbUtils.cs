namespace Utils {
    public static class ProbUtils {
        public static double Multiply(double prob1, double prob2) => prob1 * prob2;
        public static double Add(double prob1, double prob2) => (prob1 + prob2) - Multiply(prob1, prob2);
        public static float Multiply(float prob1, float prob2) => prob1 * prob2;
        public static float Add(float prob1, float prob2) => (prob1 + prob2) - Multiply(prob1, prob2);
    }
}