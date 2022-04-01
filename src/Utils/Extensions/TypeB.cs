namespace B.Utils.Extensions
{
    public static class TypeB
    {
        public static T Instantiate<T>(this Type type) => (T)Activator.CreateInstance(type)!;
    }
}
