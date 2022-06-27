using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

public class EntityHandler : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int entityCount;

    // Example Burst job that creates many entities
    [BurstCompatible]
    public struct SpawnJob : IJobParallelFor
    {
        public Entity Prototype;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(int index)
        {
            // プロトタイプとなるEntityからクローンし、新たにEntityを生成.
            // Entityの生成や変更等々はメインスレッドで実行する必要があるため、
            var e = Ecb.Instantiate(index, Prototype);
            // 必要なComponentは前もって設定されているため、それぞれのEntityに対し個別の処理を実行することが可能。
            // ここではindexに応じてTransformを変更している。
            Ecb.SetComponent(index, e, new Translation { Value = ComputeTransform(index) });
        }

        public float3 ComputeTransform(int index)
        {
            // indexに応じて適当に座標をずらす
            const int xMax = 100;
            var x = index % xMax;
            var z = index / xMax;
            // return float4x4.Translate(new float3(x, 0, z));
            return new float3(x, 0, z);
        }
    }

    void Start()
    {
        CreateEntity();
    }

    void CreateEntity()
    {
        // デフォルトのWorldを取得
        var world = World.DefaultGameObjectInjectionWorld;
        // 対象のWorldに存在するEntityの管理はすべて、EntityManagerを通して行う
        var manager = world.EntityManager;

        // スレッドセーフなコマンドバッファ。後ほど、Job内でEntityを生成するコマンドを積むために用意
        // Jobで使用するためTempJobで生成
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        // 変更されうるHybridRenderのコンポーネント一式が正しくEntityに追加されるよう、
        // RenderMeshDescriptionを生成
        var desc = new RenderMeshDescription(
            this.mesh,
            material,
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);

        // 空のEntityから、必要なComponentを追加して作りたいEntityを作成する。
        // RenderMeshDescriptionを使う関係上、ArcheTypeを先に作成することは難しいと思われる。
        // なおEntityManagerのAPIを使用してEntityを生成するのは、一般的には最も効率が悪いとのこと
        var prototype = manager.CreateEntity();
        
        // 対象のEntityに、HybridRendererが要求するComponentをAddする
        RenderMeshUtility.AddComponents(
            prototype,
            manager,
            desc);
        // と、LocalToWorldを追加
        manager.AddComponentData(prototype, new Translation());
        manager.AddComponentData(prototype, new LocalToWorld());

        // 事前に用意しておいたプロトタイプとなるEntityを、Jobを通してランタイムで大量に生成する。
        var spawnJob = new SpawnJob
        {
            Prototype = prototype,
            Ecb = ecb.AsParallelWriter(),
        };
        // Jobをスケジュール.
        // JobQueueあたりのバッチ数はサンプル通り128。公式のScriptingReferenceによれば、単純なJobなら32-128推奨とのこと
        var spawnHandle = spawnJob.Schedule(entityCount, 128);
        
        // サンプルなので即時待ちでOK
        spawnHandle.Complete();

        // EntityCommandBufferに溜めていた処理を一気に実行
        ecb.Playback(manager);
        ecb.Dispose();
        manager.DestroyEntity(prototype);
    }
}