using System;

namespace AnyClone.Tests.TestObjects
{
    public class StaticInitializerRecursive
    {
        public static readonly StaticInitializerRecursive Value1 = new StaticInitializerRecursive(1111);
        public string Name { get; set; } = "Default";
        public int Id { get; set; }

        public override string ToString() => $"{Id}";

        public StaticInitializerRecursive()
        {
            Id = new Random().Next();
        }

        public StaticInitializerRecursive(int id)
        {
            Id = id;
        }
    }
}
