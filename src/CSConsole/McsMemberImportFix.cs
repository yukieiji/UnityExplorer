#if CPP
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Mono.CSharp;
using Mono.CSharp.IL2CPP;
using UnityExplorer.Runtime;

namespace UnityExplorer.CSConsole
{
    /// <summary>
    /// Runtime fix for the C# console's member import (Mono.CSharp.ImportedTypeDefinition.LoadMembers).
    ///
    /// Background: In IL2CPP games (MelonLoader/Il2CppInterop), importing the members of a type like
    /// <c>UnityEngine.Mesh</c> is fragile for two reasons:
    ///
    ///  1) The import was all-or-nothing per type: if a single member threw during import, the whole
    ///     LoadMembers() aborted, leaving a partially-populated, permanently-frozen MemberCache.
    ///     Members processed after the abort point were missing forever. Because methods are processed
    ///     before properties, ALL properties (mesh.vertices, mesh.vertexCount, ...) ended up missing
    ///     (CS1061) and whether a given method (mesh.get_vertices()) survived depended on where the
    ///     abort happened - the observed "sometimes works, sometimes CS1061, decided at game start".
    ///
    ///  2) Properties/events are linked to their accessor methods by *reference identity* against the
    ///     MethodInfo list returned by GetMembers(). When the interop reflection returns a different
    ///     MethodInfo instance for the getter (or none at all), the property is silently dropped.
    ///
    /// This patch replaces LoadMembers with a robust equivalent that:
    ///   * imports methods first (so accessor lookup always has the full method dictionary, regardless
    ///     of GetMembers() ordering),
    ///   * never aborts the whole type on a single bad member - it skips only that member,
    ///   * falls back to matching property/event accessors by name ("get_X" / "set_X" / "add_X" /
    ///     "remove_X") when the reference lookup fails.
    ///
    ///  3) IL2CPP interop reports <c>Missing</c> as the raw default value of many optional
    ///     parameters while still marking them <c>HasDefault</c>. mcs then calls
    ///     <c>ImportConstant(Missing)</c>, which throws <c>NotImplementedException</c> and drops
    ///     the whole method (UnityEngine.Mesh.SetSubMesh, SetVertexBufferData, ...). A prefix on
    ///     <c>MetadataImporter.CreateParameters</c> repairs the ParameterInfo the importer sees so
    ///     it falls back to <c>default(T)</c> instead.
    ///
    /// This file is compiled only for IL2CPP builds (<c>CPP</c>); Mono keeps the original import
    /// path untouched. For well-behaved types the result is identical to the original implementation.
    /// </summary>
    internal static class McsMemberImportFix
    {
        static readonly HarmonyLib.Harmony harmony = new($"{ExplorerCore.GUID}.mcsmemberimport");

        static bool applied;

        // Resolved lazily (and defensively) when the patch is first applied.
        static Type importedTypeDefType;
        static MethodInfo loadMembersMethod;
        static FieldInfo importerField;
        static FieldInfo providerField;

        // mcs is compiled into this assembly after ILRepack, so resolve its types from the
        // already-loaded assembly instead of AccessTools.TypeByName (which scans every loaded
        // assembly and makes HarmonyX log a ReflectionTypeLoadException for each broken Unity
        // module on startup).
        static readonly Assembly McsAssembly = typeof(Evaluator).Assembly;

