namespace Creedengo.Sandbox.Tests;

// Mirrors GCI85.MakeTypeSealed.Tests.cs.
// Types marked "warns" should light up GCI85 in the IDE; "ok" types should stay clean.
//
// All sample types live under GCI85_Container to avoid polluting the global namespace.
internal static class GCI85Sandbox
{
    public static class GCI85_Container
    {
        // --- Sealable classes (warns) ---
        public class GCI85_SealableA; // warns
        internal class GCI85_SealableB; // warns
        public record GCI85_SealableRecord; // warns

        public class GCI85_WithInterface : GCI85_IFace { public void Method() { } } // warns — interface impl alone doesn't prevent sealing

        public interface GCI85_IFace { void Method(); }

        // --- Non-sealable / already-correct (ok) ---
        public struct GCI85_Struct; // ok — structs are implicitly sealed
        public static class GCI85_Static; // ok
        public abstract class GCI85_Abstract; // ok — abstract can't be sealed
        public sealed class GCI85_AlreadySealed; // ok

        // --- Overridable members ---
        public class GCI85_PublicVirtual { public virtual void M() { } } // ok — public virtual is meaningful subclass extension
        public class GCI85_InternalVirtual { internal virtual void M() { } } // warns — internal virtual outside this assembly is unreachable
        public class GCI85_ProtectedVirtual { protected virtual void M() { } } // ok
        public class GCI85_ProtectedInternalVirtual { protected internal virtual void M() { } } // warns
        public class GCI85_PrivateProtectedVirtual { private protected virtual void M() { } } // warns

        // --- Inheritance chain ---
        public abstract class GCI85_Base { public virtual void Overridable() { } }
        public class GCI85_Derived1 : GCI85_Base; // ok — leaves Overridable open
        public sealed class GCI85_Derived2 : GCI85_Derived1; // ok — already sealed
        public class GCI85_Derived3 : GCI85_Derived1; // ok — could be sealed but analyzer skips multi-level chains
        public class GCI85_Derived4 : GCI85_Derived1 { public override void Overridable() { } } // ok — still overridable further
        public class GCI85_DerivedSealedOverride : GCI85_Derived1 { public sealed override void Overridable() { } } // warns — sealed override closes the type
        public class GCI85_DerivedDerivedSealedOverride : GCI85_DerivedSealedOverride; // warns

        // --- Partials ---
        public partial class GCI85_Partial1; // warns — neither part has virtual members
        partial class GCI85_Partial1 { public void Method() { } }

        partial class GCI85_Partial2 { public void Method() { } } // warns
        public partial class GCI85_Partial2;

        public partial class GCI85_Partial3; // ok — second part introduces a virtual method
        partial class GCI85_Partial3 { public virtual void Method() { } }

        public partial class GCI85_Partial4; // ok — already sealed via partial
        sealed partial class GCI85_Partial4 { public void Method() { } }
    }
}
