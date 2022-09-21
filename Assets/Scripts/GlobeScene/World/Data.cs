namespace Generation {
    public static class Data
    {
        // 20,000m -> 0m = 1.0 -> 0.0
        public static float[] Heights = new float [] {
            12500f,
            6000f,
        };

        public static float GetTrueHeight(float val) {
            return val * 20000f;
        }
        public static float GetScaledHeight(float val) {
            return val / 20000f;
        }

        // Determined by temperature /_\ height
        //                         humidity
        // https://en.wikipedia.org/wiki/Biome#/media/File:Lifezones_Pengo.svg
        public enum BiomeType {

        }
    }
}
