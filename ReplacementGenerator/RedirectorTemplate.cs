namespace ReplacementGenerator
{
    /// Source of the runtime (class Replacer) that is written into every generated project as Runtime/Replacer.cs.
    /// Kept as a C# string constant (not an embedded resource) so the generator has no csproj dependency.
    /// This is the single source of truth: edit the text below.
    internal static class RedirectorTemplate
    {
        public const string Source = @"// Runtime of the generated replacement mod.
//
// Strategy: the original game code is NOT patched method-by-method. Instead
//   1. every original MonoBehaviour is swapped for its Replacement.* twin (state copied, original destroyed),
//   2. original Unity messages are neutralised so a dying original never runs its own logic,
//   3. a small list of entry points (e.g. Game.Start) is redirected to the replacement versions,
//   4. optional guards make any other original method throw, to prove nothing escapes.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace ReplacementRuntime
{
	public static class Replacer
	{
		public static ManualLogSource Log;
		public static bool Trace;
		public static bool AllowSurvivingCallers = true;
		public static string DiagnosticType = ""Game""; // static class whose fields are dumped (original vs replacement) on the first lifecycle exception
		public static string ReplacementNamespace = ""Replacement"";

		const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
		const BindingFlags InstanceDeclared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		// The first of these a type declares is where the swap happens; the rest are suppressed.
		static readonly string[] HookOrder = { ""Awake"", ""OnEnable"", ""Start"", ""OnDisable"", ""OnDestroy"", ""FixedUpdate"", ""Update"", ""LateUpdate"" };

		static readonly HashSet<string> UnityMessages = new HashSet<string> {
			""Awake"",""Start"",""OnEnable"",""OnDisable"",""OnDestroy"",""Update"",""LateUpdate"",""FixedUpdate"",
			""OnGUI"",""OnTriggerEnter"",""OnTriggerExit"",""OnTriggerStay"",""OnCollisionEnter"",""OnCollisionExit"",
			""OnCollisionStay"",""OnTriggerEnter2D"",""OnTriggerExit2D"",""OnTriggerStay2D"",""OnCollisionEnter2D"",
			""OnCollisionExit2D"",""OnCollisionStay2D"",""OnMouseDown"",""OnMouseUp"",""OnMouseEnter"",""OnMouseExit"",
			""OnMouseOver"",""OnMouseDrag"",""OnMouseUpAsButton"",""OnBecameVisible"",""OnBecameInvisible"",""OnApplicationPause"",
			""OnApplicationFocus"",""OnApplicationQuit"",""OnValidate"",""Reset"",""OnDrawGizmos"",""OnDrawGizmosSelected"",
			""OnAnimatorMove"",""OnAnimatorIK"",""OnRenderObject"",""OnPreRender"",""OnPostRender"",""OnWillRenderObject"",
			""OnTransformChildrenChanged"",""OnTransformParentChanged"",""OnRectTransformDimensionsChange"",
			""OnBeforeTransformParentChanged"",""OnParticleCollision"",""OnParticleTrigger"",""OnJointBreak"",""OnCanvasGroupChanged""
		};

		static readonly Dictionary<Type, Type> OrigToRepl = new Dictionary<Type, Type>();
		static readonly Dictionary<Type, Type> ReplToOrig = new Dictionary<Type, Type>();
		static readonly Dictionary<string, Type> ByOrigName = new Dictionary<string, Type>(); // original FullName -> replacement type
		static readonly Dictionary<string, Type> ByReplName = new Dictionary<string, Type>(); // replacement FullName -> original type
		static readonly Dictionary<MethodBase, MethodBase> EntryMap = new Dictionary<MethodBase, MethodBase>();
		static readonly HashSet<MethodBase> Patched = new HashSet<MethodBase>();
		static readonly ConditionalWeakTable<object, object> Pairs = new ConditionalWeakTable<object, object>();   // original -> twin
		static readonly ConditionalWeakTable<object, object> Reverse = new ConditionalWeakTable<object, object>(); // twin -> original
		static readonly Dictionary<GameObject, Dictionary<Component, Component>> PrefabMaps = new Dictionary<GameObject, Dictionary<Component, Component>>();
		static Transform _prefabContainer;
		static readonly ConditionalWeakTable<object, object> PrefabSource = new ConditionalWeakTable<object, object>(); // prefab twin -> prefab asset component
		static Assembly _origAsm;

		// ===================================================================== installation

		public static void Install(Harmony harmony, string[] typeNames, string[] entryPoints)
		{
			_origAsm = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == ""Assembly-CSharp"");
			var repl = Assembly.GetExecutingAssembly();
			InstallNullTracer();

			foreach (var n in typeNames)
			{
				var o = _origAsm.GetType(n); var r = repl.GetType(ReplacementNamespace + ""."" + n);
				if (o == null || r == null) { Log.LogWarning($""Missing pair for {n} (original={(o != null)}, replacement={(r != null)})""); continue; }
				Register(o, r);
			}
			Log.LogInfo($""Pairing complete: {OrigToRepl.Count} types paired (incl. nested) from {typeNames.Length} top-level names."");

			int behaviours = 0, scriptables = 0, hooks = 0, suppressed = 0, deferred = 0, failed = 0;
			foreach (var kv in OrigToRepl.ToList())
			{
				var o = kv.Key; var r = kv.Value;
				if (o.IsValueType || o.IsEnum || o.IsInterface || o.ContainsGenericParameters || typeof(Delegate).IsAssignableFrom(o)) continue;
				bool isMb = typeof(MonoBehaviour).IsAssignableFrom(o);
				bool isSo = typeof(ScriptableObject).IsAssignableFrom(o);
				if (!isMb && !isSo) continue; // plain classes need no patches: they are twinned on demand when referenced
				if (isMb) behaviours++; else scriptables++;

				var msgs = o.GetMethods(InstanceDeclared).Where(m => UnityMessages.Contains(m.Name) && !m.IsAbstract && !m.ContainsGenericParameters).ToList();
				var hook = isMb ? HookOrder.FirstOrDefault(h => msgs.Any(m => m.Name == h)) : null;
				foreach (var m in msgs)
				{
					bool isHook = m.Name == hook;
					try { harmony.Patch(m, prefix: NeutralisePrefix(m, isHook)); Patched.Add(m); if (isHook) hooks++; else suppressed++; }
					catch (Exception e) { failed++; Log.LogWarning($""Neutralise failed {o.Name}.{m.Name}: {e.Message}""); }
				}

				// While a swap is in progress the twin's Awake/OnEnable are held back until all state is copied.
				foreach (var name in new[] { ""Awake"", ""OnEnable"" })
				{
					var rm = r.GetMethod(name, InstanceDeclared, null, Type.EmptyTypes, null);
					if (rm == null || rm.IsAbstract) continue;
					try { harmony.Patch(rm, prefix: new HarmonyMethod(AccessTools.Method(typeof(Replacer), nameof(DeferPrefix)))); deferred++; }
					catch (Exception e) { failed++; Log.LogWarning($""Defer patch failed {r.Name}.{name}: {e.Message}""); }
				}
			}

			int entries = 0;
			foreach (var ep in entryPoints)
			{
				var dot = ep.LastIndexOf('.');
				if (dot <= 0) { Log.LogWarning($""Entry point '{ep}' is not in Type.Method form.""); continue; }
				string tn = ep.Substring(0, dot), mn = ep.Substring(dot + 1);
				var o = _origAsm.GetType(tn);
				if (o == null || !OrigToRepl.TryGetValue(o, out var r)) { Log.LogWarning($""Entry point '{ep}': type not found or not replaced.""); continue; }
				var ms = o.GetMethods(All).Where(m => m.Name == mn && !m.IsAbstract && !m.ContainsGenericParameters).ToList();
				if (ms.Count == 0)
				{
					Log.LogWarning($""Entry point '{ep}': no such method. Methods on {tn}: "" +
						string.Join("", "", o.GetMethods(All).Select(m => (m.IsStatic ? ""static "" : """") + m.Name).Distinct()));
					continue;
				}
				foreach (var m in ms)
				{
					var target = FindReplacement(r, m);
					if (target == null) { Log.LogWarning($""Entry point {tn}.{mn}: no matching replacement method.""); continue; }
					EntryMap[m] = target;
					try
					{
						harmony.Patch(m, prefix: PrefixFor(m)); Patched.Add(m); entries++;
						Log.LogInfo($""Entry point redirected: {tn}.{mn} -> {target.DeclaringType.FullName}.{target.Name}"");
					}
					catch (Exception e) { failed++; Log.LogWarning($""Entry patch failed {tn}.{mn}: {e.Message}""); }
				}
			}

			// [RuntimeInitializeOnLoadMethod] statics are how a game boots itself before any scene exists: run the replacement's instead.
			foreach (var kv in OrigToRepl.ToList())
			{
				var o = kv.Key; var r = kv.Value;
				if (o.ContainsGenericParameters) continue;
				foreach (var m in o.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
				{
					if (!Attribute.IsDefined(m, typeof(RuntimeInitializeOnLoadMethodAttribute)) || EntryMap.ContainsKey(m)) continue;
					var target = FindReplacement(r, m);
					if (target == null) { Log.LogWarning($""Boot method {o.Name}.{m.Name} has no matching replacement method.""); continue; }
					EntryMap[m] = target;
					try
					{
						harmony.Patch(m, prefix: PrefixFor(m)); Patched.Add(m); entries++;
						Log.LogInfo($""Boot method redirected ([RuntimeInitializeOnLoadMethod]): {o.Name}.{m.Name} -> {target.DeclaringType.FullName}.{target.Name}"");
					}
					catch (Exception e) { failed++; Log.LogWarning($""Boot patch failed {o.Name}.{m.Name}: {e.Message}""); }
				}
			}

			int boundary = PatchBoundaries(harmony);

			Log.LogInfo($""Install complete: {behaviours} MonoBehaviours + {scriptables} ScriptableObjects handled; "" +
			            $""{hooks} swap hooks, {suppressed} Unity messages suppressed, {deferred} twin lifecycle deferrals, "" +
			            $""{entries} entry points redirected, {boundary} engine/serialization boundary patches, {failed} patch failures."");
		}

		public class NotReplacedException : Exception { public NotReplacedException(string msg) : base(msg) { } }

		/// Verification mode: every remaining original method on the type (and its paired nested types) throws if called.
		/// Reaching one means something still talks to the old code (e.g. a UnityEvent listener or an unreplaced caller).
		public static void GuardOriginal(Harmony harmony, string typeName)
		{
			var orig = _origAsm.GetType(typeName);
			if (orig == null) { Log.LogWarning(""Guard: original type not found: "" + typeName); return; }
			var guard = new HarmonyMethod(AccessTools.Method(typeof(Replacer), nameof(GuardPrefix)));
			int added = 0;
			foreach (var t in OrigToRepl.Keys.Where(k => IsSameOrNestedIn(k, orig)).ToList())
			{
				if (t.IsValueType || t.IsEnum || t.IsInterface || t.ContainsGenericParameters || typeof(Delegate).IsAssignableFrom(t)) continue;
				foreach (var m in t.GetMethods(All))
				{
					if (Patched.Contains(m) || m.IsAbstract || m.ContainsGenericParameters) continue;
					if ((m.GetMethodImplementationFlags() & MethodImplAttributes.InternalCall) != 0) continue;
					if (m.Name == ""Equals"" || m.Name == ""GetHashCode"" || m.Name == ""ToString"" || m.Name.StartsWith(""<"")) continue;
					try { harmony.Patch(m, prefix: guard); Patched.Add(m); added++; }
					catch (Exception e) { Log.LogWarning($""Guard patch failed {t.Name}.{m.Name}: {e.Message}""); }
				}
			}
			if (Trace && added > 0) Log.LogInfo($""Guard {orig.Name}: {added} original method(s) now throw if called."");
		}

		static readonly HashSet<string> _allowedLogged = new HashSet<string>();

		static bool GuardPrefix(MethodBase __originalMethod)
		{
			// Original code that deliberately survives (third-party libraries and other namespaced types compiled into the game
			// assembly, ignored types) legitimately calls the original helpers it was built against. Let those through, loudly.
			if (AllowSurvivingCallers)
			{
				try
				{
					var frame = new System.Diagnostics.StackTrace(2, false).GetFrame(0)?.GetMethod();
					var ct = frame?.DeclaringType;
					if (ct != null && ct.Assembly == _origAsm)
					{
						var top = ct; while (top.DeclaringType != null) top = top.DeclaringType;
						if (!OrigToRepl.ContainsKey(top))
						{
							if (_allowedLogged.Add(top.FullName + "">"" + __originalMethod.DeclaringType.Name + ""."" + __originalMethod.Name))
								Log.LogWarning($""Surviving original code {top.FullName} calls original {__originalMethod.DeclaringType.Name}.{__originalMethod.Name}; allowed (it bypasses the replacement)."");
							return true;
						}
					}
				}
				catch { /* fall through to the throw */ }
			}
			var msg = $""Original method {__originalMethod.DeclaringType.Name}.{__originalMethod.Name} was called; the replacement should have taken over."";
			Log.LogError(msg + ""\n"" + new System.Diagnostics.StackTrace(1, true));
			throw new NotReplacedException(msg);
		}

		static bool IsSameOrNestedIn(Type t, Type root)
		{
			for (; t != null; t = t.DeclaringType) if (t == root) return true;
			return false;
		}

		static void Register(Type o, Type r)
		{
			OrigToRepl[o] = r; ReplToOrig[r] = o;
			ByOrigName[o.FullName] = r; ByReplName[r.FullName] = o;
			foreach (var no in o.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (no.Name.StartsWith(""<"") || Attribute.IsDefined(no, typeof(CompilerGeneratedAttribute))) continue;
				var nr = r.GetNestedType(no.Name, BindingFlags.Public | BindingFlags.NonPublic);
				if (nr == null) { Log.LogWarning($""No replacement nested type for {o.Name}+{no.Name}""); continue; }
				Register(no, nr);
			}
		}

		static MethodBase FindReplacement(Type repl, MethodBase orig)
		{
			var ps = orig.GetParameters().Select(p => Map(p.ParameterType)).ToArray();
			return repl.GetMethod(orig.Name, All, null, ps, null);
		}

		/// Original type -> replacement type (recursing into arrays / by-ref / generic arguments).
		public static Type Map(Type t)
		{
			if (t.IsByRef) return Map(t.GetElementType()).MakeByRefType();
			if (t.IsArray) return Map(t.GetElementType()).MakeArrayType();
			if (t.IsGenericType && !t.IsGenericTypeDefinition)
			{
				var def = t.GetGenericTypeDefinition();
				if (OrigToRepl.TryGetValue(def, out var rdef)) def = rdef;
				return def.MakeGenericType(t.GetGenericArguments().Select(Map).ToArray());
			}
			return OrigToRepl.TryGetValue(t, out var r) ? r : t;
		}

		// ===================================================================== sweep (the ""node tree"" pass)

		/// Finds every live original component (inactive ones and DontDestroyOnLoad ones included) and replaces it,
		/// highest nodes in the hierarchy first.
		public static void Sweep(string reason)
		{
			int swapped = 0;
			_depth++; // one session for the whole sweep: every twin has its state before any Awake runs
			try
			{
				EnsureAllScriptables();
				var todo = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
					.Where(mb => mb != null && mb.gameObject.scene.IsValid() && OrigToRepl.ContainsKey(mb.GetType()) && !Pairs.TryGetValue(mb, out _))
					.OrderBy(Depth).ToList();
				foreach (var mb in todo)
				{
					if (mb == null || Pairs.TryGetValue(mb, out _)) continue;
					SwapOriginal(mb); swapped++;
				}
			}
			catch (Exception e) { Log.LogError(""Sweep failed: "" + e); }
			finally { if (--_depth == 0) Flush(); }
			if (swapped > 0) Log.LogInfo($""Sweep ({reason ?? ""periodic""}): replaced {swapped} original component(s)."");
			if (reason != null) RetargetWorld(reason); // scene load: also fix listeners on objects that contain no original component
		}

		static List<Type> _soTypes;
		static bool _soInit;

		/// ScriptableObject assets that Unity has already loaded have already had OnEnable run (singletons register
		/// themselves there). Their twins must exist, and run OnEnable, before any component twin's Awake.
		static void EnsureAllScriptables()
		{
			if (_soTypes == null)
				_soTypes = OrigToRepl.Keys.Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract && !t.ContainsGenericParameters).ToList();
			int made = 0;
			_depth++;
			try
			{
				foreach (var t in _soTypes)
				{
					UnityEngine.Object[] objs;
					try { objs = Resources.FindObjectsOfTypeAll(t); } catch { continue; }
					foreach (var o in objs)
					{
						if (o == null || Pairs.TryGetValue(o, out _)) continue;
						if (GetOrCreateReplacement(o) != null) made++;
					}
				}
			}
			finally { if (--_depth == 0) Flush(); }
			if (made > 0) Log.LogInfo($""Created {made} ScriptableObject twin(s)."");
		}

		static int Depth(Component c) { int d = 0; for (var t = c.transform.parent; t != null; t = t.parent) d++; return d; }

		// ===================================================================== Harmony prefixes

		static HarmonyMethod NeutralisePrefix(MethodInfo m, bool swap)
		{
			bool en = m.ReturnType == typeof(IEnumerator);
			string name = swap ? (en ? nameof(SwapPrefixEnum) : nameof(SwapPrefix)) : (en ? nameof(SuppressPrefixEnum) : nameof(SuppressPrefix));
			return new HarmonyMethod(AccessTools.Method(typeof(Replacer), name));
		}

		// Unity calls the original's first message in its real execution order; that is the moment the twin's Awake/OnEnable run.
		static bool SwapPrefix(object __instance) { if (Trace) Log.LogInfo($""[order] hook {__instance.GetType().Name} on '{(__instance as Component)?.gameObject.name}'""); _hookFired.Add(__instance); SwapOriginal(__instance); RunPendingFor(__instance); return false; }
		static bool SwapPrefixEnum(object __instance, ref IEnumerator __result) { _hookFired.Add(__instance); SwapOriginal(__instance); RunPendingFor(__instance); __result = EmptyEnum(); return false; }
		static bool SuppressPrefix() => false;
		static bool SuppressPrefixEnum(ref IEnumerator __result) { __result = EmptyEnum(); return false; }
		static IEnumerator EmptyEnum() { yield break; }

		// ---- entry point redirection (void methods need a different prefix than value-returning ones)

		static readonly MethodInfo VoidPrefixMethod = AccessTools.Method(typeof(Replacer), nameof(RedirectPrefixVoid));
		static readonly MethodInfo GenericPrefixMethod = AccessTools.Method(typeof(Replacer), nameof(RedirectPrefix));
		static readonly Dictionary<Type, MethodInfo> ClosedPrefixes = new Dictionary<Type, MethodInfo>();

		static HarmonyMethod PrefixFor(MethodBase m)
		{
			var rt = (m as MethodInfo)?.ReturnType ?? typeof(void);
			if (rt == typeof(void)) return new HarmonyMethod(VoidPrefixMethod);
			if (rt.IsByRef || rt.IsPointer) throw new NotSupportedException(""ref/pointer return type "" + rt);
			if (!ClosedPrefixes.TryGetValue(rt, out var closed)) ClosedPrefixes[rt] = closed = GenericPrefixMethod.MakeGenericMethod(rt);
			return new HarmonyMethod(closed);
		}

		static bool RedirectPrefixVoid(MethodBase __originalMethod, object __instance, object[] __args)
			=> Dispatch(__originalMethod, __instance, __args, out _);

		static bool RedirectPrefix<T>(MethodBase __originalMethod, object __instance, object[] __args, ref T __result)
		{
			bool runOriginal = Dispatch(__originalMethod, __instance, __args, out var res);
			if (!runOriginal && res != null)
			{
				try { __result = (T)res; }
				catch (InvalidCastException)
				{
					Log.LogError($""Replacement for {__originalMethod.DeclaringType.Name}.{__originalMethod.Name} returned {res.GetType()} but {typeof(T)} was expected."");
					throw;
				}
			}
			return runOriginal;
		}

		static bool Dispatch(MethodBase __originalMethod, object __instance, object[] __args, out object __result)
		{
			__result = null;
			if (!EntryMap.TryGetValue(__originalMethod, out var target)) return true;
			if (Trace) Log.LogInfo($""entry {__originalMethod.DeclaringType.Name}.{__originalMethod.Name} -> replacement"");

			object self = __originalMethod.IsStatic ? null : GetOrCreateReplacement(__instance);
			var args = __args.Select(ToReplacement).ToArray();
			try { __result = target.Invoke(self, args); }
			catch (TargetInvocationException e)
			{
				Log.LogError($""Exception in replacement {target.DeclaringType.Name}.{target.Name}: {e.InnerException}"");
				throw e.InnerException ?? e;
			}
			var ps = __originalMethod.GetParameters();
			for (int i = 0; i < args.Length; i++) if (ps[i].ParameterType.IsByRef) __args[i] = ToOriginal(args[i]);
			if (__result != null) __result = ToOriginal(__result);
			return false;
		}

		// ===================================================================== twin fidelity audit
		// Compares a twin's fields with its (retired but still readable) original, recursively, and reports every difference.
		// Enable with the AuditTypes setting; it runs on each scene load for the first few twins of each listed type.

		public static void Audit(string[] typeNames, int perType)
		{
			foreach (var tn in typeNames)
			{
				if (!ByOrigName.TryGetValue(tn, out var rt)) { Log.LogWarning(""[audit] no replacement type for '"" + tn + ""'""); continue; }
				int nScene = 0, nPrefab = 0;
				UnityEngine.Object[] twins;
				try { twins = Resources.FindObjectsOfTypeAll(rt); } catch (Exception e) { Log.LogWarning(""[audit] lookup failed: "" + e.Message); continue; }
				foreach (var twin in twins)
				{
					var c = twin as Component;
					if (c == null || !c.gameObject.scene.IsValid() || !Reverse.TryGetValue(twin, out var orig)) continue;
					bool prefabTwin = PrefabSource.TryGetValue(twin, out var src);
					if (prefabTwin) { if (nPrefab >= perType) continue; }
					else if (nScene >= perType) continue;
					// a prefab twin's direct original is the destroyed clone component; compare with the prefab asset instead
					var reference = prefabTwin ? src : orig;
					var sb = new System.Text.StringBuilder();
					DiffObject(reference, twin, """", 0, sb, new HashSet<object>(new RefComparer()));
					var ka = SerializedKeys(reference); var kb = SerializedKeys(twin);
					if (ka != null && kb != null)
					{
						var missing = ka.Where(k => !kb.Contains(k)).ToList();
						var extra = kb.Where(k => !ka.Contains(k)).ToList();
						if (missing.Count > 0)
						{
							sb.Append(""\n    UNITY-SERIALIZED on original but NOT on twin (lost when this is Instantiated): "" + string.Join("", "", missing));
							foreach (var k in missing.Take(4)) sb.Append(""\n      "" + FieldFacts(twin.GetType(), reference.GetType(), k));
						}
						if (extra.Count > 0) sb.Append(""\n    unity-serialized on twin only: "" + string.Join("", "", extra));
					}
					Log.LogInfo($""[audit] {tn} ({(prefabTwin ? ""prefab twin"" : ""scene"")}) on '{c.gameObject.name}': "" + (sb.Length == 0 ? ""identical"" : sb.ToString()));
					if (prefabTwin) nPrefab++; else nScene++;
				}
				if (nScene + nPrefab == 0) Log.LogInfo($""[audit] no live {tn} twins found"");
			}
		}

		/// Why isn't this field serialized? Prints the facts Unity's serializer looks at, for the twin's field and the original's.
		static string FieldFacts(Type twinType, Type origType, string name)
		{
			FieldInfo tf = null, of = null;
			for (var x = twinType; x != null && tf == null; x = x.BaseType) tf = x.GetField(name, InstanceDeclared);
			for (var x = origType; x != null && of == null; x = x.BaseType) of = x.GetField(name, InstanceDeclared);
			string Describe1(FieldInfo f)
			{
				if (f == null) return ""(no such field)"";
				var ft = f.FieldType;
				var el = ft.IsArray ? ft.GetElementType() : (ft.IsGenericType ? ft.GetGenericArguments()[0] : ft);
				var attrs = string.Join(""/"", f.GetCustomAttributes(false).Select(a => a.GetType().Name).ToArray());
				var tattrs = string.Join(""/"", el.GetCustomAttributes(false).Select(a => a.GetType().Name).ToArray());
				return $""{ft} ({(f.IsPublic ? ""public"" : ""non-public"")}, field attrs [{attrs}]); element type {el.FullName} in '{el.Assembly.GetName().Name}': "" +
				       $""IsSerializable={el.IsSerializable}, IsClass={el.IsClass}, IsAbstract={el.IsAbstract}, generic={el.IsGenericType}, type attrs [{tattrs}]"";
			}
			return $""{name}:\n        twin     : {Describe1(tf)}\n        original : {Describe1(of)}"";
		}

		/// Names of the top-level fields Unity would serialize (what survives Instantiate), from JsonUtility's output.
		static HashSet<string> SerializedKeys(object o)
		{
			string json;
			try { json = JsonUtility.ToJson(o); } catch { return null; }
			var set = new HashSet<string>();
			int depth = 0;
			for (int i = 0; i < json.Length; i++)
			{
				char ch = json[i];
				if (ch == '""')
				{
					var sb = new System.Text.StringBuilder();
					int j = i + 1;
					while (j < json.Length && json[j] != '""') { if (json[j] == '\\') j++; if (j < json.Length) sb.Append(json[j]); j++; }
					if (depth == 1 && j + 1 < json.Length && json[j + 1] == ':') set.Add(sb.ToString());
					i = j;
					continue;
				}
				if (ch == '{' || ch == '[') depth++; else if (ch == '}' || ch == ']') depth--;
			}
			return set;
		}

		static void DiffObject(object o, object t, string path, int depth, System.Text.StringBuilder sb, HashSet<object> seen)
		{
			if (depth > 3 || o == null || t == null || !seen.Add(t)) return;
			for (var x = t.GetType(); !IsEngineOrSystem(x); x = x.BaseType)
				foreach (var tf in x.GetFields(InstanceDeclared))
				{
					var ff = FindField(o.GetType(), tf.Name);
					if (ff == null) { sb.Append(""\n    "").Append(path).Append(tf.Name).Append("": (no such field on the original)""); continue; }
					object ov, tv;
					try { ov = ff.GetValue(o); tv = tf.GetValue(t); } catch { continue; }
					DiffValue(ov, tv, path + tf.Name, depth, sb, seen);
				}
		}

		static void DiffValue(object ov, object tv, string path, int depth, System.Text.StringBuilder sb, HashSet<object> seen)
		{
			if (ov is IDictionary od && tv is IDictionary td)
			{
				if (od.Count != td.Count) sb.Append($""\n    {path}: original has {od.Count} entries, twin {td.Count}"");
				return;
			}
			if (ov is IList ol && tv is IList tl)
			{
				if (ol.Count != tl.Count) { sb.Append($""\n    {path}: original has {ol.Count} items, twin {tl.Count}""); return; }
				for (int i = 0; i < Math.Min(3, ol.Count); i++) DiffValue(ol[i], tl[i], path + ""["" + i + ""]"", depth + 1, sb, seen);
				return;
			}
			if (ov != null && tv != null && !(tv is UnityEngine.Object) && !tv.GetType().IsValueType &&
			    ReplToOrig.TryGetValue(tv.GetType(), out var otype) && otype == ov.GetType())
			{
				DiffObject(ov, tv, path + ""."", depth + 1, sb, seen);
				return;
			}
			string a = Describe(ov), b = Describe(tv);
			if (a != b) sb.Append($""\n    {path}: original = {a}   twin = {b}"");
		}

		// ===================================================================== NullReferenceException locator
		// Unity swallows exceptions thrown by Update/event handlers and only prints a stack without IL offsets. This hooks the moment a
		// NullReferenceException is thrown inside replacement code and prints the IL instruction that dereferenced null, plus the
		// instructions before it (which show WHICH expression was null).

		static bool _tracing;
		static Mono.Cecil.ModuleDefinition _cecilModule;
		static readonly HashSet<string> _nullSeen = new HashSet<string>();

		static void InstallNullTracer()
		{
			try
			{
				AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
				{
					if (_tracing || !(e.Exception is NullReferenceException)) return;
					_tracing = true;
					try { TraceNull(e.Exception); } catch (Exception ex) { Log.LogWarning(""Null tracer failed: "" + ex); } finally { _tracing = false; }
				};
				Log.LogInfo(""NullReferenceException locator installed."");
			}
			catch (Exception e) { Log.LogWarning(""Could not install the NullReferenceException locator: "" + e.Message); }
		}

		static void TraceNull(Exception ex)
		{
			var asm = Assembly.GetExecutingAssembly();
			var frames = new System.Diagnostics.StackTrace(ex, true).GetFrames();
			if (frames == null || frames.Length == 0) frames = new System.Diagnostics.StackTrace(true).GetFrames();
			if (frames == null) return;
			foreach (var f in frames)
			{
				var m = f.GetMethod();
				if (m == null || m.DeclaringType == null || m.DeclaringType.Assembly != asm || m.DeclaringType == typeof(Replacer)) continue;
				int off = f.GetILOffset();
				var key = m.DeclaringType.FullName + ""."" + m.Name + ""@"" + off;
				if (!_nullSeen.Add(key)) return;

				var sb = new System.Text.StringBuilder();
				sb.Append(""[null] NullReferenceException in "").Append(m.DeclaringType.FullName).Append('.').Append(m.Name).Append("" at IL_"").Append(off.ToString(""x4""));
				try
				{
					if (_cecilModule == null) _cecilModule = Mono.Cecil.ModuleDefinition.ReadModule(asm.Location, new Mono.Cecil.ReaderParameters { InMemory = true });
					var md = _cecilModule.LookupToken(m.MetadataToken) as Mono.Cecil.MethodDefinition;
					if (md != null && md.HasBody && off >= 0)
					{
						var ins = md.Body.Instructions;
						int idx = 0;
						for (int i = 0; i < ins.Count; i++) if (ins[i].Offset <= off) idx = i; else break;
						for (int i = Math.Max(0, idx - 6); i <= idx; i++)
							sb.Append(""\n    "").Append(i == idx ? "">>"" : ""  "").Append(' ').Append(ins[i]);
					}
				}
				catch (Exception ce) { sb.Append(""\n    (could not disassemble: "" + ce.Message + "")""); }
				Log.LogError(sb.ToString());
				return;
			}
			if (_nullMisses++ < 3)
				Log.LogInfo(""[null] NullReferenceException seen but no replacement frame found; top frames: "" +
					string.Join("" <- "", frames.Take(6).Select(fr => (fr.GetMethod()?.DeclaringType?.Name ?? ""?"") + ""."" + (fr.GetMethod()?.Name ?? ""?"") + ""@"" + fr.GetILOffset())));
		}
		static int _nullMisses;

		// ===================================================================== engine / serialization boundaries
		// Replacement code asks Unity and the serializer for *replacement* types, but Unity assets and save files only know the
		// ORIGINAL types. These few patches translate at the boundary instead of patching any game code.

		static int PatchBoundaries(Harmony harmony)
		{
			int n = 0;
			void P(MethodBase target, string prefix)
			{
				if (target == null) { Log.LogWarning(""Boundary patch target not found for "" + prefix); return; }
				try { harmony.Patch(target, prefix: new HarmonyMethod(AccessTools.Method(typeof(Replacer), prefix))); n++; }
				catch (Exception e) { Log.LogWarning($""Boundary patch failed {target.DeclaringType.Name}.{target.Name}: {e.Message}""); }
			}
			var t = typeof(Type);
			P(typeof(Resources).GetMethod(""Load"", new[] { typeof(string), t }), nameof(ResourcesLoadPrefix));
			P(typeof(Resources).GetMethod(""LoadAll"", new[] { typeof(string), t }), nameof(ResourcesLoadAllPrefix));
			P(typeof(Resources).GetMethod(""FindObjectsOfTypeAll"", new[] { t }), nameof(FindAllPrefix));
			P(typeof(UnityEngine.Object).GetMethod(""FindObjectsOfType"", new[] { t }), nameof(FindPrefix));
			P(typeof(UnityEngine.Object).GetMethod(""FindObjectOfType"", new[] { t }), nameof(FindPrefix));
			P(typeof(Behaviour).GetProperty(""enabled"")?.GetSetMethod(), nameof(EnabledPrefix));
			foreach (var m in typeof(BinaryFormatter).GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(m => m.Name == ""Serialize"" || m.Name == ""Deserialize""))
				P(m, nameof(BinaryFormatterPrefix));
			return n;
		}

		static bool ResourcesLoadPrefix(string path, Type systemTypeInstance, ref UnityEngine.Object __result)
		{
			if (systemTypeInstance == null || !ReplToOrig.TryGetValue(systemTypeInstance, out var ot)) return true;
			var o = Resources.Load(path, ot); // original type: not intercepted again
			__result = o == null ? null : GetOrCreateReplacement(o) as UnityEngine.Object;
			return false;
		}

		static bool ResourcesLoadAllPrefix(string path, Type systemTypeInstance, ref UnityEngine.Object[] __result)
		{
			if (systemTypeInstance == null || !ReplToOrig.TryGetValue(systemTypeInstance, out var ot)) return true;
			var list = new List<UnityEngine.Object>();
			_depth++;
			try { foreach (var o in Resources.LoadAll(path, ot)) { var tw = GetOrCreateReplacement(o) as UnityEngine.Object; if (tw != null) list.Add(tw); } }
			finally { if (--_depth == 0) Flush(); }
			var arr = (UnityEngine.Object[])Array.CreateInstance(systemTypeInstance, list.Count);
			for (int i = 0; i < arr.Length; i++) arr[i] = list[i];
			__result = arr;
			return false;
		}

		/// Unity runs a component's Awake the first time it is enabled if that has not happened yet. The game relies on it:
		/// GameManager.Awake does ""enabled = false; enabled = true"" on every system manager so each registers itself (Game.Manager.Ui = this,
		/// ...) before any other object's Awake. A twin's Awake is held back by us, so Unity would not trigger it; do it here instead.
		static bool EnabledPrefix(Behaviour __instance)
		{
			if (_pending.Count > 0 && HasPending(__instance)) RunPending(p => ReferenceEquals(p.Key, __instance));
			return true;
		}

		// FindObjectOfType<Replacement.X>() must see X's twin even if X's original has not been swapped yet.
		static bool FindPrefix(Type type) { EnsureTwins(type, false); return true; }
		static bool FindAllPrefix(Type type) { EnsureTwins(type, true); return true; }

		static void EnsureTwins(Type type, bool includeInactiveAndAssets)
		{
			if (type == null || !ReplToOrig.TryGetValue(type, out var ot)) return;
			UnityEngine.Object[] originals;
			try { originals = includeInactiveAndAssets ? Resources.FindObjectsOfTypeAll(ot) : UnityEngine.Object.FindObjectsOfType(ot); }
			catch { return; }
			_depth++;
			try
			{
				foreach (var o in originals)
				{
					if (o == null || Pairs.TryGetValue(o, out _)) continue;
					if (o is MonoBehaviour mb) { if (mb.gameObject.scene.IsValid()) SwapOriginal(mb); }
					else if (o is ScriptableObject) GetOrCreateReplacement(o);
				}
			}
			finally { if (--_depth == 0) Flush(); }
		}

		// Saves: BinaryFormatter stores type names. Read original-named types as replacements, and write replacements
		// under the ORIGINAL names so the save stays loadable by the unmodified game.
		static void BinaryFormatterPrefix(BinaryFormatter __instance)
		{
			if (__instance.Binder == null) __instance.Binder = ReplacementBinder.Instance;
		}

		sealed class ReplacementBinder : SerializationBinder
		{
			public static readonly ReplacementBinder Instance = new ReplacementBinder();
			static readonly Regex OrigRef = new Regex(@""(?<n>[A-Za-z0-9_+`.]+), Assembly-CSharp(?:, Version=[^,\]\[]+, Culture=[^,\]\[]+, PublicKeyToken=[^,\]\[]+)?"");

			public override Type BindToType(string assemblyName, string typeName)
			{
				try
				{
					if (assemblyName != null && assemblyName.StartsWith(""Assembly-CSharp"") && ByOrigName.TryGetValue(typeName, out var r)) return r;
					if (typeName.Contains(""Assembly-CSharp"")) // generic instantiations carry assembly-qualified arguments
					{
						var rewritten = OrigRef.Replace(typeName, m => ByOrigName.TryGetValue(m.Groups[""n""].Value, out var rt) ? rt.AssemblyQualifiedName : m.Value);
						return Type.GetType(assemblyName == null ? rewritten : rewritten + "", "" + assemblyName);
					}
				}
				catch (Exception e) { Log.LogWarning($""Binder could not map {typeName}: {e.Message}""); }
				return null; // default resolution
			}

			public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
			{
				assemblyName = null; typeName = null;
				if (ReplToOrig.TryGetValue(serializedType, out var ot)) { assemblyName = ot.Assembly.FullName; typeName = ot.FullName; return; }
				if (serializedType.IsGenericType && serializedType.FullName != null && serializedType.FullName.Contains(ReplacementNamespace + "".""))
				{
					var replAsm = Regex.Escape(Assembly.GetExecutingAssembly().FullName);
					typeName = Regex.Replace(serializedType.FullName, @""(?<n>[A-Za-z0-9_+`.]+), "" + replAsm,
						m => ByReplName.TryGetValue(m.Groups[""n""].Value, out var o2) ? o2.AssemblyQualifiedName : m.Value);
					assemblyName = serializedType.Assembly.FullName;
				}
			}
		}

		// ===================================================================== UnityEvent persistent listeners
		// Buttons etc. store ""call Method on Object"" in the scene/prefab data. Those targets are the ORIGINAL components, which we
		// destroy, so Unity silently skips the call (dead buttons, missing click sounds). Re-point them at the twins.

		sealed class RefComparer : IEqualityComparer<object>
		{
			public new bool Equals(object x, object y) => ReferenceEquals(x, y);
			public int GetHashCode(object o) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
		}

		static FieldInfo _fPersist, _fDirty, _fCalls, _fTarget, _fTargetType, _fArgs, _fArgObj, _fArgObjType;
		static bool _evInit, _evOk;
		static readonly HashSet<int> _retargeted = new HashSet<int>();
		static readonly Dictionary<Type, List<FieldInfo>> _eventFields = new Dictionary<Type, List<FieldInfo>>();

		static void InitEvents()
		{
			if (_evInit) return;
			_evInit = true;
			const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			_fPersist = typeof(UnityEventBase).GetField(""m_PersistentCalls"", F);
			_fDirty = typeof(UnityEventBase).GetField(""m_CallsDirty"", F);
			_fCalls = _fPersist?.FieldType.GetField(""m_Calls"", F);
			var pc = typeof(UnityEventBase).Assembly.GetType(""UnityEngine.Events.PersistentCall"");
			_fTarget = pc?.GetField(""m_Target"", F);
			_fTargetType = pc?.GetField(""m_TargetAssemblyTypeName"", F);
			_fArgs = pc?.GetField(""m_Arguments"", F);
			_fArgObj = _fArgs?.FieldType.GetField(""m_ObjectArgument"", F);
			_fArgObjType = _fArgs?.FieldType.GetField(""m_ObjectArgumentAssemblyTypeName"", F);
			_evOk = _fPersist != null && _fDirty != null && _fCalls != null && _fTarget != null && _fArgs != null;
			if (!_evOk) Log.LogWarning(""UnityEvent internals not found on this Unity version; persistent listeners cannot be retargeted."");
		}

		static void RetargetHierarchy(Transform root)
		{
			InitEvents();
			if (!_evOk || root == null) return;
			int changed = 0;
			foreach (var c in root.GetComponentsInChildren<Component>(true))
			{
				if (c == null) continue;
				_retargeted.Add(c.GetInstanceID());
				RetargetComponent(c, ref changed);
			}
			if (changed > 0 && Trace) Log.LogInfo($""Retargeted {changed} UnityEvent listener(s) under '{root.name}'."");
		}

		static void RetargetWorld(string reason)
		{
			InitEvents();
			if (!_evOk) return;
			int comps = 0, changed = 0;
			_depth++;
			try
			{
				foreach (var c in Resources.FindObjectsOfTypeAll<Component>())
				{
					if (c == null || !c.gameObject.scene.IsValid() || !_retargeted.Add(c.GetInstanceID())) continue;
					comps++;
					RetargetComponent(c, ref changed);
				}
			}
			catch (Exception e) { Log.LogError(""Retarget pass failed: "" + e); }
			finally { if (--_depth == 0) Flush(); }
			if (changed > 0) Log.LogInfo($""Retargeted {changed} UnityEvent listener(s) ({comps} new components scanned, {reason})."");
		}

		static void RetargetComponent(Component c, ref int changed)
		{
			if (Pairs.TryGetValue(c, out _)) return; // a swapped original; its events live on the twin
			var ns = c.GetType().Namespace;
			if (ns != null && ns.StartsWith(""UnityEngine"") && !ns.StartsWith(""UnityEngine.UI"") && !ns.StartsWith(""UnityEngine.EventSystems"")) return;
			RetargetObject(c, 0, ref changed, new HashSet<object>(new RefComparer()));
		}

		static bool Walkable(Type ft)
		{
			if (ft.IsPrimitive || ft.IsEnum || ft == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(ft) || typeof(Delegate).IsAssignableFrom(ft)) return false;
			var ns = ft.Namespace;
			if (ns != null && ns.StartsWith(""System"")) return false;
			if (ns != null && ns.StartsWith(""UnityEngine"") && !ns.StartsWith(""UnityEngine.UI"") && !ns.StartsWith(""UnityEngine.EventSystems"") && !ns.StartsWith(""UnityEngine.Events"")) return false;
			return typeof(UnityEventBase).IsAssignableFrom(ft) || ft.IsSerializable || Attribute.IsDefined(ft, typeof(SerializableAttribute));
		}

		static List<FieldInfo> EventFields(Type t)
		{
			if (_eventFields.TryGetValue(t, out var list)) return list;
			list = new List<FieldInfo>();
			for (var x = t; x != null && x != typeof(object); x = x.BaseType)
			{
				var ns = x.Namespace;
				if (ns == ""UnityEngine"" || (ns != null && ns.StartsWith(""UnityEngine"") && !ns.StartsWith(""UnityEngine.UI"") && !ns.StartsWith(""UnityEngine.EventSystems"") && !ns.StartsWith(""UnityEngine.Events""))) break;
				foreach (var f in x.GetFields(InstanceDeclared))
					if (Walkable(f.FieldType) || typeof(IList).IsAssignableFrom(f.FieldType)) list.Add(f);
			}
			return _eventFields[t] = list;
		}

		static void RetargetObject(object o, int depth, ref int changed, HashSet<object> seen)
		{
			if (o == null || depth > 5 || !seen.Add(o)) return;
			if (o is UnityEventBase ue) { changed += RetargetEvent(ue); return; }
			if (o is IList list)
			{
				var lt = o.GetType();
				var et = lt.IsArray ? lt.GetElementType() : (lt.IsGenericType ? lt.GetGenericArguments()[0] : null);
				if (et != null && !Walkable(et)) return; // e.g. float[] / List<string>: nothing to find, don't box every element
				foreach (var item in list) if (item != null && !(item is UnityEngine.Object) && Walkable(item.GetType())) RetargetObject(item, depth + 1, ref changed, seen);
				return;
			}
			foreach (var f in EventFields(o.GetType()))
			{
				object v; try { v = f.GetValue(o); } catch { continue; }
				RetargetObject(v, depth + 1, ref changed, seen);
			}
		}

		static int RetargetEvent(UnityEventBase ue)
		{
			int changed = 0;
			var group = _fPersist.GetValue(ue);
			var calls = group == null ? null : _fCalls.GetValue(group) as IList;
			if (calls == null) return 0;
			foreach (var call in calls)
			{
				var target = _fTarget.GetValue(call);
				if (target != null)
				{
					var nt = ToReplacement(target);
					if (!ReferenceEquals(nt, target) && nt is UnityEngine.Object nto)
					{
						_fTarget.SetValue(call, nto);
						_fTargetType?.SetValue(call, nto.GetType().AssemblyQualifiedName);
						changed++;
					}
				}
				var args = _fArgs.GetValue(call);
				if (args == null || _fArgObj == null) continue;
				var ao = _fArgObj.GetValue(args);
				if (ao != null)
				{
					var na = ToReplacement(ao);
					if (!ReferenceEquals(na, ao) && na is UnityEngine.Object nao)
					{
						_fArgObj.SetValue(args, nao);
						_fArgObjType?.SetValue(args, nao.GetType().AssemblyQualifiedName);
						changed++;
					}
				}
				else if (_fArgObjType != null && _fArgObjType.GetValue(args) is string tn && tn.Contains(""Assembly-CSharp""))
				{
					var comma = tn.IndexOf(',');
					if (ByOrigName.TryGetValue(comma > 0 ? tn.Substring(0, comma) : tn, out var rt)) { _fArgObjType.SetValue(args, rt.AssemblyQualifiedName); changed++; }
				}
			}
			if (changed > 0) _fDirty.SetValue(ue, true);
			return changed;
		}

		// ===================================================================== swap sessions
		// Creating a twin can create more twins (references). Their Awake/OnEnable are held until every state copy is done,
		// then replayed in creation order, so no Awake ever sees half-copied state.

		static int _depth;
		static bool _flushing;
		static object _flushTarget;
		static readonly List<KeyValuePair<object, MethodBase>> _queue = new List<KeyValuePair<object, MethodBase>>();
		static readonly List<KeyValuePair<Behaviour, bool>> _enableFix = new List<KeyValuePair<Behaviour, bool>>();

		static bool DeferPrefix(MethodBase __originalMethod, object __instance)
		{
			if (_depth > 0 && !ReferenceEquals(__instance, _flushTarget))
			{
				_queue.Add(new KeyValuePair<object, MethodBase>(__instance, __originalMethod));
				return false;
			}
			return true;
		}

		static int ExecOrder(Type t)
		{
			var a = (DefaultExecutionOrder)Attribute.GetCustomAttribute(t, typeof(DefaultExecutionOrder), true);
			return a != null ? a.order : 0;
		}

		// Twins whose original has not had its turn yet wait here (see SwapPrefix).
		static readonly List<KeyValuePair<object, MethodBase>> _pending = new List<KeyValuePair<object, MethodBase>>();
		static readonly HashSet<object> _hookFired = new HashSet<object>(new RefComparer());
		static readonly List<KeyValuePair<Behaviour, bool>> _enableLater = new List<KeyValuePair<Behaviour, bool>>();

		static bool ShouldRunNow(object twin)
		{
			if (!(twin is MonoBehaviour)) return true;                  // ScriptableObject twins: their originals were loaded long ago
			if (!Reverse.TryGetValue(twin, out var orig)) return true;  // not a swapped twin
			return _hookFired.Contains(orig);                           // original already had its Awake: replay immediately
		}

		static bool HasPending(Behaviour b) { foreach (var p in _pending) if (ReferenceEquals(p.Key, b)) return true; return false; }

		static void ApplyEnable(KeyValuePair<Behaviour, bool> f)
		{
			try { if (f.Key != null && f.Key.enabled != f.Value) f.Key.enabled = f.Value; } catch (Exception ex) { Log.LogError(ex); }
		}

		static void Invoke(KeyValuePair<object, MethodBase> e)
		{
			if (e.Key is UnityEngine.Object uo && !uo) return;
			var prev = _flushTarget; _flushTarget = e.Key;
			if (Trace) Log.LogInfo($""[order] run {e.Value.DeclaringType.Name}.{e.Value.Name} on '{(e.Key as Component)?.gameObject.name}'"");
			try { e.Value.Invoke(e.Key, null); }
			catch (TargetInvocationException ex) { Log.LogError($""Exception in {e.Value.DeclaringType.Name}.{e.Value.Name}: {ex.InnerException}""); DumpStatics(); }
			catch (Exception ex) { Log.LogError(ex); }
			finally { _flushTarget = prev; }
		}

		static bool _dumped;

		static string Describe(object val)
		{
			if (val == null) return ""null"";
			if (val is UnityEngine.Object uo) return uo ? $""{val.GetType().Name} '{uo.name}'"" : ""destroyed "" + val.GetType().Name;
			if (val is string str) return ""\"""" + str + ""\"""";
			if (val is ICollection c) return $""{val.GetType().Name}[{c.Count}]"";
			return val.GetType().IsPrimitive || val.GetType().IsEnum ? val.ToString() : val.GetType().Name;
		}

		/// One-time diagnostic on the first twin lifecycle exception: every static FIELD of the entry class (original vs replacement),
		/// and, for each static that holds a replaced object (e.g. Game._manager), that object's own fields. Shows which global
		/// state or reference was never initialised.
		public static void DumpStatics(bool force = false)
		{
			if ((_dumped && !force) || string.IsNullOrEmpty(DiagnosticType)) return;
			_dumped = true;
			void Dump(string label, Type t)
			{
				if (t == null) { Log.LogInfo($""[statics] {label} {DiagnosticType}: type not found""); return; }
				var sb = new System.Text.StringBuilder();
				var held = new List<KeyValuePair<string, object>>();
				foreach (var f in t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
				{
					string v;
					try { var val = f.GetValue(null); v = Describe(val); if (val != null && ReplToOrig.ContainsKey(val.GetType())) held.Add(new KeyValuePair<string, object>(f.Name, val)); }
					catch (Exception e) { v = ""<"" + e.GetType().Name + "">""; }
					sb.Append(""\n    "").Append(f.Name).Append("" = "").Append(v);
				}
				foreach (var h in held)
				{
					sb.Append(""\n  "").Append(h.Key).Append("" fields:"");
					for (var x = h.Value.GetType(); !IsEngineOrSystem(x); x = x.BaseType)
						foreach (var f in x.GetFields(InstanceDeclared))
						{
							string v; try { v = Describe(f.GetValue(h.Value)); } catch (Exception e) { v = ""<"" + e.GetType().Name + "">""; }
							sb.Append(""\n      "").Append(f.Name).Append("" = "").Append(v);
						}
				}
				Log.LogInfo($""[statics] {label} {t.FullName}:{sb}"");
			}
			Dump(""original"", _origAsm.GetType(DiagnosticType));
			Dump(""replacement"", Assembly.GetExecutingAssembly().GetType(ReplacementNamespace + ""."" + DiagnosticType));
		}

		static void Flush()
		{
			if (_flushing) return;
			_flushing = true; _depth++;
			try
			{
				while (_queue.Count > 0 || _enableFix.Count > 0)
				{
					if (_queue.Count > 0)
					{
						// Classify the whole batch BEFORE running anything: an Awake that runs now (GameManager) may enable other
						// twins in this batch, and EnabledPrefix can only find them if they are already in _pending.
						var batch = _queue.ToList(); _queue.Clear();
						var now = new List<KeyValuePair<object, MethodBase>>();
						foreach (var e in batch)
						{
							if (e.Key is UnityEngine.Object uo && !uo) continue;
							if (ShouldRunNow(e.Key)) now.Add(e); else _pending.Add(e);
						}
						foreach (var e in now) Invoke(e);
					}
					if (_enableFix.Count > 0)
					{
						var f = _enableFix[0]; _enableFix.RemoveAt(0);
						if (HasPending(f.Key)) _enableLater.Add(f); else ApplyEnable(f);
					}
				}
			}
			finally { _depth--; _flushing = false; _flushTarget = null; }
		}

		static void RunPendingFor(object original)
		{
			if (_pending.Count == 0 || !Pairs.TryGetValue(original, out var twin)) return;
			RunPending(p => ReferenceEquals(p.Key, twin));
		}

		/// Fallback for twins whose original never got a turn (destroyed before its Awake, etc.): run them in
		/// [DefaultExecutionOrder] / creation order. Called on scene load and every frame.
		public static void RunAllPending()
		{
			for (int round = 0; round < 5 && _pending.Count > 0; round++) RunPending(null);
			DestroyRetired();
		}

		static void RunPending(Func<KeyValuePair<object, MethodBase>, bool> pick)
		{
			var run = _pending.Where(p => pick == null || pick(p)).ToList();
			if (run.Count == 0) return;
			if (pick == null)
				run = run.Select((e, i) => new { e, i, o = ExecOrder(e.Key.GetType()) }).OrderBy(x => x.o).ThenBy(x => x.i).Select(x => x.e).ToList();
			foreach (var r in run) _pending.Remove(r);
			_depth++;
			try
			{
				foreach (var e in run) Invoke(e);
				for (int i = _enableLater.Count - 1; i >= 0; i--)
					if (!HasPending(_enableLater[i].Key)) { var f = _enableLater[i]; _enableLater.RemoveAt(i); ApplyEnable(f); }
			}
			finally { if (--_depth == 0) Flush(); }
		}

		// ===================================================================== twin creation

		/// Replaces this component and every other original in its hierarchy (highest node first) inside one session, so
		/// GetComponent/GetComponentInChildren in a twin's Awake always finds its sibling twins.
		public static void SwapOriginal(object inst)
		{
			var mb = inst as MonoBehaviour;
			if (mb == null) return;
			_depth++;
			try
			{
				if (!_soInit) { _soInit = true; EnsureAllScriptables(); }
				if (Pairs.TryGetValue(mb, out _)) return; // already swapped (its remaining messages are suppressed)
				if (!mb.gameObject.scene.IsValid()) { GetOrCreateReplacement(mb); return; }
				var list = mb.transform.root.GetComponentsInChildren<MonoBehaviour>(true)
					.Where(c => c != null && OrigToRepl.ContainsKey(c.GetType()) && !Pairs.TryGetValue(c, out _))
					.OrderBy(Depth).ToList();
				foreach (var c in list)
				{
					if (c == null || Pairs.TryGetValue(c, out _)) continue;
					var twin = GetOrCreateReplacement(c);
					if (Trace) Log.LogInfo($""Swapped {c.GetType().Name} on '{c.gameObject.name}' -> {twin?.GetType().FullName}"");
				}
				RetargetHierarchy(mb.transform.root);
			}
			catch (Exception e) { Log.LogError($""Swap failed for {inst.GetType().Name}: {e}""); }
			finally { if (--_depth == 0) Flush(); }
		}

		public static object GetOrCreateReplacement(object original)
		{
			if (original == null) return null;
			if (Pairs.TryGetValue(original, out var existing)) return existing;
			_depth++;
			try { return CreateTwin(original); }
			finally { if (--_depth == 0) Flush(); }
		}

		static object CreateTwin(object original)
		{
			if (original is UnityEngine.Object uo && !uo) return null;
			if (!OrigToRepl.TryGetValue(original.GetType(), out var rt)) return null;

			if (original is Component)
			{
				var mb = original as MonoBehaviour;
				if (mb == null) return null;
				if (!mb.gameObject.scene.IsValid()) return PrefabTwin(mb); // prefab asset: never touch the asset itself
				if (Trace) Log.LogInfo($""Twinning scene component {mb.GetType().Name} on '{mb.gameObject.name}'"");

				bool enabledBefore = mb.enabled;
				// RequireComponent on the twin may already have added a blank one: adopt it rather than adding a second.
				var twin = (Behaviour)mb.gameObject.GetComponents(rt).FirstOrDefault(c => !Reverse.TryGetValue(c, out _))
				           ?? (Behaviour)mb.gameObject.AddComponent(rt);
				Link(mb, twin);
				CopyState(mb, twin);
				_enableFix.Add(new KeyValuePair<Behaviour, bool>(twin, enabledBefore));
				Retire(mb);
				return twin;
			}

			object o = typeof(ScriptableObject).IsAssignableFrom(rt) ? (object)ScriptableObject.CreateInstance(rt)
			                                                          : FormatterServices.GetUninitializedObject(rt);
			Link(original, o);
			if (o is UnityEngine.Object twinObj && original is UnityEngine.Object origObj)
			{
				twinObj.name = origObj.name; // code often keys on asset names
				// Twins are referenced from plain C# objects and statics, which Unity's asset GC cannot see: without this flag every
				// scene change destroys them and `twin == null` turns true (Play(...) returns null, lookups come back empty, ...).
				twinObj.hideFlags |= HideFlags.DontUnloadUnusedAsset;
			}
			CopyState(original, o);
			return o;
		}

		static void Link(object original, object twin)
		{
			Pairs.Remove(original); Pairs.Add(original, twin);
			Reverse.Remove(twin); Reverse.Add(twin, original);
		}

		// Originals that are still waiting for their Awake must stay alive until Unity has called it: that call is what tells us
		// where this object sits in Unity's execution order (see SwapPrefix). They are disabled at once and destroyed afterwards.
		static readonly List<MonoBehaviour> _toDestroy = new List<MonoBehaviour>();

		static void Retire(MonoBehaviour orig)
		{
			if (orig == null) return;
			// Deliberately NOT touching orig.enabled: disabling a component before Unity has called its Awake can make Unity skip that
			// Awake, and the Awake call is how we learn this object's turn in Unity's execution order. All of the original's Unity
			// messages are suppressed anyway, so leaving it enabled changes nothing it can do.
			if (orig.gameObject.activeInHierarchy) _toDestroy.Add(orig);
			else UnityEngine.Object.DestroyImmediate(orig); // inactive: its Awake cannot run, nothing to wait for
		}

		static void DestroyRetired()
		{
			if (_toDestroy.Count == 0) return;
			foreach (var o in _toDestroy.ToList()) if (o != null) UnityEngine.Object.Destroy(o);
			_toDestroy.Clear();
		}

		/// A component that lives on a prefab asset: clone the prefab into a hidden inactive container, swap the originals on
		/// the CLONE for twins, and hand out the clone's twin. Instantiating it later gives a hierarchy with only replacements.
		static object PrefabTwin(MonoBehaviour src)
		{
			var root = src.transform.root.gameObject;
			if (!PrefabMaps.TryGetValue(root, out var map))
			{
				if (_prefabContainer == null)
				{
					var g = new GameObject(""[ReplacementPrefabs]"");
					g.SetActive(false);
					UnityEngine.Object.DontDestroyOnLoad(g);
					_prefabContainer = g.transform;
				}
				var clone = UnityEngine.Object.Instantiate(root, _prefabContainer);
				clone.name = root.name;
				var a = root.GetComponentsInChildren<Component>(true);
				var b = clone.GetComponentsInChildren<Component>(true);
				if (a.Length != b.Length) Log.LogWarning($""Prefab clone of '{root.name}' has a different component count ({a.Length} vs {b.Length})."");
				map = new Dictionary<Component, Component>();
				PrefabMaps[root] = map; // register first: references can loop back here
				for (int i = 0; i < Math.Min(a.Length, b.Length); i++) if (a[i] != null) map[a[i]] = b[i];

				foreach (var kv in map.ToList())
				{
					// NB: clone originals are destroyed as soon as they are swapped, so never test them with Unity's == null.
					var dst = kv.Value;
					if (ReferenceEquals(dst, null) || !(dst is MonoBehaviour) || !OrigToRepl.ContainsKey(dst.GetType())) continue;
					var twin = GetOrCreateReplacement(dst); // returns the existing twin if dst was already swapped via a reference
					if (twin != null) { Link(kv.Key, twin); PrefabSource.Remove(twin); PrefabSource.Add(twin, kv.Key); }
				}
				RetargetHierarchy(clone.transform);
				if (Trace) Log.LogInfo($""Built replacement prefab for '{root.name}' ({map.Count} components)."");
			}
			if (Pairs.TryGetValue(src, out var done)) return done;
			// not linked yet (e.g. requested while the clone was still being built): resolve through the clone's original
			if (map.TryGetValue(src, out var d) && !ReferenceEquals(d, null))
			{
				var result = GetOrCreateReplacement(d);
				if (result != null) { Link(src, result); return result; }
			}
			Log.LogWarning($""Prefab '{root.name}': no twin could be made for {src.GetType().Name}."");
			return null;
		}

		// ===================================================================== state copy

		static bool IsEngineOrSystem(Type t) =>
			t == null || t == typeof(object) || (t.Namespace != null && (t.Namespace.StartsWith(""UnityEngine"") || t.Namespace.StartsWith(""System"")));

		static void CopyState(object from, object to)
		{
			var fromType = from.GetType();
			for (var t = to.GetType(); !IsEngineOrSystem(t); t = t.BaseType)
			{
				foreach (var tf in t.GetFields(InstanceDeclared))
				{
					var ff = FindField(fromType, tf.Name);
					if (ff == null) continue;
					try
					{
						var val = ff.GetValue(from);
						var conv = ToReplacement(val);
						if (val != null && conv == null && !(val is Delegate) && !(val is UnityEngine.Object dead && !dead))
							Log.LogWarning($""{t.Name}.{tf.Name}: {val.GetType().Name} '{(val as UnityEngine.Object)?.name}' could not be turned into a replacement (null)."");
						tf.SetValue(to, conv);
					}
					catch (Exception e) { Log.LogWarning($""Could not copy {t.Name}.{tf.Name}: {e.Message}""); }
				}
			}
		}

		static FieldInfo FindField(Type t, string name)
		{
			for (; t != null; t = t.BaseType)
			{
				var f = t.GetField(name, InstanceDeclared);
				if (f != null) return f;
			}
			return null;
		}

		static object ConvertValue(object v, Type target)
		{
			if (v.GetType().IsEnum) return Enum.ToObject(target, v);
			var o = FormatterServices.GetUninitializedObject(target);
			CopyState(v, o);
			return o;
		}

		/// Original value -> value usable by replacement code. Anything not defined by the game is passed through untouched.
		static object ToReplacement(object v)
		{
			if (v == null) return null;
			var t = v.GetType();

			if (OrigToRepl.TryGetValue(t, out var rt))
			{
				if (t.IsValueType) return ConvertValue(v, rt);
				if (typeof(Delegate).IsAssignableFrom(t)) return null; // handlers are re-subscribed by replacement code
				if (Pairs.TryGetValue(v, out var existing)) return existing; // destroyed originals still resolve to their twin
				if (v is UnityEngine.Object uo && !uo) return null;
				return GetOrCreateReplacement(v);
			}

			if (t.IsArray && Map(t) != t)
			{
				var src = (Array)v;
				var dst = Array.CreateInstance(Map(t.GetElementType()), src.Length);
				for (int i = 0; i < src.Length; i++) dst.SetValue(ToReplacement(src.GetValue(i)), i);
				return dst;
			}
			if (t.IsGenericType && Map(t) != t)
			{
				if (v is IDictionary sd)
				{
					var dd = (IDictionary)Activator.CreateInstance(Map(t));
					foreach (DictionaryEntry e in sd) dd.Add(ToReplacement(e.Key), ToReplacement(e.Value));
					return dd;
				}
				if (v is IList sl)
				{
					var dl = (IList)Activator.CreateInstance(Map(t));
					foreach (var item in sl) dl.Add(ToReplacement(item));
					return dl;
				}
				Log.LogWarning($""Unsupported container {t} holds game types; left unconverted."");
			}
			return v;
		}

		static object ToOriginal(object v)
		{
			if (v == null) return null;
			var t = v.GetType();
			if (t.IsValueType && ReplToOrig.TryGetValue(t, out var ot)) return ConvertValue(v, ot);
			return Reverse.TryGetValue(v, out var o) ? o : v;
		}
	}
}
";
    }
}