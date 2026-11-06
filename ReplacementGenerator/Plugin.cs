using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReplacementGenerator
{
    [BepInPlugin("dev.replacement.generator", "Replacement Generator", "0.1.0")]
    public class GeneratorPlugin : BaseUnityPlugin
    {
        const string TargetAssembly = "Assembly-CSharp";

        ConfigEntry<string> _outDir;
        ConfigEntry<bool> _regenerate;
        ConfigEntry<bool> _verbose;
        ConfigEntry<bool> _skipIfPresent;
        ConfigEntry<string> _entryClass;
        ConfigEntry<string> _ignorePatterns;
        ConfigEntry<bool> _autoIgnoreDeps;
        List<Type> _ignored = new List<Type>();
        ConfigEntry<string> _entryPoints;
        ConfigEntry<string> _namespace;
        ConfigEntry<string> _projectName;
        string _ns;
        string _projectNameValue;
        string _projectDir;
        readonly HashSet<string> _dumpedScenes = new HashSet<string>();
        readonly StringBuilder _nodes = new StringBuilder();
        Assembly _target;
        List<Type> _roots;

        void Awake()
        {
            _outDir = Config.Bind("General", "OutputDirectory",
                Path.Combine(Paths.BepInExRootPath, "ReplacementOutput"),
                "Where the replacement project is generated.");
            _namespace = Config.Bind("General", "ReplacementNamespace", "Replacement",
                "Namespace the replacement classes are generated into. The runtime/redirector code goes in <namespace>.Runtime.");
            _projectName = Config.Bind("General", "ProjectName", "",
                "Name of the generated project folder/assembly. Empty = <GameProcessName>.Replacement (sanitised).");
            _ignorePatterns = Config.Bind("Filter", "IgnoreTypePatterns", "STM*,SuperTextMesh*,SteamManager",
                "Comma-separated class-name patterns (* and ? wildcards, case-sensitive) the generator leaves alone, e.g. third-party " +
                "libraries compiled into the game assembly. Ignored types are not decompiled or replaced and keep running as the original.");
            _autoIgnoreDeps = Config.Bind("Filter", "AutoIgnoreDependencies", true,
                "Also ignore every global type that an ignored type uses (fields, signatures, IL), transitively, so ignored library " +
                "code never ends up calling replaced code. Each addition is logged.");
            _entryClass = Config.Bind("Entry", "EntryClass", "Game",
                "Global-namespace class (typically the static 'Game' class) whose startup the replacement must take over.");
            _entryPoints = Config.Bind("Entry", "EntryPoints", "",
                "Comma-separated Type.Method list of ORIGINAL methods to redirect to the replacement (e.g. Game.Start). " +
                "Empty = auto-detect common startup names (Start/Initialize/Init/Awake/Boot/Begin/Run/Main) on EntryClass.");
            _skipIfPresent = Config.Bind("General", "SkipIfReplacementInstalled", true,
                "Do nothing if the generated replacement mod DLL is installed under BepInEx/plugins, so you can leave both installed.");
            _regenerate = Config.Bind("General", "RegenerateSources", false,
                "Overwrite existing generated sources (WILL destroy your edits to them!).");

            _verbose = Config.Bind("Debug", "VerboseLogging", false,
                "Log every type/scene/node-dump step at Info level (otherwise only summaries).");
            _ns = string.IsNullOrWhiteSpace(_namespace.Value) ? "Replacement" : _namespace.Value.Trim();
            _projectNameValue = Sanitize(string.IsNullOrWhiteSpace(_projectName.Value)
                ? Paths.ProcessName + ".Replacement" : _projectName.Value.Trim());
            Logger.LogInfo($"Replacement Generator starting. Config file: {Config.ConfigFilePath}");
            Logger.LogInfo($"Output dir: {_outDir.Value}, project: {_projectNameValue}, namespace: {_ns}, regenerate={_regenerate.Value}, verbose={_verbose.Value}");

            bool skipGeneration = false;
            if (_skipIfPresent.Value)
            {
                string existing = null;
                try { existing = Directory.GetFiles(Paths.PluginPath, _projectNameValue + ".dll", SearchOption.AllDirectories).FirstOrDefault(); }
                catch (Exception e) { Logger.LogWarning("Could not scan plugins folder for the replacement mod: " + e.Message); }
                if (existing != null)
                {
                    Logger.LogWarning($"Replacement mod is installed ({existing}); source generation is skipped for this run " +
                                      "(only the idempotent [System.Serializable] fix-up of existing files still runs). " +
                                      "Set SkipIfReplacementInstalled=false or remove that DLL to regenerate.");
                    skipGeneration = true;
                }
            }

            _target = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == TargetAssembly);
            if (_target == null) { Logger.LogError(TargetAssembly + " is not loaded; generator aborted."); return; }
            Logger.LogInfo($"Target assembly: {_target.FullName} ({_target.Location})");

            _projectDir = Path.Combine(_outDir.Value, _projectNameValue);
            Directory.CreateDirectory(_projectDir);
            Logger.LogInfo("Project directory: " + _projectDir);

            // Top-level, global-namespace, non-compiler-generated types only.
            var candidates = _target.GetTypes()
                .Where(t => string.IsNullOrEmpty(t.Namespace) && !t.IsNested
                            && !t.Name.StartsWith("<")
                            && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)))
                .OrderBy(t => t.Name).ToList();

            var matchers = (_ignorePatterns.Value ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => s.Length > 0)
                .Select(s => new Regex("^" + Regex.Escape(s).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.CultureInvariant))
                .ToList();
            _ignored = candidates.Where(t => matchers.Any(m => m.IsMatch(t.Name))).ToList();
            if (_autoIgnoreDeps.Value && _ignored.Count > 0)
            {
                try
                {
                    var pulled = DependencyScanner.FindDependencies(_target.Location, _ignored.Select(t => t.Name),
                        new HashSet<string>(candidates.Select(t => t.Name)));
                    foreach (var kv in pulled) Logger.LogInfo($"Auto-ignoring '{kv.Key}' (used by ignored type '{kv.Value}').");
                    _ignored = candidates.Where(t => _ignored.Contains(t) || pulled.ContainsKey(t.Name)).ToList();
                }
                catch (Exception e) { Logger.LogWarning("Dependency scan of ignored types failed; add the helper classes to IgnoreTypePatterns by hand. " + e.Message); }
            }
            _roots = candidates.Except(_ignored).ToList();

            if (skipGeneration)
            {
                // Cheap, idempotent maintenance that must not wait for a full regeneration: make sure the existing sources carry
                // [System.Serializable] where the original types had it. Rebuild the replacement project afterwards to pick it up.
                int fixedNow = 0;
                var existingGen = Path.Combine(_projectDir, "Generated");
                foreach (var t in _roots)
                {
                    var f = Path.Combine(existingGen, Sanitize(t.Name) + ".cs");
                    if (File.Exists(f)) fixedNow += EnsureSerializable(f, t);
                }
                Logger.LogInfo(fixedNow > 0
                    ? $"Added [System.Serializable] to {fixedNow} type(s) in the existing generated sources. REBUILD the replacement project to apply it."
                    : "Existing generated sources already carry [System.Serializable] where needed.");
                return;
            }

            Logger.LogInfo($"Found {candidates.Count} global-namespace top-level types: {_roots.Count} to replace, " +
                           $"{_ignored.Count} ignored by pattern [{_ignorePatterns.Value}] ({_target.GetTypes().Length} types total in assembly).");
            if (_ignored.Count > 0) Logger.LogInfo("Ignored types: " + string.Join(", ", _ignored.Select(t => t.Name)));

            try
            {
                var sw = Stopwatch.StartNew();
                GenerateSources();
                GenerateProjectFiles();
                Logger.LogInfo($"Generation finished in {sw.Elapsed.TotalSeconds:F1}s -> {_projectDir}");
            }
            catch (Exception e) { Logger.LogError("Generation failed: " + e); }

            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogInfo("Subscribed to scene loads; node map will be written to nodes.txt as scenes load.");
            try { DumpResources(); } catch (Exception e) { Logger.LogError("Resource dump failed: " + e); }
        }

        // ---------- source generation ----------

        void GenerateSources()
        {
            var genDir = Path.Combine(_projectDir, "Generated");
            Directory.CreateDirectory(genDir);

            var settings = new DecompilerSettings(LanguageVersion.CSharp7_3)
            {
                ThrowOnAssemblyResolveErrors = false,
                UsingDeclarations = true,
                ShowXmlDocumentation = false,
            };
            var resolver = new UniversalAssemblyResolver(_target.Location, false, ".NETFramework,Version=v4.7.2");
            resolver.AddSearchDirectory(Path.GetDirectoryName(_target.Location));
            var decompiler = new CSharpDecompiler(_target.Location, resolver, settings);
            Logger.LogInfo("Decompiler initialised; decompiling types into " + genDir);

            var guardDir = Path.Combine(genDir, "Guards");
            Directory.CreateDirectory(guardDir);

            // Output from earlier runs for types that are now ignored would shadow the original type inside the replacement
            // namespace, so remove it.
            foreach (var t in _ignored)
            {
                foreach (var stale in new[] { Path.Combine(genDir, Sanitize(t.Name) + ".cs"), Path.Combine(guardDir, Sanitize(t.Name) + ".Guard.cs") })
                    if (File.Exists(stale)) { File.Delete(stale); Logger.LogInfo("Removed stale generated file for ignored type: " + Path.GetFileName(stale)); }
            }

            int written = 0, skipped = 0, failed = 0, index = 0, serializableFixed = 0;
            foreach (var t in _roots)
            {
                index++;
                if (!t.IsGenericTypeDefinition)
                    File.WriteAllText(Path.Combine(guardDir, Sanitize(t.Name) + ".Guard.cs"), GuardSource(t)); // always regenerated
                var path = Path.Combine(genDir, Sanitize(t.Name) + ".cs");
                if (File.Exists(path) && !_regenerate.Value)
                {
                    skipped++;
                    Trace($"[{index}/{_roots.Count}] skip (exists): {t.Name}");
                    serializableFixed += EnsureSerializable(path, t); // the only edit made to existing files; idempotent
                    continue; // protect user edits
                }
                try
                {
                    var code = decompiler.DecompileTypeAsString(new FullTypeName(t.FullName));
                    File.WriteAllText(path, WrapInNamespace(code));
                    serializableFixed += EnsureSerializable(path, t);
                    written++;
                    Trace($"[{index}/{_roots.Count}] decompiled: {t.Name} ({code.Length} chars)");
                }
                catch (Exception e)
                {
                    failed++;
                    Logger.LogWarning($"Decompile failed for {t.FullName}: {e.Message}");
                    File.WriteAllText(path, $"// Decompilation failed for {t.FullName}: {e.Message}\n");
                }
            }
            Logger.LogInfo($"Sources: {written} written, {skipped} skipped (already exist), {failed} failed. " +
                           $"[System.Serializable] added to {serializableFixed} type(s).");
        }

        /// ILSpy does not emit [Serializable] (it is a type flag, not a real attribute), so a decompiled serializable class would no
        /// longer be serialized by Unity: its data survives our reflection copy on scene objects but is silently dropped whenever a
        /// replacement prefab is Instantiated. Adds [System.Serializable] to every class/struct that was serializable in the original.
        /// Safe to run on files you have edited: it only inserts that one line, and only where it is missing.
        int EnsureSerializable(string path, Type root)
        {
            string text;
            try { text = File.ReadAllText(path); } catch { return 0; }
            int added = 0;
            foreach (var t in SelfAndNested(root))
            {
                if (!t.IsSerializable || t.IsEnum || t.IsInterface || t.IsGenericTypeDefinition || typeof(Delegate).IsAssignableFrom(t)) continue;
                if (typeof(UnityEngine.Object).IsAssignableFrom(t) || (t.IsAbstract && t.IsSealed)) continue; // components/assets, static classes
                if (t.Name.StartsWith("<") || Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute))) continue;
                var rx = new Regex(@"(?m)^(?<indent>[ \t]*)(?:(?:public|internal|private|protected|sealed|abstract|unsafe|new|partial)\s+)*(?:class|struct)\s+" +
                                   Regex.Escape(t.Name) + @"(?![\w<])");
                var m = rx.Match(text);
                if (!m.Success) continue;
                var above = text.Substring(0, m.Index).Split('\n');
                bool already = above.Skip(Math.Max(0, above.Length - 7)).Any(l => l.Contains("Serializable")) ||
                               text.Substring(m.Index, Math.Min(m.Length, 120)).Contains("Serializable");
                if (already) continue;
                text = text.Insert(m.Index, m.Groups["indent"].Value + "[System.Serializable]\n");
                added++;
            }
            if (added > 0) File.WriteAllText(path, text);
            return added;
        }

        static IEnumerable<Type> SelfAndNested(Type t)
        {
            yield return t;
            foreach (var n in t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                foreach (var x in SelfAndNested(n)) yield return x;
        }

        /// Per-type guard: patches (via the shared Redirector) every original method that has no replacement to throw,
        /// so a call that escapes into original code is caught immediately. The type is referenced by name because
        /// many global types are internal and cannot be named with typeof() from another assembly.
        string GuardSource(Type t) =>
            "// <auto-generated> Guard for original type '" + t.Name + "'. Regenerated on every run; do not edit.\n" +
            "namespace " + _ns + "\n{\n" +
            "\tinternal static class " + Sanitize(t.Name) + "_OriginalGuard\n\t{\n" +
            "\t\tpublic const string OriginalType = \"" + t.Name + "\";\n" +
            "\t\tpublic static void Apply(HarmonyLib.Harmony harmony) => " + _ns + ".Runtime.Replacer.GuardOriginal(harmony, OriginalType);\n" +
            "\t}\n}\n";

        /// Keeps using-directives/attributes header at the top, wraps the rest in the replacement namespace.
        string WrapInNamespace(string code)
        {
            var lines = code.Replace("\r\n", "\n").Split('\n');
            int i = 0;
            var header = new StringBuilder();
            while (i < lines.Length && (lines[i].StartsWith("using ") || lines[i].Trim() == "" || lines[i].StartsWith("//")))
                header.AppendLine(lines[i++]);
            var body = string.Join("\n", lines.Skip(i));
            return header + "namespace " + _ns + "\n{\n" + Indent(body) + "\n}\n";
        }

        void Trace(string msg) { if (_verbose.Value) Logger.LogInfo(msg); else Logger.LogDebug(msg); }

        static string Indent(string s) => string.Join("\n", s.Split('\n').Select(l => l.Length == 0 ? l : "\t" + l));
        static string Sanitize(string n) => string.Concat(n.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));

        // ---------- project files ----------

        // Managed DLLs that the net472 targeting pack already provides, or that must never be referenced.
        static readonly string[] SkipAssemblyPrefixes =
        {
            "System", "mscorlib", "netstandard", "Mono.", "Microsoft.", "I18N", "ICSharpCode", "Boo.", "UnityScript", "Accessibility"
        };

        const string CsprojTemplate = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>latest</LangVersion>
    <AssemblyName>%NAME%</AssemblyName>
    <RootNamespace>%NAME%</RootNamespace>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <DebugType>embedded</DebugType>
    <NoWarn>CS0649;CS0169;CS0414;CS0108;CS0114;CS0219;CS0162;CS8981</NoWarn>
    <GameDir Condition=""'$(GameDir)' == ''"">%GAMEDIR%</GameDir>
    <RestoreAdditionalProjectSources>https://nuget.bepinex.dev/v3/index.json</RestoreAdditionalProjectSources>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""BepInEx.Core"" Version=""5.4.21"" IncludeAssets=""compile"" />
    <PackageReference Include=""Mono.Cecil"" Version=""0.11.4"" IncludeAssets=""compile"" />
  </ItemGroup>

  <ItemGroup>