        internal static void Apply()
        {
            if (applied)
                return;
            applied = true;

            try
            {
                importedTypeDefType = McsAssembly.GetType("Mono.CSharp.ImportedTypeDefinition", false);
                if (importedTypeDefType == null)
                {
                    ExplorerCore.LogWarning("McsMemberImportFix: could not find Mono.CSharp.ImportedTypeDefinition, skipping patch.");
                    return;
                }

                Type importedDefinition = McsAssembly.GetType("Mono.CSharp.ImportedDefinition", false);
                importerField = importedDefinition == null ? null : AccessTools.Field(importedDefinition, "importer");
                providerField = importedDefinition == null ? null : AccessTools.Field(importedDefinition, "provider");

                loadMembersMethod = AccessTools.Method(importedTypeDefType, "LoadMembers",
                    new[] { typeof(TypeSpec), typeof(bool), typeof(MemberCache).MakeByRefType() });

                if (loadMembersMethod == null)
                {
                    ExplorerCore.LogWarning("McsMemberImportFix: could not find ImportedTypeDefinition.LoadMembers, skipping patch.");
                    return;
                }

                harmony.Patch(loadMembersMethod,
                    prefix: new HarmonyMethod(AccessTools.Method(typeof(McsMemberImportFix), nameof(LoadMembersPrefix))));

                // IL2CPP interop reports Missing.Value as the raw default of optional parameters
                // while still setting HasDefault. Repair those parameters before mcs tries to
                // import them, otherwise ImportConstant(Missing) drops the whole method.
                Type metadataImporterType = McsAssembly.GetType("Mono.CSharp.MetadataImporter", false);
                MethodInfo createParameters = metadataImporterType == null
                    ? null
                    : AccessTools.Method(metadataImporterType, "CreateParameters",
                        new[] { typeof(TypeSpec), typeof(ParameterInfo[]), typeof(MethodBase) });

                if (createParameters != null)
                {
                    harmony.Patch(createParameters,
                        prefix: new HarmonyMethod(AccessTools.Method(typeof(McsMemberImportFix), nameof(SanitizeParameters))));
                    ExplorerCore.Log("McsMemberImportFix: patched MetadataImporter.CreateParameters to repair IL2CPP 'Missing' optional defaults.");
                }
                else
                {
                    ExplorerCore.LogWarning("McsMemberImportFix: could not find MetadataImporter.CreateParameters; optional parameters with 'Missing' defaults may still be dropped.");
                }

                ExplorerCore.Log($"McsMemberImportFix: patched {loadMembersMethod.DeclaringType?.FullName}.{loadMembersMethod.Name} for robust member import.");
            }
            catch (Exception ex)
            {
                ExplorerCore.LogWarning($"McsMemberImportFix: failed to apply patch: {ex}");
            }
        }

        /// <summary>
        /// Returns false to skip the original (we fully handled it), true to let the original run.
        /// On any unexpected error we reset the cache and fall back to the original behaviour.
        /// </summary>
        private static bool LoadMembersPrefix(object __instance, ref MemberCache cache, TypeSpec declaringType, bool onlyTypes)
        {
            try
            {
                var importer = (MetadataImporter)importerField.GetValue(__instance);
                var type = (Type)providerField.GetValue(__instance);

                LoadMembersRobust(importer, type, ref cache, declaringType, onlyTypes);
                return false; // fully handled -> skip the original
            }
            catch (Exception ex)
            {
                // Never let the fix break the console: fall back to the original, which rebuilds cleanly.
                ExplorerCore.LogWarning($"McsMemberImportFix: LoadMembers fix failed for `{declaringType.GetSignatureForError()}`, falling back to original: {ex}");
                cache = null;
                return true; // let the original run
            }
        }

