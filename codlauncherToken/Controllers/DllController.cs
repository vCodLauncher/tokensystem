using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DllGeneratorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DllController : ControllerBase
    {
        [HttpGet("generate-dll")]
        public async Task<IActionResult> GenerateDll(string playerId, string token)
        {
            string sourceCode = $@"
            using System;
            namespace CustomDll
            {{
                public class TokenManager
                {{
                    public static string GetToken() => ""{token}"";
                }}
            }}";

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            string assemblyName = Path.GetRandomFileName();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                return BadRequest("Compilation failed");
            }

            ms.Seek(0, SeekOrigin.Begin);
            return File(ms.ToArray(), "application/octet-stream", "CustomDll.dll");
        }
    }
}