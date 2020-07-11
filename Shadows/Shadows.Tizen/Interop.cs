using System;
using System.Runtime.InteropServices;

namespace Shadows.Tizen
{
    internal static class Interop
    {
        const string Evas = "libevas.so.1";

        [DllImport(Evas)]
        internal static extern void evas_object_clip_unset(IntPtr obj);
    }
}
