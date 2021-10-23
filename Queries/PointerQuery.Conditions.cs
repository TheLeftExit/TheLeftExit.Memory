using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLeftExit.Memory.RTTI;
using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.Queries {
    public partial class PointerQuery {
        public static void AOB(object sender, ProcessStepEventArgs e) {
            PointerQuery query = sender as PointerQuery;
            if (query.Tag is not byte?[] pattern)
                throw new ApplicationException($"AOB query must have an array of {nameof(Nullable<byte>)} as its {nameof(Tag)}.");
            Span<byte> buffer = stackalloc byte[pattern.Length];
            if(!e.Source.TryRead(e.Address, buffer)) {
                e.Result = ProcessStepResult.Break;
                return;
            }
            for(int i = 0; i < buffer.Length; i++)
                if(pattern[i].HasValue && pattern[i] != buffer[i]) {
                    e.Result = ProcessStepResult.Continue;
                    return;
                }
            e.Result = ProcessStepResult.Return;
        }

        public static void RTTIByRef(object sender, ProcessStepEventArgs e) {
            PointerQuery query = sender as PointerQuery;
            if (query.Tag is not string className)
                throw new ApplicationException($"RTTI query must have an class name as its {nameof(Tag)}.");
            if (!e.Source.TryRead(e.Address, out ulong target)) {
                e.Result = ProcessStepResult.Break;
                return;
            }
            if (e.Source.GetRTTIClassName64(target, 1) == className) {
                e.Result = ProcessStepResult.Return;
                return;
            }
            e.Result = ProcessStepResult.Continue;
        }

        public static void RTTIByVal(object sender, ProcessStepEventArgs e) {
            PointerQuery query = sender as PointerQuery;
            if (query.Tag is not string className)
                throw new ApplicationException($"RTTI query must have an class name as its {nameof(Tag)}.");
            if (!e.Source.TryRead(e.Address, out ulong target)) {
                e.Result = ProcessStepResult.Break;
                return;
            }
            if (e.Source.GetRTTIClassName64(target, 0) == className) {
                e.Result = ProcessStepResult.Return;
                return;
            }
            e.Result = ProcessStepResult.Continue;
        }
    }
}
