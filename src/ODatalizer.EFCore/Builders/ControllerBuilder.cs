using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using ODatalizer.EFCore.Templates;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
        public Assembly Build<TDbContext>(IEdmModel edmModel, TDbContext db, string routeName, string @namespace) where TDbContext : DbContext
        {
            var code = ControllerGenerator.Create(edmModel, db, routeName, @namespace).TransformText();
            _logger.LogInformation(ControllerCodeGenerated, code);
            return Build(code, db);
        }
        public Assembly Build<TDbContext>(string code, TDbContext db) where TDbContext : DbContext
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var imports = new[] { 
                "mscorlib", 
                "netstandard", 
                "System.Private.CoreLib", 
                "System.Private.Uri", 
                "System.Runtime", 
                "System.Linq.Expressions", 
                "System.Threading.Tasks", 
                "System.Threading.Tasks.Extensions", 
                "Microsoft.Bcl.AsyncInterfaces" 
            };
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(asm => imports.Contains(asm.GetName().Name)).Concat(new[] {
                typeof(DbContext).Assembly,
                typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly,
                typeof(Microsoft.AspNetCore.Mvc.OkResult).Assembly,
                typeof(Microsoft.AspNet.OData.ODataController).Assembly,
                typeof(ILogger).Assembly,
                db.GetType().Assembly,
                Assembly.GetEntryAssembly(),
                Assembly.GetCallingAssembly(),
                Assembly.GetExecutingAssembly(),
            }).Distinct();
            var comp = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] { tree },
                references: assemblies.Select(asm => MetadataReference.CreateFromFile(asm.Location)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using (var ms = new MemoryStream())
            {
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

                return Assembly.Load(ms.ToArray());
            }
        }
    }
}
