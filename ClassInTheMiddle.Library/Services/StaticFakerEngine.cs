using System;
using System.Collections.Generic;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    class StaticFakerEngine
    {
        List<StaticInvoke> staticInvokes;
        const string TYPE_NAME = "__StaticProxy";
        const string FAKE_METHOD_NAME_ENDING = "__StaticFake";

        public StaticFakerEngine(List<StaticInvoke> staticInvokes)
        {
            this.staticInvokes = staticInvokes;
        }

        public void Create()
        {
            var typeBuilder = TypeBuilderFactory.GetTypeBuilder(TYPE_NAME);
            foreach (var staticInvoke in staticInvokes)
            {
                var methodBuilder = typeBuilder.DefineMethod(
                    staticInvoke.OriginMethodInfo.Name + FAKE_METHOD_NAME_ENDING, 
                    staticInvoke.OriginMethodInfo.Attributes);
                var opcodeGenerator = new OpcodeGenerator(methodBuilder.GetILGenerator());
                opcodeGenerator.CallStaticMethod(staticInvoke.FakeMethodInfo);
                MethodChanger.Change(staticInvoke.OriginMethodInfo, methodBuilder);
            }
        }
    }
}
