using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Assertions;

namespace Chichiche.JobSystemUtilities
{
    public struct JobContext<TInput, TOutput> : IDisposable
        where TInput : struct
        where TOutput : struct
    {
        readonly int _innerLoopBatchCount;
        readonly int _length;
        bool _disposed;

        // ReSharper disable MemberCanBePrivate.Global
        public NativeArray<TInput> Input;
        public NativeArray<TOutput> Output;

        public JobContext(int length, Allocator allocator, int innerLoopBatchCount = 64)
        {
            _length = length;
            _innerLoopBatchCount = innerLoopBatchCount;
            _disposed = default;
            Input = new NativeArray<TInput>(length, allocator);
            Output = new NativeArray<TOutput>(length, allocator);
        }

        public JobHandle Run<TJob>(JobHandle dependsOn = default) where TJob : struct, IJobParallelFor<TInput, TOutput>
        {
            Assert.IsFalse(_disposed, $"{nameof( JobContext<TInput, TOutput> )} is disposed");
            var job = new TJob();
            job.SetContext(ref this);
            return job.Schedule(_length, _innerLoopBatchCount, dependsOn);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Input.Dispose();
            Output.Dispose();
        }
    }

    public interface IJobParallelFor<TInput, TOutput> : IJobParallelFor
        where TInput : struct
        where TOutput : struct
    {
        public void SetContext(ref JobContext<TInput, TOutput> context);
    }
}