using SiraUtil.Affinity;
using Zenject;

namespace EditorEX.SDK.Integration.Patches
{
    internal class AllowSignalInterfacesPatches : IAffinity
    {
        [AffinityPatch(
            typeof(SignalBus),
            nameof(SignalBus.GetDeclaration),
            AffinityMethodType.Normal,
            null,
            typeof(BindingId),
            typeof(bool)
        )]
        [AffinityPrefix]
        private bool GetDeclarationType(
            SignalBus __instance,
            BindingId signalId,
            bool requireDeclaration,
            ref SignalDeclaration __result
        )
        {
            if (
                SignalDeclarationLookup.TryGet(
                    __instance._localDeclarationMap,
                    signalId,
                    out SignalDeclaration? signalDeclaration
                )
            )
            {
                __result = signalDeclaration;
                return false;
            }

            if (__instance._parentBus != null)
            {
                __result = __instance._parentBus.GetDeclaration(signalId, requireDeclaration);
                return false;
            }

            if (requireDeclaration)
            {
                throw ModestTree.Assert.CreateException("Fired undeclared signal '{0}'!", signalId);
            }

            __result = null;
            return false;
        }
    }
}
