﻿using Microsoft.ML.OnnxRuntime;
using System.Runtime.InteropServices;

namespace StableDiffusion.ML.OnnxRuntime
{
    public static class DirectXHelper
    {
        [DllImport("DirectXAdapterSelector.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int SelectDirectXAdapterId(bool preferHighPerformance);
    }

    public class StableDiffusionConfig
    {

        public enum ExecutionProvider
        {
            DirectML = 0,
            Cuda = 1,
            Cpu = 2
        }
        // default props
        public int NumInferenceSteps = 15;
        public ExecutionProvider ExecutionProviderTarget = ExecutionProvider.DirectML;
        public double GuidanceScale = 7.5;
        public int Height = 512;
        public int Width = 512;
        public int DeviceId = 1; // for CUDA
        public bool PreferHighPowerDirectXAdapter = true; // for DirectML

        public string TokenizerOnnxPath = "cliptokenizer.onnx";
        public string TextEncoderOnnxPath = "";
        public string UnetOnnxPath = "";
        public string VaeDecoderOnnxPath = "";
        public string SafetyModelPath = "";

        // default directory for images
        public string ImageOutputPath = "";

        public SessionOptions GetSessionOptionsForEp()
        {
            var sessionOptions = new SessionOptions();

            switch (this.ExecutionProviderTarget)
            {
                case ExecutionProvider.DirectML:
                    var deviceId = DirectXHelper.SelectDirectXAdapterId(this.PreferHighPowerDirectXAdapter);
                    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    sessionOptions.EnableMemoryPattern = false;
                    sessionOptions.AppendExecutionProvider_DML(deviceId);

                    return sessionOptions;
                case ExecutionProvider.Cpu:
                    sessionOptions.AppendExecutionProvider_CPU();
                    return sessionOptions;
                default:
                case ExecutionProvider.Cuda:
                    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    //default to CUDA, fall back on CPU if CUDA is not available.
                    sessionOptions.AppendExecutionProvider_CUDA(this.DeviceId);
                    sessionOptions.AppendExecutionProvider_CPU();
                    //sessionOptions = SessionOptions.MakeSessionOptionWithCudaProvider(cudaProviderOptions);
                    return sessionOptions;

            }

        }



    }


}
