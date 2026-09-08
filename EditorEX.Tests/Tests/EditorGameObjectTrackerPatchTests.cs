using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using EditorEX.Tests.Harness;
using HarmonyLib;
using Xunit;

namespace EditorEX.Tests.Tests
{
    // The editor recycles beatmap object views through Zenject pools, so a view that is not
    // pulled out of its Heck tracks on despawn stays in them forever and gets handed to the
    // next note that reuses it. Vivify's CullingCameraController walks every tracked object
    // every frame, so the leak shows up as a per-frame layer-setting cost that grows without
    // bound. These tests pin the transpiled IL: track bookkeeping has to be adjacent to the
    // pool call it mirrors, not appended at the end of the method.
    public class EditorGameObjectTrackerPatchTests
    {
        private static readonly Type _trackerType = Type.GetType(
            "EditorEX.Heck.Patches.EditorGameObjectTracker, EditorEX",
            throwOnError: true
        )!;

        [Fact]
        public void TrackRemovalPrecedesEveryPoolDespawn()
        {
            int despawns = 0;

            foreach (TranspiledMethod patched in TranspiledMethods())
            {
                List<CodeInstruction> il = patched.Result;
                for (int i = 0; i < il.Count; i++)
                {
                    if (!IsPoolDespawn(il[i]))
                    {
                        continue;
                    }

                    despawns++;
                    Assert.True(
                        i > 0 && IsTrackerCall(il[i - 1], "RemoveObject"),
                        $"{patched.Describe()}: pool Despawn at [{i}] is not immediately preceded "
                            + "by EditorGameObjectTracker.RemoveObject, so the view keeps its tracks "
                            + "after going back into the pool."
                    );
                }
            }

            Assert.Equal(10, despawns);
        }

        [Fact]
        public void TrackAdditionPrecedesEveryViewRegistration()
        {
            int registrations = 0;

            foreach (TranspiledMethod patched in TranspiledMethods())
            {
                List<CodeInstruction> il = patched.Result;
                for (int i = 0; i < il.Count; i++)
                {
                    if (!IsViewDictionaryAdd(il[i]))
                    {
                        continue;
                    }

                    registrations++;
                    Assert.True(
                        i > 0 && IsTrackerCall(il[i - 1], "AddObject"),
                        $"{patched.Describe()}: view registration at [{i}] is not immediately "
                            + "preceded by EditorGameObjectTracker.AddObject."
                    );
                }
            }

            Assert.Equal(5, registrations);
        }

        private static IEnumerable<TranspiledMethod> TranspiledMethods()
        {
            object instance = RuntimeHelpers.GetUninitializedObject(_trackerType);

            foreach (
                MethodInfo transpiler in _trackerType.GetMethods(
                    BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                )
            )
            {
                List<Attribute> attributes = transpiler.GetCustomAttributes().ToList();
                if (attributes.All(n => n.GetType().Name != "AffinityTranspilerAttribute"))
                {
                    continue;
                }

                foreach (
                    Attribute patch in attributes.Where(n =>
                        n.GetType().Name == "AffinityPatchAttribute"
                    )
                )
                {
                    Type declaringType = (Type)Property(patch, "DeclaringType")!;
                    string methodName = (string)Property(patch, "MethodName")!;
                    // `params Type[]` means "no overload hint" arrives as an empty array.
                    Type[]? argumentTypes = (Type[]?)Property(patch, "ArgumentTypes") is
                    {
                        Length: > 0
                    } declared
                        ? declared
                        : null;
                    MethodInfo? target =
                        argumentTypes == null
                            ? declaringType.GetMethod(
                                methodName,
                                BindingFlags.Instance
                                    | BindingFlags.Static
                                    | BindingFlags.Public
                                    | BindingFlags.NonPublic
                            )
                            : declaringType.GetMethod(
                                methodName,
                                BindingFlags.Instance
                                    | BindingFlags.Static
                                    | BindingFlags.Public
                                    | BindingFlags.NonPublic,
                                null,
                                argumentTypes,
                                null
                            );
                    Assert.True(
                        target != null,
                        $"could not resolve {declaringType?.FullName}.{methodName} "
                            + $"(args: {(argumentTypes == null ? "<null>" : string.Join(",", argumentTypes.Select(n => n.Name)))}) "
                            + $"for transpiler {transpiler.Name}"
                    );

                    List<CodeInstruction> original = MethodIlReader.Read(target!);
                    IEnumerable<CodeInstruction> result = (IEnumerable<CodeInstruction>)
                        transpiler.Invoke(
                            transpiler.IsStatic ? null : instance,
                            new object[] { original }
                        )!;

                    yield return new TranspiledMethod(target, result.ToList());
                }
            }
        }

        private static object? Property(object instance, string name) =>
            instance
                .GetType()
                .GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(instance);

        private static bool IsPoolDespawn(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Callvirt
            && instruction.operand is MethodInfo { Name: "Despawn" };

        // The object views keep their spawned views in a Dictionary<BeatmapEditorObjectId, TView>.
        private static bool IsViewDictionaryAdd(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Callvirt
            && instruction.operand is MethodInfo { Name: "Add" } method
            && method.DeclaringType is { IsGenericType: true } type
            && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        private static bool IsTrackerCall(CodeInstruction instruction, string name) =>
            instruction.opcode == OpCodes.Call
            && instruction.operand is MethodInfo method
            && method.DeclaringType == _trackerType
            && method.Name == name;

        private sealed class TranspiledMethod
        {
            internal TranspiledMethod(MethodInfo target, List<CodeInstruction> result)
            {
                Target = target;
                Result = result;
            }

            internal MethodInfo Target { get; }

            internal List<CodeInstruction> Result { get; }

            internal string Describe() => $"{Target.DeclaringType!.Name}.{Target.Name}";
        }
    }
}
