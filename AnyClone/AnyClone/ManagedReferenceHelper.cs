using System;
using System.Reflection.Emit;

namespace AnyClone
{
    public static class ManagedReferenceHelper
    {
        public static readonly Action<object, Action<IntPtr>> GetPinnedPtr;

        static ManagedReferenceHelper()
        {
            var dyn = new DynamicMethod("GetPinnedPtr", typeof(void), new[] { typeof(object), typeof(Action<IntPtr>) }, typeof(ManagedReferenceHelper).Module);
            var il = dyn.GetILGenerator();
            il.DeclareLocal(typeof(object), true);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Call, typeof(Action<IntPtr>).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);
            GetPinnedPtr = (Action<object, Action<IntPtr>>)dyn.CreateDelegate(typeof(Action<object, Action<IntPtr>>));
        }
    }
}
