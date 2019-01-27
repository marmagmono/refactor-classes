using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{

    /// <summary>
    /// <see cref="ConstructorPropertyRelationshipAnalyser"/> helps in building a model 
    /// which can answer questions about which property or field is set in constructor and how.
    /// </summary>
    public sealed class ConstructorPropertyRelationshipAnalyser
    {
        private readonly IFieldSymbol[] fields;
        private readonly IPropertySymbol[] properties;

        public ConstructorPropertyRelationshipAnalyser(
            IFieldSymbol[] fields,
            IPropertySymbol[] properties)
        {
            this.fields = fields ?? throw new ArgumentNullException(nameof(fields));
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            // Try to handle both 'this.prop = prop1;' and 'prop = prop1;'
            // TODO: exclude constant fields ?
            // TODO: test with arrow expression constructors ?
            // TODO: test with assignment inside if(....) {  } else { } / switch() ... etc.
            // TODO: Ensure it works assignments like A = a ?? throw or A = a == null ? xxx : yyy or ???
            // TODO: What to do in case of 
            // TODO: handle things like (a, b, c) = (10, 20, 30);
            // Exclude constructors with : this() ???
            // Constructors that call base ?
        }

        public ConstructorPropertyRelationshipAnalyserResult Analyze(
            SemanticModel documentModel,
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            var searcher = new AssignmentSearcher(documentModel, fields, properties);
            searcher.Visit(constructorDeclaration);
            return searcher.GetResult();
        }
    }
}
