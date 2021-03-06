﻿namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class Runner
    {
        readonly Assembly assembly;
        readonly string[] customArguments;
        readonly Bus bus;

        public Runner(Assembly assembly, Listener listener)
            : this(assembly, new string[] {}, listener) { }

        public Runner(Assembly assembly, string[] customArguments, params Listener[] listeners)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
            bus = new Bus(listeners);
        }

        public void Discover()
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
                Discover(assembly.GetTypes(), discovery);
            }
            finally
            {
                discovery.Dispose();
            }
        }

        public ExecutionSummary Run()
        {
            return Run(assembly.GetTypes());
        }

        public ExecutionSummary Run(IReadOnlyList<Test> tests)
        {
            var request = new Dictionary<string, HashSet<string>>();
            var types = new List<Type>();

            foreach (var test in tests)
            {
                if (!request.ContainsKey(test.Class))
                {
                    request.Add(test.Class, new HashSet<string>());

                    var type = assembly.GetType(test.Class);

                    if (type != null)
                        types.Add(type);
                }

                request[test.Class].Add(test.Method);
            }

            return Run(types, method => request[method.ReflectedType!.FullName!].Contains(method.Name));
        }

        ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Func<MethodInfo, bool>? selected = null)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                return Run(candidateTypes, discovery, execution, selected);
            }
            finally
            {
                if (execution != discovery)
                    execution.Dispose();

                discovery.Dispose();
            }
        }

        internal void Discover(IReadOnlyList<Type> candidateTypes, Discovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in classes)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.Publish(new TestDiscovered(new Test(testMethod)));
        }

        internal ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution, Func<MethodInfo, bool>? selected = null)
        {
            var recorder = new ExecutionRecorder(bus);
            var classDiscoverer = new ClassDiscoverer(discovery);
            var classes = classDiscoverer.TestClasses(candidateTypes);
            var methodDiscoverer = new MethodDiscoverer(discovery);

            var testAssembly = new TestAssembly(assembly, recorder, classes, methodDiscoverer, selected, execution);
            recorder.Start(testAssembly);
            testAssembly.Run();
            return recorder.Complete(testAssembly);
        }
    }
}