        private static void LoadMembersRobust(MetadataImporter importer, Type type, ref MemberCache cache, TypeSpec declaringType, bool onlyTypes)
        {
            if (declaringType.IsPrivate && importer.IgnorePrivateMembers)
            {
                cache = MemberCache.Empty;
                return;
            }

            MemberInfo[] members = type.GetMembers(
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic);

            if (cache == null)
            {
                cache = new MemberCache(members.Length);

                // nested types (must be added before anything else)
                foreach (MemberInfo mi in members)
                {
                    if (mi.MemberType != MemberTypes.NestedType)
                        continue;
                    Type nested = (Type)mi;
                    if ((nested.Attributes & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedPrivate && importer.IgnorePrivateMembers)
                        continue;
                    try
                    {
                        MemberSpec spec = importer.CreateNestedType(nested, declaringType);
                        if (spec != null)
                            cache.AddMemberImported(spec);
                    }
                    catch (Exception e)
                    {
                        // FIX: skip a single bad nested type instead of aborting the whole type import.
                        LogSkipped(declaringType, nested, e);
                    }
                }

                // import base types of nested types
                foreach (MemberInfo mi in members)
                {
                    if (mi.MemberType != MemberTypes.NestedType)
                        continue;
                    Type nested = (Type)mi;
                    if ((nested.Attributes & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedPrivate && importer.IgnorePrivateMembers)
                        continue;
                    try
                    {
                        importer.ImportTypeBase(nested);
                    }
                    catch (Exception e)
                    {
                        LogSkipped(declaringType, nested, e);
                    }
                }
            }

            if (declaringType.IsInterface && declaringType.Interfaces != null)
            {
                foreach (TypeSpec iface in declaringType.Interfaces)
                    cache.AddInterface(iface);
            }

            if (onlyTypes)
                return;

            var dictionary = new Dictionary<MethodBase, MethodSpec>(MethodBaseReferenceComparer.Instance);
            var pendingEvents = new List<EventSpec>();

            // ---- PASS 1: methods first ----
            // Ensures the accessor dictionary is complete before we try to link
            // properties/events to their get_/set_/add_/remove_ methods, no matter what
            // order GetMembers() returns them in.
            foreach (MemberInfo mi in members)
            {
                if (mi.MemberType != MemberTypes.Method)
                    continue;
                try
                {
                    ImportMethod(importer, (MethodBase)mi, declaringType, dictionary, cache, members);
                }
                catch (Exception e)
                {
                    LogSkipped(declaringType, mi, e);
                }
            }

            // ---- PASS 2: everything else, preserving the original relative order ----
            foreach (MemberInfo mi in members)
            {
                if (mi.MemberType == MemberTypes.Method || mi.MemberType == MemberTypes.NestedType)
                    continue;

                if (IsMemberBlacklisted(declaringType, mi))
                    continue;

                try
                {
                    switch (mi.MemberType)
                    {
                        case MemberTypes.Constructor:
                        {
                            if (declaringType.IsInterface)
                                continue;
                            ImportMethod(importer, (MethodBase)mi, declaringType, dictionary, cache, members);
                            break;
                        }

                        case MemberTypes.Event:
                        {
                            if (dictionary.Count == 0)
                                continue;
                            var ei = (EventInfo)mi;
                            MethodSpec addSpec = FindAccessor(ei.GetAddMethod(true), "add_" + ei.Name, dictionary, members);
                            MethodSpec removeSpec = FindAccessor(ei.GetRemoveMethod(true), "remove_" + ei.Name, dictionary, members);
                            if (addSpec == null || removeSpec == null)
                                continue;
                            EventSpec es = importer.CreateEvent(ei, declaringType, addSpec, removeSpec);
                            if (!importer.IgnorePrivateMembers)
                                pendingEvents.Add(es);
                            if (!es.IsStatic || !declaringType.IsInterface)
                                cache.AddMemberImported(es);
                            break;
                        }

                        case MemberTypes.Field:
                        {
                            var fi = (FieldInfo)mi;
                            MemberSpec spec = importer.CreateField(fi, declaringType);
                            if (spec == null)
                                continue;

                            // try to link an event backing field
                            bool matched = false;
                            for (int i = 0; i < pendingEvents.Count; i++)
                            {
                                if (pendingEvents[i].Name == fi.Name)
                                {
                                    pendingEvents[i].BackingField = (FieldSpec)spec;
                                    pendingEvents.RemoveAt(i);
                                    matched = true;
                                    break;
                                }
                            }
                            if (matched)
                                continue;

                            if (!spec.IsStatic || !declaringType.IsInterface)
                                cache.AddMemberImported(spec);
                            break;
                        }

                        case MemberTypes.Property:
                        {
                            if (dictionary.Count == 0)
                                continue;
                            var pi = (PropertyInfo)mi;
                            MethodSpec getSpec = FindAccessor(pi.GetGetMethod(true), "get_" + pi.Name, dictionary, members);
                            MethodSpec setSpec = FindAccessor(pi.GetSetMethod(true), "set_" + pi.Name, dictionary, members);
                            if (getSpec == null && setSpec == null)
                                continue;
                            MemberSpec spec = importer.CreateProperty(pi, declaringType, getSpec, setSpec);
                            if (spec == null)
                                continue;
                            if (!spec.IsStatic || !declaringType.IsInterface)
                                cache.AddMemberImported(spec);
                            break;
                        }

                        default:
                            // The original threw NotImplementedException here. We skip it so a
                            // single unexpected member type can't break the whole import.
                            continue;
                    }
                }
                catch (Exception e)
                {
                    // FIX: skip a single bad member instead of aborting the whole type import.
                    LogSkipped(declaringType, mi, e);
                }
            }
        }

        /// <summary>
        /// Faithful port of the original method/constructor import path (private-member filter,
        /// CreateMethod, dictionary registration for methods, cache registration).
        /// </summary>
        private static void ImportMethod(MetadataImporter importer, MethodBase methodBase, TypeSpec declaringType,
            Dictionary<MethodBase, MethodSpec> dictionary, MemberCache cache, MemberInfo[] allMembers)
        {
            if (IsMemberBlacklisted(declaringType, methodBase))
                return;

            MethodAttributes attributes = methodBase.Attributes;
            if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private &&
                (importer.IgnorePrivateMembers ||
                 (attributes & (MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot)) ==
                 (MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot) ||
                 MetadataImporter.HasAttribute(CustomAttributeData.GetCustomAttributes(methodBase), "CompilerGeneratedAttribute", MetadataImporter.CompilerServicesNamespace)))
            {
                return;
            }

            MemberSpec spec = importer.CreateMethod(methodBase, declaringType);
            if (spec == null)
                return;

            if (spec.Kind == MemberKind.Method && !spec.IsGeneric)
                dictionary.Add(methodBase, (MethodSpec)spec);

            if (!spec.IsStatic || !declaringType.IsInterface)
                cache.AddMemberImported(spec);
        }

        /// <summary>
        /// Blacklist check used by mcs plus UnityExplorer's own blacklist.
        /// The mcs-internal check (Mono.CSharp.IL2CPP.Blacklist) only matches simple type names, so
        /// nested-type entries such as "UnityEngine.Foo+Bar.Invoke" are missed. Applying
        /// UERuntimeHelper.IsBlacklisted (which uses FullName) makes sure those known-crash members
        /// are also skipped before we try to import them.
        /// </summary>
        private static bool IsMemberBlacklisted(TypeSpec declaringType, MemberInfo member)
        {
            try
            {
                if (Blacklist.IsBlacklisted(declaringType, member))
                    return true;
                if (UERuntimeHelper.IsBlacklisted(member))
                    return true;
            }
            catch
            {
                // if the blacklist machinery itself fails, don't let it break import
            }
            return false;
        }

        /// <summary>
        /// Look up an accessor MethodSpec. Tries reference-identity lookup first (matching the
        /// original behaviour), then falls back to a name-based lookup among the type's own
        /// declared members.
        /// </summary>
        private static MethodSpec FindAccessor(MethodInfo accessor, string fallbackName,
            Dictionary<MethodBase, MethodSpec> dictionary, MemberInfo[] allMembers)
        {
            if (accessor != null)
            {
                if (dictionary.TryGetValue(accessor, out MethodSpec found))
                    return found;
            }

            // FIX: name-based fallback - the interop reflection may hand back a different
            // MethodInfo instance (or none) for the accessor, so match by name instead.
            foreach (MemberInfo mi in allMembers)
            {
                if (mi.MemberType == MemberTypes.Method && mi.Name == fallbackName)
                {
                    if (dictionary.TryGetValue((MethodInfo)mi, out MethodSpec foundByName))
                        return foundByName;
                }
            }
            return null;
        }

        private static void LogSkipped(TypeSpec declaringType, MemberInfo member, Exception ex)
        {
            ExplorerCore.LogWarning(
                $"McsMemberImportFix: skipped importing `{declaringType.GetSignatureForError()}.{member.Name}` ({member.MemberType}): {ex.GetType().Name}");
        }

        /// <summary>
        /// Harmony prefix for MetadataImporter.CreateParameters.
        /// Replaces ParameterInfo instances whose default value is the IL2CPP "unknown" sentinel
        /// (<see cref="Missing"/>) with a wrapper that reports no default. mcs then takes its
        /// default(T) path (MetadataImporter line ~474) instead of ImportConstant(Missing), which
        /// would throw NotImplementedException and drop the entire method.
        /// </summary>
        private static void SanitizeParameters(ParameterInfo[] pi)
        {
            if (pi == null)
                return;

            for (int i = 0; i < pi.Length; i++)
            {
                ParameterInfo parameter = pi[i];
                if (parameter == null || !parameter.IsOptional)
                    continue;

                try
                {
                    if ((parameter.Attributes & ParameterAttributes.HasDefault) == 0)
                        continue;

                    if (parameter.RawDefaultValue is Missing)
                        pi[i] = new RepairedOptionalParameter(parameter);
                }
                catch
                {
                    // Best effort: leave the original parameter; mcs will skip the method as before.
                }
            }
        }

        /// <summary>
        /// ParameterInfo wrapper that removes HasDefault and reports no default value.
        /// Only the members the importer reads are overridden; the rest are pre-seeded through
        /// ParameterInfo's protected fields because IsOptional/IsIn/... are not virtual on .NET 6.
        /// </summary>
        private sealed class RepairedOptionalParameter : ParameterInfo
        {
            private readonly ParameterInfo _inner;

            public RepairedOptionalParameter(ParameterInfo inner)
            {
                _inner = inner;

                AttrsImpl = inner.Attributes & ~ParameterAttributes.HasDefault;
                ClassImpl = inner.ParameterType;
                DefaultValueImpl = null;
                MemberImpl = inner.Member;
                NameImpl = inner.Name;
                PositionImpl = inner.Position;
            }

            public override Type ParameterType => _inner.ParameterType;
            public override string Name => _inner.Name;
            public override int Position => _inner.Position;
            public override ParameterAttributes Attributes => _inner.Attributes & ~ParameterAttributes.HasDefault;
            public override MemberInfo Member => _inner.Member;
            public override object DefaultValue => null;
            public override object RawDefaultValue => null;
            public override bool HasDefaultValue => false;

            public override object[] GetCustomAttributes(bool inherit) => _inner.GetCustomAttributes(inherit);
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _inner.GetCustomAttributes(attributeType, inherit);
            public override bool IsDefined(Type attributeType, bool inherit) => _inner.IsDefined(attributeType, inherit);
            public override IList<CustomAttributeData> GetCustomAttributesData() => _inner.GetCustomAttributesData();
            public override Type[] GetOptionalCustomModifiers() => _inner.GetOptionalCustomModifiers();
            public override Type[] GetRequiredCustomModifiers() => _inner.GetRequiredCustomModifiers();
            public override string ToString() => _inner.ToString();
        }

        /// <summary>
        /// Reference-equality comparer matching Mono.CSharp's internal ReferenceEquality&lt;MethodBase&gt;.
        /// </summary>
        private sealed class MethodBaseReferenceComparer : IEqualityComparer<MethodBase>
        {
            public static readonly MethodBaseReferenceComparer Instance = new();

            public bool Equals(MethodBase x, MethodBase y)
                => ReferenceEquals(x, y);

            public int GetHashCode(MethodBase obj)
                => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
#endif
