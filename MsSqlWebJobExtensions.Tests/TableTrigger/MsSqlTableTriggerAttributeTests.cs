using System.Linq;
using Xunit;

namespace MsSqlWebJobExtensions.Tests.TableTrigger
{
    public class MsSqlTableTriggerAttributeTests
    {
        const int EXPECTED_DEFAULT_POLLING_INTERVAL = 5000;
        internal const string IRRELEVANT = "IRRELEVANT";
        private const int ALTERNATIVE_POLLING_INTERVAL = 1000;

        [Fact]
        public void Initializes_with_only_table_name()
        {
            var actualModules = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(c => c.Name == nameof(TestTarget));

            var actualTargetMethod = actualModules
                .SelectMany(m => m.GetMethods())
                .Where(m => m.Name == nameof(TestTarget.TargetMethod))
                .Where(m => m.GetParameters()
                    .Any(p => p.CustomAttributes
                        .Any(a => a.AttributeType == typeof(MsSqlTableTriggerAttribute))
                    )
                ).Single();

            var actualAttribute = actualTargetMethod.GetParameters()
                .SelectMany(p => p.CustomAttributes)
                .Where(a => a.AttributeType == typeof(MsSqlTableTriggerAttribute))
                .Single();

            Assert.Collection(actualAttribute.ConstructorArguments,
                arg => Assert.Equal("TABLE_NAME", arg.Value),
                arg => Assert.Equal(EXPECTED_DEFAULT_POLLING_INTERVAL, arg.Value)
            );
        }

        [Fact]
        public void Initializes_with_polling_interval()
        {
            var actualModules = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(c => c.Name == nameof(TestTarget));

            var actualTargetMethod = actualModules
                .SelectMany(m => m.GetMethods())
                .Where(m=>m.Name == nameof(TestTarget.TargetMethod2))
                .Where(m => m.GetParameters()
                    .Any(p => p.CustomAttributes
                        .Any(a => a.AttributeType == typeof(MsSqlTableTriggerAttribute))
                    )
                ).Single();

            var actualAttribute = actualTargetMethod.GetParameters()
                .SelectMany(p => p.CustomAttributes)
                .Where(a => a.AttributeType == typeof(MsSqlTableTriggerAttribute))
                .Single();

            Assert.Collection(actualAttribute.ConstructorArguments,
                arg => Assert.Equal(IRRELEVANT, arg.Value),
                arg => Assert.Equal(ALTERNATIVE_POLLING_INTERVAL, arg.Value)
            );
        }
    }


    public class TestTarget
    {
        public void TargetMethod([MsSqlTableTrigger("TABLE_NAME")] object targetProperty)
        {

        }

        public void TargetMethod2([MsSqlTableTrigger(MsSqlTableTriggerAttributeTests.IRRELEVANT, 1000)] object targetProperty)
        {

        }
    }
}
