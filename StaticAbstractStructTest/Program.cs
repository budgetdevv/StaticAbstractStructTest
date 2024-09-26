using System.Runtime.CompilerServices;

namespace StaticAbstractStructTest
{
    internal static class Program
    {
        private interface IOuterConfig
        {
            public static abstract InnerConfig Config { get; }
            
            public static abstract InnerConfig Config2 { get; }
            
            public static abstract InnerConfig ConfigUnoptimized { get; }
        }
        
        private struct OuterConfig: IOuterConfig
        {
            public static InnerConfig Config => new(true);

            private static readonly InnerConfig Config2 = ConfigUnoptimized;
            
            static InnerConfig IOuterConfig.Config2 { get; }

            public static InnerConfig ConfigUnoptimized
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(Random.Shared.Next(0, 1 + 1) == 0);
            }
        }

        private struct InnerConfig
        {
            public bool IsEnabled;

            public InnerConfig(bool isEnabled)
            {
                IsEnabled = isEnabled;
            }
        }

        [ModuleInitializer]
        internal static void Init()
        {
            // Avoid cctor deopt caused by AggressiveOptimization
            RuntimeHelpers.RunClassConstructor(typeof(OuterConfig).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(InnerConfigWithInstanceMethod).TypeHandle);
        }

        private static void Main(string[] args)
        {
            // How to check codegen:
            // Mac:
            // export DOTNET_JitDisasm="*OptimizedMethod*"
            
            // Windows:
            // $Env:DOTNET_JitDisasm="*OptimizedMethod*"
            // dotnet run -c Release

            OptimizedMethod<OuterConfig>();
            OptimizedMethod2<OuterConfig>();
            UnOptimizedMethod3();
            // UnOptimizedMethod<OuterConfig>();
            // UnOptimizedMethod2<OuterConfig>(true);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private static nint OptimizedMethod<ConfigT>() where ConfigT : IOuterConfig
        {
            if (ConfigT.Config.IsEnabled)
            {
                return 69;
            }
            
            else
            {
                return 0;
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private static nint OptimizedMethod2<ConfigT>() where ConfigT : IOuterConfig
        {
            if (ConfigT.Config2.IsEnabled)
            {
                return 69;
            }
            
            else
            {
                return 0;
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private static nint UnOptimizedMethod<ConfigT>() where ConfigT : IOuterConfig
        {
            if (ConfigT.ConfigUnoptimized.IsEnabled)
            {
                return 69;
            }
            
            else
            {
                return 0;
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private static nint UnOptimizedMethod2<ConfigT>(bool isEnabled) where ConfigT : IOuterConfig
        {
            if (isEnabled)
            {
                return 69;
            }
            
            else
            {
                return 0;
            }
        }

        private struct InnerConfigWithInstanceMethod
        {
            public static readonly InnerConfigWithInstanceMethod CONFIG = new(true);
            
            private bool IsEnabled;
            
            public InnerConfigWithInstanceMethod(bool isEnabled)
            {
                IsEnabled = isEnabled;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly nint Run()
            {
                if (IsEnabled)
                {
                    return 69;
                }
            
                else
                {
                    return 0;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private static nint UnOptimizedMethod3()
        {
            return InnerConfigWithInstanceMethod.CONFIG.Run();
        }
    }
}