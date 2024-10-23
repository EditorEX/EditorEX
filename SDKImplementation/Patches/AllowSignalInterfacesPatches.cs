using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace EditorEX.SDKImplementation.Patches
{
    internal class AllowSignalInterfacesPatches : IAffinity
    {
        [AffinityPatch(typeof(BindingId), "op_Equality")]
        [AffinityPrefix]
        private bool EqualityPatch(BindingId left, BindingId right, ref bool __result)
        {
            __result = (left.Type == right.Type || left.Type.IsAssignableFrom(right.Type) || right.Type.IsAssignableFrom(left.Type)) && Equals(left.Identifier, right.Identifier);
            return false;
        }

        [AffinityPatch(typeof(SignalBus), nameof(SignalBus.GetDeclaration), AffinityMethodType.Normal, null, new Type[] { typeof(BindingId), typeof(bool) })]
        [AffinityPrefix]
        private bool GetDeclarationType(SignalBus __instance, BindingId signalId, bool requireDeclaration, ref SignalDeclaration __result)
        {
            KeyValuePair<BindingId, SignalDeclaration> result = __instance._localDeclarationMap.FirstOrDefault((KeyValuePair<BindingId, SignalDeclaration> x) => x.Key == signalId);

            if (__instance._localDeclarationMap.TryGetValue(signalId, out var signalDeclaration))
            {
                __result = signalDeclaration;
                return false;
            }

            if (__instance._parentBus != null)
            {
                __result = __instance._parentBus.GetDeclaration(signalId, requireDeclaration);
                return false;
            }

            if (result.Value != null)
            {
                __result = result.Value;
                return false;
            }

            if (requireDeclaration)
            {
                throw ModestTree.Assert.CreateException("Fired undeclared signal '{0}'!", new object[] { signalId });
            }

            __result = null;
            return false;
        }
    }
}
