using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
public class JobGravitySystem : JobComponentSystem
{

    [BurstCompile]
    struct GravityJob : IJobProcessComponentData<GravityComponentData, Position>
    {
        //运行在其它线程中，需要拷贝一份GravityJobSystem的数据
        public float G;
        public float topY;
        public float bottomY;

        //非主线程不能使用Time.deltaTime
        public float deltaTime;


        public void Execute(ref GravityComponentData gravityData, ref Position position)
        {
            if (gravityData.delay > 0)
            {
                gravityData.delay -= deltaTime;
            }
            else
            {
                Vector3 pos = position.Value;
                float v = gravityData.velocity + G * gravityData.mass * deltaTime;
                pos.y += v;
                if (pos.y < bottomY)
                {
                    pos.y = topY;
                    gravityData.velocity = 0f;
                    gravityData.delay = 5;

                }

                position = new Position() { Value = pos };
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        GravityJob jobs = new GravityJob()
        {
            G = InitDemo.G,
            topY = InitDemo.topY,
            bottomY = InitDemo.bottomY,
            deltaTime = Time.deltaTime,
            //random = new Unity.Mathematics.Random((uint)(Time.time * 1000 + 1)),
        };

        return jobs.Schedule(this, inputDeps);
    }
}
