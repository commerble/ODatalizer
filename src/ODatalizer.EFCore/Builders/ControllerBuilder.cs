using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using ODatalizer.EFCore.Extensions;
using ODatalizer.EFCore.Templates;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ODatalizer.EFCore.Builders
{
    public class ControllerBuilder
    {
        private readonly ILogger<ControllerBuilder> _logger;
        private static readonly EventId ControllerCodeGenerated = new EventId(90000, "ControllerCodeGenerated");
        private static readonly EventId ControllerCodeCompileFaild= new EventId(90001, "ControllerCodeCompileFaild");
        public ControllerBuilder(ILogger<ControllerBuilder> logger)
        {
            _logger = logger;
        }
        public Assembly Build(ODatalizerEndpoint ep)
        {
            var generator = ControllerGenerator.Create(ep);
            var code = generator.TransformText();
            _logger.LogInformation(ControllerCodeGenerated, code);
            return Build(code, generator.Namespace);
        }
        public Assembly Build(string code, string @namespace)
        {
            var dllPath = CalcFileName(code, @namespace);

            if (File.Exists(dllPath))
                return Assembly.LoadFrom(dllPath);

            var tree = CSharpSyntaxTree.ParseText(code);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.IsDynamic == false && string.IsNullOrEmpty(asm.Location) == false).Distinct();
            var comp = CSharpCompilation.Create(
                assemblyName: @namespace,
                syntaxTrees: new[] { tree },
                references: assemblies.Select(asm => MetadataReference.CreateFromFile(asm.Location)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            
            var result = comp.Emit(ms);
            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                foreach(var failur in failures)
                {
                    _logger.LogError(ControllerCodeCompileFaild, $"{failur.Id} - {failur.GetMessage()}");
                }

                throw new InvalidOperationException("Cannot compile controller code.");
            }

            try
            {
                var file = File.Open(dllPath, FileMode.CreateNew);
                ms.CopyTo(file);
            }
            catch(IOException)
            {
                if (File.Exists(dllPath))
                    return Assembly.LoadFrom(dllPath);
            }

            return Assembly.LoadFrom(dllPath);
        }

        public static string CalcFileName(string code, string @namespace)
        {
            using var hasher = new MD5CryptoServiceProvider();

            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(code)).Select(b => b.ToString("x2")).Join(string.Empty);
            
            hasher.Clear();

            return Path.Combine(Path.GetTempPath(), $"{@namespace}.{hash}.dll");
        }
    }
}