%REFS%  </ItemGroup>

  <Target Name=""DeployToGame"" AfterTargets=""Build"" Condition=""Exists('$(GameDir)\BepInEx\plugins')"">
    <Copy SourceFiles=""$(OutDir)%NAME%.dll"" DestinationFolder=""$(GameDir)\BepInEx\plugins\%NAME%"" />
  </Target>

</Project>
";

        const string PluginTemplate = @"using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace %NS%.Runtime
{
	[BepInPlugin(""dev.replacement.%GUID%"", ""%NAME%"", ""0.1.0"")]
	public class Plugin : BaseUnityPlugin
	{
		static readonly string[] Types = new[] {
			%TYPES%
		};

		float _interval, _next;

		void Awake()
		{
			Replacer.Log = Logger;
			Replacer.ReplacementNamespace = ""%NS%"";
			Replacer.Trace = Config.Bind(""Debug"", ""Trace"", false, ""Log every swap / entry redirect (noisy)."").Value;
			Replacer.DiagnosticType = ""%ENTRYCLASS%"";
			var audit = Config.Bind(""Debug"", ""AuditTypes"", """",
				""Comma-separated ORIGINAL type names (e.g. ButtonBehavior,AudioManager). On each scene load the first few twins of each are compared field-by-field with their original and every difference is logged as [audit]."").Value
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
			var auditCount = Config.Bind(""Debug"", ""AuditPerType"", 2, ""How many twins of each AuditTypes type to audit per scene load."").Value;
			Replacer.AllowSurvivingCallers = Config.Bind(""Debug"", ""AllowSurvivingOriginalCallers"", true,
				""Strict guards let original code that deliberately survives (third-party libs, namespaced types) call original helpers; each is logged as a warning."").Value;

			var entry = Config.Bind(""General"", ""EntryPoints"", ""%ENTRY%"",
				""Comma-separated Type.Method ORIGINAL methods redirected to the replacement (e.g. Game.Start)."").Value;
			_interval = Config.Bind(""General"", ""SweepIntervalSeconds"", 1f,
				""How often to scan all live objects for original components still to be replaced. 0 = only on scene load."").Value;

			Logger.LogInfo(""%NAME% loading; %COUNT% top-level types, entry points: [""+ entry + ""]"");
			var harmony = new Harmony(Info.Metadata.GUID);
			Replacer.Install(harmony, Types, entry.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray());

			if (Config.Bind(""Debug"", ""StrictGuards"", true,
				""Make every remaining original method throw NotReplacedException if it is ever called (verifies nothing escapes)."").Value)
			{
%GUARDS%
				Logger.LogInfo(""Strict guards applied."");
			}

			SceneManager.sceneLoaded += (s, m) => { Replacer.RunAllPending(); Replacer.Sweep(""scene '"" + s.name + ""'""); if (Replacer.Trace) Replacer.DumpStatics(true); if (audit.Length > 0) Replacer.Audit(audit, auditCount); };
		}

		void Update()
		{
			Replacer.RunAllPending();
			if (_interval > 0f && Time.unscaledTime >= _next) { _next = Time.unscaledTime + _interval; Replacer.Sweep(null); }
		}
	}
}
";

        void GenerateProjectFiles()
        {
            var name = _projectNameValue;
            var managed = Paths.ManagedPath;

            // Reference every game/Unity assembly (Assembly-CSharp, UnityEngine.*Module, third-party libs...).
            var refs = new StringBuilder();
            int refCount = 0;
            foreach (var dll in Directory.GetFiles(managed, "*.dll").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var n = Path.GetFileNameWithoutExtension(dll);
                if (SkipAssemblyPrefixes.Any(p => n.StartsWith(p, StringComparison.OrdinalIgnoreCase))) continue;
                refCount++;
                refs.AppendLine($"    <Reference Include=\"{n}\"><HintPath>{dll}</HintPath><Private>false</Private></Reference>");
            }

            Logger.LogInfo($"Referencing {refCount} assemblies from {managed}");
            File.WriteAllText(Path.Combine(_projectDir, name + ".csproj"),
                CsprojTemplate
                    .Replace("%NAME%", name)
                    .Replace("%GAMEDIR%", Paths.GameRootPath)
                    .Replace("%REFS%", refs.ToString()));
            Logger.LogInfo("Wrote " + name + ".csproj");

            // Runtime redirector source lives in RedirectorTemplate.cs.
            var rtDir = Path.Combine(_projectDir, "Runtime");
            Directory.CreateDirectory(rtDir);
            var oldRuntime = Path.Combine(rtDir, "Redirector.cs"); // superseded by Replacer.cs
            if (File.Exists(oldRuntime)) { File.Delete(oldRuntime); Logger.LogInfo("Removed obsolete Runtime/Redirector.cs"); }
            File.WriteAllText(Path.Combine(rtDir, "Replacer.cs"),
                RedirectorTemplate.Source.Replace("namespace ReplacementRuntime", "namespace " + _ns + ".Runtime"));
            Logger.LogInfo("Wrote Runtime/Replacer.cs");

            // Plugin entry point (always regenerated; contains only the type list).
            var list = string.Join(",\n\t\t\t", _roots.Select(t => "\"" + t.Name + "\""));
            File.WriteAllText(Path.Combine(_projectDir, "Plugin.cs"),
                PluginTemplate
                    .Replace("%NS%", _ns)
                    .Replace("%NAME%", name)
                    .Replace("%GUID%", name.ToLowerInvariant())
                    .Replace("%GUARDS%", string.Join("\n", _roots.Where(t => !t.IsGenericTypeDefinition)
                        .Select(t => "\t\t\t\t" + Sanitize(t.Name) + "_OriginalGuard.Apply(harmony);")))
                    .Replace("%ENTRY%", DiscoverEntryPoints())
                    .Replace("%ENTRYCLASS%", _entryClass.Value)
                    .Replace("%TYPES%", list)
                    .Replace("%COUNT%", _roots.Count.ToString()));
            Logger.LogInfo($"Wrote Plugin.cs with {_roots.Count} types");
        }

        static readonly string[] EntryNames = { "Start", "Initialize", "Init", "Awake", "Boot", "Begin", "Run", "Main", "Startup" };

        /// Works out which ORIGINAL methods the replacement must take over, and logs the entry class's methods so you can
        /// adjust the EntryPoints setting if the auto-detection guessed wrong.
        string DiscoverEntryPoints()
        {
            if (!string.IsNullOrWhiteSpace(_entryPoints.Value))
            {
                Logger.LogInfo("Using configured EntryPoints: " + _entryPoints.Value);
                return _entryPoints.Value.Trim();
            }
            var t = _roots.FirstOrDefault(x => x.Name == _entryClass.Value);
            if (t == null)
            {
                Logger.LogWarning($"Entry class '{_entryClass.Value}' not found in the global namespace; no entry points redirected. Set Entry/EntryClass or Entry/EntryPoints.");
                return "";
            }
            const BindingFlags F = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var methods = t.GetMethods(F).Where(m => !m.Name.StartsWith("<") && !m.IsSpecialName).ToList();
            Logger.LogInfo($"Entry class {t.Name} (static={t.IsAbstract && t.IsSealed}) declares: " +
                           string.Join(", ", methods.Select(m => (m.IsStatic ? "static " : "") + m.Name).Distinct()));
            var picked = methods.Where(m => EntryNames.Contains(m.Name)).Select(m => t.Name + "." + m.Name).Distinct().ToList();
            Logger.LogInfo(picked.Count > 0 ? "Auto-detected entry points: " + string.Join(", ", picked)
                                            : "No startup-like method found on the entry class; set Entry/EntryPoints manually.");
            return string.Join(",", picked);
        }

        // ---------- node map ----------

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_dumpedScenes.Add(scene.name))
            {
                Trace($"Scene '{scene.name}' already dumped ({mode}); skipping.");
                return;
            }
            Logger.LogInfo($"Scene loaded: '{scene.name}' ({mode}); dumping nodes.");
            _nodes.AppendLine($"# scene: {scene.name}");
            foreach (var go in scene.GetRootGameObjects())
                DumpNode(go.transform, "", go.name);
            File.WriteAllText(Path.Combine(_projectDir, "nodes.txt"), _nodes.ToString());
            Logger.LogInfo($"nodes.txt updated ({_nodes.Length} chars, {_dumpedScenes.Count} scene(s)).");
        }

        void DumpNode(Transform t, string indent, string path)
        {
            _nodes.AppendLine($"{indent}{t.name}  [{path}] active={t.gameObject.activeSelf}");
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) continue;
                var ct = c.GetType();
                var global = ct.Assembly == _target && string.IsNullOrEmpty(ct.Namespace);
                _nodes.AppendLine($"{indent}  - {ct.FullName}{(global ? " (REPLACED)" : "")}");
                if (!global) continue;
                foreach (var f in AllFields(ct))
                {
                    object v; try { v = f.GetValue(c); } catch { v = "<err>"; }
                    _nodes.AppendLine($"{indent}      {f.FieldType.Name} {f.Name} = {Describe(v)}");
                }
            }
            foreach (Transform child in t) DumpNode(child, indent + "  ", path + "/" + child.name);
        }

        static IEnumerable<FieldInfo> AllFields(Type t)
        {
            const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (; t != null && t != typeof(MonoBehaviour) && t != typeof(object); t = t.BaseType)
                foreach (var f in t.GetFields(F))
                    if (f.IsPublic || Attribute.IsDefined(f, typeof(SerializeField))) yield return f;
        }

        static string Describe(object v)
        {
            if (v == null) return "null";
            if (v is UnityEngine.Object o) return o ? $"{o.GetType().Name}'{o.name}'" : "destroyed";
            if (v is string s) return "\"" + s + "\"";
            if (v is System.Collections.ICollection c) return $"{v.GetType().Name}[{c.Count}]";
            return v.ToString();
        }

        void DumpResources()
        {
            var sb = new StringBuilder();
            int n = 0;
            foreach (var o in Resources.LoadAll("", typeof(UnityEngine.Object)))
            { sb.AppendLine($"{o.GetType().FullName}\t{o.name}"); n++; }
            Logger.LogInfo($"resources.txt: {n} Resources assets.");
            File.WriteAllText(Path.Combine(_projectDir, "resources.txt"), sb.ToString());
        }
    }
}