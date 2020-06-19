// ------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン: 16.0.0.0
//  
//     このファイルへの変更は、正しくない動作の原因になる可能性があり、
//     コードが再生成されると失われます。
// </auto-generated>
// ------------------------------------------------------------------------------
namespace ODatalizer.EFCore.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.OData.Edm;
    using ODatalizer.EFCore.Extensions;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class ControllerGenerator : ControllerGeneratorBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write("\n");
            this.Write(@"
//------------------------------------------------------------------------------
// <auto-generated>
//    This code is generated from a template.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ");
            this.Write(this.ToStringHelper.ToStringWithCulture(Namespace));
            this.Write("\n{\n");
 
    foreach(var entitySet in EdmModel.EntityContainer.EntitySets()) {
        var entitySetName = entitySet.Name;
        var keys = entitySet.EntityType().DeclaredKey;
        var controllerName = entitySetName + "Controller";
        var entityName = entitySet.EntityType().FullTypeName();
        var keysTypeNameComma = keys.Select(key => Type(key.Type) + " " + key.Name).Join(", ");
        var keysNameComma = keys.Select(key => key.Name).Join(", ");
        var keysNameBraceComma = keys.Select(key => "{" + key.Name + "}").Join(", ");

            this.Write("\n    public class ");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write(" : ODataController\n    {\n        private readonly ");
            this.Write(this.ToStringHelper.ToStringWithCulture(DbContextTypeName));
            this.Write(" _db;\n        private readonly ILogger<");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("> _logger;\n        public ");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(DbContextTypeName));
            this.Write(" db, ILogger<");
            this.Write(this.ToStringHelper.ToStringWithCulture(controllerName));
            this.Write("> logger)\n        {\n            _db = db;\n            _logger = logger;\n        }" +
                    "\n\n        [EnableQuery]\n        [ODataRoute(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public IActionResult Get()\n        {\n            return Ok(_db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(");\n        }\n\n        [EnableQuery]\n        [ODataRoute(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameBraceComma));
            this.Write(")\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public async Task<IActionResult> Get(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysTypeNameComma));
            this.Write(")\n        {\n            var entity = await _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".FindAsync(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameComma));
            this.Write(");\n\n            if (entity == null)\n                return NotFound();\n\n         " +
                    "   return Ok(entity);\n        }\n\n        [ODataRoute(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public async Task<IActionResult> Post([FromBody]");
            this.Write(this.ToStringHelper.ToStringWithCulture(entityName));
            this.Write(" entity)\n        {\n            if (!ModelState.IsValid)\n                return Ba" +
                    "dRequest(entity);\n\n            _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".Add(entity);\n\n            await _db.SaveChangesAsync();\n            \n           " +
                    " return Created(entity);\n        }\n\n        [ODataRoute(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameBraceComma));
            this.Write(")\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public async Task<IActionResult> Put(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysTypeNameComma));
            this.Write(", [FromBody]");
            this.Write(this.ToStringHelper.ToStringWithCulture(entityName));
            this.Write(" entity)\n        {\n            if (");
            this.Write(this.ToStringHelper.ToStringWithCulture(keys.Select(key => key.Name + " != entity." + key.Name).Join(" || ")));
            this.Write(")\n                return BadRequest();\n        \n            if (!ModelState.IsVal" +
                    "id)\n                return BadRequest(entity);\n\n            //using var tran = _" +
                    "db.Database.BeginTransaction();\n\n            var original = await _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".FindAsync(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameComma));
            this.Write(@");

            if (original == null)
                return NotFound();

            var entry = _db.Entry(entity);

            entry.State = EntityState.Modified;

            await _db.SaveChangesAsync();

            //tran.Commit();

            return NoContent();
        }

        [ODataRoute(""");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameBraceComma));
            this.Write(")\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public async Task<IActionResult> Patch(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysTypeNameComma));
            this.Write(", [FromBody]Delta<");
            this.Write(this.ToStringHelper.ToStringWithCulture(entityName));
            this.Write("> delta)\n        {\n            //using var tran = _db.Database.BeginTransaction()" +
                    ";\n            var original = await _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".FindAsync(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameComma));
            this.Write(");\n\n            if (original == null)\n                return NotFound();\n\n       " +
                    "     delta.Patch(original);\n\n            await _db.SaveChangesAsync();\n\n        " +
                    "    //tran.Commit();\n\n            return NoContent();\n        }\n\n        [ODataR" +
                    "oute(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write("(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameBraceComma));
            this.Write(")\", RouteName = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(RouteNameValue));
            this.Write(")]\n        public async Task<IActionResult> Delete(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysTypeNameComma));
            this.Write(")\n        {\n            //using var tran = _db.Database.BeginTransaction();\n     " +
                    "    \n            var entity = await _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".FindAsync(");
            this.Write(this.ToStringHelper.ToStringWithCulture(keysNameComma));
            this.Write(");\n\n            if (entity == null)\n                return NotFound();\n\n         " +
                    "   _db.");
            this.Write(this.ToStringHelper.ToStringWithCulture(entitySetName));
            this.Write(".Remove(entity);\n\n            await _db.SaveChangesAsync();\n\n            //tran.C" +
                    "ommit();\n\n            return NoContent();\n        }\n    }\n");
 } 
            this.Write("\n}");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class ControllerGeneratorBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
