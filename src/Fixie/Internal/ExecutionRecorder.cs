﻿namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    class ExecutionRecorder
    {
        static readonly object[] EmptyParameters = { };

        readonly Bus bus;

        readonly ExecutionSummary assemblySummary;
        ExecutionSummary classSummary;

        readonly Stopwatch assemblyStopwatch;
        readonly Stopwatch classStopwatch;
        readonly Stopwatch caseStopwatch;

        public ExecutionRecorder(Bus bus)
        {
            this.bus = bus;

            assemblySummary = new ExecutionSummary();
            classSummary = new ExecutionSummary();
            
            assemblyStopwatch = new Stopwatch();
            classStopwatch = new Stopwatch();
            caseStopwatch = new Stopwatch();
        }

        public void Start(Assembly testAssembly)
        {
            bus.Publish(new AssemblyStarted(testAssembly));
            assemblyStopwatch.Restart();
        }

        public void Start(Type testClass)
        {
            classSummary = new ExecutionSummary();
            bus.Publish(new ClassStarted(testClass));
            classStopwatch.Restart();
            caseStopwatch.Restart();
        }

        public void Start(MethodInfo testMethod)
        {
            var test = new Test(testMethod);
            bus.Publish(new TestStarted(test));
        }

        public void Skip(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseSkipped(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Skip(MethodInfo testMethod)
        {
            var @case = new Case(testMethod, EmptyParameters);
            Skip(@case);
        }

        public void Pass(Case @case, string output)
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CasePassed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Fail(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseFailed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Fail(MethodInfo testMethod, Exception exception)
        {
            var @case = new Case(testMethod, EmptyParameters);
            @case.Fail(exception);
            Fail(@case);
        }

        public void Complete(Type testClass)
        {
            var duration = classStopwatch.Elapsed;
            classStopwatch.Stop();
            bus.Publish(new ClassCompleted(testClass, classSummary, duration));
            assemblySummary.Add(classSummary);
        }

        public ExecutionSummary Complete(Assembly testAssembly)
        {
            assemblyStopwatch.Stop();
            bus.Publish(new AssemblyCompleted(testAssembly, assemblySummary, assemblyStopwatch.Elapsed));
            return assemblySummary;
        }
    }
}