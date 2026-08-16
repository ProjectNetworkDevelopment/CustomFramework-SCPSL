using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp3114;
using System;

namespace CustomFramework
{
    public static class Scp3114Util
    {
        public static void ForceAbortStrangle(this Scp3114Role role)
        {
            if (role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle call))
            {
                var syncTarget = AccessTools.Method(typeof(Scp3114Strangle), "set_SyncTarget");
                var rpcType = AccessTools.Field(typeof(Scp3114Strangle), "_rpcType");
                var rpcTypeEnum = rpcType.FieldType;
                var sendRpc = AccessTools.Method(typeof(Scp3114Strangle), "ServerSendRpc", new Type[] { typeof(bool) });

                syncTarget.Invoke(call, new object[] { null });
                rpcType.SetValue(call, Enum.Parse(rpcTypeEnum, "AttackInterrupted"));
                sendRpc.Invoke(call, new object[] { true });
            }
        }
    }
}
