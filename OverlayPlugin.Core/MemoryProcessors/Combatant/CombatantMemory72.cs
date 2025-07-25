﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RainbowMage.OverlayPlugin.MemoryProcessors.Combatant
{
    interface ICombatantMemory72 : ICombatantMemory { }

    class CombatantMemory72 : CombatantMemory, ICombatantMemory72
    {
        private const string charmapSignature = "488B5720B8000000E0483BD00F84????????488D0D";

        // TODO: Once all regions are on 7.2, remove the new methods for `GetEffectEntries` and `GetEffectEntryFromByteArray`
        // Remove the struct for `EffectMemory72`, and adjust the parent struct to have the correct size of 16 bytes.
        public CombatantMemory72(TinyIoCContainer container)
            : base(container, charmapSignature, CombatantMemory.Size, EffectMemory72.Size, 629)
        {

        }

        public override Version GetVersion()
        {
            return new Version(7, 2);
        }

        // Returns a combatant if the combatant is a mob or a PC.
        protected override unsafe Combatant GetMobFromByteArray(byte[] source, uint mycharID)
        {
            fixed (byte* p = source)
            {
                CombatantMemory mem = *(CombatantMemory*)&p[0];
                ObjectType type = (ObjectType)mem.Type;
                if (mem.ID == 0 || mem.ID == emptyID)
                    return null;
            }
            return GetCombatantFromByteArray(source, mycharID, false);
        }

        // Will return any kind of combatant, even if not a mob.
        // This function always returns a combatant object, even if empty.
        protected override unsafe Combatant GetCombatantFromByteArray(byte[] source, uint mycharID, bool isPlayer, bool exceptEffects = false)
        {
            fixed (byte* p = source)
            {
                CombatantMemory mem = *(CombatantMemory*)&p[0];

                if (isPlayer)
                {
                    mycharID = mem.ID;
                }

                Combatant combatant = new Combatant()
                {
                    Name = FFXIVMemory.GetStringFromBytes(mem.Name, CombatantMemory.NameBytes),
                    Job = mem.Job,
                    ID = mem.ID,
                    OwnerID = mem.OwnerID == emptyID ? 0 : mem.OwnerID,
                    Type = (ObjectType)mem.Type,
                    MonsterType = 0,
                    Status = (ObjectStatus)mem.Status,
                    ModelStatus = (ModelStatus)mem.ModelStatus,
                    // Normalize all possible aggression statuses into the basic 4 ones.
                    AggressionStatus = 0,
                    NPCTargetID = mem.NPCTargetID,
                    RawEffectiveDistance = mem.EffectiveDistance,
                    PosX = mem.PosX,
                    // Y and Z are deliberately swapped to match FFXIV_ACT_Plugin's data model
                    PosY = mem.PosZ,
                    PosZ = mem.PosY,
                    Heading = mem.Heading,
                    Radius = mem.Radius,
                    // In-memory there are separate values for PC's current target and NPC's current target
                    TargetID = (ObjectType)mem.Type == ObjectType.PC ? mem.PCTargetID : mem.NPCTargetID,
                    CurrentHP = mem.CurrentHP,
                    MaxHP = mem.MaxHP,
                    Effects = exceptEffects ? new List<EffectEntry>() : GetEffectEntries(mem.Effects, (ObjectType)mem.Type, mycharID),

                    BNpcID = mem.BNpcID,
                    CurrentMP = mem.CurrentMP,
                    MaxMP = mem.MaxMP,
                    CurrentGP = mem.CurrentGP,
                    MaxGP = mem.MaxGP,
                    CurrentCP = mem.CurrentCP,
                    MaxCP = mem.MaxCP,
                    Level = mem.Level,
                    PCTargetID = mem.PCTargetID,

                    BNpcNameID = mem.BNpcNameID,

                    WorldID = mem.WorldID,
                    CurrentWorldID = mem.CurrentWorldID,

                    IsCasting1 = mem.IsCasting1,
                    IsCasting2 = mem.IsCasting2,
                    CastBuffID = mem.CastBuffID,
                    CastTargetID = mem.CastTargetID,
                    // Y and Z are deliberately swapped to match FFXIV_ACT_Plugin's data model
                    CastGroundTargetX = mem.CastGroundTargetX,
                    CastGroundTargetY = mem.CastGroundTargetZ,
                    CastGroundTargetZ = mem.CastGroundTargetY,
                    CastDurationCurrent = mem.CastDurationCurrent,
                    CastDurationMax = mem.CastDurationMax,

                    TransformationId = mem.TransformationId,
                    WeaponId = mem.WeaponId
                };
                combatant.IsTargetable =
                    (combatant.ModelStatus == ModelStatus.Visible)
                    && ((combatant.Status == ObjectStatus.NormalActorStatus) || (combatant.Status == ObjectStatus.NormalSubActorStatus));
                if (combatant.Type != ObjectType.PC && combatant.Type != ObjectType.Monster)
                {
                    // Other types have garbage memory for hp.
                    combatant.CurrentHP = 0;
                    combatant.MaxHP = 0;
                }
                return combatant;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct CombatantMemory
        {
            public static int Size => Marshal.SizeOf(typeof(CombatantMemory));

            // 64 bytes per both FFXIV_ACT_Plugin and aers/FFXIVClientStructs
            public const int NameBytes = 64;

            public const int EffectCount = 60;
            public const int EffectBytes = EffectMemory72.Size * EffectCount;

            [FieldOffset(0x30)]
            public fixed byte Name[NameBytes];

            [FieldOffset(0x74)]
            public uint ID;

            [FieldOffset(0x80)]
            public uint BNpcID;

            [FieldOffset(0x84)]
            public uint OwnerID;

            [FieldOffset(0x8C)]
            public byte Type;

            [FieldOffset(0x92)]
            public byte EffectiveDistance;

            [FieldOffset(0x94)]
            public byte Status;

            [FieldOffset(0xA0)]
            public Single PosX;

            [FieldOffset(0xA4)]
            public Single PosY;

            [FieldOffset(0xA8)]
            public Single PosZ;

            [FieldOffset(0xB0)]
            public Single Heading;

            [FieldOffset(0xC0)]
            public Single Radius;

            [FieldOffset(0x108)]
            public int ModelStatus;

            [FieldOffset(0x19C)]
            public int CurrentHP;

            [FieldOffset(0x1A0)]
            public int MaxHP;

            [FieldOffset(0x1A4)]
            public int CurrentMP;

            [FieldOffset(0x1A8)]
            public int MaxMP;

            [FieldOffset(0x1AC)]
            public ushort CurrentGP;

            [FieldOffset(0x1AE)]
            public ushort MaxGP;

            [FieldOffset(0x1B0)]
            public ushort CurrentCP;

            [FieldOffset(0x1B2)]
            public ushort MaxCP;

            [FieldOffset(0x1B4)]
            public short TransformationId;

            [FieldOffset(0x1BA)]
            public byte Job;

            [FieldOffset(0x1BB)]
            public byte Level;

            [FieldOffset(0xC60)]
            public byte WeaponId;

            // TODO: Verify for 7.1. Could potentially be 0xD50, 0xF30, 0x1110
            [FieldOffset(0xD40)]
            public uint PCTargetID;

            // TODO: Verify for 7.1. Could potentially be 0x19EC or 0x2288
            [FieldOffset(0x2278)]
            public uint NPCTargetID;

            [FieldOffset(0x22A4)]
            public uint BNpcNameID;

            [FieldOffset(0x22C8)]
            public ushort CurrentWorldID;

            [FieldOffset(0x22CA)]
            public ushort WorldID;

            [FieldOffset(0x2328)]
            public fixed byte Effects[EffectBytes];

            [FieldOffset(0x2700)]
            public byte IsCasting1;

            [FieldOffset(0x2702)]
            public byte IsCasting2;

            [FieldOffset(0x2704)]
            public uint CastBuffID;

            [FieldOffset(0x2710)]
            public uint CastTargetID;

            [FieldOffset(0x2720)]
            public float CastGroundTargetX;

            [FieldOffset(0x2724)]
            public float CastGroundTargetY;

            [FieldOffset(0x2728)]
            public float CastGroundTargetZ;

            [FieldOffset(0x2734)]
            public float CastDurationCurrent;

            [FieldOffset(0x2738)]
            public float CastDurationMax;

            // Missing PartyType
        }

        protected new unsafe List<EffectEntry> GetEffectEntries(byte* source, ObjectType type, uint mycharID)
        {
            var result = new List<EffectEntry>();
            int maxEffects = (type == ObjectType.PC) ? 30 : 60;
            var size = EffectMemory72.Size * maxEffects;

            var bytes = new byte[size];
            Marshal.Copy((IntPtr)source, bytes, 0, size);

            for (int i = 0; i < maxEffects; i++)
            {
                var effect = GetEffectEntryFromByteArray(bytes, i);

                if (effect.BuffID > 0 &&
                    effect.Stack >= 0 &&
                    effect.Timer >= 0.0f &&
                    effect.ActorID > 0)
                {
                    effect.isOwner = effect.ActorID == mycharID;

                    result.Add(effect);
                }
            }

            return result;
        }

        protected new unsafe EffectEntry GetEffectEntryFromByteArray(byte[] source, int num = 0)
        {
            fixed (byte* p = source)
            {
                EffectMemory72 mem = *(EffectMemory72*)&p[num * EffectMemory72.Size];

                EffectEntry effectEntry = new EffectEntry()
                {
                    BuffID = mem.BuffID,
                    Stack = mem.Stack,
                    Timer = mem.Timer,
                    ActorID = mem.ActorID,
                    isOwner = false,
                };

                return effectEntry;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = Size)]
        public struct EffectMemory72
        {
            public const int Size = 16;

            [FieldOffset(0)]
            public ushort BuffID;

            [FieldOffset(2)]
            public ushort Stack;

            [FieldOffset(4)]
            public float Timer;

            [FieldOffset(8)]
            public uint ActorID;
        }
    }
}
