using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ODatalizer.EFCore.Templates;
using ODatalizer.Extensions;
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
        
        public ControllerBuilder(ILogger<ControllerBuilder> logger)
        {
            _logger = logger;
        }
        public Assembly Build(ODatalizerEndpoint ep)
        {
            var generator = ControllerGenerator.Create(ep);
            var code = generator.TransformText();
            _logger.LogDebug(ODatalizerLogEvents.ControllerCodeGenerated, code);
            return Build(code, generator.Namespace);
        }
        public Assembly Build(string code, string @namespace)
        {
            var dllPath = CalcFileName(code, @namespace);

            if (File.Exists(dllPath))
                return Assembly.LoadFrom(dllPath);

            var tree = CSharpSyntaxTree.ParseText(code);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.IsDynamic == false && string.IsNullOrEmpty(asm.Location) == false).Concat(new[] { typeof(DbSet<>).Assembly } ).Distinct();
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
                    _logger.LogError(ODatalizerLogEvents.ControllerCodeCompileFaild, $"{failur.Id} - {failur.GetMessage()}");
                }

                throw new InvalidOperationException("Cannot compile controller code.");
            }

            var tmpPath = Path.GetTempFileName();
            File.WriteAllBytes(tmpPath, ms.ToArray());
            

            try
            {
                File.Move(tmpPath, dllPath);
            }
            catch(IOException ex)
            {
                if (File.Exists(dllPath) == false)
                    throw ex;
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
