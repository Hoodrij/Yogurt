namespace Yogurt
{
    public static class Consts
    {
        public const int INITIAL_ENTITIES_COUNT = 100;

        // Component mask configuration
        // 4 ulongs = 256 components, 8 ulongs = 512 components, 16 ulongs = 1024 components
        public const int MASK_ULONGS = 4;
        public const int MAX_COMPONENTS = MASK_ULONGS * 64;
    }
}