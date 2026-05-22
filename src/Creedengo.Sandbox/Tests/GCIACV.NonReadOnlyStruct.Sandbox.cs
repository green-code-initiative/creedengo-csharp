namespace Creedengo.Sandbox.Tests;

// Mirrors GCIACV.NonReadOnlyStruct.Tests.cs.
// Parameter types marked "warns" should light up GCIACV in the IDE; "ok" parameters should stay clean.
internal static class GCIACVSandbox
{
    public class GCIACV_Class
    {
        public void M(in GCIACV_Class self) { } // ok — classes aren't structs
        public void M(in string s) { } // ok — string is a reference type
    }

    public readonly struct GCIACV_ReadOnlyStruct
    {
        public void M_In(in GCIACV_ReadOnlyStruct self) { } // ok — readonly struct
        public void M_RefReadOnly(ref readonly GCIACV_ReadOnlyStruct self) { } // ok
    }

    public struct GCIACV_NonReadOnlyStruct
    {
        private int _value;

        public void M_NoIn(GCIACV_NonReadOnlyStruct self) { } // ok — no 'in' modifier
        public void M_In(in GCIACV_NonReadOnlyStruct self) { } // warns
        public void M_RefReadOnly(ref readonly GCIACV_NonReadOnlyStruct self) { } // warns
        public void M_Mixed(in GCIACV_NonReadOnlyStruct a, int value, in GCIACV_NonReadOnlyStruct b) { } // warns twice
        public void M_MixedRefAndIn(ref readonly GCIACV_NonReadOnlyStruct a, int value, in GCIACV_NonReadOnlyStruct b) { } // warns twice

        public void Positive_ComplexBody(in GCIACV_NonReadOnlyStruct self) // warns
        {
            var v = self._value;
            System.Console.WriteLine(v);
            System.Console.WriteLine("Processing completed");
        }
    }

    public struct GCIACV_OuterStruct
    {
        public struct GCIACV_InnerStruct { }

        public void M(in GCIACV_InnerStruct inner) { } // warns — inner struct isn't readonly
    }
}

internal static class GCIACVExtensions
{
    public static void Extend(this in GCIACVSandbox.GCIACV_NonReadOnlyStruct self) { } // warns — extension method with 'this in' on non-readonly struct
}
