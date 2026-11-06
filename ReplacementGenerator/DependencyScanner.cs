using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ReplacementGenerator
{
    /// Finds which global-namespace types of a module are used by a set of "seed" types (fields, signatures, base types,
    /// attributes and every IL instruction), transitively. Used so that ignoring a library class also keeps the helper
    /// classes it depends on as originals, instead of leaving it calling code that has been replaced.
    internal static class DependencyScanner
    {
        /// Returns dependency name -> name of the type that pulled it in, for every candidate reachable from the seeds
        /// (seeds themselves excluded).
        public static Dictionary<string, string> FindDependencies(string assemblyPath, IEnumerable<string> seeds, ISet<string> candidates)
        {
            var module = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters { InMemory = true });
            var defs = new Dictionary<string, TypeDefinition>();
            foreach (var t in module.Types) if (string.IsNullOrEmpty(t.Namespace) && !defs.ContainsKey(t.Name)) defs[t.Name] = t;

            var known = new HashSet<string>(seeds);
            var pulledBy = new Dictionary<string, string>();
            var queue = new Queue<string>(known);
            while (queue.Count > 0)
            {
                var name = queue.Dequeue();
                if (!defs.TryGetValue(name, out var def)) continue;
                var refs = new HashSet<string>();
                Scan(def, module, refs);
                foreach (var r in refs)
                {
                    if (!candidates.Contains(r) || !known.Add(r)) continue;
                    pulledBy[r] = name;
                    queue.Enqueue(r);
                }
            }
            return pulledBy;
        }

        static void Scan(TypeDefinition t, ModuleDefinition module, HashSet<string> refs)
        {
            void Type(TypeReference tr)
            {
                while (tr != null)
                {
                    if (tr is GenericInstanceType git) { foreach (var a in git.GenericArguments) Type(a); tr = git.ElementType; continue; }
                    if (tr is TypeSpecification ts) { tr = ts.ElementType; continue; }
                    if (tr is GenericParameter) return;
                    var top = tr;
                    while (top.DeclaringType != null) top = top.DeclaringType;
                    if (string.IsNullOrEmpty(top.Namespace) && top.Scope is ModuleDefinition) refs.Add(top.Name);
                    return;
                }
            }
            void Method(MethodReference m)
            {
                Type(m.DeclaringType); Type(m.ReturnType);
                foreach (var p in m.Parameters) Type(p.ParameterType);
                if (m is GenericInstanceMethod gim) foreach (var a in gim.GenericArguments) Type(a);
            }
            void Attrs(IEnumerable<CustomAttribute> attrs) { foreach (var a in attrs) Type(a.AttributeType); }

            Type(t.BaseType);
            foreach (var i in t.Interfaces) Type(i.InterfaceType);
            Attrs(t.CustomAttributes);
            foreach (var f in t.Fields) { Type(f.FieldType); Attrs(f.CustomAttributes); }
            foreach (var p in t.Properties) Type(p.PropertyType);
            foreach (var e in t.Events) Type(e.EventType);
            foreach (var m in t.Methods)
            {
                Type(m.ReturnType);
                foreach (var p in m.Parameters) Type(p.ParameterType);
                Attrs(m.CustomAttributes);
                if (!m.HasBody) continue;
                foreach (var v in m.Body.Variables) Type(v.VariableType);
                foreach (var h in m.Body.ExceptionHandlers) Type(h.CatchType);
                foreach (var ins in m.Body.Instructions)
                {
                    switch (ins.Operand)
                    {
                        case TypeReference tr: Type(tr); break;
                        case MethodReference mr: Method(mr); break;
                        case FieldReference fr: Type(fr.DeclaringType); Type(fr.FieldType); break;
                    }
                }
            }
            foreach (var n in t.NestedTypes) Scan(n, module, refs);
        }
    }
}