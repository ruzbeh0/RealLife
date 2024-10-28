using Game;
using Game.Simulation;
using Game.Citizens;
using Game.Common;
using Game.Prefabs;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

#nullable disable
namespace RealLife.Systems
{
    public partial class RealLifeCitizenSystem : GameSystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_CitizenPrefabs;
        private RealLifeCitizenSystem.TypeHandle __TypeHandle;

        [UnityEngine.Scripting.Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            // ISSUE: reference to a compiler-generated field
            this.m_Query = this.GetEntityQuery(ComponentType.ReadOnly<Citizen>(), ComponentType.ReadOnly<PrefabRef>(), ComponentType.Exclude<RandomLocalization>());
            // ISSUE: reference to a compiler-generated field
            this.m_CitizenPrefabs = this.GetEntityQuery(ComponentType.ReadOnly<CitizenData>());
            // ISSUE: reference to a compiler-generated field
            this.RequireForUpdate(this.m_Query);
        }

        [UnityEngine.Scripting.Preserve]
        protected override void OnUpdate()
        {
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            this.__TypeHandle.__Game_Prefabs_CitizenData_RO_ComponentLookup.Update(ref this.CheckedStateRef);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RW_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref this.CheckedStateRef);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref this.CheckedStateRef);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            RealLifeCitizenSystem.CitizenJob jobData = new RealLifeCitizenSystem.CitizenJob()
            {
                m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle,
                m_CitizenType = this.__TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle,
                m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RW_ComponentTypeHandle,
                m_CitizenPrefabs = this.m_CitizenPrefabs.ToEntityArray((AllocatorManager.AllocatorHandle)Allocator.TempJob),
                m_CitizenDatas = this.__TypeHandle.__Game_Prefabs_CitizenData_RO_ComponentLookup,
                m_RandomSeed = RandomSeed.Next()
            };
            // ISSUE: reference to a compiler-generated field
            this.Dependency = jobData.Schedule<RealLifeCitizenSystem.CitizenJob>(this.m_Query, this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            // ISSUE: reference to a compiler-generated method
            this.__AssignQueries(ref this.CheckedStateRef);
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated method
            this.__TypeHandle.__AssignHandles(ref this.CheckedStateRef);
        }

        [UnityEngine.Scripting.Preserve]
        public RealLifeCitizenSystem()
        {
        }

        //[BurstCompile]
        private struct CitizenJob : IJobChunk
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> m_CitizenPrefabs;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> m_CitizenType;
            public ComponentTypeHandle<PrefabRef> m_PrefabType;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentLookup<CitizenData> m_CitizenDatas;
            public RandomSeed m_RandomSeed;

            public void Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                // ISSUE: reference to a compiler-generated field
                NativeArray<Entity> nativeArray1 = chunk.GetNativeArray(this.m_EntityType);
                // ISSUE: reference to a compiler-generated field
                NativeArray<Citizen> nativeArray2 = chunk.GetNativeArray<Citizen>(ref this.m_CitizenType);
                // ISSUE: reference to a compiler-generated field
                NativeArray<PrefabRef> nativeArray3 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                // ISSUE: reference to a compiler-generated field
                Random random = this.m_RandomSeed.GetRandom(unfilteredChunkIndex);
                for (int index = 0; index < nativeArray2.Length; ++index)
                {
                    Entity entity = nativeArray1[index];
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated method
                    Entity prefab = RealLifeCitizenInitializeSystem.GetPrefab(this.m_CitizenPrefabs, nativeArray2[index], this.m_CitizenDatas, random);
                    nativeArray3[index] = new PrefabRef()
                    {
                        m_Prefab = prefab
                    };
                }
            }

            void IJobChunk.Execute(
              in ArchetypeChunk chunk,
              int unfilteredChunkIndex,
              bool useEnabledMask,
              in v128 chunkEnabledMask)
            {
                // ISSUE: reference to a compiler-generated method
                this.Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
            }
        }

        private struct TypeHandle
        {
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;
            [ReadOnly]
            public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RW_ComponentTypeHandle;
            [ReadOnly]
            public ComponentLookup<CitizenData> __Game_Prefabs_CitizenData_RO_ComponentLookup;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                // ISSUE: reference to a compiler-generated field
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                // ISSUE: reference to a compiler-generated field
                this.__Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>(true);
                // ISSUE: reference to a compiler-generated field
                this.__Game_Prefabs_PrefabRef_RW_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>();
                // ISSUE: reference to a compiler-generated field
                this.__Game_Prefabs_CitizenData_RO_ComponentLookup = state.GetComponentLookup<CitizenData>(true);
            }
        }
    }
}
