namespace Generation {
    public static class Data
    {
        // 20,000m -> 0m = 1.0 -> 0.0
        public static float[] Heights = new float [] {
            11000f,
            6000f,
        };

        public static float GetTrueHeight(float val) {
            return val * 20000f;
        }
        public static float GetScaledHeight(float val) {
            return val / 20000f;
        }

        public static float GetTrueTemperature(float val) {
            return val * (40f + 30f) - 30f;
        }
        public static float GetScaledTemperature(float val) {
            return (val + 30f) / (40f + 30f);
        }

        // Determined by temperature /_\ height
        //                         humidity
        // https://en.wikipedia.org/wiki/Biome#/media/File:Lifezones_Pengo.svg
        // divide into 5 segments, .2 per, on 1 to 0
        // Temperature 1 -> 0 : very hot, hot, mild, cold, very cold
        // Humidity 1 -> 0 : very humid, humid, mild, arid, very arid
        // Height 1-> 0 : snow, alpine, motane, foothill, mediterranean
        //   COLD,       SNOW
        //  /                \
        // HOT,ARID _ HUMID, MEDITERRANEAN 
        // going from the bottom row, left to right, ascending
        public enum BiomeType {
            // LAND
            desert, drylands, grasslands, tundra, frozenDesert,
            dryForest, forest, rainForest, coldForest,
            desertHills, smallHills,  
            mesa, hills, tundraHills, frozenHills,
            largeHills, 
            mountains,
            warmOcean, coldOcean, glacial,
        }
    }
}
