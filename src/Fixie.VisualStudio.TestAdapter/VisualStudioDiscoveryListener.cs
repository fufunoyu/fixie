namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;
    using Runner;
    using Wrappers;

    public class VisualStudioDiscoveryListener : Handler<MethodGroupDiscovered>
    {
        readonly MessageLogger log;
        readonly TestCaseDiscoverySink discoverySink;
        readonly string assemblyPath;
        readonly SourceLocationProvider sourceLocationProvider;

        public VisualStudioDiscoveryListener(MessageLogger log, TestCaseDiscoverySink discoverySink, string assemblyPath)
        {
            this.log = log;
            this.discoverySink = discoverySink;
            this.assemblyPath = assemblyPath;
            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodGroupDiscovered message)
        {
            var methodGroup = message.MethodGroup;

            var testCase = new TestCaseModel
            {
                MethodGroup = methodGroup.FullName,
                AssemblyPath = assemblyPath
            };

            try
            {
                SourceLocation sourceLocation;
                if (sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation))
                {
                    testCase.CodeFilePath = sourceLocation.CodeFilePath;
                    testCase.LineNumber = sourceLocation.LineNumber;
                }
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
            }

            discoverySink.SendTestCase(testCase);
        }
    }
}