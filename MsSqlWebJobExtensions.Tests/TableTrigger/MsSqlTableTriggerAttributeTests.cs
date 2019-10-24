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
            var targetMethod = typeof(TestTarget).GetMethod(nameof(TestTarget.TargetMethod));
            var attribute = GetAttributeData(targetMethod, typeof(MsSqlTableTriggerAttribute));

            Assert.Collection(attribute.ConstructorArguments,
                arg => Assert.Equal("TABLE_NAME", arg.Value),
                arg => Assert.Equal(EXPECTED_DEFAULT_POLLING_INTERVAL, arg.Value)
            );
        }

        [Fact]
        public void Initializes_with_polling_interval()
        {
            var targetMethod = typeof(TestTarget).GetMethod(nameof(TestTarget.TargetMethod2));
            var attribute = GetAttributeData(targetMethod, typeof(MsSqlTableTriggerAttribute));

            Assert.Collection(attribute.ConstructorArguments,
                arg => Assert.Equal(IRRELEVANT, arg.Value),
                arg => Assert.Equal(ALTERNATIVE_POLLING_INTERVAL, arg.Value)
            );
        }

        private static System.Reflection.CustomAttributeData GetAttributeData(System.Reflection.MethodInfo actualTargetMethod, System.Type type)
        {
            return actualTargetMethod.GetParameters()
                .SelectMany(p => p.CustomAttributes)
                .Where(a => a.AttributeType == type)
                .Single();
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
